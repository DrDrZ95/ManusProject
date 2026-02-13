namespace Agent.Application.Services.Memory;

/// <summary>
/// Short-term memory implementation using Cache and DB
/// </summary>
public class ShortTermMemoryService : IShortTermMemory
{
    private readonly IAgentCacheService _cacheService;
    private readonly IRepository<ChatMessageEntity, Guid> _messageRepo;
    private readonly ISemanticKernelService _semanticKernel;
    private readonly ILogger<ShortTermMemoryService> _logger;

    public ShortTermMemoryService(
        IAgentCacheService cacheService,
        IRepository<ChatMessageEntity, Guid> messageRepo,
        ISemanticKernelService semanticKernel,
        ILogger<ShortTermMemoryService> logger)
    {
        _cacheService = cacheService;
        _messageRepo = messageRepo;
        _semanticKernel = semanticKernel;
        _logger = logger;
    }

    public async Task<List<ChatMessage>> GetRecentHistoryAsync(string sessionId, int tokenLimit = 4000)
    {
        var cacheKey = $"chat_history:{sessionId}";

        // Try to get from cache first (L1/L2)
        // We use a factory that loads from DB if cache miss
        var history = await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return new List<ChatMessage>();
            }

            var entities = await _messageRepo.FindAsync(m => m.SessionId == sessionGuid);
            return entities.OrderBy(m => m.CreatedAt)
                           .Select(m => new ChatMessage { Role = m.Role, Content = m.Content })
                           .ToList();
        }, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));

        // Sliding window logic based on token limit (approximation)
        // Assuming 1 token ~= 4 chars for English, maybe 1 char for Chinese. 
        // A simple heuristic: take last N messages that fit.

        var result = new List<ChatMessage>();
        int currentTokens = 0;

        for (int i = history.Count - 1; i >= 0; i--)
        {
            var msg = history[i];
            int estimatedTokens = (msg.Content.Length / 4) + (msg.Role.Length / 4); // Rough estimate
            if (currentTokens + estimatedTokens > tokenLimit)
            {
                break;
            }
            result.Insert(0, msg);
            currentTokens += estimatedTokens;
        }

        return result;
    }

    public async Task AddMessageAsync(string sessionId, ChatMessage message)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            throw new ArgumentException("Invalid Session ID");
        }

        // 1. Save to DB
        var entity = new ChatMessageEntity
        {
            SessionId = sessionGuid,
            Role = message.Role,
            Content = message.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepo.AddAsync(entity);

        // 2. Update Cache
        var cacheKey = $"chat_history:{sessionId}";
        var history = await GetRecentHistoryAsync(sessionId, int.MaxValue);
        history.Add(message);
        await _cacheService.SetAsync(cacheKey, history, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));

        // 3. Auto-compress if history is too long (e.g. > 50 messages)
        if (history.Count > 50)
        {
            _ = CompressHistoryAsync(sessionId); // Run in background
        }
    }

    public async Task CompressHistoryAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid)) return;

        _logger.LogInformation("Compressing chat history for session {SessionId}", sessionId);

        var history = await GetRecentHistoryAsync(sessionId, int.MaxValue);
        if (history.Count < 20) return; // Not enough to compress

        // Take the oldest 30 messages to compress into a summary
        var toCompress = history.Take(30).ToList();
        var historyText = string.Join("\n", toCompress.Select(m => $"{m.Role}: {m.Content}"));

        var prompt = $@"Please summarize the following conversation history into a concise summary that preserves key facts and context.
Conversation:
{historyText}

Summary:";

        try
        {
            var summary = await _semanticKernel.GetChatCompletionAsync(prompt, "You are a helpful assistant that summarizes conversations.");

            // Create summary message
            var summaryMsg = new ChatMessage
            {
                Role = "system",
                Content = $"[History Summary]: {summary}",
                Metadata = new Dictionary<string, object> { { "type", "summary" }, { "compressed_count", toCompress.Count } }
            };

            // Save summary to DB
            var entity = new ChatMessageEntity
            {
                SessionId = sessionGuid,
                Role = summaryMsg.Role,
                Content = summaryMsg.Content,
                Metadata = JsonSerializer.Serialize(summaryMsg.Metadata),
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepo.AddAsync(entity);

            // In a real production system, we would mark the original 30 messages as 'archived' or delete them.
            // For now, we'll just invalidate the cache to force a reload from DB in next request.
            // (Note: To truly compress, the DB query in GetRecentHistoryAsync would need to exclude archived messages)
            var cacheKey = $"chat_history:{sessionId}";
            await _cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation("Compressed {Count} messages into summary for session {SessionId}", toCompress.Count, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compress history for session {SessionId}", sessionId);
        }
    }

    public async Task ClearHistoryAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid)) return;

        var messages = await _messageRepo.FindAsync(m => m.SessionId == sessionGuid);
        foreach (var msg in messages)
        {
            await _messageRepo.DeleteAsync(msg.Id);
        }

        await _cacheService.RemoveAsync($"chat_history:{sessionId}");
        _logger.LogInformation("Cleared history for session {SessionId}", sessionId);
    }
}

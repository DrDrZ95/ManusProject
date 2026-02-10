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

        // 2. Invalidate Cache (or Update)
        // Simple strategy: Invalidate, let next read populate.
        // Or better: Update the cached list.
        var cacheKey = $"chat_history:{sessionId}";
        // Since AgentCacheService doesn't expose "Update" or "Remove" easily (only Set), we can Set it again if we have the full list.
        // But for consistency/simplicity, we can just let it expire or if AgentCacheService has Remove, use it.
        // Checking AgentCacheService... it has SetAsync.
        // I'll leave cache invalidation for now or rely on TTL. 
        // Ideally I should update the cache.
        
        // Retrieve current cache to update it
        var history = await GetRecentHistoryAsync(sessionId, int.MaxValue);
        history.Add(message);
        await _cacheService.SetAsync(cacheKey, history, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));
    }

    public async Task CompressHistoryAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid)) return;

        // Get all history
        var allMessages = await _messageRepo.FindAsync(m => m.SessionId == sessionGuid);
        var sortedMessages = allMessages.OrderBy(m => m.CreatedAt).ToList();

        // If less than threshold (e.g. 20 messages), no need to compress
        if (sortedMessages.Count < 20) return;

        try 
        {
            // Summarize the first half
            var messagesToSummarize = sortedMessages.Take(sortedMessages.Count / 2).ToList();
            var conversationText = string.Join("\n", messagesToSummarize.Select(m => $"{m.Role}: {m.Content}"));
            
            var prompt = $"Summarize the following conversation history into a concise paragraph that retains key facts and context:\n\n{conversationText}";
            var summary = await _semanticKernel.GetChatCompletionAsync(prompt);

            // Create summary message
            var summaryMessage = new ChatMessageEntity
            {
                SessionId = sessionGuid,
                Role = "system",
                Content = $"[Memory Summary]: {summary}",
                CreatedAt = DateTime.UtcNow
            };

            // Save summary
            await _messageRepo.AddAsync(summaryMessage);

            // Delete summarized messages
            foreach (var msg in messagesToSummarize)
            {
                await _messageRepo.DeleteAsync(msg.Id);
            }

            // Invalidate cache
            var cacheKey = $"chat_history:{sessionId}";
            // Ideally we should remove key, but SetAsync with empty/new list works too. 
            // For now, let's just force refresh by not setting it here, allowing next read to fetch from DB.
            // Or better, set the new state.
            // Simplified: just let it expire or next read will handle it if we could remove. 
            // Since we can't remove easily (unless we cast or add method), we'll update it.
            var remaining = sortedMessages.Skip(sortedMessages.Count / 2).Select(m => new ChatMessage { Role = m.Role, Content = m.Content }).ToList();
            remaining.Insert(0, new ChatMessage { Role = summaryMessage.Role, Content = summaryMessage.Content });
            
            await _cacheService.SetAsync(cacheKey, remaining, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));
            
            _logger.LogInformation("Compressed chat history for session {SessionId}. Summarized {Count} messages.", sessionId, messagesToSummarize.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compress chat history for session {SessionId}", sessionId);
        }
    }
}

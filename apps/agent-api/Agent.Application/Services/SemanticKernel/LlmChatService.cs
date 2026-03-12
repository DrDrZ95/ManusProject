using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Runtime.CompilerServices;

namespace Agent.Application.Services.SemanticKernel;

public class LlmChatService : ILlmChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<LlmChatService> _logger;
    private readonly IAgentTraceService _agentTraceService;
    private readonly TokenCounterFactory _tokenCounterFactory;
    private readonly ITokenUsageRepository _tokenUsageRepo;
    private readonly ITokenBudgetService _budgetService;
    private readonly SemanticCacheLayer _semanticCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LlmChatService(
        Kernel kernel,
        IChatCompletionService chatService,
        SemanticKernelOptions options,
        ILogger<LlmChatService> logger,
        IAgentTraceService agentTraceService,
        TokenCounterFactory tokenCounterFactory,
        ITokenUsageRepository tokenUsageRepo,
        ITokenBudgetService budgetService,
        SemanticCacheLayer semanticCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _kernel = kernel;
        _chatService = chatService;
        _options = options;
        _logger = logger;
        _agentTraceService = agentTraceService;
        _tokenCounterFactory = tokenCounterFactory;
        _tokenUsageRepo = tokenUsageRepo;
        _budgetService = budgetService;
        _semanticCache = semanticCache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> GetChatCompletionAsync(string prompt, string? systemMessage = null)
    {
        return await _semanticCache.GetSemanticAsync<string>(
            prompt,
            async () =>
            {
                try
                {
                    _logger.LogInformation("Getting chat completion for prompt length: {PromptLength}", prompt.Length);

                    var chatHistory = new ChatHistory();
                    if (!string.IsNullOrEmpty(systemMessage)) chatHistory.AddSystemMessage(systemMessage);
                    chatHistory.AddUserMessage(prompt);

                    var executionSettings = new OpenAIPromptExecutionSettings
                    {
                        MaxTokens = _options.MaxTokens,
                        Temperature = _options.Temperature
                    };

                    var result = await _chatService.GetChatMessageContentAsync(chatHistory, executionSettings);
                    _logger.LogInformation("Chat completion successful, response length: {ResponseLength}", result.Content?.Length ?? 0);

                    return result.Content ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get chat completion");
                    throw;
                }
            }
        );
    }

    public async IAsyncEnumerable<string> GetStreamingChatCompletionAsync(string prompt, string? systemMessage = null)
    {
        _logger.LogInformation("Getting streaming chat completion for prompt length: {PromptLength}", prompt.Length);

        var chatHistory = new ChatHistory();
        if (!string.IsNullOrEmpty(systemMessage)) chatHistory.AddSystemMessage(systemMessage);
        chatHistory.AddUserMessage(prompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature
        };

        await foreach (var content in GetStreamingResponseAsync(chatHistory, executionSettings))
        {
            yield return content;
        }
    }

    public async Task<string> GetChatCompletionWithHistoryAsync(IEnumerable<ChatMessage> chatHistory)
    {
        try
        {
            _logger.LogInformation("Getting chat completion with {MessageCount} messages in history", chatHistory.Count());

            var kernelChatHistory = new ChatHistory();
            foreach (var message in chatHistory)
            {
                switch (message.Role.ToLowerInvariant())
                {
                    case "system": kernelChatHistory.AddSystemMessage(message.Content); break;
                    case "user": kernelChatHistory.AddUserMessage(message.Content); break;
                    case "assistant": kernelChatHistory.AddAssistantMessage(message.Content); break;
                    default: _logger.LogWarning("Unknown message role: {Role}", message.Role); break;
                }
            }

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature
            };

            var result = await _chatService.GetChatMessageContentAsync(kernelChatHistory, executionSettings);
            _logger.LogInformation("Chat completion with history successful");
            return result.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chat completion with history");
            throw;
        }
    }

    public async Task<string> ExecutePromptAsync(string prompt)
    {
        var traceId = Guid.NewGuid().ToString();
        var sessionId = GetCurrentSessionId();

        var thinkingTrace = new AgentTrace
        {
            TraceId = traceId,
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow,
            Type = TraceType.Thinking,
            Data = new Dictionary<string, object> { ["prompt"] = prompt }
        };

        await _agentTraceService.RecordTraceAsync(thinkingTrace);

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await _kernel.InvokePromptAsync(prompt);
            stopwatch.Stop();

            var resultText = result.ToString();
            var counter = _tokenCounterFactory.GetCounter(_options.ChatModel);
            int promptTokens = counter.CountTokens(prompt, _options.ChatModel);
            int completionTokens = counter.CountTokens(resultText, _options.ChatModel);
            
            var (actualPrompt, actualCompletion) = counter.ParseUsageFromResponse(result);
            if (actualPrompt > 0) promptTokens = actualPrompt;
            if (actualCompletion > 0) completionTokens = actualCompletion;
            int totalTokens = promptTokens + completionTokens;

            var finalCostUsd = CalculateCost(promptTokens, completionTokens);
            await _budgetService.RecordUsageAsync(sessionId, finalCostUsd);
            await RecordTokenUsageAsync(promptTokens, completionTokens, finalCostUsd, sessionId);

            var resultTrace = new AgentTrace
            {
                TraceId = traceId,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow,
                Type = TraceType.Result,
                Data = new Dictionary<string, object> { ["prompt"] = prompt, ["response"] = resultText },
                Metadata = new Dictionary<string, object>
                {
                    ["latency_ms"] = stopwatch.ElapsedMilliseconds,
                    ["prompt_tokens"] = promptTokens,
                    ["completion_tokens"] = completionTokens,
                    ["total_tokens"] = totalTokens,
                    ["model"] = _options.ChatModel
                },
                CostUsd = finalCostUsd,
                TokenCount = totalTokens
            };

            await _agentTraceService.RecordTraceAsync(resultTrace);
            return resultText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute prompt");
            var errorTrace = new AgentTrace
            {
                TraceId = traceId,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow,
                Type = TraceType.Result,
                Data = new Dictionary<string, object> { ["prompt"] = prompt, ["error"] = ex.Message }
            };
            await _agentTraceService.RecordTraceAsync(errorTrace);
            throw;
        }
    }

    private string GetCurrentSessionId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId) ? "system" : userId;
    }

    private double CalculateCost(int promptTokens, int completionTokens)
    {
        if (!_options.ModelCosts.TryGetValue(_options.ChatModel, out var costInfo)) return 0;
        return (promptTokens / 1000.0) * costInfo.InputCostPer1kTokens + (completionTokens / 1000.0) * costInfo.OutputCostPer1kTokens;
    }

    private async Task RecordTokenUsageAsync(int promptTokens, int completionTokens, double cost, string? sessionId = null)
    {
        try
        {
            var userId = GetCurrentSessionId();
            var record = new TokenUsageRecord
            {
                ModelId = _options.ChatModel,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                Cost = cost,
                UserId = userId,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            };
            await _tokenUsageRepo.AddRecordAsync(record);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record token usage to database");
        }
    }

    private async IAsyncEnumerable<string> GetStreamingResponseAsync(ChatHistory chatHistory, OpenAIPromptExecutionSettings executionSettings, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var streamingResult = _chatService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, cancellationToken: cancellationToken);
        await foreach (var content in streamingResult.WithCancellation(cancellationToken))
        {
            if (!string.IsNullOrEmpty(content.Content)) yield return content.Content;
        }
    }
}

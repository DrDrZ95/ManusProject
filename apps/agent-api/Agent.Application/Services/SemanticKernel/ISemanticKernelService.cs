namespace Agent.Application.Services.SemanticKernel;

/// <summary>
/// Semantic Kernel service interface for AI operations
/// 语义内核服务接口，用于AI操作
/// </summary>
public interface ISemanticKernelService
{
    // Chat Completion - 聊天完成
    Task<string> GetChatCompletionAsync(string prompt, string? systemMessage = null);
    IAsyncEnumerable<string> GetStreamingChatCompletionAsync(string prompt, string? systemMessage = null);
    Task<string> GetChatCompletionWithHistoryAsync(IEnumerable<ChatMessage> chatHistory);

    // Text Embeddings - 文本嵌入
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts);

    // Kernel Functions - 内核函数
    Task<string> InvokeFunctionAsync(string plugName, string functionName, Dictionary<string, object>? arguments = null);
    Task<T> InvokeFunctionAsync<T>(string plugName, string functionName, Dictionary<string, object>? arguments = null);

    // Plugin Management - 插件管理
    void AddPlugin(object plugin, string? pluginName = null);
    void AddPluginFromType<T>(string? pluginName = null) where T : class, new();
    IEnumerable<string> GetAvailableFunctions();

    // Memory Operations - 记忆操作
    Task SaveMemoryAsync(string collectionName, string text, string id, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<MemoryResult>> SearchMemoryAsync(string collectionName, string query, int limit = 10, float minRelevance = 0.7f);
    Task RemoveMemoryAsync(string collectionName, string id);
    Task<string> ExecutePromptAsync(string initialLlmInteractionFor);
}


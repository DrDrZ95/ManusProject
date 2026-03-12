namespace Agent.Application.Services.SemanticKernel;

public interface ILlmChatService
{
    Task<string> GetChatCompletionAsync(string prompt, string? systemMessage = null);
    IAsyncEnumerable<string> GetStreamingChatCompletionAsync(string prompt, string? systemMessage = null);
    Task<string> GetChatCompletionWithHistoryAsync(IEnumerable<ChatMessage> chatHistory);
    Task<string> ExecutePromptAsync(string prompt);
}

public interface IEmbeddingAndMemoryService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts);
    Task SaveMemoryAsync(string collectionName, string text, string id, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<MemoryResult>> SearchMemoryAsync(string collectionName, string query, int limit = 10, float minRelevance = 0.7f);
    Task RemoveMemoryAsync(string collectionName, string id);
}

public interface IPluginOrchestrationService
{
    Task<string> InvokeFunctionAsync(string plugName, string functionName, Dictionary<string, object>? arguments = null, string? fallbackPlugin = null, string? fallbackFunction = null);
    Task<T> InvokeFunctionAsync<T>(string plugName, string functionName, Dictionary<string, object>? arguments = null, string? fallbackPlugin = null, string? fallbackFunction = null);
    void AddPlugin(object plugin, string? pluginName = null);
    void AddPluginFromType<T>(string? pluginName = null) where T : class, new();
    IEnumerable<string> GetAvailableFunctions();
}

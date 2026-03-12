namespace Agent.Application.Services.SemanticKernel;

public class SemanticKernelServiceFacade : ISemanticKernelService
{
    private readonly ILlmChatService _chatService;
    private readonly IEmbeddingAndMemoryService _memoryService;
    private readonly IPluginOrchestrationService _pluginService;

    public SemanticKernelServiceFacade(
        ILlmChatService chatService,
        IEmbeddingAndMemoryService memoryService,
        IPluginOrchestrationService pluginService)
    {
        _chatService = chatService;
        _memoryService = memoryService;
        _pluginService = pluginService;
    }

    // ILlmChatService methods
    public Task<string> GetChatCompletionAsync(string prompt, string? systemMessage = null) => 
        _chatService.GetChatCompletionAsync(prompt, systemMessage);

    public IAsyncEnumerable<string> GetStreamingChatCompletionAsync(string prompt, string? systemMessage = null) => 
        _chatService.GetStreamingChatCompletionAsync(prompt, systemMessage);

    public Task<string> GetChatCompletionWithHistoryAsync(IEnumerable<ChatMessage> chatHistory) => 
        _chatService.GetChatCompletionWithHistoryAsync(chatHistory);

    public Task<string> ExecutePromptAsync(string prompt) => 
        _chatService.ExecutePromptAsync(prompt);

    // IEmbeddingAndMemoryService methods
    public Task<float[]> GenerateEmbeddingAsync(string text) => 
        _memoryService.GenerateEmbeddingAsync(text);

    public Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts) => 
        _memoryService.GenerateEmbeddingsAsync(texts);

    public Task SaveMemoryAsync(string collectionName, string text, string id, Dictionary<string, object>? metadata = null) => 
        _memoryService.SaveMemoryAsync(collectionName, text, id, metadata);

    public Task<IEnumerable<MemoryResult>> SearchMemoryAsync(string collectionName, string query, int limit = 10, float minRelevance = 0.7f) => 
        _memoryService.SearchMemoryAsync(collectionName, query, limit, minRelevance);

    public Task RemoveMemoryAsync(string collectionName, string id) => 
        _memoryService.RemoveMemoryAsync(collectionName, id);

    // IPluginOrchestrationService methods
    public Task<string> InvokeFunctionAsync(string plugName, string functionName, Dictionary<string, object>? arguments = null, string? fallbackPlugin = null, string? fallbackFunction = null) => 
        _pluginService.InvokeFunctionAsync(plugName, functionName, arguments, fallbackPlugin, fallbackFunction);

    public Task<T> InvokeFunctionAsync<T>(string plugName, string functionName, Dictionary<string, object>? arguments = null, string? fallbackPlugin = null, string? fallbackFunction = null) => 
        _pluginService.InvokeFunctionAsync<T>(plugName, functionName, arguments, fallbackPlugin, fallbackFunction);

    public void AddPlugin(object plugin, string? pluginName = null) => 
        _pluginService.AddPlugin(plugin, pluginName);

    public void AddPluginFromType<T>(string? pluginName = null) where T : class, new() => 
        _pluginService.AddPluginFromType<T>(pluginName);

    public IEnumerable<string> GetAvailableFunctions() => 
        _pluginService.GetAvailableFunctions();
}

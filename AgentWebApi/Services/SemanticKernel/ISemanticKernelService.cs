using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace AgentWebApi.Services.SemanticKernel;

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
    Task<string> InvokeFunctionAsync(string functionName, Dictionary<string, object>? arguments = null);
    Task<T> InvokeFunctionAsync<T>(string functionName, Dictionary<string, object>? arguments = null);

    // Plugin Management - 插件管理
    void AddPlugin(object plugin, string? pluginName = null);
    void AddPluginFromType<T>(string? pluginName = null) where T : class, new();
    IEnumerable<string> GetAvailableFunctions();

    // Memory Operations - 记忆操作
    Task SaveMemoryAsync(string collectionName, string text, string id, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<MemoryResult>> SearchMemoryAsync(string collectionName, string query, int limit = 10, float minRelevance = 0.7f);
    Task RemoveMemoryAsync(string collectionName, string id);
}

/// <summary>
/// Chat message representation
/// 聊天消息表示
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Memory search result
/// 记忆搜索结果
/// </summary>
public class MemoryResult
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public float Relevance { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Semantic Kernel configuration options
/// 语义内核配置选项
/// </summary>
public class SemanticKernelOptions
{
    /// <summary>
    /// OpenAI API key - OpenAI API密钥
    /// </summary>
    public string? OpenAIApiKey { get; set; }

    /// <summary>
    /// OpenAI model name for chat completion - 用于聊天完成的OpenAI模型名称
    /// </summary>
    public string ChatModel { get; set; } = "gpt-3.5-turbo";

    /// <summary>
    /// OpenAI model name for embeddings - 用于嵌入的OpenAI模型名称
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";

    /// <summary>
    /// Azure OpenAI endpoint - Azure OpenAI端点
    /// </summary>
    public string? AzureOpenAIEndpoint { get; set; }

    /// <summary>
    /// Azure OpenAI API key - Azure OpenAI API密钥
    /// </summary>
    public string? AzureOpenAIApiKey { get; set; }

    /// <summary>
    /// Azure OpenAI deployment name for chat - 用于聊天的Azure OpenAI部署名称
    /// </summary>
    public string? AzureChatDeploymentName { get; set; }

    /// <summary>
    /// Azure OpenAI deployment name for embeddings - 用于嵌入的Azure OpenAI部署名称
    /// </summary>
    public string? AzureEmbeddingDeploymentName { get; set; }

    /// <summary>
    /// Maximum tokens for completion - 完成的最大令牌数
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for completion - 完成的温度
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Enable memory functionality - 启用记忆功能
    /// </summary>
    public bool EnableMemory { get; set; } = true;

    /// <summary>
    /// Memory collection name - 记忆集合名称
    /// </summary>
    public string DefaultMemoryCollection { get; set; } = "default";
}


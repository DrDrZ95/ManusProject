namespace Agent.Application.Services.SemanticKernel;

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
    
    /// <summary>
    /// 嵌入结果缓存 TTL (Embedding Results Cache TTL) - 24 小时
    /// </summary>
    public TimeSpan EmbeddingResultsTtl { get; set; } = TimeSpan.FromHours(24);
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

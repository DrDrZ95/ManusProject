namespace Agent.Application.Services.RAG;

/// <summary>
/// RAG generation request
/// RAG生成请求
/// </summary>
public class RagGenerationRequest
{
    /// <summary>
    /// User query - 用户查询
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// System prompt template - 系统提示模板
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Retrieval options - 检索选项
    /// </summary>
    public RagQuery? RetrievalOptions { get; set; }

    /// <summary>
    /// Generation options - 生成选项
    /// </summary>
    public RagGenerationOptions? GenerationOptions { get; set; }

    /// <summary>
    /// Include sources in response - 在响应中包含来源
    /// </summary>
    public bool IncludeSources { get; set; } = true;

    /// <summary>
    /// Conversation history - 对话历史
    /// </summary>
    public List<ChatMessage>? ConversationHistory { get; set; }

    public string Prompt { get; set; }
}

/// <summary>
/// RAG generation options
/// RAG生成选项
/// </summary>
public class RagGenerationOptions
{
    /// <summary>
    /// Maximum tokens for generation - 生成的最大令牌数
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for generation - 生成的温度
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Top-p for generation - 生成的top-p
    /// </summary>
    public double TopP { get; set; } = 0.9;

    /// <summary>
    /// Enable streaming response - 启用流式响应
    /// </summary>
    public bool EnableStreaming { get; set; } = false;

    /// <summary>
    /// Response format - 响应格式
    /// </summary>
    public ResponseFormat Format { get; set; } = ResponseFormat.Text;
}

/// <summary>
/// RAG response
/// RAG响应
/// </summary>
public class RagResponse
{
    /// <summary>
    /// Generated response - 生成的响应
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Source chunks used for generation - 用于生成的源块
    /// </summary>
    public List<RagRetrievedChunk> Sources { get; set; } = new();

    /// <summary>
    /// Retrieval result - 检索结果
    /// </summary>
    public RagRetrievalResult? RetrievalResult { get; set; }

    /// <summary>
    /// Generation metadata - 生成元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Response confidence score - 响应置信度分数
    /// </summary>
    public float? ConfidenceScore { get; set; }

    /// <summary>
    /// Generation time - 生成时间
    /// </summary>
    public long GenerationTimeMs { get; set; }

    public string Summary { get; set; }
    public string  AnalysisResult { get; set; }
}
using Agent.Core.Services.VectorDatabase;
using Agent.Core.Services.SemanticKernel;

namespace Agent.Core.Services.RAG;

/// <summary>
/// RAG (Retrieval Augmented Generation) service interface
/// RAG（检索增强生成）服务接口
/// </summary>
public interface IRagService
{
    // Document Management - 文档管理
    Task<string> AddDocumentAsync(string collectionName, RagDocument document);
    Task<IEnumerable<RagDocument>> GetDocumentsAsync(string collectionName, IEnumerable<string>? ids = null);
    Task UpdateDocumentAsync(string collectionName, RagDocument document);
    Task DeleteDocumentAsync(string collectionName, string documentId);
    Task<int> GetDocumentCountAsync(string collectionName);

    // Hybrid Retrieval - 混合检索
    Task<RagRetrievalResult> HybridRetrievalAsync(string collectionName, RagQuery query);
    Task<RagRetrievalResult> VectorRetrievalAsync(string collectionName, string query, int topK = 10);
    Task<RagRetrievalResult> KeywordRetrievalAsync(string collectionName, string query, int topK = 10);
    Task<RagRetrievalResult> SemanticRetrievalAsync(string collectionName, string query, int topK = 10);

    // RAG Generation - RAG生成
    Task<RagResponse> GenerateResponseAsync(string collectionName, RagGenerationRequest request);
    Task<IAsyncEnumerable<string>> GenerateStreamingResponseAsync(string collectionName, RagGenerationRequest request);

    // Enterprise Scenarios - 企业场景
    Task<RagResponse> EnterpriseQAAsync(string knowledgeBase, string question, RagOptions? options = null);
    Task<RagResponse> DocumentSummarizationAsync(string collectionName, string documentId, RagSummaryOptions? options = null);
    Task<RagResponse> MultiDocumentAnalysisAsync(string collectionName, IEnumerable<string> documentIds, string analysisQuery);

    // Collection Management - 集合管理
    Task<string> CreateKnowledgeBaseAsync(string name, RagCollectionConfig config);
    Task DeleteKnowledgeBaseAsync(string name);
    Task<IEnumerable<string>> ListKnowledgeBasesAsync();
}

/// <summary>
/// RAG document representation
/// RAG文档表示
/// </summary>
public class RagDocument
{
    /// <summary>
    /// Document unique identifier - 文档唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document title - 文档标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Document content - 文档内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Document summary - 文档摘要
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Document metadata - 文档元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Document chunks for better retrieval - 用于更好检索的文档块
    /// </summary>
    public List<RagDocumentChunk> Chunks { get; set; } = new();

    /// <summary>
    /// Document creation time - 文档创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Document last update time - 文档最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// RAG document chunk for granular retrieval
/// 用于细粒度检索的RAG文档块
/// </summary>
public class RagDocumentChunk
{
    /// <summary>
    /// Chunk unique identifier - 块唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Parent document ID - 父文档ID
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Chunk content - 块内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Chunk position in document - 块在文档中的位置
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Chunk metadata - 块元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Pre-computed embedding - 预计算的嵌入
    /// </summary>
    public float[]? Embedding { get; set; }
}

/// <summary>
/// RAG query for hybrid retrieval
/// 用于混合检索的RAG查询
/// </summary>
public class RagQuery
{
    /// <summary>
    /// Query text - 查询文本
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Retrieval strategy - 检索策略
    /// </summary>
    public RetrievalStrategy Strategy { get; set; } = RetrievalStrategy.Hybrid;

    /// <summary>
    /// Number of results to retrieve - 要检索的结果数量
    /// </summary>
    public int TopK { get; set; } = 10;

    /// <summary>
    /// Minimum similarity threshold - 最小相似度阈值
    /// </summary>
    public float MinSimilarity { get; set; } = 0.7f;

    /// <summary>
    /// Metadata filters - 元数据过滤器
    /// </summary>
    public Dictionary<string, object>? Filters { get; set; }

    /// <summary>
    /// Hybrid retrieval weights - 混合检索权重
    /// </summary>
    public HybridRetrievalWeights? Weights { get; set; }

    /// <summary>
    /// Re-ranking options - 重排序选项
    /// </summary>
    public ReRankingOptions? ReRanking { get; set; }
}

/// <summary>
/// Hybrid retrieval weights for different strategies
/// 不同策略的混合检索权重
/// </summary>
public class HybridRetrievalWeights
{
    /// <summary>
    /// Vector similarity weight - 向量相似度权重
    /// </summary>
    public float VectorWeight { get; set; } = 0.6f;

    /// <summary>
    /// Keyword matching weight - 关键词匹配权重
    /// </summary>
    public float KeywordWeight { get; set; } = 0.3f;

    /// <summary>
    /// Semantic similarity weight - 语义相似度权重
    /// </summary>
    public float SemanticWeight { get; set; } = 0.1f;
}

/// <summary>
/// Re-ranking options for retrieved results
/// 检索结果的重排序选项
/// </summary>
public class ReRankingOptions
{
    /// <summary>
    /// Enable re-ranking - 启用重排序
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Re-ranking model - 重排序模型
    /// </summary>
    public string Model { get; set; } = "cross-encoder";

    /// <summary>
    /// Maximum results to re-rank - 最大重排序结果数
    /// </summary>
    public int MaxResults { get; set; } = 50;

    /// <summary>
    /// Re-ranking threshold - 重排序阈值
    /// </summary>
    public float Threshold { get; set; } = 0.5f;
}

/// <summary>
/// RAG retrieval result
/// RAG检索结果
/// </summary>
public class RagRetrievalResult
{
    /// <summary>
    /// Retrieved chunks - 检索到的块
    /// </summary>
    public List<RagRetrievedChunk> Chunks { get; set; } = new();

    /// <summary>
    /// Total number of matches - 匹配总数
    /// </summary>
    public int TotalMatches { get; set; }

    /// <summary>
    /// Retrieval execution time - 检索执行时间
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Retrieval strategy used - 使用的检索策略
    /// </summary>
    public RetrievalStrategy Strategy { get; set; }

    /// <summary>
    /// Retrieval metadata - 检索元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Retrieved chunk with relevance score
/// 带有相关性分数的检索块
/// </summary>
public class RagRetrievedChunk
{
    /// <summary>
    /// Document chunk - 文档块
    /// </summary>
    public RagDocumentChunk Chunk { get; set; } = new();

    /// <summary>
    /// Relevance score - 相关性分数
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Vector similarity score - 向量相似度分数
    /// </summary>
    public float VectorScore { get; set; }

    /// <summary>
    /// Keyword matching score - 关键词匹配分数
    /// </summary>
    public float KeywordScore { get; set; }

    /// <summary>
    /// Semantic similarity score - 语义相似度分数
    /// </summary>
    public float SemanticScore { get; set; }

    /// <summary>
    /// Re-ranking score - 重排序分数
    /// </summary>
    public float? ReRankScore { get; set; }

    /// <summary>
    /// Highlighted content - 高亮内容
    /// </summary>
    public string? HighlightedContent { get; set; }
}

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
}

/// <summary>
/// RAG options for enterprise scenarios
/// 企业场景的RAG选项
/// </summary>
public class RagOptions
{
    /// <summary>
    /// Enable citation - 启用引用
    /// </summary>
    public bool EnableCitation { get; set; } = true;

    /// <summary>
    /// Maximum context length - 最大上下文长度
    /// </summary>
    public int MaxContextLength { get; set; } = 4000;

    /// <summary>
    /// Response language - 响应语言
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// Domain-specific filters - 领域特定过滤器
    /// </summary>
    public Dictionary<string, object>? DomainFilters { get; set; }
}

/// <summary>
/// RAG summary options
/// RAG摘要选项
/// </summary>
public class RagSummaryOptions
{
    /// <summary>
    /// Summary length - 摘要长度
    /// </summary>
    public SummaryLength Length { get; set; } = SummaryLength.Medium;

    /// <summary>
    /// Summary style - 摘要风格
    /// </summary>
    public SummaryStyle Style { get; set; } = SummaryStyle.Informative;

    /// <summary>
    /// Include key points - 包含要点
    /// </summary>
    public bool IncludeKeyPoints { get; set; } = true;

    /// <summary>
    /// Target audience - 目标受众
    /// </summary>
    public string? TargetAudience { get; set; }
}

/// <summary>
/// RAG collection configuration
/// RAG集合配置
/// </summary>
public class RagCollectionConfig
{
    /// <summary>
    /// Collection description - 集合描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Chunk size for document splitting - 文档分割的块大小
    /// </summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>
    /// Chunk overlap - 块重叠
    /// </summary>
    public int ChunkOverlap { get; set; } = 200;

    /// <summary>
    /// Embedding model - 嵌入模型
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";

    /// <summary>
    /// Enable keyword indexing - 启用关键词索引
    /// </summary>
    public bool EnableKeywordIndex { get; set; } = true;

    /// <summary>
    /// Enable semantic indexing - 启用语义索引
    /// </summary>
    public bool EnableSemanticIndex { get; set; } = true;

    /// <summary>
    /// Collection metadata - 集合元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Retrieval strategy enumeration
/// 检索策略枚举
/// </summary>
public enum RetrievalStrategy
{
    Vector,      // 向量检索
    Keyword,     // 关键词检索
    Semantic,    // 语义检索
    Hybrid       // 混合检索
}

/// <summary>
/// Response format enumeration
/// 响应格式枚举
/// </summary>
public enum ResponseFormat
{
    Text,        // 文本
    Markdown,    // Markdown
    Json,        // JSON
    Structured   // 结构化
}

/// <summary>
/// Summary length enumeration
/// 摘要长度枚举
/// </summary>
public enum SummaryLength
{
    Short,       // 短
    Medium,      // 中
    Long,        // 长
    Detailed     // 详细
}

/// <summary>
/// Summary style enumeration
/// 摘要风格枚举
/// </summary>
public enum SummaryStyle
{
    Informative, // 信息性
    Executive,   // 执行摘要
    Technical,   // 技术性
    Casual       // 随意
}


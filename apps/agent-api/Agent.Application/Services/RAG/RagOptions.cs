namespace Agent.Application.Services.RAG;

/// <summary>
/// RAG 服务配置选项 (RAG Service Configuration Options)
/// </summary>
public class RagOptions
{
    /// <summary>
    /// 配置节名称 (Configuration section name)
    /// </summary>
    public const string Rag = "Rag";

    /// <summary>
    /// 热门 RAG 查询结果缓存 TTL (Popular RAG Query Results Cache TTL) - 1 小时
    /// </summary>
    public TimeSpan PopularRagQueryTtl { get; set; } = TimeSpan.FromHours(1);
    
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

    public int MaxResults { get; set; }
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
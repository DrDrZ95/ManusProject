namespace Agent.Application.Services.VectorDatabase;

/// <summary>
/// Vector document representation
/// 向量文档表示
/// </summary>
public class VectorDocument
{
    /// <summary>
    /// Document unique identifier - 文档唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document content - 文档内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Pre-computed embedding vector - 预计算的嵌入向量
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Document metadata - 文档元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Document modality type - 文档模态类型
    /// </summary>
    public Modality Modality { get; set; } = Modality.Text;

    /// <summary>
    /// Binary data for non-text modalities - 非文本模态的二进制数据
    /// </summary>
    public byte[]? BinaryData { get; set; }

    /// <summary>
    /// MIME type for binary data - 二进制数据的MIME类型
    /// </summary>
    public string? MimeType { get; set; }
}


/// <summary>
/// Vector search request
/// 向量搜索请求
/// </summary>
public class VectorSearchRequest
{
    /// <summary>
    /// Query text for text-based search - 基于文本搜索的查询文本
    /// </summary>
    public string? QueryText { get; set; }

    /// <summary>
    /// 查询文本数组（例如，转录文本作为辅助查询）。
    /// </summary>
    public string[] QueryTexts { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Query embedding vector - 查询嵌入向量
    /// </summary>
    public float[]? QueryEmbedding { get; set; }

    /// <summary>
    /// Search options - 搜索选项
    /// </summary>
    public VectorSearchOptions? Options { get; set; }

    /// <summary>
    /// Metadata filter - 元数据过滤器
    /// </summary>
    public VectorFilter? Filter { get; set; }

    /// <summary>
    /// TopK - 返回结果的数量（Top K）。
    /// </summary>
    public int TopK { get; set; }
}

/// <summary>
/// Multimodal search request
/// 多模态搜索请求
/// </summary>
public class MultimodalSearchRequest
{
    /// <summary>
    /// Text query - 文本查询
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Image data - 图像数据
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// Audio data - 音频数据
    /// </summary>
    public byte[]? AudioData { get; set; }

    /// <summary>
    /// Video data - 视频数据
    /// </summary>
    public byte[]? VideoData { get; set; }

    /// <summary>
    /// Search options - 搜索选项
    /// </summary>
    public VectorSearchOptions? Options { get; set; }

    /// <summary>
    /// Metadata filter - 元数据过滤器
    /// </summary>
    public VectorFilter? Filter { get; set; }

    /// <summary>
    /// Modality weights for fusion - 融合的模态权重
    /// </summary>
    public Dictionary<Modality, float>? ModalityWeights { get; set; }
}

/// <summary>
/// Vector search options
/// 向量搜索选项
/// </summary>
public class VectorSearchOptions
{
    /// <summary>
    /// Maximum number of results to return - 返回的最大结果数
    /// </summary>
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// Minimum similarity threshold - 最小相似度阈值
    /// </summary>
    public float? MinSimilarity { get; set; }

    /// <summary>
    /// Include embeddings in results - 在结果中包含嵌入
    /// </summary>
    public bool IncludeEmbeddings { get; set; } = false;

    /// <summary>
    /// Include metadata in results - 在结果中包含元数据
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;

    /// <summary>
    /// Include document content in results - 在结果中包含文档内容
    /// </summary>
    public bool IncludeContent { get; set; } = true;

    /// <summary>
    /// Metadata filter - 元数据过滤器
    /// </summary>
    public VectorFilter? Filter { get; set; }

    /// <summary>
    /// 返回结果的数量（Top K）。
    /// </summary>
    public int TopK { get; set; } = 10;
}

/// <summary>
/// Vector search result
/// 向量搜索结果
/// </summary>
public class VectorSearchResult
{
    /// <summary>
    /// Search results - 搜索结果
    /// </summary>
    public IEnumerable<VectorSearchMatch> Matches { get; set; } = new List<VectorSearchMatch>();

    /// <summary>
    /// Total number of matches found - 找到的匹配总数
    /// </summary>
    public int TotalMatches { get; set; }

    /// <summary>
    /// Search execution time in milliseconds - 搜索执行时间（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Search metadata - 搜索元数据
    /// </summary>
    public Dictionary<string, object>? SearchMetadata { get; set; }
}

/// <summary>
/// Vector search match result
/// 向量搜索匹配结果
/// </summary>
public class VectorSearchMatch
{
    /// <summary>
    /// Document ID - 文档ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Similarity score (0-1, higher is more similar) - 相似度分数（0-1，越高越相似）
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Distance from query - 与查询的距离
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// Document content - 文档内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Document embedding - 文档嵌入
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Document metadata - 文档元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Document modality - 文档模态
    /// </summary>
    public Modality Modality { get; set; }
}

/// <summary>
/// Vector filter for metadata-based filtering
/// 基于元数据过滤的向量过滤器
/// </summary>
public class VectorFilter
{
    /// <summary>
    /// Equality filters - 等值过滤器
    /// </summary>
    public Dictionary<string, object>? Equals { get; set; }

    /// <summary>
    /// Not equals filters - 不等值过滤器
    /// </summary>
    public Dictionary<string, object>? NotEquals { get; set; }

    /// <summary>
    /// In filters - 包含过滤器
    /// </summary>
    public Dictionary<string, IEnumerable<object>>? In { get; set; }

    /// <summary>
    /// Not in filters - 不包含过滤器
    /// </summary>
    public Dictionary<string, IEnumerable<object>>? NotIn { get; set; }

    /// <summary>
    /// Greater than filters - 大于过滤器
    /// </summary>
    public Dictionary<string, object>? GreaterThan { get; set; }

    /// <summary>
    /// Less than filters - 小于过滤器
    /// </summary>
    public Dictionary<string, object>? LessThan { get; set; }

    /// <summary>
    /// Contains filters for string fields - 字符串字段的包含过滤器
    /// </summary>
    public Dictionary<string, string>? Contains { get; set; }
}

/// <summary>
/// Supported modalities for multimodal AI
/// 多模态AI支持的模态
/// </summary>
public enum Modality
{
    Text,
    Image,
    Audio,
    Video,
    Document,
    Code,
    Structured
}

/// <summary>
/// Distance metrics for vector similarity
/// 向量相似度的距离度量
/// </summary>
public enum DistanceMetric
{
    Cosine,
    Euclidean,
    Manhattan,
    DotProduct
}


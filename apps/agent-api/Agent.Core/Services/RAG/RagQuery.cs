namespace Agent.Core.Services.RAG;


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

    public string QueryText { get; set; }
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

    public IEnumerable<string>? RetrievedDocuments { get; set; }
}
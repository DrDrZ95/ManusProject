namespace Agent.Core.Services.RAG;

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
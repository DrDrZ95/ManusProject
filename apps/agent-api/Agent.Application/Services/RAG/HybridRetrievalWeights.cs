namespace Agent.Application.Services.RAG;

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


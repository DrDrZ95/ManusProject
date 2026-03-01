namespace Agent.Core.Cache;

/// <summary>
/// 语义相似度缓存配置 (Semantic Similarity Cache Options)
/// </summary>
public class SemanticCacheOptions
{
    /// <summary>
    /// 语义相似度阈值，大于该值则认为是命中缓存 (0-1.0)
    /// </summary>
    public float SimilarityThreshold { get; set; } = 0.92f;

    /// <summary>
    /// 最大索引大小（内存/Redis 中保留的最近向量数量）
    /// </summary>
    public int MaxIndexSize { get; set; } = 100;

    /// <summary>
    /// 是否启用语义缓存
    /// </summary>
    public bool Enabled { get; set; } = true;
}

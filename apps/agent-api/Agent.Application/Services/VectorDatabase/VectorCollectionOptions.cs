namespace Agent.Application.Services.VectorDatabase;

/// <summary>
/// Vector collection configuration
/// 向量集合配置
/// </summary>
public class VectorCollectionOptions
{
    /// <summary>
    /// Embedding dimension - 嵌入维度
    /// </summary>
    public int? EmbeddingDimension { get; set; }

    /// <summary>
    /// Distance metric for similarity calculation - 相似度计算的距离度量
    /// </summary>
    public DistanceMetric DistanceMetric { get; set; } = DistanceMetric.Cosine;

    /// <summary>
    /// Collection metadata - 集合元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Supported modalities - 支持的模态
    /// </summary>
    public HashSet<Modality> SupportedModalities { get; set; } = new() { Modality.Text };
}


/// <summary>
/// Vector collection information
/// 向量集合信息
/// </summary>
public class VectorCollection
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public VectorCollectionOptions? Config { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


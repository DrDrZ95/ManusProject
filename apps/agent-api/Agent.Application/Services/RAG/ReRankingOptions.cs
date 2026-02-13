namespace Agent.Application.Services.RAG;

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


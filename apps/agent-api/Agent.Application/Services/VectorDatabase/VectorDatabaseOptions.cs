namespace Agent.Application.Services.VectorDatabase;

/// <summary>
/// 向量数据库配置选项 (Vector Database Configuration Options)
/// </summary>
public class VectorDatabaseOptions
{
    /// <summary>
    /// 配置节名称 (Configuration section name)
    /// </summary>
    public const string VectorDatabase = "VectorDatabase";

    /// <summary>
    /// 文档向量元数据缓存 TTL (Document Vectors Metadata Cache TTL) - 永久（由业务逻辑控制清除）
    /// </summary>
    public TimeSpan DocumentVectorMetadataTtl { get; set; } = TimeSpan.FromDays(365 * 10); // 模拟永久
}


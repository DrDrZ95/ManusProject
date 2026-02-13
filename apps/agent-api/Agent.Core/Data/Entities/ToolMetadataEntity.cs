namespace Agent.Core.Data.Entities;


/// <summary>
/// 工具元数据实体 (Tool Metadata Entity)
/// 用于在 PostgreSQL 中存储工具的定义、配置、权限和指标。
/// </summary>
[Table("ToolMetadata")]
public class ToolMetadataEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// 工具名称 (Unique Name)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工具描述 (Description)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 工具版本 (Version)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 工具类型 (Type: Plugin, WebAPI, Composite, etc.)
    /// </summary>
    [MaxLength(50)]
    public string Type { get; set; } = "Plugin";

    /// <summary>
    /// 参数 Schema (JSON Schema format)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? ParameterSchema { get; set; }

    /// <summary>
    /// 权限要求 (Required roles/policies)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? Permissions { get; set; }

    /// <summary>
    /// 成本信息 (API call cost, estimated time)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? CostInfo { get; set; }

    /// <summary>
    /// 可靠性指标 (Success rate, latency)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? ReliabilityMetrics { get; set; }

    /// <summary>
    /// 依赖关系 (Prerequisites)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? Dependencies { get; set; }

    /// <summary>
    /// 插件程序集/端点配置
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? Configuration { get; set; }

    /// <summary>
    /// 是否启用 (Is Enabled)
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 灰度发布比例 (0.0 - 1.0)
    /// </summary>
    public double ReleaseWeight { get; set; } = 1.0;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

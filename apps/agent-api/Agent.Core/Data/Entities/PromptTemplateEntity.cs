namespace Agent.Core.Data.Entities;

/// <summary>
/// Prompt Template Entity
/// 提示词模板实体
/// 
/// Manages prompt templates for optimization and versioning
/// 用于优化和版本管理的提示词模板管理
/// </summary>
[Table("prompt_templates")]
public class PromptTemplateEntity
{
    /// <summary>
    /// Unique identifier
    /// 唯一标识符
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Template Name
    /// 模板名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category (RAG, Workflow, General)
    /// 分类（RAG, Workflow, General）
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Version number
    /// 版本号
    /// </summary>
    [Required]
    [Column("version")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Prompt Content
    /// Prompt 内容
    /// </summary>
    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Metadata (Token count, parameter definitions, etc.)
    /// 元数据（包含 token 数、参数定义等）
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    [MaxLength(100)]
    [Column("experiment_id")]
    public string? ExperimentId { get; set; }

    [MaxLength(50)]
    [Column("variant_name")]
    public string? VariantName { get; set; }

    /// <summary>
    /// Created At
    /// 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Created By
    /// 创建人
    /// </summary>
    [MaxLength(100)]
    [Column("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Is Active (Is current production version)
    /// 是否为当前生产版本
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; }
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Data.Entities;

/// <summary>
/// API Key Usage Log Entity
/// API Key 使用日志实体
/// 
/// Records usage details for billing and auditing
/// 记录使用详情用于计费和审计
/// </summary>
[Table("api_key_usage_logs")]
public class ApiKeyUsageLogEntity
{
    /// <summary>
    /// Unique identifier
    /// 唯一标识符
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// API Key ID
    /// 关联的 API Key ID
    /// </summary>
    [Required]
    [Column("api_key_id")]
    public Guid ApiKeyId { get; set; }

    /// <summary>
    /// User ID who made the request
    /// 发起请求的用户 ID
    /// </summary>
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Model Name Used
    /// 使用的模型名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("model_name")]
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Input Tokens Count
    /// 输入 Token 数
    /// </summary>
    [Column("input_tokens")]
    public int InputTokens { get; set; }

    /// <summary>
    /// Output Tokens Count
    /// 输出 Token 数
    /// </summary>
    [Column("output_tokens")]
    public int OutputTokens { get; set; }

    /// <summary>
    /// Cost of the request
    /// 请求费用
    /// </summary>
    [Column("cost", TypeName = "decimal(18,6)")]
    public decimal Cost { get; set; }

    /// <summary>
    /// Request Duration (ms)
    /// 请求持续时间（毫秒）
    /// </summary>
    [Column("request_duration")]
    public int RequestDuration { get; set; }

    /// <summary>
    /// Is Success
    /// 是否成功
    /// </summary>
    [Column("is_success")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error Message (if failed)
    /// 错误信息（如果失败）
    /// </summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Executed At
    /// 执行时间
    /// </summary>
    [Column("executed_at")]
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    // Navigation property could be added here
    // public virtual ApiKeyPoolEntity ApiKeyPool { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Data.Entities;

/// <summary>
/// API Key Pool Entity
/// API Key 池实体
/// 
/// Manages multi-tenant API keys and quotas
/// 管理多租户 API Key 和配额
/// </summary>
[Table("api_key_pool")]
public class ApiKeyPoolEntity
{
    /// <summary>
    /// Unique identifier
    /// 唯一标识符
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Provider (OpenAI, DeepSeek, Kimi, etc.)
    /// 提供商
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// API Key (Encrypted storage)
    /// API Key（加密存储）
    /// </summary>
    [Required]
    [MaxLength(500)]
    [Column("api_key")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Alias for easy identification
    /// 便于识别的别名
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("alias")]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Monthly Budget
    /// 月度预算
    /// </summary>
    [Column("monthly_budget", TypeName = "decimal(18,2)")]
    public decimal MonthlyBudget { get; set; }

    /// <summary>
    /// Current Month Usage Amount
    /// 当前月份已使用金额
    /// </summary>
    [Column("current_month_usage", TypeName = "decimal(18,2)")]
    public decimal CurrentMonthUsage { get; set; }

    /// <summary>
    /// Daily Request Quota
    /// 每日请求配额
    /// </summary>
    [Column("daily_quota")]
    public int DailyQuota { get; set; }

    /// <summary>
    /// Current Day Requests Used
    /// 今日已使用请求数
    /// </summary>
    [Column("current_day_requests")]
    public int CurrentDayRequests { get; set; }

    /// <summary>
    /// Priority (for round-robin/failover)
    /// 优先级（用于轮询）
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// Is Active
    /// 是否激活
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Health Status (healthy, degraded, quota_exceeded, error)
    /// 健康状态
    /// </summary>
    [MaxLength(50)]
    [Column("health_status")]
    public string HealthStatus { get; set; } = "healthy";

    /// <summary>
    /// Last Health Check Time
    /// 最后健康检查时间
    /// </summary>
    [Column("last_health_check")]
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Assigned To Tenant ID (null for public keys)
    /// 分配给租户 ID（null 表示公共 Key）
    /// </summary>
    [MaxLength(100)]
    [Column("assigned_to")]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Created At
    /// 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

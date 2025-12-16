namespace Agent.Core.Data.Entities;

/// <summary>
/// Audit log entity for database storage
/// 用于数据库存储的审计日志实体
/// </summary>
[Table("audit_logs")]
public class AuditLogEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UserId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Changes { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
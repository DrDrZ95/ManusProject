namespace Agent.Core.Data.Entities;

/// <summary>
/// Structured memory entity for storing user preferences, facts, etc.
/// 结构化记忆实体，用于存储用户偏好、事实等
/// </summary>
[Table("structured_memories")]
public class StructuredMemoryEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string? UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // UserPreference, Fact, TaskHistory

    [Required]
    public string Content { get; set; } = string.Empty;

    public double ImportanceScore { get; set; } = 1.0;

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastAccessedAt { get; set; }
}

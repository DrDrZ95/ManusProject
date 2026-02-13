namespace Agent.Core.Data.Entities;

/// <summary>
/// Chat session entity for database storage
/// 用于数据库存储的聊天会话实体
/// </summary>
[Table("chat_sessions")]
public class ChatSessionEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(100)]
    public string? UserId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

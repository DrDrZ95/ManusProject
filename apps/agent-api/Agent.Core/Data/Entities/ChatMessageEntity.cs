namespace Agent.Core.Data.Entities;

/// <summary>
/// Chat message entity for database storage
/// 用于数据库存储的聊天消息实体
/// </summary>
[Table("chat_messages")]
public class ChatMessageEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty; // user, assistant, system

    [Required]
    public string Content { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
namespace Agent.Core.Models.Memory;

/// <summary>
/// Conversation metadata table / 会话元数据表
/// </summary>
[Table("conversation")]
public class Conversation
{
    /// <summary>
    /// Primary Key / 主键ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Conversation Title / 会话标题
    /// </summary>
    [Column("title")]
    [MaxLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// User Identifier / 用户标识
    /// </summary>
    [Column("user_id")]
    [MaxLength(64)]
    public string? UserId { get; set; }

    /// <summary>
    /// Creation Time / 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last Updated Time / 更新时间
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    public ICollection<ConversationSnapshot> Snapshots { get; set; } = new List<ConversationSnapshot>();
    public ICollection<ConversationAbilityLog> AbilityLogs { get; set; } = new List<ConversationAbilityLog>();
}

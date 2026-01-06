namespace Agent.Core.Models.Memory;

/// <summary>
/// Conversation Snapshot / 为快速恢复会话提供摘要与记忆
/// </summary>
[Table("conversation_snapshot")]
public class ConversationSnapshot
{
    /// <summary>
    /// Primary Key / 主键ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// Conversation ID / 会话ID (foreign key)
    /// </summary>
    [Column("conversation_id")]
    public long ConversationId { get; set; }

    /// <summary>
    /// Summary for fast restoration / 会话摘要用于快速恢复
    /// </summary>
    [Column("summary", TypeName = "text")]
    public string? Summary { get; set; }

    /// <summary>
    /// Memory elements or state / AI 对话记忆与状态
    /// </summary>
    [Column("memory", TypeName = "text")]
    public string? Memory { get; set; }

    /// <summary>
    /// Snapshot Time / 快照创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;
}

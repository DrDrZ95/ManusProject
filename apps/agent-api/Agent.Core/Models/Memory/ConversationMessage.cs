using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Models.Memory;

/// <summary>
/// Conversation Message Table / 会话所有消息内容存储
/// </summary>
[Table("conversation_message")]
public class ConversationMessage
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
    /// Message role user/assistant/system / 消息角色 user/assistant/system
    /// </summary>
    [Column("role")]
    [MaxLength(32)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Message Content / 消息内容
    /// </summary>
    [Column("content", TypeName = "text")] // LONGTEXT in MySQL maps to text in PostgreSQL/EF Core
    public string? Content { get; set; }

    /// <summary>
    /// Token count / 消耗 Token 数
    /// </summary>
    [Column("tokens")]
    public int? Tokens { get; set; }

    /// <summary>
    /// Creation Time / 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;
}

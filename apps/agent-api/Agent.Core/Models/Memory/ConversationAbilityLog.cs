using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Models.Memory;

/// <summary>
/// Log of tool/ability executions inside conversations / 会话期间工具调用日志
/// </summary>
[Table("conversation_ability_log")]
public class ConversationAbilityLog
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
    /// Message ID that triggered tool call / 触发调用的消息ID
    /// </summary>
    [Column("message_id")]
    public long? MessageId { get; set; }

    /// <summary>
    /// Ability or tool name / 工具或能力名称
    /// </summary>
    [Column("ability_name")]
    [MaxLength(255)]
    public string AbilityName { get; set; } = string.Empty;

    /// <summary>
    /// Arguments for call / 调用参数
    /// </summary>
    [Column("request_payload", TypeName = "text")]
    public string? RequestPayload { get; set; }

    /// <summary>
    /// Execution result / 执行结果
    /// </summary>
    [Column("response_payload",TypeName = "text")]
    public string? ResponsePayload { get; set; }

    /// <summary>
    /// Call status success/failed / 执行状态 成功/失败
    /// </summary>
    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = "success";

    /// <summary>
    /// Creation Time / 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;
}

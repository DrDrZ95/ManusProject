using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Data.Entities;

/// <summary>
/// Token usage record entity for database storage
/// Token 使用记录实体，用于数据库存储
/// </summary>
[Table("token_usage_records")]
public class TokenUsageRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string ModelId { get; set; } = string.Empty;

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens => PromptTokens + CompletionTokens;

    public double Cost { get; set; }

    [MaxLength(100)]
    public string? UserId { get; set; }

    [MaxLength(100)]
    public string? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }
}

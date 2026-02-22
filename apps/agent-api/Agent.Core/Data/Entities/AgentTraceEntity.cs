namespace Agent.Core.Data.Entities;

[Table("agent_traces")]
public class AgentTraceEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("trace_id")]
    public string TraceId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    [Column("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("data", TypeName = "jsonb")]
    public string? Data { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("cost_usd", TypeName = "numeric(10,4)")]
    public decimal? CostUsd { get; set; }

    [Column("token_count")]
    public int? TokenCount { get; set; }
}


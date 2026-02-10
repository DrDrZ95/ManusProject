namespace Agent.Core.Data.Entities;

/// <summary>
/// Task execution context entity for workflow tasks
/// 工作流任务的执行上下文实体
/// </summary>
[Table("task_execution_contexts")]
public class TaskExecutionContextEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string WorkflowId { get; set; } = string.Empty;

    public string? StepId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? ContextData { get; set; } // Variable state, intermediate results

    [Column(TypeName = "jsonb")]
    public string? ToolCallHistory { get; set; }

    [Column(TypeName = "jsonb")]
    public string? DecisionPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

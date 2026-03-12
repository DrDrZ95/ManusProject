namespace Agent.Core.Workflow;

public class WorkflowPlan
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStep> Steps { get; set; } = new();
    public List<string> ExecutorKeys { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public PlanStatus Status { get; set; }
    public WorkflowState CurrentState { get; set; }
    public string? VisualGraphJson { get; set; }
    public int FailureCount { get; set; }
}

public class WorkflowStep
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public PlanStepStatus Status { get; set; }
    public string? Result { get; set; }
    public string? Error { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? PerformanceData { get; set; }
    public bool HasBreakpoint { get; set; }

    /// <summary>
    /// 前置步骤索引列表 (List of prerequisite step indices)
    /// </summary>
    public List<int> DependsOn { get; set; } = new();

    /// <summary>
    /// 执行该步骤的条件表达式 (Conditional expression for executing this step)
    /// 例如: "step[0].result == 'success'"
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// 条件表达式的类型 (Type of conditional expression)
    /// </summary>
    public string? ConditionalExpressionType { get; set; }
}

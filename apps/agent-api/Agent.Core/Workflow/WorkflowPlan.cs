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
}

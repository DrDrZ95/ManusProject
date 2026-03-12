namespace Agent.Application.Services.Workflow;

public class WorkflowProgress { 
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public double Percentage { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}

public class WorkflowPerformanceReport {
    public Guid PlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<int, StepPerformanceMetric> StepMetrics { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
}

public class StepPerformanceMetric
{
    public string Text { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public bool IsSuccess { get; set; }
    public double Cost { get; set; }
    public bool IsBottleneck { get; set; }
}

namespace Agent.Core.Workflow;

public class CreatePlanRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public List<string> ExecutorKeys { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string? UserGoal { get; set; }
}

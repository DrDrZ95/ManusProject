namespace Agent.Application.Services.Prompts;

public class MlflowOptions
{
    public string TrackingUri { get; set; } = string.Empty;
    public string ExperimentName { get; set; } = "prompt_experiments";
}


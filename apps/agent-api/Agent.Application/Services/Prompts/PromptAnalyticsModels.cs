namespace Agent.Application.Services.Prompts;

public class PromptExecutionMetrics
{
    public Guid PromptTemplateId { get; set; }
    public Guid? ExecutionId { get; set; }
    public int TokensUsed { get; set; }
    public int ResponseTimeMs { get; set; }
    public int? UserFeedback { get; set; }
    public decimal? QualityScore { get; set; }
    public decimal? CostUsd { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string? ExperimentId { get; set; }
    public string? VariantName { get; set; }
}

public class PromptVariantStatistics
{
    public Guid PromptTemplateId { get; set; }
    public string? ExperimentId { get; set; }
    public string? VariantName { get; set; }
    public int ExecutionCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double AverageQualityScore { get; set; }
    public double AverageUserFeedback { get; set; }
    public double AverageCostUsd { get; set; }
    public double SuccessRate { get; set; }
}

public class AbTestResult
{
    public string ExperimentId { get; set; } = string.Empty;
    public string ControlVariant { get; set; } = "A";
    public string TreatmentVariant { get; set; } = "B";
    public bool IsStatisticallySignificant { get; set; }
    public double PValue { get; set; }
    public string PrimaryMetric { get; set; } = "quality_score";
    public double ControlMetricValue { get; set; }
    public double TreatmentMetricValue { get; set; }
}


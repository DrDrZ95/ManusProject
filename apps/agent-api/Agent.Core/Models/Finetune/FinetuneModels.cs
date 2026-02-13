namespace Agent.Core.Models.Finetune;

public class StartFinetuneResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ChineseMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FinetuneJobStatusResponse
{
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string ChineseStatusDisplay { get; set; } = string.Empty;
    public double Progress { get; set; }
    public int CurrentEpoch { get; set; }
    public int TotalEpochs { get; set; }
    public double? CurrentLoss { get; set; }
    public double? BestLoss { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputPath { get; set; }
    public string FineTunedModel { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Error { get; set; }
}

public class FinetuneJobSummary
{
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string BaseModel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string ChineseStatusDisplay { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class JobLogsResponse
{
    public string JobId { get; set; } = string.Empty;
    public IEnumerable<string> Logs { get; set; } = new List<string>();
}


namespace Agent.Core.Models;

public class MetricEntry
{
    public string MetricName { get; set; }
    public long Value { get; set; }
    public DateTime Timestamp { get; set; }
}

namespace Agent.Application.Services.Prometheus;

public class PrometheusService : IPrometheusService
{
    private static readonly Counter RequestCounter = Metrics.CreateCounter("http_requests_total", "Total number of HTTP requests.", new CounterConfiguration
    {
        LabelNames = new[] { "endpoint" }
    });

    private static readonly Histogram RequestDuration = Metrics.CreateHistogram("http_request_duration_seconds", "Duration of HTTP requests in seconds.", new HistogramConfiguration
    {
        Buckets = Histogram.ExponentialBuckets(0.01, 2, 10), // 0.01 to 10.24 seconds
        LabelNames = new[] { "endpoint" }
    });

    private static readonly Counter ToolCallCounter = Metrics.CreateCounter("agent_tool_calls_total", "Total number of tool calls.", new CounterConfiguration
    {
        LabelNames = new[] { "plugin", "function", "status" }
    });

    private static readonly Histogram ToolCallDuration = Metrics.CreateHistogram("agent_tool_call_duration_seconds", "Duration of tool calls in seconds.", new HistogramConfiguration
    {
        Buckets = Histogram.ExponentialBuckets(0.01, 2, 12), // 0.01 to ~40 seconds
        LabelNames = new[] { "plugin", "function" }
    });

    private static readonly Counter ToolCostCounter = Metrics.CreateCounter("agent_tool_cost_total", "Total cost of tool calls.", new CounterConfiguration
    {
        LabelNames = new[] { "plugin", "function", "unit" }
    });

    private static readonly Counter ToolTokenCounter = Metrics.CreateCounter("agent_tool_tokens_total", "Total tokens consumed by tool calls.", new CounterConfiguration
    {
        LabelNames = new[] { "plugin", "function", "type" }
    });

    public void IncrementRequestCounter(string endpoint)
    {
        RequestCounter.WithLabels(endpoint).Inc();
    }

    public void ObserveRequestDuration(string endpoint, double duration)
    {
        RequestDuration.WithLabels(endpoint).Observe(duration);
    }

    public void IncrementToolCallCounter(string pluginName, string functionName, string status)
    {
        ToolCallCounter.WithLabels(pluginName, functionName, status).Inc();
    }

    public void ObserveToolCallDuration(string pluginName, string functionName, double duration)
    {
        ToolCallDuration.WithLabels(pluginName, functionName).Observe(duration);
    }

    public void RecordToolCost(string pluginName, string functionName, double cost, string unit)
    {
        ToolCostCounter.WithLabels(pluginName, functionName, unit).Inc(cost);
    }

    public void RecordToolTokens(string pluginName, string functionName, string tokenType, int count)
    {
        ToolTokenCounter.WithLabels(pluginName, functionName, tokenType).Inc(count);
    }
}


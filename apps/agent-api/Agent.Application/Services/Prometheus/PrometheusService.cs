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

    public void IncrementRequestCounter(string endpoint)
    {
        RequestCounter.WithLabels(endpoint).Inc();
    }

    public void ObserveRequestDuration(string endpoint, double duration)
    {
        RequestDuration.WithLabels(endpoint).Observe(duration);
    }
}
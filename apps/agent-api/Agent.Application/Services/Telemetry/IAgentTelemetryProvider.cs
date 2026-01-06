namespace Agent.Application.Services.Telemetry;

public interface IAgentTelemetryProvider
{
    IAgentSpan StartSpan(string operationName, SpanKind kind = SpanKind.Internal);
    void RecordMetric<T>(string name, T value, params KeyValuePair<string, object?>[] tags);
    void RecordEvent(string name, object? data = null);
}

public interface IAgentSpan : IDisposable
{
    void SetAttribute(string key, object? value);
    void RecordException(Exception exception);
    void SetStatus(ActivityStatusCode statusCode, string? description = null);
    void AddEvent(string name, object? data = null);
}
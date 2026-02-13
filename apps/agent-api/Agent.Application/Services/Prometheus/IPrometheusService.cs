namespace Agent.Application.Services.Prometheus;

public interface IPrometheusService
{
    void IncrementRequestCounter(string endpoint);
    void ObserveRequestDuration(string endpoint, double duration);

    // Tool specific metrics
    void IncrementToolCallCounter(string pluginName, string functionName, string status);
    void ObserveToolCallDuration(string pluginName, string functionName, double duration);
    void RecordToolCost(string pluginName, string functionName, double cost, string unit);
    void RecordToolTokens(string pluginName, string functionName, string tokenType, int count);
}


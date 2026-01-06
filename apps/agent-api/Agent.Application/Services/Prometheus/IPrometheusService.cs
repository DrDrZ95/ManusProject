namespace Agent.Application.Services.Prometheus
{
    public interface IPrometheusService
    {
        void IncrementRequestCounter(string endpoint);
        void ObserveRequestDuration(string endpoint, double duration);
    }
}


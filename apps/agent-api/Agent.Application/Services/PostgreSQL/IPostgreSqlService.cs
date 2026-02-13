namespace Agent.Application.Services.PostgreSQL;

public interface IPostgreSqlService
{
    Task AddMetricEntryAsync(MetricEntry metricEntry);
}


using Agent.Core.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Agent.Core.Services.PostgreSQL
{
    public class PostgreSqlService : IPostgreSqlService
    {
        private readonly ILogger<PostgreSqlService> _logger;

        public PostgreSqlService(ILogger<PostgreSqlService> logger)
        {
            _logger = logger;
        }

        public Task AddMetricEntryAsync(MetricEntry metricEntry)
        {
            _logger.LogInformation("Simulating saving metric {MetricName} with value {Value} to PostgreSQL.", 
                metricEntry.MetricName, metricEntry.Value);
            // In a real application, this would involve using Entity Framework Core
            // or another ORM to save the metricEntry to a PostgreSQL database.
            return Task.CompletedTask;
        }
    }
}

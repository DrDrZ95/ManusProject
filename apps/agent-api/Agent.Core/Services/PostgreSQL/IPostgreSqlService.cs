using Agent.Core.Models;
using System.Threading.Tasks;

namespace Agent.Core.Services.PostgreSQL
{
    public interface IPostgreSqlService
    {
        Task AddMetricEntryAsync(MetricEntry metricEntry);
    }
}

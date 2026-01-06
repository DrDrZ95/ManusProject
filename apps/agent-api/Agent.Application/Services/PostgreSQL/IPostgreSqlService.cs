using Agent.Core.Models;
using System.Threading.Tasks;

namespace Agent.Application.Services.PostgreSQL
{
    public interface IPostgreSqlService
    {
        Task AddMetricEntryAsync(MetricEntry metricEntry);
    }
}

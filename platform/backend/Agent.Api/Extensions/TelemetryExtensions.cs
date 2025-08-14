
using Agent.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Core.Extensions
{
    public static class TelemetryExtensions
    {
        public static IServiceCollection AddAgentTelemetry(this IServiceCollection services, string activitySourceName)
        {
            services.AddSingleton<IAgentTelemetryProvider>(new AgentTelemetryProvider(activitySourceName));
            return services;
        }
    }
}



using AgentWebApi.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace AgentWebApi.Extensions
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


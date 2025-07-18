using AgentWebApi.Services.Prometheus;
using Prometheus;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PrometheusExtensions
    {
        public static IServiceCollection AddPrometheusMetrics(this IServiceCollection services)
        {
            services.AddSingleton<IPrometheusService, PrometheusService>();
            return services;
        }

        public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app)
        {
            app.UseMetricServer(); // Exposes metrics at /metrics
            return app;
        }
    }
}


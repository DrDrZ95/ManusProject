using Agent.Metering.eBPF;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Metering.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEbpfMetering(this IServiceCollection services)
        {
            services.AddSingleton<IEbpfService, EbpfService>();
            return services;
        }
    }
}

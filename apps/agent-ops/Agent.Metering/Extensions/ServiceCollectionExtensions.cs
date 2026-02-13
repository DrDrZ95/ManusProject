
namespace Agent.Metering.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEbpfMetering(this IServiceCollection services)
        {
            services.AddSingleton<IEbpfService, EbpfService>();
            return services;
        }

        public static void AddCoreServices(this IServiceCollection services)
        {

        }
    }
}

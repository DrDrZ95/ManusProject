using Agent.Core.Services.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddSingleton<IPostgreSqlService, PostgreSqlService>();
            return services;
        }
    }
}

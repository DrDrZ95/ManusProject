

namespace Agent.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IPostgreSqlService, PostgreSqlService>();
        return services;
    }
}
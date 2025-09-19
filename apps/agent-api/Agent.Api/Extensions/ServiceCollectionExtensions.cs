using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Agent.McpGateway;

namespace Agent.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMcpClients(this IServiceCollection services)
        {
            services.AddScoped<IMcpClientFactory, McpClientFactory>();

            services.Scan(scan => scan
                .FromAssemblyOf<IMcpClient>()
                .AddClasses(classes => classes.AssignableTo<IMcpClient>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}


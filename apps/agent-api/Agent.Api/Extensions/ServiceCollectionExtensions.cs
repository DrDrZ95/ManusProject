namespace Agent.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMcpClients(this IServiceCollection services)
    {
        // Register the generic IMcpClientFactory
        services.AddScoped<IMcpClientFactory, McpClientFactory>();

        // Register specific IMcpClient implementations
        services.AddScoped<IMcpClient<ClaudeEntity>, ClaudeMcpClient>();
        services.AddScoped<IMcpClient<ChromeEntity>, ChromeMcpClient>();
        services.AddScoped<IMcpClient<GitHubEntity>, GitHubMcpClient>();
        services.AddScoped<IMcpClient<PostgreSqlEntity>, PostgreSqlClient>();

        return services;
    }
}


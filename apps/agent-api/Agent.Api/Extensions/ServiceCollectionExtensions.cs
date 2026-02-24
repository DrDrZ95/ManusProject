namespace Agent.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers()
                .AddApplicationPart(typeof(Program).Assembly);
        
        // Add API documentation services (Swagger/OpenAPI)
        // 添加API文档服务 (Swagger/OpenAPI)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSwaggerDocumentation();

        services.AddAgentTelemetry("AI-Agent.WebApi");
        services.AddApiVersioningServices();
        services.AddMcpClients();
        services.AddIdentityServices(configuration);
        services.AddUserInputServices();
        services.AddFileUploadServices();
        services.AddPrometheusMetrics();
        services.AddSignalRServices(configuration);
        services.AddHangfireServices(configuration);
        services.AddRedisDistributedCache(configuration);

        return services;
    }

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


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
        services.AddIdentityServices(configuration);
        services.AddFileUploadServices();
        services.AddPrometheusMetrics();
        services.AddSignalRServices(configuration);
        services.AddHangfireServices(configuration);
        services.AddRedisDistributedCache(configuration);

        return services;
    }

    /// <summary>
    /// 使用 Autofac 自动注册所有程序集中的服务
    /// Automatically register services from all assemblies using Autofac
    /// </summary>
    public static void RegisterAgentServices(this ContainerBuilder builder)
    {
        var assemblies = new[]
        {
            typeof(Program).Assembly,                                               // Agent.Api
            typeof(Agent.Application.Services.Telemetry.IAgentTraceService).Assembly, // Agent.Application
            typeof(Agent.Core.Cache.IAgentCacheService).Assembly,                   // Agent.Core
            typeof(Agent.McpGateway.IMcpClientFactory).Assembly                         // Agent.McpGateway
        };

        // 自动注册所有类：只要类实现了接口，就以接口形式注入
        // Register all classes: as long as a class implements an interface, it is injected as an interface
        builder.RegisterAssemblyTypes(assemblies)
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Length > 0)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope(); // 默认为 Scoped 注入

        // 特殊处理：如果需要单例或特定的注入方式，可以在此处覆盖
        // Special handling: If singleton or specific injection is needed, it can be overridden here
    }
}


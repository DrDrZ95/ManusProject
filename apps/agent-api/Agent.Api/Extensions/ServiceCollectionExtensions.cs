namespace Agent.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers();
        
        // Add API Versioning - 优先注册版本控制
        services.AddApiVersioningServices();

        // Add API documentation services (Swagger/OpenAPI)
        // 添加API文档服务 (Swagger/OpenAPI)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSwaggerDocumentation();
        services.AddOpenApiDocumentation();

        services.AddAgentTelemetry("AI-Agent.WebApi");
        services.AddIdentityServices(configuration);
        services.AddFileUploadServices();
        services.AddPrometheusMetrics();
        services.AddSignalRServices(configuration);
        services.AddHangfireServices(configuration);
        services.AddRedisDistributedCache(configuration);
        services.AddAiAgentYarp(configuration);
        
        // Add Core Component Services - 添加核心组件服务
        services.AddPostgreSqlDatabase(configuration);
        services.AddSemanticKernel(configuration);
        services.AddChromaDb(configuration);
        services.AddDaprServices();
        services.AddRagServices(configuration);
        services.AddPromptsServices();
        services.AddPythonFinetune(configuration);
        services.AddHdfsServices(configuration);
        services.AddSandboxTerminal(configuration);
        services.AddWorkflowServices(configuration);

        return services;
    }

    /// <summary>
    /// 使用 Autofac 自动注册所有程序集中的服务
    /// Automatically register services from all assemblies using Autofac
    /// </summary>
    public static void RegisterAgentServices(this ContainerBuilder builder)
    {
        // 1. 获取核心程序集
        // Get core assemblies
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),                                         // Agent.Api
            Assembly.Load("Agent.Application"),                                     // Agent.Application
            Assembly.Load("Agent.Core"),                                            // Agent.Core
            Assembly.Load("Agent.McpGateway")                                       // Agent.McpGateway
        };

        // 2. 自动化注入：只要类实现了接口，就以接口形式注入 (默认 Scoped)
        // 排除掉已经手动注册或需要特殊处理的类
        builder.RegisterAssemblyTypes(assemblies)
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Length > 0 && 
                        t.Name != "AgentTelemetryProvider" && 
                        t.Name != "PostgreSqlService" &&
                        t.Name != "ChromaClient" &&
                        t.Name != "ConfigureSwaggerOptions")
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // 3. 特殊注入处理 (手动覆盖自动化规则)
        // Special injection handling (Manually override automation rules)
        
        // 单例服务注册
        // Singleton service registrations
        builder.RegisterType<Agent.Application.Services.PostgreSQL.PostgreSqlService>()
            .As<Agent.Application.Services.PostgreSQL.IPostgreSqlService>()
            .SingleInstance();

        // 仓储层特殊注册（如果需要）
        // Repository layer special registration (if needed)
        builder.RegisterGeneric(typeof(Agent.Core.Data.Repositories.Repository<,>))
            .As(typeof(Agent.Core.Data.Repositories.IRepository<,>))
            .InstancePerLifetimeScope();

        // 4. 特殊工具或需要复杂配置的组件
        // Special tools or components requiring complex configuration
        builder.RegisterType<Agent.Application.Services.Tokens.TokenCounterFactory>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Agent.Application.Services.Prompts.PromptComposer>()
            .AsSelf()
            .InstancePerLifetimeScope();

        // (此处保留扩展空间)
    }
}


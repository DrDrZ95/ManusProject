using AgentWebApi.Services.Workflow;

namespace AgentWebApi.Extensions;

/// <summary>
/// Workflow service extensions for dependency injection
/// 工作流服务的依赖注入扩展
/// 
/// 基于OpenManus项目的planning.py功能转换而来
/// Converted from OpenManus project's planning.py functionality
/// </summary>
public static class WorkflowExtensions
{
    /// <summary>
    /// Add Workflow services to the service collection
    /// 将工作流服务添加到服务集合
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddWorkflowServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置选项 - Configure options
        services.Configure<WorkflowOptions>(
            configuration.GetSection("Workflow"));

        // 注册服务 - Register services
        services.AddScoped<IWorkflowService, WorkflowService>();

        // 添加日志记录 - Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }

    /// <summary>
    /// Add Workflow services with custom options
    /// 使用自定义选项添加工作流服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configureOptions">Options configuration action - 选项配置操作</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddWorkflowServices(
        this IServiceCollection services,
        Action<WorkflowOptions> configureOptions)
    {
        // 配置选项 - Configure options
        services.Configure(configureOptions);

        // 注册服务 - Register services
        services.AddScoped<IWorkflowService, WorkflowService>();

        // 添加日志记录 - Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }

    /// <summary>
    /// Add Workflow services with default configuration
    /// 使用默认配置添加工作流服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        // 使用默认选项 - Use default options
        services.Configure<WorkflowOptions>(options =>
        {
            // 默认配置已在WorkflowOptions类中设置
            // Default configuration is already set in WorkflowOptions class
        });

        // 注册服务 - Register services
        services.AddScoped<IWorkflowService, WorkflowService>();

        // 添加日志记录 - Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }
}


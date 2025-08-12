using Agent.Core.Services.Sandbox;

namespace Agent.Core.Extensions;

/// <summary>
/// Sandbox Terminal service extensions for dependency injection
/// 沙盒终端服务的依赖注入扩展
/// </summary>
public static class SandboxTerminalExtensions
{
    /// <summary>
    /// Add Sandbox Terminal services to the service collection
    /// 将沙盒终端服务添加到服务集合
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddSandboxTerminal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置选项 - Configure options
        services.Configure<SandboxTerminalOptions>(
            configuration.GetSection("SandboxTerminal"));

        // 注册服务 - Register services
        services.AddScoped<ISandboxTerminalService, SandboxTerminalService>();

        // 添加日志记录 - Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }

    /// <summary>
    /// Add Sandbox Terminal services with custom options
    /// 使用自定义选项添加沙盒终端服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configureOptions">Options configuration action - 选项配置操作</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddSandboxTerminal(
        this IServiceCollection services,
        Action<SandboxTerminalOptions> configureOptions)
    {
        // 配置选项 - Configure options
        services.Configure(configureOptions);

        // 注册服务 - Register services
        services.AddScoped<ISandboxTerminalService, SandboxTerminalService>();

        // 添加日志记录 - Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }

    /// <summary>
    /// Add Sandbox Terminal services with default configuration
    /// 使用默认配置添加沙盒终端服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddSandboxTerminal(this IServiceCollection services)
    {
        // 使用默认选项 - Use default options
        services.Configure<SandboxTerminalOptions>(options =>
        {
            // 默认配置已在SandboxTerminalOptions类中设置
            // Default configuration is already set in SandboxTerminalOptions class
        });

        // 注册服务 - Register services
        services.AddScoped<ISandboxTerminalService, SandboxTerminalService>();

        // 添加日志记录 - Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }
}


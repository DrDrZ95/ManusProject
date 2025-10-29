namespace Agent.Api.Extensions;

/// <summary>
/// YARP 网关和熔断器扩展方法 - YARP Gateway and Circuit Breaker Extension Methods
/// </summary>
public static class YarpExtensions
{
    /// <summary>
    /// 添加 YARP 反向代理服务 - Add YARP Reverse Proxy Services
    /// </summary>
    /// <param name="services">服务集合 - Service collection</param>
    /// <param name="configuration">配置 - Configuration</param>
    /// <returns>服务集合 - Service collection</returns>
    public static IServiceCollection AddAiAgentYarp(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置 YARP 反向代理 - Configure YARP Reverse Proxy
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("Yarp"));

        // 添加熔断器中间件选项 - Add Circuit Breaker Middleware options
        services.Configure<CircuitBreakerOptions>(configuration.GetSection(CircuitBreakerOptions.SectionName));

        return services;
    }

    /// <summary>
    /// 使用 YARP 反向代理和熔断器中间件 - Use YARP Reverse Proxy and Circuit Breaker Middleware
    /// </summary>
    /// <param name="app">应用构建器 - Application builder</param>
    /// <returns>应用构建器 - Application builder</returns>
    public static IApplicationBuilder UseAiAgentYarp(this IApplicationBuilder app)
    {
        // 使用熔断器中间件 - Use Circuit Breaker Middleware
        app.UseMiddleware<CircuitBreakerMiddleware>();

        // 使用 YARP 反向代理 - Use YARP Reverse Proxy
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapReverseProxy();
        });

        return app;
    }
}



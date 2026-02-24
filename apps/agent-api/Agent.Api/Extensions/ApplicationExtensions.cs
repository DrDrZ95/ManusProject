namespace Agent.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring application services.
/// 提供用于配置应用程序服务的扩展方法。
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Adds core application services to the service collection.
    /// 将核心应用程序服务添加到服务集合中。
    /// </summary>
    /// <param name="services">The IServiceCollection instance. IServiceCollection实例。</param>
    /// <returns>The IServiceCollection instance for chaining. 用于链式调用的IServiceCollection实例。</returns>
    public static IServiceCollection AddCoreApplicationServices(this IServiceCollection services)
    {
        // Add MCP Server and register tools from the current assembly
        // 添加MCP服务器并从当前程序集注册工具
        services.AddMcpServer()
            .WithToolsFromAssembly(typeof(Program).Assembly);

        return services;
    }
}





/// <summary>
/// Provides extension methods for configuring the application's request pipeline.
/// 提供用于配置应用程序请求管道的扩展方法。
/// </summary>
public static class ApplicationPipelineExtensions
{
    /// <summary>
    /// Configures the full application request pipeline.
    /// 配置完整的应用程序请求管道。
    /// </summary>
    public static void UseFullApplicationPipeline(this WebApplication app, IConfiguration configuration)
    {
        // Swagger documentation generation - Swagger 文档生成
        app.UseSwaggerDocumentation();

        // ReDoc UI middleware - ReDoc UI 中间件
        app.UseRedocDocumentation();

        app.ConfigureApplicationPipeline();
        app.UseFileUploadSecurity();
        app.UseIdentityServices();
        app.UseSignalRServices(configuration);
        app.UseAiAgentYarp();
        app.UsePrometheusMetrics();
        app.UseHangfireDashboard();
        app.MapControllers();

        // Map OpenAPI JSON endpoint to /openapi.json
        // 将 OpenAPI JSON 端点映射到 /openapi.json
        app.MapOpenApi();
    }

    /// <summary>
    /// Configures the application's HTTP request pipeline.
    /// 配置应用程序的HTTP请求管道。
    /// </summary>
    /// <param name="app">The WebApplication instance. WebApplication实例。</param>
    public static void ConfigureApplicationPipeline(this WebApplication app)
    {
        // Configure global exception handling middleware - 中间件模式
        app.UseMiddleware<Middleware.GlobalExceptionMiddleware>();

        // 配置Dapr中间件 - Middleware Pattern
        // 配置Dapr中间件 - 中间件模式
        app.UseDaprMiddleware();
    }
}

/// <summary>
/// Provides extension methods for tool registry operations.
/// 提供用于工具注册表操作的扩展方法。
/// </summary>
public static class ToolRegistryExtensions
{
    /// <summary>
    /// Hot-loads tools from the registry.
    /// 从注册中心热加载工具。
    /// </summary>
    public static async Task HotLoadToolsAsync(this IHost app)
    {
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var toolRegistry = scope.ServiceProvider.GetRequiredService<IToolRegistryService>();
                await toolRegistry.HotLoadToolsAsync();
            }
        }
        catch (Exception ex)
        {
            // 使用外部组件日志记录器，输出黄色加粗提示，但不阻塞程序启动
            ExternalComponentLogger.LogConnectionError("Tool Registry HotLoad", ex, "无法热加载工具注册表。这通常是因为数据库连接失败或模型映射错误。程序将继续运行，但工具发现功能可能受限。");
        }
    }
}


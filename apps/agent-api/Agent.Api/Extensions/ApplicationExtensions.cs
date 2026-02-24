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
        // Add API documentation services (Swagger/OpenAPI)
        // 添加API文档服务 (Swagger/OpenAPI)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

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

public static class ToolRegistryExtensions
{
    /// <summary>
    /// Hot-loads tools from the registry.
    /// 从注册中心热加载工具。
    /// </summary>
    public static async Task HotLoadToolsAsync(this IHost app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var toolRegistry = scope.ServiceProvider.GetRequiredService<IToolRegistryService>();
            await toolRegistry.HotLoadToolsAsync();
        }
    }
}





/// <summary>
/// Provides extension methods for configuring authentication and authorization services.
/// 提供用于配置认证和授权服务的扩展方法。
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Adds basic authentication and authorization services to the service collection.
    /// 将基本的认证和授权服务添加到服务集合中。
    /// </summary>
    /// <param name="services">The IServiceCollection instance. IServiceCollection实例。</param>
    /// <returns>The IServiceCollection instance for chaining. 用于链式调用的IServiceCollection实例。</returns>
    public static IServiceCollection AddBasicAuth(this IServiceCollection services)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                // Configure JWT Bearer options here if needed
                // 例如，可以配置Authority, Audience, RequireHttpsMetadata等
                // For simplicity, we are not adding full JWT configuration here.
                // 简化起见，此处未添加完整的JWT配置。
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                // Add specific claims or roles if needed
                // 如果需要，可以添加特定的声明或角色
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the application to use authentication and authorization middleware.
    /// 配置应用程序使用认证和授权中间件。
    /// </summary>
    /// <param name="app">The WebApplication instance. WebApplication实例。</param>
    /// <returns>The WebApplication instance for chaining. 用于链式调用的WebApplication实例。</returns>
    public static WebApplication UseBasicAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}


using Agent.Core.McpTools;
using Agent.Core.Services.Qwen;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Core.Extensions
{
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

            // Register HttpClient for QwenServiceClient for external API calls
            // 注册QwenServiceClient的HttpClient，用于外部API调用
            services.AddHttpClient<IQwenServiceClient, QwenServiceClient>();

            // Add MCP Server and register tools from the current assembly
            // 添加MCP服务器并从当前程序集注册工具
            services.AddMcpServer()
                .WithToolsFromAssembly(typeof(Program).Assembly);

            return services;
        }
    }
}




namespace Agent.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring the application's request pipeline.
    /// 提供用于配置应用程序请求管道的扩展方法。
    /// </summary>
    public static class ApplicationPipelineExtensions
    {
        /// <summary>
        /// Configures the application's HTTP request pipeline.
        /// 配置应用程序的HTTP请求管道。
        /// </summary>
        /// <param name="app">The WebApplication instance. WebApplication实例。</param>
        public static void ConfigureApplicationPipeline(this WebApplication app)
        {
            // Configure development environment - Strategy Pattern
            // 配置开发环境 - 策略模式
            //app.ConfigureDevelopmentEnvironment();
            
            // Configure static files - Chain of Responsibility Pattern
            // 配置静态文件 - 责任链模式
            //app.ConfigureStaticFiles();
            
            // Configure MCP endpoints - Facade Pattern
            // 配置MCP端点 - 外观模式
            //app.ConfigureMcpEndpoints();
            
            // Configure development test endpoints - Command Pattern
            // 配置开发测试端点 - 命令模式
            //app.ConfigureDevTestEndpoints();
            
            // Configure Dapr middleware - Middleware Pattern
            // 配置Dapr中间件 - 中间件模式
            app.UseDaprMiddleware();
        }
    }
}




namespace Agent.Core.Extensions
{
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
}



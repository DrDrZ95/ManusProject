using AgentWebApi.Extensions;
using AgentWebApi.McpTools;
using AgentWebApi.Services;
using AgentWebApi.Services.Qwen;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Extensions;

/// <summary>
/// Main entry point for the AgentWebApi application.
/// 应用程序的主入口点。
/// </summary>
/// <remarks>
/// This class follows the Minimal API pattern introduced in .NET 6, combined with
/// the Extension Method pattern for modular configuration.
/// 
/// 该类遵循.NET 6中引入的最小API模式，结合扩展方法模式实现模块化配置。
/// </remarks>
public class Program
{
    /// <summary>
    /// Application entry point that configures and runs the web application.
    /// 配置并运行Web应用程序的入口点。
    /// </summary>
    /// <param name="args">Command line arguments. 命令行参数。</param>
    public static void Main(string[] args)
    {
        // Create the WebApplicationBuilder - Builder Pattern
        // 创建WebApplicationBuilder - 构建器模式
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure services using Extension Methods - Extension Method Pattern
        // 使用扩展方法配置服务 - 扩展方法模式
        ConfigureServices(builder);
        
        // Build the application - Builder Pattern
        // 构建应用程序 - 构建器模式
        var app = builder.Build();
        
        // Configure the HTTP request pipeline using Extension Methods - Extension Method Pattern
        // 使用扩展方法配置HTTP请求管道 - 扩展方法模式
        ConfigureApplication(app);
        
        // Run the application
        // 运行应用程序
        app.Run();
    }
    
    /// <summary>
    /// Configures the application's services using the Extension Method pattern.
    /// 使用扩展方法模式配置应用程序的服务。
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder. WebApplicationBuilder实例。</param>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add API documentation services - Decorator Pattern
        // 添加API文档服务 - 装饰器模式
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Register HttpClient for QwenServiceClient - Factory Pattern + Dependency Injection
        // 注册QwenServiceClient的HttpClient - 工厂模式 + 依赖注入
        builder.Services.AddHttpClient<IQwenServiceClient, QwenServiceClient>();
        
        // Add MCP Server and register tools - Plugin Pattern
        // 添加MCP服务器并注册工具 - 插件模式
        builder.Services.AddMcpServer()
            .WithToolsFromAssembly(typeof(Program).Assembly);
            
        // Add OpenTelemetry services - Observer Pattern
        // 添加OpenTelemetry服务 - 观察者模式
        builder.Services.AddOpenTelemetryServices();
        
        // Add Dapr services - Sidecar Pattern
        // 添加Dapr服务 - 边车模式
        builder.Services.AddDaprServices();
    }
    
    /// <summary>
    /// Configures the application's request pipeline using the Extension Method pattern.
    /// 使用扩展方法模式配置应用程序的请求管道。
    /// </summary>
    /// <param name="app">The WebApplication. WebApplication实例。</param>
    private static void ConfigureApplication(WebApplication app)
    {
        // Configure development environment - Strategy Pattern
        // 配置开发环境 - 策略模式
        app.ConfigureDevelopmentEnvironment();
        
        // Configure static files - Chain of Responsibility Pattern
        // 配置静态文件 - 责任链模式
        app.ConfigureStaticFiles();
        
        // Configure MCP endpoints - Facade Pattern
        // 配置MCP端点 - 外观模式
        app.ConfigureMcpEndpoints();
        
        // Configure development test endpoints - Command Pattern
        // 配置开发测试端点 - 命令模式
        app.ConfigureDevTestEndpoints();
        
        // Configure Dapr middleware - Middleware Pattern
        // 配置Dapr中间件 - 中间件模式
        app.UseDaprMiddleware();
    }
}

/// <summary>
/// Main entry point for the Agent.Api application.
/// 应用程序的主入口点。
/// </summary>
/// <remarks>
/// This class follows the Minimal API pattern introduced in .NET 6, combined with
/// the Extension Method pattern for modular configuration.
/// 
/// 该类遵循NET 中引入的最小API模式，结合扩展方法模式实现模块化配置。
/// </remarks>

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Create the WebApplicationBuilder - Builder Pattern
// 创建WebApplicationBuilder - 构建器模式
services.AddHttpContextAccessor();
services.AddAgentTelemetry("AI-Agent.WebApi"); // Centralized telemetry provider
services.AddApiVersioningServices();
services.AddSwaggerDocumentation();
services.AddMcpClients(); // Add MCP Clients using Scrutor
services.AddIdentityServices(builder.Configuration); // Add Identity services with PostgreSQL and JWT - 添加带有PostgreSQL和JWT的Identity服务
services.AddUserInputServices(); // Add UserInput services - 添加用户输入服务
services.AddFileUploadServices(); // Add FileUpload services with OWASP security - 添加文件上传服务和OWASP安全措施
services.AddPrometheusMetrics(); // Add Prometheus metrics services - 添加Prometheus指标服务
services.AddSignalRServices(builder.Configuration); // Add SignalR services with JWT authentication - 添加SignalR服务和JWT认证
services.AddHangfireServices(builder.Configuration);
services.AddRedisDistributedCache(builder.Configuration); // Add Hangfire services - 添加Hangfire服务

// Build the application - Builder Pattern
// 构建应用程序 - 构建器模式
var app = builder.Build();

// Configure the HTTP request pipeline using Extension Methods - Extension Method Pattern
// 使用扩展方法配置HTTP请求管道 - 扩展方法模式
app.UseSwaggerDocumentation();
app.ConfigureApplicationPipeline();
app.UseFileUploadSecurity(); // Use FileUpload security middleware with OWASP measures - 使用文件上传安全中间件和OWASP措施
app.UseIdentityServices(); // Use Identity middleware with authentication and authorization - 使用带有认证和授权的Identity中间件
app.UseSignalRServices(builder.Configuration); // SignalR middleware with automatic reconnection - SignalR中间件和自动重连
app.UseAiAgentYarp(); // Optional AI-Agent gateway middleware - 可选的AI-Agent网关中间件
app.UsePrometheusMetrics(); // Use Prometheus metrics middleware - 使用Prometheus指标中间件
app.UseHangfireDashboard(); // Use Hangfire Dashboard - 使用Hangfire Dashboard
app.MapControllers(); // Map eBPF controllers

// Initialize Identity database
// 初始化Identity数据库
await app.InitializeIdentityDatabaseAsync();

// Hot-load tools from registry
// 从注册中心热加载工具
using (var scope = app.Services.CreateScope())
{
    var toolRegistry = scope.ServiceProvider.GetRequiredService<IToolRegistryService>();
    await toolRegistry.HotLoadToolsAsync();
}

// 2. Execute startup telemetry spans simulation
// 执行启动遥测 Span 模拟
await app.ExecuteStartupTelemetrySpansAsync();

// Run the application
// 运行应用程序
app.Run();


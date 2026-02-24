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

// Use Autofac as the service provider factory
// 使用 Autofac 作为服务提供程序工厂
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Configure Autofac container
// 配置 Autofac 容器
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterAgentServices();
});

// 1. Add application services
// 添加应用程序服务
builder.Services.AddApplicationServices(builder.Configuration);

// Build the application
// 构建应用程序
var app = builder.Build();

// 2. Configure the HTTP request pipeline
// 配置HTTP请求管道
app.UseFullApplicationPipeline(builder.Configuration);

// 3. Initialize Identity database
// 初始化Identity数据库
await app.InitializeIdentityDatabaseAsync();

// 4. Hot-load tools from registry
// 从注册中心热加载工具
await app.HotLoadToolsAsync();

// 5. Execute startup telemetry spans simulation
// 执行启动遥测 Span 模拟
await app.ExecuteStartupTelemetrySpansAsync();

// Run the application
// 运行应用程序
app.Run();


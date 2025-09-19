/// <summary>
/// Main entry point for the Agent.Api application.
/// 应用程序的主入口点。
/// </summary>
/// <remarks>
/// This class follows the Minimal API pattern introduced in .NET 6, combined with
/// the Extension Method pattern for modular configuration.
/// 
/// 该类遵循.NET 8中引入的最小API模式，结合扩展方法模式实现模块化配置。
/// </remarks>

using Agent.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Create the WebApplicationBuilder - Builder Pattern
// 创建WebApplicationBuilder - 构建器模式
services.AddAgentTelemetry("AI-Agent.WebApi"); // Centralized telemetry provider
services.AddMcpClients(); // Add MCP Clients using Scrutor
services.AddIdentityServices(builder.Configuration); // Add Identity services with PostgreSQL and JWT - 添加带有PostgreSQL和JWT的Identity服务
services.AddUserInputServices(); // Add UserInput services - 添加用户输入服务
services.AddFileUploadServices(); // Add FileUpload services with OWASP security - 添加文件上传服务和OWASP安全措施
services.AddPrometheusMetrics(); // Add Prometheus metrics services - 添加Prometheus指标服务
services.AddSignalRServices(builder.Configuration); // Add SignalR services with JWT authentication - 添加SignalR服务和JWT认证

// Build the application - Builder Pattern
// 构建应用程序 - 构建器模式
var app = builder.Build();

var telemetryProvider = app.Services.GetRequiredService<IAgentTelemetryProvider>();

// 2. Start main application activity span
// 启动主应用程序活动Span
using (var activity = telemetryProvider.StartSpan("AI-Agent.ApplicationStartup"))
{
    activity?.SetAttribute("app.version", "1.0.0");
    activity?.SetAttribute("app.environment", app.Environment.EnvironmentName);

    // Configure the HTTP request pipeline using Extension Methods - Extension Method Pattern
    // 使用扩展方法配置HTTP请求管道 - 扩展方法模式
    app.ConfigureApplicationPipeline();
    app.UseFileUploadSecurity(); // Use FileUpload security middleware with OWASP measures - 使用文件上传安全中间件和OWASP措施
    app.UseIdentityServices(); // Use Identity middleware with authentication and authorization - 使用带有认证和授权的Identity中间件
    app.UseSignalRServices(builder.Configuration); // SignalR middleware with automatic reconnection - SignalR中间件和自动重连
    app.UseAiAgentYarp(); // Optional AI-Agent gateway middleware - 可选的AI-Agent网关中间件
    app.UsePrometheusMetrics(); // Use Prometheus metrics middleware - 使用Prometheus指标中间件
    app.MapControllers(); // Map eBPF controllers

    // Initialize Identity database
    // 初始化Identity数据库
    await app.InitializeIdentityDatabaseAsync();

    // 3. Simulate a typical Agent application execution sequence
    // 模拟典型的Agent应用程序执行序列
    using (var agentFlowActivity = telemetryProvider.StartSpan("AI-Agent.ExecutionFlow"))
    {
        agentFlowActivity?.SetAttribute("flow.description", "User input -> LLM interaction -> RAG (optional) -> Generate to-do list -> Process interaction");

        // 3.1 User Input Processing
        // 用户输入处理
        using (var userInputActivity = telemetryProvider.StartSpan("AI-Agent.UserInputProcessing"))
        {
            userInputActivity?.SetAttribute("input.type", "text");
            userInputActivity?.SetAttribute("input.length", 120);
        }

        // 3.2 LLM Interaction
        // LLM交互
        using (var llmInteractionActivity = telemetryProvider.StartSpan("AI-Agent.LLMInteraction"))
        {
            llmInteractionActivity?.SetAttribute("llm.model", "gpt-4");
            llmInteractionActivity?.SetAttribute("llm.prompt_tokens", 50);
            llmInteractionActivity?.SetAttribute("llm.completion_tokens", 150);
        }

        // 3.3 RAG (Retrieval Augmented Generation) - Optional
        // RAG (检索增强生成) - 可选
        using (var ragActivity = telemetryProvider.StartSpan("AI-Agent.RAG"))
        {
            ragActivity?.SetAttribute("rag.enabled", true);
            ragActivity?.SetAttribute("rag.retrieved_documents", 3);
            ragActivity?.SetAttribute("rag.query", "employee leave policy");
        }

        // 3.4 Generate To-Do List
        // 生成待办事项列表
        using (var todoListActivity = telemetryProvider.StartSpan("AI-Agent.GenerateTodoList"))
        {
            todoListActivity?.SetAttribute("todo.items_count", 5);
            todoListActivity?.SetAttribute("todo.workflow_id", "plan_12345");
        }

        // 3.5 Process Interaction (e.g., Sandbox Terminal, Workflow Execution)
        // 进程交互 (例如, 沙盒终端, 工作流执行)
        using (var processInteractionActivity = telemetryProvider.StartSpan("AI-Agent.ProcessInteraction"))
        {
            processInteractionActivity?.SetAttribute("process.type", "sandbox_command");
            processInteractionActivity?.SetAttribute("process.command", "ls -la");
        }
    }
}

// Run the application
// 运行应用程序
app.Run();



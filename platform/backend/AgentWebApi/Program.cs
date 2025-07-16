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

// Create the WebApplicationBuilder - Builder Pattern
// 创建WebApplicationBuilder - 构建器模式
builder.Services.AddAgentTelemetry("AI-Agent.WebApi"); // Centralized telemetry provider
builder.Services.AddUserInputServices(); // Add UserInput services - 添加用户输入服务

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
    // app.UseIdentityServerServices(app.Environment); // Optional IdentityServer4 middleware - 可选的IdentityServer4中间件
    // app.UseSignalRServices(builder.Configuration); // Optional SignalR middleware - 可选的SignalR中间件
    app.UseAiAgentYarp(); // Optional AI-Agent gateway middleware - 可选的AI-Agent网关中间件
    app.UseBasicAuth(); // Configure application to use authentication and authorization middleware
    app.MapControllers(); // Map eBPF controllers

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



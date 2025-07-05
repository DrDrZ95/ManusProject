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
var builder = WebApplication.CreateBuilder(args);

// 1. Define ActivitySource for tracing
// 定义用于跟踪的ActivitySource
using var activitySource = new ActivitySource("AI-Agent.WebApi");

// Configure services using Extension Methods - Extension Method Pattern
// 使用扩展方法配置服务 - 扩展方法模式
builder.Services.AddCoreApplicationServices();
builder.Services.AddOpenTelemetryServices();
builder.Services.AddDaprServices();
builder.Services.AddChromaDb(builder.Configuration); // Add ChromaDB services
builder.Services.AddVectorDatabase(builder.Configuration); // Add Vector Database services
builder.Services.AddSemanticKernel(builder.Configuration); // Add Semantic Kernel services
builder.Services.AddRagServices(builder.Configuration); // Add RAG services - 添加RAG服务
builder.Services.AddSandboxTerminal(builder.Configuration); // Add Sandbox Terminal services - 添加沙盒终端服务
builder.Services.AddWorkflowServices(builder.Configuration); // Add Workflow services - 添加工作流服务
builder.Services.AddPromptsServices(); // Add Prompts services - 添加提示词服务
builder.Services.AddeBPFDetectiveServices(); // Add eBPF Detective services - 添加eBPF侦探服务
// builder.Services.AddHdfsServices(builder.Configuration); // Optional HDFS services - 可选的HDFS服务
// builder.Services.AddPostgreSqlDatabase(builder.Configuration); // Optional PostgreSQL database - 可选的PostgreSQL数据库
// builder.Services.AddPythonFinetune(builder.Configuration); // Optional Python.NET fine-tuning - 可选的Python.NET微调
// builder.Services.AddIdentityServerServices(builder.Configuration); // Optional IdentityServer4 authentication - 可选的IdentityServer4身份验证
// builder.Services.AddSignalRServices(builder.Configuration); // Optional SignalR real-time communication - 可选的SignalR实时通信
builder.Services.AddAiAgentYarp(builder.Configuration); // Optional YARP gateway with circuit breaker - 可选的AI-Agent网关与熔断器
builder.Services.AddBasicAuth(); // Add basic authentication and authorization services

// Build the application - Builder Pattern
// 构建应用程序 - 构建器模式
var app = builder.Build();

// 2. Start main application activity span
// 启动主应用程序活动Span
using (var activity = activitySource.StartActivity("AI-Agent.ApplicationStartup"))
{
    activity?.SetTag("app.version", "1.0.0");
    activity?.SetTag("app.environment", app.Environment.EnvironmentName);

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
    using (var agentFlowActivity = activitySource.StartActivity("AI-Agent.ExecutionFlow"))
    {
        agentFlowActivity?.SetTag("flow.description", "User input -> LLM interaction -> RAG (optional) -> Generate to-do list -> Process interaction");

        // 3.1 User Input Processing
        // 用户输入处理
        using (var userInputActivity = activitySource.StartActivity("AI-Agent.UserInputProcessing"))
        {
            userInputActivity?.SetTag("input.type", "text");
            userInputActivity?.SetTag("input.length", 120);
        }

        // 3.2 LLM Interaction
        // LLM交互
        using (var llmInteractionActivity = activitySource.StartActivity("AI-Agent.LLMInteraction"))
        {
            llmInteractionActivity?.SetTag("llm.model", "gpt-4");
            llmInteractionActivity?.SetTag("llm.prompt_tokens", 50);
            llmInteractionActivity?.SetTag("llm.completion_tokens", 150);
        }

        // 3.3 RAG (Retrieval Augmented Generation) - Optional
        // RAG (检索增强生成) - 可选
        using (var ragActivity = activitySource.StartActivity("AI-Agent.RAG"))
        {
            ragActivity?.SetTag("rag.enabled", true);
            ragActivity?.SetTag("rag.retrieved_documents", 3);
            ragActivity?.SetTag("rag.query", "employee leave policy");
        }

        // 3.4 Generate To-Do List
        // 生成待办事项列表
        using (var todoListActivity = activitySource.StartActivity("AI-Agent.GenerateTodoList"))
        {
            todoListActivity?.SetTag("todo.items_count", 5);
            todoListActivity?.SetTag("todo.workflow_id", "plan_12345");
        }

        // 3.5 Process Interaction (e.g., Sandbox Terminal, Workflow Execution)
        // 进程交互 (例如, 沙盒终端, 工作流执行)
        using (var processInteractionActivity = activitySource.StartActivity("AI-Agent.ProcessInteraction"))
        {
            processInteractionActivity?.SetTag("process.type", "sandbox_command");
            processInteractionActivity?.SetTag("process.command", "ls -la");
        }
    }
}

// Run the application
// 运行应用程序
app.Run();



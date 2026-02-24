namespace Agent.Api.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddAgentTelemetry(this IServiceCollection services, string activitySourceName)
    {
        services.AddSingleton<IAgentTelemetryProvider>(new AgentTelemetryProvider(activitySourceName));
        return services;
    }

    /// <summary>
    /// 执行应用程序启动时的遥测 Span 模拟。
    /// </summary>
    public static void ExecuteStartupTelemetrySpans(this IHost app)
    {
        // 异步在后台执行遥测模拟，不阻塞主线程
        // Execute telemetry simulation asynchronously in the background, without blocking the main thread
        Task.Run(() =>
        {
            try
            {
                var telemetryProvider = app.Services.GetRequiredService<IAgentTelemetryProvider>();
                var environment = app.Services.GetRequiredService<IHostEnvironment>();

                // 1. Start main application activity span
                using (var activity = telemetryProvider.StartSpan("AI-Agent.ApplicationStartup"))
                {
                    activity?.SetAttribute("app.version", "1.0.0");
                    activity?.SetAttribute("app.environment", environment.EnvironmentName);

                    // 2. Simulate a typical Agent application execution sequence
                    using (var agentFlowActivity = telemetryProvider.StartSpan("AI-Agent.ExecutionFlow"))
                    {
                        agentFlowActivity?.SetAttribute("flow.description", "User input -> LLM interaction -> RAG (optional) -> Generate to-do list -> Process interaction");

                        // 2.1 User Input Processing
                        using (var userInputActivity = telemetryProvider.StartSpan("AI-Agent.UserInputProcessing"))
                        {
                            userInputActivity?.SetAttribute("input.type", "text");
                            userInputActivity?.SetAttribute("input.length", 120);
                        }

                        // 2.2 LLM Interaction
                        using (var llmInteractionActivity = telemetryProvider.StartSpan("AI-Agent.LLMInteraction"))
                        {
                            llmInteractionActivity?.SetAttribute("llm.model", "gpt-4");
                            llmInteractionActivity?.SetAttribute("llm.prompt_tokens", 50);
                            llmInteractionActivity?.SetAttribute("llm.completion_tokens", 150);
                        }

                        // 2.3 RAG (Retrieval Augmented Generation) - Optional
                        using (var ragActivity = telemetryProvider.StartSpan("AI-Agent.RAG"))
                        {
                            ragActivity?.SetAttribute("rag.enabled", true);
                            ragActivity?.SetAttribute("rag.retrieved_documents", 3);
                            ragActivity?.SetAttribute("rag.query", "employee leave policy");
                        }

                        // 2.4 Generate To-Do List
                        using (var todoListActivity = telemetryProvider.StartSpan("AI-Agent.GenerateTodoList"))
                        {
                            todoListActivity?.SetAttribute("todo.items_count", 5);
                            todoListActivity?.SetAttribute("todo.workflow_id", "plan_12345");
                        }

                        // 2.5 Process Interaction
                        using (var processInteractionActivity = telemetryProvider.StartSpan("AI-Agent.ProcessInteraction"))
                        {
                            processInteractionActivity?.SetAttribute("process.type", "sandbox_command");
                            processInteractionActivity?.SetAttribute("process.command", "ls -la");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 使用外部组件日志记录器记录遥测启动异常
                ExternalComponentLogger.LogConnectionError("Telemetry Startup", ex, "启动遥测模拟失败。程序将继续运行。");
            }
        });
    }
}


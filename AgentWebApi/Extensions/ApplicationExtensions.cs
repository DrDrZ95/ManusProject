using AgentWebApi.McpTools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Extensions;
using ModelContextProtocol.Models.Contexts;
using ModelContextProtocol.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AgentWebApi.Extensions
{
    /// <summary>
    /// Extension methods for configuring the application request pipeline.
    /// 应用程序请求管道配置的扩展方法。
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Configures the application's development environment features.
        /// 配置应用程序的开发环境功能。
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static WebApplication ConfigureDevelopmentEnvironment(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage(); // Useful for debugging 用于调试
            }

            return app;
        }

        /// <summary>
        /// Configures the application's static file handling.
        /// 配置应用程序的静态文件处理。
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static WebApplication ConfigureStaticFiles(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseDefaultFiles(); // Looks for index.html, default.html etc.
            app.UseStaticFiles();

            return app;
        }

        /// <summary>
        /// Configures the MCP (Model Context Protocol) endpoints.
        /// 配置MCP（模型上下文协议）端点。
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static WebApplication ConfigureMcpEndpoints(this WebApplication app)
        {
            // Map the MCP Server-Sent Events endpoint
            app.MapMcpSse(); // Default path is /mcp/sse

            // Optional: Add a simple GET endpoint to list available MCP tools for verification
            app.MapGet("/mcp/tools", (McpToolHost host) =>
            {
                return Results.Ok(host.GetToolDefinitions().Select(td => new { td.Name, td.Description }));
            })
            .WithName("GetMcpTools")
            .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Configures the development test endpoints.
        /// 配置开发测试端点。
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static WebApplication ConfigureDevTestEndpoints(this WebApplication app)
        {
            // Add a test endpoint to trigger the QwenDialogueTool for the demo HTML page
            app.MapGet("/dev/test-qwen-dialogue", async (string prompt, McpToolHost toolHost, ILogger<Program> logger) =>
            {
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    return Results.BadRequest("Prompt cannot be empty.");
                }

                logger.LogInformation("Test endpoint called with prompt: {Prompt}", prompt);

                // Create a simple McpContext for the tool call
                var mcpContext = new McpContext
                {
                    ConversationId = "test-conv-" + Guid.NewGuid().ToString(),
                    RequestId = "test-req-" + Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    InteractionType = "UserRequest",
                    Messages = new List<McpMessage> { new McpMessage { Role = "User", Content = prompt } }
                };

                var toolCall = new McpToolCall
                {
                    ToolName = "QwenDialogue",
                    ToolCallId = "test-tcall-" + Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, McpPrimitive?>
                    {
                        { "prompt", new McpString(prompt) }
                    }
                };

                try
                {
                    // Process the tool call. This will eventually send results via SSE if the tool executes.
                    // Note: ProcessToolCallAsync itself might not directly return the tool's output here,
                    // as MCP is designed for asynchronous communication, often via SSE for results.
                    // The SSE client (index.html) should pick up the ToolResult event.
                    await toolHost.ProcessToolCallAsync(mcpContext, toolCall, CancellationToken.None);
                    logger.LogInformation("Tool call processed for prompt: {Prompt}. Check SSE stream for results.", prompt);
                    return Results.Ok(new { message = "Tool call for QwenDialogue initiated. Check SSE stream for results." });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing tool call in test endpoint for prompt: {Prompt}", prompt);
                    return Results.Problem($"Error processing tool call: {ex.Message}");
                }
            })
            .WithName("TestQwenDialogue")
            .WithOpenApi();

            return app;
        }
    }
}

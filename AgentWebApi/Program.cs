using AgentWebApi.McpTools;
using AgentWebApi.Services.Qwen;
using ModelContextProtocol.Extensions;
using ModelContextProtocol.Models.Contexts; // Required for McpContext
using ModelContextProtocol.Models.Primitives; // Required for McpString

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient for QwenServiceClient
builder.Services.AddHttpClient<IQwenServiceClient, QwenServiceClient>();

// Add MCP Server and register tools from the current assembly
builder.Services.AddMcpServer()
    .WithToolsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Useful for debugging
}

app.UseHttpsRedirection();

// Enable static files to serve index.html
app.UseDefaultFiles(); // Looks for index.html, default.html etc.
app.UseStaticFiles();

// Map the MCP Server-Sent Events endpoint
app.MapMcpSse(); // Default path is /mcp/sse

// Optional: Add a simple GET endpoint to list available MCP tools for verification
app.MapGet("/mcp/tools", (McpToolHost host) =>
{
    return Results.Ok(host.GetToolDefinitions().Select(td => new { td.Name, td.Description }));
})
.WithName("GetMcpTools")
.WithOpenApi();

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

app.Run();


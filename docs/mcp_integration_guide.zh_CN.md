# .NET Web API 与 ModelContextProtocol (MCP) 及 Qwen2 服务集成指南

本文档详细介绍了如何在 .NET 8.0 Web API 项目中集成 `ModelContextProtocol` (MCP)，并将其连接到本地运行的 Qwen2 AI 服务 (Python FastAPI 应用)，以实现通过 MCP 进行对话交互。

## 1. 引言

`ModelContextProtocol` (MCP) 是一个旨在标准化与大型语言模型 (LLM) 和其他 AI 工具交互的协议。通过在我们的 .NET Web API 中集成 MCP，我们可以创建一个统一的接口来调用不同的 AI 功能，并通过 Server-Sent Events (SSE) 进行通信。

本项目中，我们将 MCP 用于：
- 定义一个与本地 Qwen2 模型服务交互的“工具” (`QwenDialogueTool`)。
- 通过 MCP 的 SSE 端点暴露此工具的调用和结果。

## 2. 环境与前提条件

- .NET 8.0 SDK 已安装。
- 本地 Qwen2 Python FastAPI 服务正在 `http://localhost:2025/generate` 上运行。
- `AgentWebApi` 项目已按先前步骤搭建。

## 3. 集成步骤

### 3.1. 安装 ModelContextProtocol NuGet 包

首先，将 `ModelContextProtocol` NuGet 包添加到 `AgentWebApi` 项目。由于该包可能仍处于预发布阶段，需要使用 `--prerelease` 选项：

```bash
cd /home/ubuntu/ai-agent/AgentWebApi
export PATH="$PATH:/home/ubuntu/.dotnet" 
dotnet add package ModelContextProtocol --prerelease
```

### 3.2. 创建 Qwen2 服务客户端

为了让 .NET Web API 与 Python Qwen2 服务通信，我们创建了一个 HTTP 客户端。

**a. 数据传输对象 (DTOs) - `Services/QwenDtos.cs`**

定义请求和响应的数据结构：

```csharp
// 文件: AgentWebApi/Services/QwenDtos.cs
using System.Text.Json.Serialization;

namespace AgentWebApi.Services.Qwen;

public class QwenGenerationRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
    // ... 其他参数如 MaxLength, Temperature 等
}

public class QwenGenerationResponse
{
    [JsonPropertyName("generated_texts")]
    public List<string> GeneratedTexts { get; set; } = new List<string>();
}
```

**b. Qwen 服务客户端实现 - `Services/QwenServiceClient.cs`**

实现一个服务，通过 HTTP POST 请求调用 Qwen2 Python API：

```csharp
// 文件: AgentWebApi/Services/QwenServiceClient.cs
namespace AgentWebApi.Services.Qwen;

public interface IQwenServiceClient
{
    Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}

public class QwenServiceClient : IQwenServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QwenServiceClient> _logger;
    private const string QwenApiUrl = "http://localhost:2025/generate";

    public QwenServiceClient(HttpClient httpClient, ILogger<QwenServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var requestPayload = new QwenGenerationRequest { Prompt = prompt };
        // ... 实现调用逻辑，处理响应和错误
        // 示例:
        // HttpResponseMessage response = await _httpClient.PostAsJsonAsync(QwenApiUrl, requestPayload, cancellationToken);
        // if (response.IsSuccessStatusCode) { ... }
        // 返回生成的文本或 null
        return "示例响应：" + prompt; // 替换为实际调用
    }
}
```
*(请参考实际项目中 `QwenServiceClient.cs` 的完整实现，包括错误处理和日志记录)*

### 3.3. 实现 MCP 工具 (QwenDialogueTool)

创建一个实现 `ModelContextProtocol.Models.Tools.ITool` 接口的类，该类将使用 `IQwenServiceClient` 与 Qwen2 模型交互。

```csharp
// 文件: AgentWebApi/McpTools/QwenDialogueTool.cs
using AgentWebApi.Services.Qwen;
using ModelContextProtocol.Models.Contexts;
using ModelContextProtocol.Models.Primitives;
using ModelContextProtocol.Models.Tools;

namespace AgentWebApi.McpTools;

public class QwenDialogueTool : ITool
{
    private readonly IQwenServiceClient _qwenServiceClient;
    // ... 构造函数和日志记录器 ...

    public string Name => "QwenDialogue";
    public string Description => "与 Qwen2 AI 模型进行对话。";

    public async Task<ToolOutput> ExecuteAsync(ToolInput toolInput, CancellationToken cancellationToken = default)
    {
        // ... 从 toolInput 获取 "prompt" 参数 ...
        // ... 调用 _qwenServiceClient.GenerateTextAsync(userPrompt, cancellationToken) ...
        // ... 构建并返回 ToolOutput ...
        // 示例:
        // var qwenResponse = await _qwenServiceClient.GenerateTextAsync(userPrompt, cancellationToken);
        // return new ToolOutput { IsSuccessful = true, Results = new Dictionary<string, McpPrimitive> { { "response", new McpString(qwenResponse ?? "") } } };
        return new ToolOutput { IsSuccessful = true, Results = new Dictionary<string, McpPrimitive> { { "response", new McpString("工具执行成功：" + toolInput.Parameters?["prompt"].Value) } } }; // 替换为实际逻辑
    }

    public ToolDefinition GetDefinition()
    {
        // ... 定义工具的名称、描述、输入和输出模式 ...
        return new ToolDefinition
        {
            Name = Name,
            Description = Description,
            InputSchema = new McpSchema(McpSchemaType.Object, properties: new() { { "prompt", new McpSchema(McpSchemaType.String) } }, required: new() { "prompt" }),
            OutputSchema = new McpSchema(McpSchemaType.Object, properties: new() { { "response", new McpSchema(McpSchemaType.String) } })
        };
    }
}
```
*(请参考实际项目中 `QwenDialogueTool.cs` 的完整实现，包括参数验证和错误处理)*

### 3.4. 配置 Program.cs

在 `AgentWebApi/Program.cs` 文件中，我们需要：
1.  注册 `HttpClient` 和 `IQwenServiceClient`。
2.  添加 MCP 服务并注册我们的工具。
3.  映射 MCP SSE 端点。
4.  (可选) 添加静态文件服务以托管测试页面。
5.  (可选) 添加一个测试端点以方便从浏览器触发工具调用。

```csharp
// 文件: AgentWebApi/Program.cs
using AgentWebApi.McpTools;
using AgentWebApi.Services.Qwen;
using ModelContextProtocol.Extensions;
using ModelContextProtocol.Models.Contexts;
using ModelContextProtocol.Models.Primitives;

var builder = WebApplication.CreateBuilder(args);

// 注册 HttpClient for QwenServiceClient
builder.Services.AddHttpClient<IQwenServiceClient, QwenServiceClient>();

// 添加 MCP 服务并从当前程序集注册工具 (会自动发现 QwenDialogueTool)
builder.Services.AddMcpServer()
    .WithToolsFromAssembly(typeof(Program).Assembly);

// ... 其他服务如 Swagger ...
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// 启用静态文件服务 (用于 index.html 测试页面)
app.UseDefaultFiles();
app.UseStaticFiles();

// 映射 MCP Server-Sent Events 端点
app.MapMcpSse(); // 默认路径是 /mcp/sse

// (可选) 测试端点，用于从浏览器触发 QwenDialogueTool
app.MapGet("/dev/test-qwen-dialogue", async (string prompt, McpToolHost toolHost, ILogger<Program> logger) =>
{
    // ... 实现调用 toolHost.ProcessToolCallAsync(...) 的逻辑 ...
    // 示例:
    // var mcpContext = new McpContext { /* ... */ };
    // var toolCall = new McpToolCall { ToolName = "QwenDialogue", Parameters = new() { { "prompt", new McpString(prompt) } } };
    // await toolHost.ProcessToolCallAsync(mcpContext, toolCall, CancellationToken.None);
    // return Results.Ok(new { message = "工具调用已发起，请检查 SSE 流获取结果。" });
    return Results.Ok(new { message = "测试端点：工具调用已发起，提示 - " + prompt }); // 替换为实际逻辑
});

app.Run();
```
*(请参考实际项目中 `Program.cs` 的完整实现)*

## 4. 使用与测试

### 4.1. 启动服务

1.  确保 Qwen2 Python FastAPI 服务正在运行 (通常在 `http://localhost:2025`)。
2.  启动 .NET Web API 服务：
    ```bash
    cd /home/ubuntu/ai-agent/AgentWebApi
    dotnet run
    ```
    Web API 通常会监听如 `https://localhost:7XXX` 和 `http://localhost:5XXX` 的端口。

### 4.2. 测试对话客户端 (index.html)

项目中包含一个位于 `AgentWebApi/wwwroot/index.html` 的简单 HTML 页面，用于测试与 MCP SSE 端点的交互。

- 在浏览器中打开 Web API 的根 URL (例如 `https://localhost:7258`，具体端口请查看 `dotnet run` 的输出)。`index.html` 应该会自动加载。
- 页面会尝试连接到 `/mcp/sse` 端点。
- 您可以在输入框中输入提示，然后点击 “Send”。这会调用 `/dev/test-qwen-dialogue` 测试端点。
- `QwenDialogueTool` 的执行结果将通过 SSE 事件推送到页面，并显示在聊天记录和原始 SSE 事件区域。

### 4.3. 直接测试 MCP 端点

- **查看可用工具**: 访问 `GET /mcp/tools` (例如 `https://localhost:7258/mcp/tools`) 可以看到已注册的 MCP 工具列表及其描述。
- **SSE 端点**: `/mcp/sse` 是 MCP 事件流的端点。可以使用支持 SSE 的客户端 (如 curl 或 Postman) 来监听事件。

## 5. 工作流程说明

1.  用户通过客户端 (如 `index.html`) 发起一个对话请求 (例如，调用 `/dev/test-qwen-dialogue?prompt=你好`)。
2.  Web API 中的测试端点接收到请求，构造一个 `McpContext` 和 `McpToolCall` 对象，指定调用 `QwenDialogueTool` 并传入用户提示。
3.  测试端点调用 `McpToolHost.ProcessToolCallAsync()`。
4.  MCP 服务框架查找名为 `QwenDialogue` 的已注册工具。
5.  `QwenDialogueTool.ExecuteAsync()` 方法被调用。
6.  `QwenDialogueTool` 使用注入的 `IQwenServiceClient` 向本地 Qwen2 Python 服务 (`http://localhost:2025/generate`) 发送 HTTP 请求。
7.  Qwen2 Python 服务处理提示并返回生成的文本。
8.  `IQwenServiceClient` 将响应返回给 `QwenDialogueTool`。
9.  `QwenDialogueTool` 将 Qwen2 的响应包装在 `ToolOutput` 对象中。
10. MCP 服务框架接收到 `ToolOutput`，并通过 `/mcp/sse` 端点将一个 `ToolResult` 事件发送给所有连接的 SSE 客户端。
11. `index.html` 页面中的 JavaScript 监听到此 SSE 事件，解析 JSON 数据，并将 AI 的响应显示在聊天界面上。

## 6. 后续优化与扩展

- **完善错误处理**: 在 `QwenServiceClient` 和 `QwenDialogueTool` 中增加更健壮的错误处理和重试逻辑。
- **配置化**: 将 Qwen2 服务 URL、默认参数等移至 `appsettings.json`。
- **上下文管理**: `QwenDialogueTool` 可以扩展以处理更复杂的对话历史，而不仅仅是单个提示。
- **更多工具**: 可以按照类似模式添加更多与不同 AI 服务或本地功能交互的 MCP 工具。
- **安全性**: 为 MCP 端点和工具调用添加认证和授权机制。
- **MCP 输入端点**: 实现一个标准的 MCP 输入端点 (例如 `POST /mcp/input`) 来接收完整的 MCP 请求对象，而不是依赖测试端点。

此集成提供了一个灵活的基础，用于通过 .NET Web API 构建和管理与 AI 模型的交互。

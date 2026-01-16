# .NET Web API 与 ModelContextProtocol (MCP) 及 Llama3.1 服务集成指南

本文档详细介绍了如何在 .NET 8.0 Web API 项目中集成 `ModelContextProtocol` (MCP)，并将其连接到本地运行的 Llama3.1 AI 服务 (Python FastAPI 应用)，以实现通过 MCP 进行对话交互。

## 1. 引言

`ModelContextProtocol` (MCP) 是一个旨在标准化与大型语言模型 (LLM) 和其他 AI 工具交互的协议。通过在我们的 .NET Web API 中集成 MCP，我们可以创建一个统一的接口来调用不同的 AI 功能，并通过 Server-Sent Events (SSE) 进行通信。

本项目中，我们将 MCP 用于：
- 定义一个与本地 Llama3.1 模型服务交互的"工具" (`McpDialogueTool`)。
- 通过 MCP 的 SSE 端点暴露此工具的调用和结果。

## 2. 环境与前提条件

- .NET 8.0 SDK 已安装。
- 本地 Llama3.1 Python FastAPI 服务正在 `http://localhost:2025/generate` 上运行。
- `Agent.Api` 项目已按先前步骤搭建。

## 3. 集成步骤

### 3.1. 安装 ModelContextProtocol NuGet 包

首先，将 `ModelContextProtocol` NuGet 包添加到 `Agent.Api` 项目。由于该包可能仍处于预发布阶段，需要使用 `--prerelease` 选项：

```bash
cd /home/ubuntu/ai-agent/Agent.Api
export PATH="$PATH:/home/ubuntu/.dotnet" 
dotnet add package ModelContextProtocol --prerelease
```

### 3.2. 创建 Llama3.1 服务客户端

为了让 .NET Web API 与 Python Llama3.1 服务通信，我们创建了一个 HTTP 客户端。

**a. 数据传输对象 (DTOs) - `Services/McpDtos.cs`**

定义请求和响应的数据结构：

```csharp
// 文件: Agent.Core/Services/McpDtos.cs
using System.Text.Json.Serialization;

namespace Agent.Core.Services.Mcp;

public class McpGenerationRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
    
    [JsonPropertyName("max_length")]
    public int MaxLength { get; set; } = 512;
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;
    
    [JsonPropertyName("top_p")]
    public float TopP { get; set; } = 0.9f;
    
    [JsonPropertyName("top_k")]
    public int TopK { get; set; } = 50;
    
    [JsonPropertyName("num_return_sequences")]
    public int NumReturnSequences { get; set; } = 1;
}

public class McpGenerationResponse
{
    [JsonPropertyName("generated_texts")]
    public List<string> GeneratedTexts { get; set; } = new List<string>();
    
    [JsonPropertyName("parameters")]
    public McpGenerationRequest? Parameters { get; set; }
}
```

**b. Llama3.1 服务客户端实现 - `Services/McpServiceClient.cs`**

实现一个服务，通过 HTTP POST 请求调用 Llama3.1 Python API：

```csharp
// 文件: Agent.Core/Services/McpServiceClient.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace Agent.Core.Services.Mcp;

public interface IMcpServiceClient
{
    Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string?> GenerateTextAsync(McpGenerationRequest request, CancellationToken cancellationToken = default);
}

public class McpServiceClient : IMcpServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpServiceClient> _logger;
    private const string McpApiUrl = "http://localhost:2025/generate";

    public McpServiceClient(HttpClient httpClient, ILogger<McpServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var requestPayload = new McpGenerationRequest { Prompt = prompt };
        return await GenerateTextAsync(requestPayload, cancellationToken);
    }

    public async Task<string?> GenerateTextAsync(McpGenerationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending request to Llama3.1 API with prompt: {Prompt}", request.Prompt);
            
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(McpApiUrl, request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<McpGenerationResponse>(cancellationToken: cancellationToken);
                if (result?.GeneratedTexts != null && result.GeneratedTexts.Count > 0)
                {
                    return result.GeneratedTexts[0];
                }
                else
                {
                    _logger.LogWarning("Llama3.1 API returned empty result");
                    return null;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Llama3.1 API returned error: {StatusCode}, {ErrorContent}", 
                    response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Llama3.1 API");
            return null;
        }
    }
}
```

### 3.3. 实现 MCP 工具 (McpDialogueTool)

创建一个实现 `ModelContextProtocol.Models.Tools.ITool` 接口的类，该类将使用 `IMcpServiceClient` 与 Llama3.1 模型交互。

```csharp
// 文件: Agent.Core/McpTools/McpDialogueTool.cs
using Agent.Core.Services.Mcp;
using ModelContextProtocol.Models.Contexts;
using ModelContextProtocol.Models.Primitives;
using ModelContextProtocol.Models.Tools;

namespace Agent.Core.McpTools;

public class McpDialogueTool : ITool
{
    private readonly IMcpServiceClient _McpServiceClient;
    private readonly ILogger<McpDialogueTool> _logger;

    public McpDialogueTool(IMcpServiceClient McpServiceClient, ILogger<McpDialogueTool> logger)
    {
        _McpServiceClient = McpServiceClient;
        _logger = logger;
    }

    public string Name => "McpDialogue";
    public string Description => "与 Llama3.1 AI 模型进行对话，利用其增强的推理能力和更准确的响应。";

    public async Task<ToolOutput> ExecuteAsync(ToolInput toolInput, CancellationToken cancellationToken = default)
    {
        try
        {
            if (toolInput.Parameters == null || !toolInput.Parameters.TryGetValue("prompt", out var promptPrimitive))
            {
                _logger.LogError("Required parameter 'prompt' is missing");
                return new ToolOutput 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Required parameter 'prompt' is missing" 
                };
            }

            string userPrompt = promptPrimitive.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                _logger.LogError("Parameter 'prompt' cannot be empty");
                return new ToolOutput 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Parameter 'prompt' cannot be empty" 
                };
            }

            // 可选参数处理
            float temperature = 0.7f;
            if (toolInput.Parameters.TryGetValue("temperature", out var tempPrimitive) && 
                float.TryParse(tempPrimitive.Value?.ToString(), out var tempValue))
            {
                temperature = tempValue;
            }

            var request = new McpGenerationRequest
            {
                Prompt = userPrompt,
                Temperature = temperature
                // 可以添加更多参数
            };

            _logger.LogInformation("Executing McpDialogueTool with prompt: {Prompt}", userPrompt);
            
            var McpResponse = await _McpServiceClient.GenerateTextAsync(request, cancellationToken);
            
            if (McpResponse == null)
            {
                _logger.LogWarning("Llama3.1 service returned null response");
                return new ToolOutput 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Failed to get response from Llama3.1 service" 
                };
            }

            return new ToolOutput 
            { 
                IsSuccessful = true, 
                Results = new Dictionary<string, McpPrimitive> 
                { 
                    { "response", new McpString(McpResponse) } 
                } 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing McpDialogueTool");
            return new ToolOutput 
            { 
                IsSuccessful = false, 
                ErrorMessage = $"Error executing tool: {ex.Message}" 
            };
        }
    }

    public ToolDefinition GetDefinition()
    {
        return new ToolDefinition
        {
            Name = Name,
            Description = Description,
            InputSchema = new McpSchema(
                McpSchemaType.Object, 
                properties: new() 
                { 
                    { "prompt", new McpSchema(McpSchemaType.String, description: "要发送给 Llama3.1 模型的提示或问题") },
                    { "temperature", new McpSchema(McpSchemaType.Number, description: "控制生成文本随机性的温度参数 (0.0-1.0)") }
                }, 
                required: new() { "prompt" }
            ),
            OutputSchema = new McpSchema(
                McpSchemaType.Object, 
                properties: new() 
                { 
                    { "response", new McpSchema(McpSchemaType.String, description: "Llama3.1 模型生成的响应文本") } 
                }
            )
        };
    }
}
```

### 3.4. 配置 Program.cs

在 `Agent.Api/Program.cs` 文件中，我们需要：
1.  注册 `HttpClient` 和 `IMcpServiceClient`。
2.  添加 MCP 服务并注册我们的工具。
3.  映射 MCP SSE 端点。
4.  (可选) 添加静态文件服务以托管测试页面。
5.  (可选) 添加一个测试端点以方便从浏览器触发工具调用。

```csharp
// 文件: Agent.Api/Program.cs
using Agent.Core.McpTools;
using Agent.Core.Services.Mcp;
using ModelContextProtocol.Extensions;
using ModelContextProtocol.Models.Contexts;
using ModelContextProtocol.Models.Primitives;
using ModelContextProtocol.Models.Tools;

var builder = WebApplication.CreateBuilder(args);

// 注册 HttpClient for McpServiceClient
builder.Services.AddHttpClient<IMcpServiceClient, McpServiceClient>();

// 添加 MCP 服务并从当前程序集注册工具 (会自动发现 McpDialogueTool)
builder.Services.AddMcpServer()
    .WithToolsFromAssembly(typeof(Program).Assembly);

// 添加 CORS 支持
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 添加 Swagger
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
app.UseCors("AllowAll");

// 启用静态文件服务 (用于 index.html 测试页面)
app.UseDefaultFiles();
app.UseStaticFiles();

// 映射 MCP Server-Sent Events 端点
app.MapMcpSse(); // 默认路径是 /mcp/sse

// 映射 MCP 工具列表端点
app.MapGet("/mcp/tools", (McpToolHost toolHost) =>
{
    var tools = toolHost.GetRegisteredTools();
    return Results.Ok(tools.Select(t => t.GetDefinition()));
});

// 测试端点，用于从浏览器触发 McpDialogueTool
app.MapGet("/dev/test-Mcp-dialogue", async (string prompt, float? temperature, McpToolHost toolHost, ILogger<Program> logger) =>
{
    try
    {
        var mcpContext = new McpContext
        {
            ConversationId = Guid.NewGuid().ToString(),
            UserId = "test-user"
        };

        var parameters = new Dictionary<string, McpPrimitive>
        {
            { "prompt", new McpString(prompt) }
        };

        if (temperature.HasValue)
        {
            parameters.Add("temperature", new McpNumber(temperature.Value));
        }

        var toolCall = new McpToolCall
        {
            ToolName = "McpDialogue",
            Parameters = parameters
        };

        logger.LogInformation("Initiating tool call to McpDialogue with prompt: {Prompt}", prompt);
        
        await toolHost.ProcessToolCallAsync(mcpContext, toolCall, CancellationToken.None);
        
        return Results.Ok(new { message = "工具调用已发起，请检查 SSE 流获取结果。" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing tool call");
        return Results.Problem(ex.Message);
    }
});

app.Run();
```

## 4. 使用与测试

### 4.1. 启动服务

1.  确保 Llama3.1 Python FastAPI 服务正在运行 (通常在 `http://localhost:2025`)。
2.  启动 .NET Web API 服务：
    ```bash
    cd /home/ubuntu/ai-agent/apps/agent-api/Agent.Api
    dotnet run
    ```
    Web API 通常会监听如 `https://localhost:7XXX` 和 `http://localhost:5XXX` 的端口。

### 4.2. 测试对话客户端 (index.html)

项目中包含一个位于 `Agent.Api/wwwroot/index.html` 的简单 HTML 页面，用于测试与 MCP SSE 端点的交互。

- 在浏览器中打开 Web API 的根 URL (例如 `https://localhost:7258`，具体端口请查看 `dotnet run` 的输出)。`index.html` 应该会自动加载。
- 页面会尝试连接到 `/mcp/sse` 端点。
- 您可以在输入框中输入提示，然后点击 "Send"。这会调用 `/dev/test-Mcp-dialogue` 测试端点。
- `McpDialogueTool` 的执行结果将通过 SSE 事件推送到页面，并显示在聊天记录和原始 SSE 事件区域。

### 4.3. 直接测试 MCP 端点

- **查看可用工具**: 访问 `GET /mcp/tools` (例如 `https://localhost:7258/mcp/tools`) 可以看到已注册的 MCP 工具列表及其描述。
- **SSE 端点**: `/mcp/sse` 是 MCP 事件流的端点。可以使用支持 SSE 的客户端 (如 curl 或 Postman) 来监听事件。

## 5. Llama3.1 特有功能

Llama3.1 相比之前的版本提供了多项改进，您可以在集成中利用这些新功能：

### 5.1. 增强的对话格式

Llama3.1 支持更结构化的对话格式，可以通过以下方式在 `McpServiceClient` 中实现：

```csharp
public async Task<string?> GenerateChatResponseAsync(List<ChatMessage> messages, CancellationToken cancellationToken = default)
{
    // 将消息格式化为 Llama3.1 的对话格式
    string formattedPrompt = "";
    foreach (var msg in messages)
    {
        switch (msg.Role)
        {
            case "system":
                formattedPrompt += $"<|im_start|>system\n{msg.Content}<|im_end|>\n";
                break;
            case "user":
                formattedPrompt += $"<|im_start|>user\n{msg.Content}<|im_end|>\n";
                break;
            case "assistant":
                formattedPrompt += $"<|im_start|>assistant\n{msg.Content}<|im_end|>\n";
                break;
        }
    }
    
    // 添加最终的助手提示
    formattedPrompt += "<|im_start|>assistant\n";
    
    var request = new McpGenerationRequest
    {
        Prompt = formattedPrompt,
        Temperature = 0.7f,
        MaxLength = 1024
    };
    
    return await GenerateTextAsync(request, cancellationToken);
}
```

### 5.2. 流式响应

对于更好的用户体验，您可以实现流式响应：

```csharp
// 在 McpServiceClient 中添加
public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(
    string prompt, 
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    // 实现流式响应逻辑
    // 这需要 Llama3.1 Python API 支持流式输出
    // 可以使用 SSE 或 WebSockets 实现
}
```

## 6. 工作流程说明

1.  用户通过客户端 (如 `index.html`) 发起一个对话请求 (例如，调用 `/dev/test-Mcp-dialogue?prompt=你好`)。
2.  Web API 中的测试端点接收到请求，构造一个 `McpContext` 和 `McpToolCall` 对象，指定调用 `McpDialogueTool` 并传入用户提示。
3.  测试端点调用 `McpToolHost.ProcessToolCallAsync()`。
4.  MCP 服务框架查找名为 `McpDialogue` 的已注册工具。
5.  `McpDialogueTool.ExecuteAsync()` 方法被调用。
6.  `McpDialogueTool` 使用注入的 `IMcpServiceClient` 向本地 Llama3.1 Python 服务 (`http://localhost:2025/generate`) 发送 HTTP 请求。
7.  Llama3.1 Python 服务处理提示并返回生成的文本。
8.  `IMcpServiceClient` 将响应返回给 `McpDialogueTool`。
9.  `McpDialogueTool` 将 Llama3.1 的响应包装在 `ToolOutput` 对象中。
10. MCP 服务框架接收到 `ToolOutput`，并通过 `/mcp/sse` 端点将一个 `ToolResult` 事件发送给所有连接的 SSE 客户端。
11. `index.html` 页面中的 JavaScript 监听到此 SSE 事件，解析 JSON 数据，并将 AI 的响应显示在聊天界面上。

## 7. 后续优化与扩展

- **完善错误处理**: 在 `McpServiceClient` 和 `McpDialogueTool` 中增加更健壮的错误处理和重试逻辑。
- **配置化**: 将 Llama3.1 服务 URL、默认参数等移至 `appsettings.json`。
- **上下文管理**: 扩展 `McpDialogueTool` 以处理完整的对话历史，利用 Llama3.1 的增强上下文处理能力。
- **流式响应**: 实现基于 SSE 的流式响应，提供更好的用户体验。
- **量化支持**: 添加对 Llama3.1 量化模型的支持，以优化资源使用。
- **更多工具**: 可以按照类似模式添加更多与不同 AI 服务或本地功能交互的 MCP 工具。
- **安全性**: 为 MCP 端点和工具调用添加认证和授权机制。
- **MCP 输入端点**: 实现一个标准的 MCP 输入端点 (例如 `POST /mcp/input`) 来接收完整的 MCP 请求对象，而不是依赖测试端点。

此集成提供了一个灵活的基础，用于通过 .NET Web API 构建和管理与 Llama3.1 AI 模型的交互，充分利用其增强的性能和功能。

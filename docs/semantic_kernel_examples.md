# Microsoft Semantic Kernel 集成示例

## 概述
本文档提供了在 AgentWebApi 项目中使用 Microsoft Semantic Kernel 的详细代码示例，包括聊天完成、嵌入生成、记忆操作和向量搜索。

## 配置示例

### appsettings.json 配置
```json
{
  "SemanticKernel": {
    "OpenAIApiKey": "your-openai-api-key",
    "ChatModel": "gpt-3.5-turbo",
    "EmbeddingModel": "text-embedding-ada-002",
    "MaxTokens": 1000,
    "Temperature": 0.7,
    "EnableMemory": true,
    "DefaultMemoryCollection": "default"
  },
  "ConnectionStrings": {
    "ChromaDb": "http://localhost:8000"
  }
}
```

### Azure OpenAI 配置
```json
{
  "SemanticKernel": {
    "AzureOpenAIEndpoint": "https://your-resource.openai.azure.com/",
    "AzureOpenAIApiKey": "your-azure-openai-key",
    "AzureChatDeploymentName": "gpt-35-turbo",
    "AzureEmbeddingDeploymentName": "text-embedding-ada-002",
    "MaxTokens": 1000,
    "Temperature": 0.7
  }
}
```

## API 使用示例

### 1. 聊天完成 (Chat Completion)

#### 基本聊天完成
```http
POST /api/semantickernel/chat/completion
Content-Type: application/json

{
  "prompt": "解释什么是人工智能",
  "systemMessage": "你是一个专业的AI助手，请用简洁明了的语言回答问题。"
}
```

**响应示例:**
```json
{
  "response": "人工智能（AI）是一种计算机科学技术，旨在创建能够执行通常需要人类智能的任务的系统...",
  "success": true
}
```

#### 流式聊天完成
```http
POST /api/semantickernel/chat/completion/stream
Content-Type: application/json

{
  "prompt": "写一首关于春天的诗",
  "systemMessage": "你是一位诗人，请创作优美的诗歌。"
}
```

**响应格式:** Server-Sent Events (SSE)
```
data: 春风
data: 轻抚
data: 大地
data: ，
data: 万物
data: 复苏
...
```

#### 带对话历史的聊天
```http
POST /api/semantickernel/chat/completion/history
Content-Type: application/json

{
  "messages": [
    {
      "role": "system",
      "content": "你是一个编程助手"
    },
    {
      "role": "user", 
      "content": "什么是递归？"
    },
    {
      "role": "assistant",
      "content": "递归是一种编程技术，函数调用自身来解决问题..."
    },
    {
      "role": "user",
      "content": "能给我一个递归的例子吗？"
    }
  ]
}
```

### 2. 文本嵌入 (Text Embeddings)

#### 生成单个文本嵌入
```http
POST /api/semantickernel/embeddings/generate
Content-Type: application/json

{
  "text": "这是一个测试文本，用于生成向量嵌入。"
}
```

**响应示例:**
```json
{
  "embedding": [0.1234, -0.5678, 0.9012, ...],
  "dimension": 1536,
  "success": true
}
```

#### 批量生成嵌入
```http
POST /api/semantickernel/embeddings/generate/batch
Content-Type: application/json

{
  "texts": [
    "第一个文本",
    "第二个文本", 
    "第三个文本"
  ]
}
```

**响应示例:**
```json
{
  "embeddings": [
    [0.1234, -0.5678, ...],
    [0.2345, -0.6789, ...],
    [0.3456, -0.7890, ...]
  ],
  "count": 3,
  "success": true
}
```

### 3. 记忆操作 (Memory Operations)

#### 保存记忆
```http
POST /api/semantickernel/memory/save
Content-Type: application/json

{
  "collectionName": "knowledge_base",
  "text": "Python是一种高级编程语言，以其简洁的语法和强大的功能而闻名。",
  "id": "python_intro_001",
  "metadata": {
    "category": "programming",
    "language": "chinese",
    "difficulty": "beginner",
    "tags": ["python", "programming", "introduction"]
  }
}
```

#### 搜索记忆
```http
POST /api/semantickernel/memory/search
Content-Type: application/json

{
  "collectionName": "knowledge_base",
  "query": "什么是Python编程语言？",
  "limit": 5,
  "minRelevance": 0.7
}
```

**响应示例:**
```json
{
  "results": [
    {
      "id": "python_intro_001",
      "text": "Python是一种高级编程语言，以其简洁的语法和强大的功能而闻名。",
      "relevance": 0.95,
      "metadata": {
        "category": "programming",
        "language": "chinese",
        "difficulty": "beginner"
      }
    }
  ],
  "count": 1,
  "success": true
}
```

#### 删除记忆
```http
DELETE /api/semantickernel/memory/knowledge_base/python_intro_001
```

### 4. 语义搜索 (Semantic Search)

#### 执行语义搜索
```http
POST /api/semantickernel/search/semantic
Content-Type: application/json

{
  "collectionName": "documents",
  "query": "机器学习算法的应用",
  "maxResults": 10,
  "minSimilarity": 0.75
}
```

**响应示例:**
```json
{
  "matches": [
    {
      "id": "ml_doc_001",
      "score": 0.92,
      "distance": 0.08,
      "content": "机器学习算法在图像识别、自然语言处理等领域有广泛应用...",
      "metadata": {
        "title": "机器学习应用指南",
        "author": "张三",
        "category": "AI"
      },
      "modality": "Text"
    }
  ],
  "totalMatches": 1,
  "executionTimeMs": 150,
  "success": true
}
```

### 5. 文档管理 (Document Management)

#### 添加文档到向量数据库
```http
POST /api/semantickernel/documents/add
Content-Type: application/json

{
  "collectionName": "tech_docs",
  "documents": [
    {
      "id": "doc_001",
      "content": "深度学习是机器学习的一个子领域，使用多层神经网络来学习数据的复杂模式。",
      "metadata": {
        "title": "深度学习简介",
        "category": "AI",
        "difficulty": "intermediate",
        "language": "chinese"
      }
    },
    {
      "id": "doc_002", 
      "content": "自然语言处理（NLP）是人工智能的一个分支，专注于计算机与人类语言的交互。",
      "metadata": {
        "title": "NLP基础",
        "category": "AI",
        "difficulty": "beginner",
        "language": "chinese"
      }
    }
  ]
}
```

## C# 代码示例

### 1. 依赖注入配置
```csharp
// Program.cs
using AgentWebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加语义内核服务
builder.Services.AddSemanticKernel(builder.Configuration);

// 添加向量数据库服务
builder.Services.AddVectorDatabase(builder.Configuration);

var app = builder.Build();
app.Run();
```

### 2. 控制器中使用语义内核
```csharp
[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly ISemanticKernelService _semanticKernel;
    private readonly IVectorDatabaseService _vectorDb;

    public AIController(
        ISemanticKernelService semanticKernel,
        IVectorDatabaseService vectorDb)
    {
        _semanticKernel = semanticKernel;
        _vectorDb = vectorDb;
    }

    /// <summary>
    /// 智能问答示例 - 结合记忆搜索和聊天完成
    /// </summary>
    [HttpPost("intelligent-qa")]
    public async Task<IActionResult> IntelligentQA([FromBody] QARequest request)
    {
        try
        {
            // 1. 搜索相关记忆
            var memoryResults = await _semanticKernel.SearchMemoryAsync(
                "knowledge_base", 
                request.Question, 
                limit: 3, 
                minRelevance: 0.7f);

            // 2. 构建上下文
            var context = string.Join("\n", 
                memoryResults.Select(r => $"相关信息: {r.Text}"));

            // 3. 生成回答
            var systemMessage = $@"
你是一个智能助手。请基于以下相关信息回答用户问题：

{context}

如果相关信息不足以回答问题，请诚实地说明。
";

            var response = await _semanticKernel.GetChatCompletionAsync(
                request.Question, 
                systemMessage);

            return Ok(new { 
                answer = response, 
                sources = memoryResults.Select(r => r.Id).ToList(),
                success = true 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class QARequest
{
    public string Question { get; set; } = string.Empty;
}
```

### 3. 自定义插件示例
```csharp
/// <summary>
/// 数学计算插件示例
/// </summary>
public class MathPlugin
{
    /// <summary>
    /// 计算两个数的和
    /// </summary>
    [KernelFunction, Description("计算两个数字的和")]
    public double Add(
        [Description("第一个数字")] double a,
        [Description("第二个数字")] double b)
    {
        return a + b;
    }

    /// <summary>
    /// 计算圆的面积
    /// </summary>
    [KernelFunction, Description("根据半径计算圆的面积")]
    public double CalculateCircleArea(
        [Description("圆的半径")] double radius)
    {
        return Math.PI * radius * radius;
    }
}

// 在服务中注册插件
public class CustomSemanticKernelService : ISemanticKernelService
{
    public void RegisterPlugins()
    {
        // 添加数学插件
        AddPluginFromType<MathPlugin>("Math");
        
        // 获取可用函数
        var functions = GetAvailableFunctions();
        // 输出: ["Math.Add", "Math.CalculateCircleArea"]
    }
}
```

### 4. 多模态支持准备
```csharp
/// <summary>
/// 多模态文档处理示例（为未来扩展准备）
/// </summary>
public class MultimodalDocumentService
{
    private readonly IVectorDatabaseService _vectorDb;
    private readonly ISemanticKernelService _semanticKernel;

    public async Task ProcessMultimodalDocument(
        string collectionName,
        string documentId,
        string textContent,
        byte[]? imageData = null,
        byte[]? audioData = null)
    {
        var documents = new List<VectorDocument>();

        // 处理文本内容
        if (!string.IsNullOrEmpty(textContent))
        {
            var textEmbedding = await _semanticKernel.GenerateEmbeddingAsync(textContent);
            documents.Add(new VectorDocument
            {
                Id = $"{documentId}_text",
                Content = textContent,
                Embedding = textEmbedding,
                Modality = Modality.Text,
                Metadata = new Dictionary<string, object>
                {
                    ["document_id"] = documentId,
                    ["content_type"] = "text"
                }
            });
        }

        // 处理图像内容（占位符，未来实现）
        if (imageData != null)
        {
            documents.Add(new VectorDocument
            {
                Id = $"{documentId}_image",
                Content = "图像内容描述", // 未来通过图像识别生成
                BinaryData = imageData,
                MimeType = "image/jpeg",
                Modality = Modality.Image,
                Metadata = new Dictionary<string, object>
                {
                    ["document_id"] = documentId,
                    ["content_type"] = "image"
                }
            });
        }

        // 批量添加到向量数据库
        await _vectorDb.AddDocumentsAsync(collectionName, documents);
    }
}
```

## 错误处理和最佳实践

### 1. 异常处理
```csharp
public async Task<string> SafeChatCompletion(string prompt)
{
    try
    {
        return await _semanticKernel.GetChatCompletionAsync(prompt);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "API请求失败");
        return "抱歉，服务暂时不可用，请稍后重试。";
    }
    catch (ArgumentException ex)
    {
        _logger.LogError(ex, "输入参数无效");
        return "输入内容有误，请检查后重试。";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "未知错误");
        return "处理请求时发生错误。";
    }
}
```

### 2. 性能优化
```csharp
/// <summary>
/// 批量处理优化示例
/// </summary>
public async Task<List<string>> BatchProcessTexts(List<string> texts)
{
    // 使用并发处理提高性能
    var semaphore = new SemaphoreSlim(5); // 限制并发数
    var tasks = texts.Select(async text =>
    {
        await semaphore.WaitAsync();
        try
        {
            return await _semanticKernel.GetChatCompletionAsync(text);
        }
        finally
        {
            semaphore.Release();
        }
    });

    return (await Task.WhenAll(tasks)).ToList();
}
```

### 3. 缓存策略
```csharp
/// <summary>
/// 嵌入缓存示例
/// </summary>
public class CachedEmbeddingService
{
    private readonly IMemoryCache _cache;
    private readonly ISemanticKernelService _semanticKernel;

    public async Task<float[]> GetCachedEmbedding(string text)
    {
        var cacheKey = $"embedding_{text.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out float[]? cachedEmbedding))
        {
            return cachedEmbedding!;
        }

        var embedding = await _semanticKernel.GenerateEmbeddingAsync(text);
        
        _cache.Set(cacheKey, embedding, TimeSpan.FromHours(1));
        
        return embedding;
    }
}
```

## 部署和监控

### 1. Docker 部署
```yaml
# docker-compose.yml
version: '3.8'
services:
  agent-webapi:
    build: .
    environment:
      - SemanticKernel__OpenAIApiKey=${OPENAI_API_KEY}
      - ConnectionStrings__ChromaDb=http://chromadb:8000
    depends_on:
      - chromadb
    ports:
      - "5000:8080"

  chromadb:
    image: chromadb/chroma:latest
    ports:
      - "8000:8000"
    volumes:
      - chromadb_data:/chroma/chroma

volumes:
  chromadb_data:
```

### 2. 健康检查
```csharp
[HttpGet("health")]
public async Task<IActionResult> HealthCheck()
{
    try
    {
        // 检查语义内核服务
        var testResponse = await _semanticKernel.GetChatCompletionAsync("测试");
        
        // 检查向量数据库连接
        var collections = await _vectorDb.ListCollectionsAsync();
        
        return Ok(new
        {
            status = "healthy",
            semanticKernel = "ok",
            vectorDatabase = "ok",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return StatusCode(503, new
        {
            status = "unhealthy",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
}
```

这些示例展示了如何在实际项目中使用 Microsoft Semantic Kernel 和向量数据库，包括基本操作、高级功能、错误处理和性能优化。代码中的中文注释帮助理解每个功能的用途和实现方式。


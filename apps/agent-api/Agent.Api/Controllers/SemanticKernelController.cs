namespace Agent.Api.Controllers;

/// <summary>
/// Semantic Kernel API controller for AI operations
/// 语义内核API控制器，用于AI操作
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SemanticKernelController : ControllerBase
{
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly IVectorDatabaseService _vectorDatabaseService;
    private readonly ILogger<SemanticKernelController> _logger;

    public SemanticKernelController(
        ISemanticKernelService semanticKernelService,
        IVectorDatabaseService vectorDatabaseService,
        ILogger<SemanticKernelController> logger)
    {
        _semanticKernelService = semanticKernelService ?? throw new ArgumentNullException(nameof(semanticKernelService));
        _vectorDatabaseService = vectorDatabaseService ?? throw new ArgumentNullException(nameof(vectorDatabaseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Chat Completion - 聊天完成

    /// <summary>
    /// Get chat completion response
    /// 获取聊天完成响应
    /// </summary>
    [HttpPost("chat/completion")]
    public async Task<IActionResult> GetChatCompletion([FromBody] ChatCompletionRequest request)
    {
        try
        {
            _logger.LogInformation("Processing chat completion request");

            var response = await _semanticKernelService.GetChatCompletionAsync(
                request.Prompt, 
                request.SystemMessage);

            return Ok(new ChatCompletionResponse
            {
                Response = response,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process chat completion request");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get streaming chat completion response
    /// 获取流式聊天完成响应
    /// </summary>
    [HttpPost("chat/completion/stream")]
    public async Task<IActionResult> GetStreamingChatCompletion([FromBody] ChatCompletionRequest request)
    {
        try
        {
            _logger.LogInformation("Processing streaming chat completion request");

            var responseStream = _semanticKernelService.GetStreamingChatCompletionAsync(
                request.Prompt, 
                request.SystemMessage);

            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            await foreach (var chunk in responseStream)
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }

            return new ContentResult {
                StatusCode = StatusCodes.Status404NotFound,   // 对应 HTTP 204
                Content    = string.Empty,                     // NoContent 一般没内容
                ContentType = "text/plain; charset=utf-8"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process streaming chat completion request");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get chat completion with conversation history
    /// 使用对话历史获取聊天完成
    /// </summary>
    [HttpPost("chat/completion/history")]
    public async Task<IActionResult> GetChatCompletionWithHistory([FromBody] ChatHistoryRequest request)
    {
        try
        {
            _logger.LogInformation("Processing chat completion with history request");

            var response = await _semanticKernelService.GetChatCompletionWithHistoryAsync(request.Messages);

            return Ok(new ChatCompletionResponse
            {
                Response = response,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process chat completion with history request");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Embeddings - 嵌入

    /// <summary>
    /// Generate text embedding
    /// 生成文本嵌入
    /// </summary>
    [HttpPost("embeddings/generate")]
    public async Task<IActionResult> GenerateEmbedding([FromBody] EmbeddingRequest request)
    {
        try
        {
            _logger.LogInformation("Generating embedding for text");

            var embedding = await _semanticKernelService.GenerateEmbeddingAsync(request.Text);

            return Ok(new EmbeddingResponse
            {
                Embedding = embedding,
                Dimension = embedding.Length,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate multiple text embeddings
    /// 生成多个文本嵌入
    /// </summary>
    [HttpPost("embeddings/generate/batch")]
    public async Task<IActionResult> GenerateEmbeddings([FromBody] BatchEmbeddingRequest request)
    {
        try
        {
            _logger.LogInformation("Generating embeddings for {Count} texts", request.Texts.Count());

            var embeddings = await _semanticKernelService.GenerateEmbeddingsAsync(request.Texts);

            return Ok(new BatchEmbeddingResponse
            {
                Embeddings = embeddings.ToList(),
                Count = embeddings.Count(),
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Memory Operations - 记忆操作

    /// <summary>
    /// Save text to memory
    /// 将文本保存到记忆中
    /// </summary>
    [HttpPost("memory/save")]
    public async Task<IActionResult> SaveMemory([FromBody] SaveMemoryRequest request)
    {
        try
        {
            _logger.LogInformation("Saving memory to collection: {CollectionName}", request.CollectionName);

            await _semanticKernelService.SaveMemoryAsync(
                request.CollectionName,
                request.Text,
                request.Id,
                request.Metadata);

            return Ok(new { success = true, message = "Memory saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save memory");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Search memory
    /// 搜索记忆
    /// </summary>
    [HttpPost("memory/search")]
    public async Task<IActionResult> SearchMemory([FromBody] SearchMemoryRequest request)
    {
        try
        {
            _logger.LogInformation("Searching memory in collection: {CollectionName}", request.CollectionName);

            var results = await _semanticKernelService.SearchMemoryAsync(
                request.CollectionName,
                request.Query,
                request.Limit,
                request.MinRelevance);

            return Ok(new SearchMemoryResponse
            {
                Results = results.ToList(),
                Count = results.Count(),
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search memory");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove memory
    /// 删除记忆
    /// </summary>
    [HttpDelete("memory/{collectionName}/{id}")]
    public async Task<IActionResult> RemoveMemory(string collectionName, string id)
    {
        try
        {
            _logger.LogInformation("Removing memory from collection: {CollectionName}, ID: {Id}", collectionName, id);

            await _semanticKernelService.RemoveMemoryAsync(collectionName, id);

            return Ok(new { success = true, message = "Memory removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove memory");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Functions - 函数

    /// <summary>
    /// Invoke a kernel function
    /// 调用内核函数
    /// </summary>
    [HttpPost("functions/invoke")]
    public async Task<IActionResult> InvokeFunction([FromBody] InvokeFunctionRequest request)
    {
        try
        {
            _logger.LogInformation("Invoking function: {FunctionName}", request.FunctionName);

            var result = await _semanticKernelService.InvokeFunctionAsync(
                request.PlugName,
                request.FunctionName,
                request.Arguments);

            return Ok(new InvokeFunctionResponse
            {
                Result = result,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke function: {FunctionName}", request.FunctionName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get available functions
    /// 获取可用函数
    /// </summary>
    [HttpGet("functions")]
    public IActionResult GetAvailableFunctions()
    {
        try
        {
            _logger.LogInformation("Getting available functions");

            var functions = _semanticKernelService.GetAvailableFunctions();

            return Ok(new AvailableFunctionsResponse
            {
                Functions = functions.ToList(),
                Count = functions.Count(),
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available functions");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Vector Database Integration - 向量数据库集成

    /// <summary>
    /// Semantic search using vector database
    /// 使用向量数据库进行语义搜索
    /// </summary>
    [HttpPost("search/semantic")]
    public async Task<IActionResult> SemanticSearch([FromBody] SemanticSearchRequest request)
    {
        try
        {
            _logger.LogInformation("Performing semantic search in collection: {CollectionName}", request.CollectionName);

            var searchResult = await _vectorDatabaseService.SearchByTextAsync(
                request.CollectionName,
                request.Query,
                new VectorSearchOptions
                {
                    MaxResults = request.MaxResults,
                    MinSimilarity = request.MinSimilarity,
                    IncludeContent = true,
                    IncludeMetadata = true
                });

            return Ok(new SemanticSearchResponse
            {
                Matches = searchResult.Matches.ToList(),
                TotalMatches = searchResult.TotalMatches,
                ExecutionTimeMs = searchResult.ExecutionTimeMs,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform semantic search");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Add documents with embeddings to vector database
    /// 向向量数据库添加带有嵌入的文档
    /// </summary>
    [HttpPost("documents/add")]
    public async Task<IActionResult> AddDocuments([FromBody] AddDocumentsRequest request)
    {
        try
        {
            _logger.LogInformation("Adding {Count} documents to collection: {CollectionName}", 
                request.Documents.Count(), request.CollectionName);

            var vectorDocuments = new List<VectorDocument>();

            foreach (var doc in request.Documents)
            {
                // Generate embedding if not provided - 如果未提供嵌入则生成
                var embedding = doc.Embedding ?? await _semanticKernelService.GenerateEmbeddingAsync(doc.Content);

                vectorDocuments.Add(new VectorDocument
                {
                    Id = doc.Id,
                    Content = doc.Content,
                    Embedding = embedding,
                    Metadata = doc.Metadata,
                    Modality = Modality.Text
                });
            }

            await _vectorDatabaseService.AddDocumentsAsync(request.CollectionName, vectorDocuments);

            return Ok(new { success = true, message = $"Added {vectorDocuments.Count} documents successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add documents");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion
}

#region Request/Response Models - 请求/响应模型

public class ChatCompletionRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? SystemMessage { get; set; }
}

public class ChatCompletionResponse
{
    public string Response { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class ChatHistoryRequest
{
    public IEnumerable<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class EmbeddingRequest
{
    public string Text { get; set; } = string.Empty;
}

public class EmbeddingResponse
{
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public int Dimension { get; set; }
    public bool Success { get; set; }
}

public class BatchEmbeddingRequest
{
    public IEnumerable<string> Texts { get; set; } = new List<string>();
}

public class BatchEmbeddingResponse
{
    public List<float[]> Embeddings { get; set; } = new();
    public int Count { get; set; }
    public bool Success { get; set; }
}

public class SaveMemoryRequest
{
    public string CollectionName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class SearchMemoryRequest
{
    public string CollectionName { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
    public float MinRelevance { get; set; } = 0.7f;
}

public class SearchMemoryResponse
{
    public List<MemoryResult> Results { get; set; } = new();
    public int Count { get; set; }
    public bool Success { get; set; }
}

public class InvokeFunctionRequest
{
    public string PlugName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
}

public class InvokeFunctionResponse
{
    public string Result { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class AvailableFunctionsResponse
{
    public List<string> Functions { get; set; } = new();
    public int Count { get; set; }
    public bool Success { get; set; }
}

public class SemanticSearchRequest
{
    public string CollectionName { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
    public float MinSimilarity { get; set; } = 0.7f;
}

public class SemanticSearchResponse
{
    public List<VectorSearchMatch> Matches { get; set; } = new();
    public int TotalMatches { get; set; }
    public long ExecutionTimeMs { get; set; }
    public bool Success { get; set; }
}

public class DocumentRequest
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

#endregion


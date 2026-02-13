namespace Agent.Api.Controllers;

/// <summary>
/// RAG (Retrieval Augmented Generation) controller for enterprise scenarios
/// RAG（检索增强生成）控制器，用于企业场景
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class RagController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagController> _logger;
    private readonly IAgentTelemetryProvider _telemetryProvider;

    public RagController(IRagService ragService, ILogger<RagController> logger, IAgentTelemetryProvider telemetryProvider)
    {
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryProvider = telemetryProvider ?? throw new ArgumentNullException(nameof(telemetryProvider));
    }

    #region Document Management - 文档管理

    /// <summary>
    /// Add document to knowledge base
    /// 向知识库添加文档
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="document">Document content / 文档内容</param>
    /// <returns>Result of the operation / 操作结果</returns>
    [HttpPost("collections/{collectionName}/documents")]
    [SwaggerOperation(
        Summary = "Add document to knowledge base",
        Description = "Adds a new document to the specified RAG collection.",
        OperationId = "AddDocument",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> AddDocument(string collectionName, [FromBody] RagDocument document)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.AddDocument"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.document_id", document.Id);
            try
            {
                _logger.LogInformation("Adding document {DocumentId} to collection {CollectionName}",
                    document.Id, collectionName);

                var documentId = await _ragService.AddDocumentAsync(collectionName, document);
                span.SetAttribute("rag.document_added_id", documentId);

                return Ok(ApiResponse<object>.Ok(new
                {
                    documentId = documentId,
                    message = $"Document {documentId} added successfully to {collectionName}"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add document to collection {CollectionName}", collectionName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get documents from knowledge base
    /// 从知识库获取文档
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="ids">Document IDs / 文档ID列表</param>
    /// <returns>List of documents / 文档列表</returns>
    [HttpGet("collections/{collectionName}/documents")]
    [SwaggerOperation(
        Summary = "Get documents from knowledge base",
        Description = "Retrieves documents from the specified RAG collection.",
        OperationId = "GetDocuments",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> GetDocuments(string collectionName, [FromQuery] string[]? ids = null)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.GetDocuments"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.requested_ids_count", ids?.Length ?? 0);
            try
            {
                _logger.LogInformation("Getting documents from collection {CollectionName}", collectionName);

                var documents = await _ragService.GetDocumentsAsync(collectionName, ids);
                span.SetAttribute("rag.returned_documents_count", documents.Count());

                return Ok(ApiResponse<object>.Ok(new
                {
                    documents = documents,
                    count = documents.Count()
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get documents from collection {CollectionName}", collectionName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Update document in knowledge base
    /// 更新知识库中的文档
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="documentId">Document ID / 文档ID</param>
    /// <param name="document">Document content / 文档内容</param>
    /// <returns>Result of the operation / 操作结果</returns>
    [HttpPut("collections/{collectionName}/documents/{documentId}")]
    [SwaggerOperation(
        Summary = "Update document in knowledge base",
        Description = "Updates an existing document in the specified RAG collection.",
        OperationId = "UpdateDocument",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDocument(string collectionName, string documentId, [FromBody] RagDocument document)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.UpdateDocument"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.document_id", documentId);
            try
            {
                document.Id = documentId; // Ensure ID matches URL parameter

                _logger.LogInformation("Updating document {DocumentId} in collection {CollectionName}",
                    documentId, collectionName);

                await _ragService.UpdateDocumentAsync(collectionName, document);
                span.SetAttribute("rag.update_success", true);

                return Ok(ApiResponse<object>.Ok(new
                {
                    message = $"Document {documentId} updated successfully"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update document {DocumentId}", documentId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Delete document from knowledge base
    /// 从知识库删除文档
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="documentId">Document ID / 文档ID</param>
    /// <returns>Result of the operation / 操作结果</returns>
    [HttpDelete("collections/{collectionName}/documents/{documentId}")]
    [SwaggerOperation(
        Summary = "Delete document from knowledge base",
        Description = "Deletes a document from the specified RAG collection.",
        OperationId = "DeleteDocument",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(string collectionName, string documentId)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.DeleteDocument"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.document_id", documentId);
            try
            {
                _logger.LogInformation("Deleting document {DocumentId} from collection {CollectionName}",
                    documentId, collectionName);

                await _ragService.DeleteDocumentAsync(collectionName, documentId);
                span.SetAttribute("rag.delete_success", true);

                return Ok(ApiResponse<object>.Ok(new
                {
                    message = $"Document {documentId} deleted successfully"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete document {DocumentId}", documentId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get document count in collection
    /// 获取集合中的文档数量
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <returns>Document count / 文档数量</returns>
    [HttpGet("collections/{collectionName}/count")]
    [SwaggerOperation(
        Summary = "Get document count in collection",
        Description = "Gets the total number of documents in the specified RAG collection.",
        OperationId = "GetDocumentCount",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> GetDocumentCount(string collectionName)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.GetDocumentCount"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            try
            {
                var count = await _ragService.GetDocumentCountAsync(collectionName);
                span.SetAttribute("rag.document_count", count);

                return Ok(ApiResponse<object>.Ok(new
                {
                    collectionName = collectionName,
                    documentCount = count
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get document count for collection {CollectionName}", collectionName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    #endregion

    #region Retrieval - 检索

    /// <summary>
    /// Hybrid retrieval combining multiple strategies
    /// 结合多种策略的混合检索
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="query">Query options / 查询选项</param>
    /// <returns>Retrieval results / 检索结果</returns>
    [HttpPost("collections/{collectionName}/retrieve/hybrid")]
    [SwaggerOperation(
        Summary = "Hybrid retrieval",
        Description = "Performs hybrid retrieval combining multiple strategies (keyword, vector, semantic).",
        OperationId = "HybridRetrieval",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> HybridRetrieval(string collectionName, [FromBody] RagQuery query)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.HybridRetrieval"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.query_text", query.QueryText);
            try
            {
                _logger.LogInformation("Performing hybrid retrieval in collection {CollectionName}", collectionName);

                var result = await _ragService.HybridRetrievalAsync(collectionName, query);
                span.SetAttribute("rag.retrieval_result_count", result.RetrievedDocuments?.Count() ?? 0);

                return Ok(ApiResponse<object>.Ok(new
                {
                    result = result
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform hybrid retrieval");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Vector similarity retrieval
    /// 向量相似度检索
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="request">Vector retrieval request / 向量检索请求</param>
    /// <returns>Retrieval results / 检索结果</returns>
    [HttpPost("collections/{collectionName}/retrieve/vector")]
    [SwaggerOperation(
        Summary = "Vector similarity retrieval",
        Description = "Performs vector-based similarity retrieval.",
        OperationId = "VectorRetrieval",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> VectorRetrieval(string collectionName, [FromBody] VectorRetrievalRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.VectorRetrieval"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.query_text", request.Query);
            span.SetAttribute("rag.top_k", request.TopK);
            try
            {
                _logger.LogInformation("Performing vector retrieval in collection {CollectionName}", collectionName);

                var result = await _ragService.VectorRetrievalAsync(collectionName, request.Query, request.TopK);
                span.SetAttribute("rag.retrieval_result_count", result.RetrievedDocuments?.Count() ?? 0);

                return Ok(ApiResponse<object>.Ok(new
                {
                    result = result
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform vector retrieval");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Keyword-based retrieval
    /// 基于关键词的检索
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="request">Keyword retrieval request / 关键词检索请求</param>
    /// <returns>Retrieval results / 检索结果</returns>
    [HttpPost("collections/{collectionName}/retrieve/keyword")]
    [SwaggerOperation(
        Summary = "Keyword-based retrieval",
        Description = "Performs keyword-based retrieval.",
        OperationId = "KeywordRetrieval",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> KeywordRetrieval(string collectionName, [FromBody] KeywordRetrievalRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.KeywordRetrieval"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.query_text", request.Query);
            span.SetAttribute("rag.top_k", request.TopK);
            try
            {
                _logger.LogInformation("Performing keyword retrieval in collection {CollectionName}", collectionName);

                var result = await _ragService.KeywordRetrievalAsync(collectionName, request.Query, request.TopK);
                span.SetAttribute("rag.retrieval_result_count", result.RetrievedDocuments?.Count() ?? 0);

                return Ok(ApiResponse<object>.Ok(new
                {
                    result = result
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform keyword retrieval");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Semantic retrieval with re-ranking
    /// 带重排序的语义检索
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="request">Semantic retrieval request / 语义检索请求</param>
    /// <returns>Retrieval results / 检索结果</returns>
    [HttpPost("collections/{collectionName}/retrieve/semantic")]
    [SwaggerOperation(
        Summary = "Semantic retrieval with re-ranking",
        Description = "Performs semantic retrieval with re-ranking.",
        OperationId = "SemanticRetrieval",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> SemanticRetrieval(string collectionName, [FromBody] SemanticRetrievalRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.SemanticRetrieval"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.query_text", request.Query);
            span.SetAttribute("rag.top_k", request.TopK);
            try
            {
                _logger.LogInformation("Performing semantic retrieval in collection {CollectionName}", collectionName);

                var result = await _ragService.SemanticRetrievalAsync(collectionName, request.Query, request.TopK);
                span.SetAttribute("rag.retrieval_result_count", result.RetrievedDocuments?.Count() ?? 0);

                return Ok(ApiResponse<object>.Ok(new
                {
                    result = result
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform semantic retrieval");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    #endregion

    #region RAG Generation - RAG生成

    /// <summary>
    /// Generate response using RAG
    /// 使用RAG生成响应
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="request">Generation request / 生成请求</param>
    /// <returns>Generated response / 生成的响应</returns>
    [HttpPost("collections/{collectionName}/generate")]
    [SwaggerOperation(
        Summary = "Generate response using RAG",
        Description = "Generates a response using Retrieval Augmented Generation.",
        OperationId = "GenerateResponse",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> GenerateResponse(string collectionName, [FromBody] RagGenerationRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.GenerateResponse"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.prompt_length", request.Prompt?.Length);
            try
            {
                _logger.LogInformation("Generating RAG response for collection {CollectionName}", collectionName);

                var response = await _ragService.GenerateResponseAsync(collectionName, request);
                span.SetAttribute("rag.generated_response_length", response.Response?.Length);

                return Ok(ApiResponse<object>.Ok(new
                {
                    response = response
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate RAG response");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Generate streaming response using RAG
    /// 使用RAG生成流式响应
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="request">Generation request / 生成请求</param>
    /// <returns>Streaming response / 流式响应</returns>
    [HttpPost("collections/{collectionName}/generate/stream")]
    [SwaggerOperation(
        Summary = "Generate streaming response using RAG",
        Description = "Generates a streaming response using Retrieval Augmented Generation (Server-Sent Events).",
        OperationId = "GenerateStreamingResponse",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateStreamingResponse(string collectionName, [FromBody] RagGenerationRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.GenerateStreamingResponse"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.prompt_length", request.Prompt?.Length);
            try
            {
                _logger.LogInformation("Generating streaming RAG response for collection {CollectionName}", collectionName);

                var responseStream = await _ragService.GenerateStreamingResponseAsync(collectionName, request);

                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                await foreach (var chunk in responseStream)
                {
                    await Response.WriteAsync($"data: {chunk}\n\n");
                    await Response.Body.FlushAsync();
                }
                span.SetAttribute("rag.streaming_completed", true);

                return new ContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate streaming RAG response");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    #endregion

    #region Enterprise Scenarios - 企业场景

    /// <summary>
    /// Enterprise Q&A with domain knowledge
    /// 具有领域知识的企业问答
    /// </summary>
    /// <param name="request">Enterprise Q&A request / 企业问答请求</param>
    /// <returns>Q&A response / 问答响应</returns>
    [HttpPost("enterprise/qa")]
    [SwaggerOperation(
        Summary = "Enterprise Q&A with domain knowledge",
        Description = "Performs Question & Answering using enterprise domain knowledge.",
        OperationId = "EnterpriseQA",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> EnterpriseQA([FromBody] EnterpriseQARequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.EnterpriseQA"))
        {
            span.SetAttribute("rag.knowledge_base", request.KnowledgeBase);
            span.SetAttribute("rag.question_length", request.Question?.Length);
            try
            {
                _logger.LogInformation("Processing enterprise Q&A for knowledge base {KnowledgeBase}",
                    request.KnowledgeBase);

                var response = await _ragService.EnterpriseQAAsync(
                    request.KnowledgeBase,
                    request.Question,
                    request.Options);
                span.SetAttribute("rag.qa_response_length", response.Response?.Length);

                return Ok(ApiResponse<object>.Ok(new
                {
                    response = response,
                    knowledgeBase = request.KnowledgeBase
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process enterprise Q&A");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Document summarization
    /// 文档摘要
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="documentId">Document ID / 文档ID</param>
    /// <param name="options">Summary options / 摘要选项</param>
    /// <returns>Document summary / 文档摘要</returns>
    [HttpPost("collections/{collectionName}/documents/{documentId}/summarize")]
    [SwaggerOperation(
        Summary = "Document summarization",
        Description = "Generates a summary for the specified document.",
        OperationId = "SummarizeDocument",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> SummarizeDocument(string collectionName, string documentId, [FromBody] RagSummaryOptions? options = null)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.SummarizeDocument"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.document_id", documentId);
            try
            {
                _logger.LogInformation("Summarizing document {DocumentId} in collection {CollectionName}",
                    documentId, collectionName);

                var response = await _ragService.DocumentSummarizationAsync(collectionName, documentId, options);
                span.SetAttribute("rag.summary_length", response.Summary?.Length);

                return Ok(ApiResponse<object>.Ok(new
                {
                    summary = response,
                    documentId = documentId
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to summarize document {DocumentId}", documentId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Multi-document analysis
    /// 多文档分析
    /// </summary>
    /// <param name="collectionName">Collection name / 集合名称</param>
    /// <param name="request">Analysis request / 分析请求</param>
    /// <returns>Analysis result / 分析结果</returns>
    [HttpPost("collections/{collectionName}/analyze")]
    [SwaggerOperation(
        Summary = "Multi-document analysis",
        Description = "Analyzes multiple documents in the RAG collection.",
        OperationId = "AnalyzeDocuments",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> AnalyzeDocuments(string collectionName, [FromBody] MultiDocumentAnalysisRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.AnalyzeDocuments"))
        {
            span.SetAttribute("rag.collection_name", collectionName);
            span.SetAttribute("rag.document_ids_count", request.DocumentIds?.Count() ?? 0);
            span.SetAttribute("rag.analysis_query_length", request.AnalysisQuery?.Length);
            try
            {
                _logger.LogInformation("Analyzing {DocumentCount} documents in collection {CollectionName}",
                    request.DocumentIds.Count(), collectionName);

                var response = await _ragService.MultiDocumentAnalysisAsync(
                    collectionName,
                    request.DocumentIds,
                    request.AnalysisQuery);
                span.SetAttribute("rag.analysis_result_length", response.AnalysisResult?.Length);

                return Ok(ApiResponse<object>.Ok(new
                {
                    analysis = response,
                    documentCount = request.DocumentIds.Count()
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze documents");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    #endregion

    #region Knowledge Base Management - 知识库管理

    /// <summary>
    /// Create knowledge base
    /// 创建知识库
    /// </summary>
    /// <param name="request">Creation request / 创建请求</param>
    /// <returns>Result of operation / 操作结果</returns>
    [HttpPost("knowledge-bases")]
    [SwaggerOperation(
        Summary = "Create knowledge base",
        Description = "Creates a new knowledge base with the specified configuration.",
        OperationId = "CreateKnowledgeBase",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> CreateKnowledgeBase([FromBody] CreateKnowledgeBaseRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.CreateKnowledgeBase"))
        {
            span.SetAttribute("rag.knowledge_base_name", request.Name);
            try
            {
                _logger.LogInformation("Creating knowledge base {Name}", request.Name);

                var collectionId = await _ragService.CreateKnowledgeBaseAsync(request.Name, request.Config);
                span.SetAttribute("rag.created_collection_id", collectionId);

                return Ok(ApiResponse<object>.Ok(new
                {
                    collectionId = collectionId,
                    name = request.Name,
                    message = $"Knowledge base \'{request.Name}\' created successfully"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create knowledge base {Name}", request.Name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Delete knowledge base
    /// 删除知识库
    /// </summary>
    /// <param name="name">Knowledge base name / 知识库名称</param>
    /// <returns>Result of operation / 操作结果</returns>
    [HttpDelete("knowledge-bases/{name}")]
    [SwaggerOperation(
        Summary = "Delete knowledge base",
        Description = "Deletes an existing knowledge base.",
        OperationId = "DeleteKnowledgeBase",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteKnowledgeBase(string name)
    {
        using (var span = _telemetryProvider.StartSpan("RagController.DeleteKnowledgeBase"))
        {
            span.SetAttribute("rag.knowledge_base_name", name);
            try
            {
                _logger.LogInformation("Deleting knowledge base {Name}", name);

                await _ragService.DeleteKnowledgeBaseAsync(name);
                span.SetAttribute("rag.delete_success", true);

                return Ok(ApiResponse<object>.Ok(new
                {
                    message = $"Knowledge base \'{name}\' deleted successfully"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete knowledge base {Name}", name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// List all knowledge bases
    /// 列出所有知识库
    /// </summary>
    /// <returns>List of knowledge bases / 知识库列表</returns>
    [HttpGet("knowledge-bases")]
    [SwaggerOperation(
        Summary = "List all knowledge bases",
        Description = "Lists all available knowledge bases.",
        OperationId = "ListKnowledgeBases",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> ListKnowledgeBases()
    {
        using (var span = _telemetryProvider.StartSpan("RagController.ListKnowledgeBases"))
        {
            try
            {
                _logger.LogInformation("Listing all knowledge bases");

                var knowledgeBases = await _ragService.ListKnowledgeBasesAsync();
                span.SetAttribute("rag.knowledge_base_count", knowledgeBases.Count());

                return Ok(ApiResponse<object>.Ok(new
                {
                    knowledgeBases = knowledgeBases,
                    count = knowledgeBases.Count()
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list knowledge bases");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    #endregion

    #region Health Check - 健康检查

    /// <summary>
    /// RAG service health check
    /// RAG服务健康检查
    /// </summary>
    /// <returns>Health status / 健康状态</returns>
    [HttpGet("health")]
    [SwaggerOperation(
        Summary = "RAG service health check",
        Description = "Checks the health status of the RAG service.",
        OperationId = "RagHealthCheck",
        Tags = new[] { "RAG" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<object>>> HealthCheck()
    {
        using (var span = _telemetryProvider.StartSpan("RagController.HealthCheck"))
        {
            try
            {
                // Check if we can list knowledge bases - 检查是否可以列出知识库
                var knowledgeBases = await _ragService.ListKnowledgeBasesAsync();
                span.SetAttribute("rag.health_check_knowledge_base_count", knowledgeBases.Count());

                return Ok(ApiResponse<object>.Ok(new
                {
                    status = "healthy",
                    service = "RAG",
                    knowledgeBaseCount = knowledgeBases.Count(),
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RAG service health check failed");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(503, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    #endregion
}

#region Request/Response Models - 请求/响应模型

/// <summary>
/// Vector retrieval request
/// 向量检索请求
/// </summary>
public class VectorRetrievalRequest
{
    /// <summary>
    /// Query text - 查询文本
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Number of results to return - 返回结果数量
    /// </summary>
    public int TopK { get; set; } = 10;
}

/// <summary>
/// Keyword retrieval request
/// 关键词检索请求
/// </summary>
public class KeywordRetrievalRequest
{
    /// <summary>
    /// Query text - 查询文本
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Number of results to return - 返回结果数量
    /// </summary>
    public int TopK { get; set; } = 10;
}

/// <summary>
/// Semantic retrieval request
/// 语义检索请求
/// </summary>
public class SemanticRetrievalRequest
{
    /// <summary>
    /// Query text - 查询文本
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Number of results to return - 返回结果数量
    /// </summary>
    public int TopK { get; set; } = 10;
}

/// <summary>
/// Enterprise Q&A request
/// 企业问答请求
/// </summary>
public class EnterpriseQARequest
{
    /// <summary>
    /// Knowledge base name - 知识库名称
    /// </summary>
    public string KnowledgeBase { get; set; } = string.Empty;

    /// <summary>
    /// User question - 用户问题
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// RAG options - RAG选项
    /// </summary>
    public RagOptions? Options { get; set; }
}

/// <summary>
/// Multi-document analysis request
/// 多文档分析请求
/// </summary>
public class MultiDocumentAnalysisRequest
{
    /// <summary>
    /// Document IDs to analyze - 要分析的文档ID
    /// </summary>
    public IEnumerable<string> DocumentIds { get; set; } = new List<string>();

    /// <summary>
    /// Analysis query - 分析查询
    /// </summary>
    public string AnalysisQuery { get; set; } = string.Empty;
}

/// <summary>
/// Create knowledge base request
/// 创建知识库请求
/// </summary>
public class CreateKnowledgeBaseRequest
{
    /// <summary>
    /// Knowledge base name - 知识库名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Collection configuration - 集合配置
    /// </summary>
    public RagCollectionConfig Config { get; set; } = new();
}

#endregion


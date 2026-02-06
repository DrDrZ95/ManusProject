namespace Agent.Api.Controllers;

/// <summary>
/// Controller for ChromaDB operations
/// ChromaDB 操作控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class ChromaDbController : ControllerBase
{
    private readonly IChromaDbService _chromaDbService;
    private readonly ILogger<ChromaDbController> _logger;
    private readonly IAgentTelemetryProvider _telemetryProvider;

    public ChromaDbController(IChromaDbService chromaDbService, ILogger<ChromaDbController> logger, IAgentTelemetryProvider telemetryProvider)
    {
        _chromaDbService = chromaDbService ?? throw new ArgumentNullException(nameof(chromaDbService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryProvider = telemetryProvider ?? throw new ArgumentNullException(nameof(telemetryProvider));
    }

    /// <summary>
    /// Get all collections
    /// 获取所有集合
    /// </summary>
    /// <returns>List of collections</returns>
    [HttpGet("collections")]
    [SwaggerOperation(
        Summary = "Get all collections",
        Description = "Retrieves a list of all available ChromaDB collections.",
        OperationId = "GetCollections",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VectorCollection>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VectorCollection>>>> GetCollections()
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.GetCollections"))
        {
            try
            {
                var collections = await _chromaDbService.ListCollectionsAsync();
                span.SetAttribute("chromadb.collection_count", collections.Count());
                return Ok(ApiResponse<IEnumerable<VectorCollection>>.Ok(collections));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get collections");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<IEnumerable<VectorCollection>>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Create a new collection
    /// 创建新集合
    /// </summary>
    /// <param name="request">Collection creation request</param>
    /// <returns>Created collection</returns>
    [HttpPost("collections")]
    [SwaggerOperation(
        Summary = "Create a new collection",
        Description = "Creates a new ChromaDB collection with the specified name and metadata.",
        OperationId = "CreateCollection",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<VectorCollection>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<VectorCollection>>> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.CreateCollection"))
        {
            span.SetAttribute("chromadb.collection_name", request.Name);
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                span.SetStatus(ActivityStatusCode.Error, "Collection name is required");
                return BadRequest(ApiResponse<VectorCollection>.Fail("Collection name is required"));
            }

            try
            {
                var collection = await _chromaDbService.CreateCollectionAsync(request.Name, request.Metadata);
                span.SetAttribute("chromadb.collection_id", collection.Id);
                return CreatedAtAction(nameof(GetCollection), new { name = request.Name, version = "1.0" }, ApiResponse<VectorCollection>.Ok(collection));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create collection: {CollectionName}", request.Name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<VectorCollection>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get a specific collection
    /// 获取特定集合
    /// </summary>
    /// <param name="name">Collection name</param>
    /// <returns>Collection details</returns>
    [HttpGet("collections/{name}")]
    [SwaggerOperation(
        Summary = "Get a specific collection",
        Description = "Retrieves details of a specific ChromaDB collection by name.",
        OperationId = "GetCollection",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<VectorCollection>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<VectorCollection>>> GetCollection(string name)
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.GetCollection"))
        {
            span.SetAttribute("chromadb.collection_name", name);
            try
            {
                var collection = await _chromaDbService.GetCollectionAsync(name);
                span.SetAttribute("chromadb.collection_id", collection.Id);
                return Ok(ApiResponse<VectorCollection>.Ok(collection));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get collection: {CollectionName}", name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<VectorCollection>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Delete a collection
    /// 删除集合
    /// </summary>
    /// <param name="name">Collection name</param>
    /// <returns>Success status</returns>
    [HttpDelete("collections/{name}")]
    [SwaggerOperation(
        Summary = "Delete a collection",
        Description = "Deletes a specific ChromaDB collection by name.",
        OperationId = "DeleteCollection",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCollection(string name)
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.DeleteCollection"))
        {
            span.SetAttribute("chromadb.collection_name", name);
            try
            {
                var success = await _chromaDbService.DeleteCollectionAsync(name);
                span.SetAttribute("chromadb.delete_success", success);
                if (success)
                {
                    return Ok(ApiResponse<object>.Ok(new { message = "Collection deleted successfully" }));
                }
                span.SetStatus(ActivityStatusCode.Error, "Collection not found");
                return NotFound(ApiResponse<object>.Fail("Collection not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete collection: {CollectionName}", name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Add documents to a collection
    /// 向集合添加文档
    /// </summary>
    /// <param name="collectionName">Collection name</param>
    /// <param name="request">Add documents request</param>
    /// <returns>Success status</returns>
    [HttpPost("collections/{collectionName}/documents")]
    [SwaggerOperation(
        Summary = "Add documents to a collection",
        Description = "Adds one or more documents to the specified ChromaDB collection.",
        OperationId = "AddDocuments",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> AddDocuments(string collectionName, [FromBody] AddDocumentsRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.AddDocuments"))
        {
            span.SetAttribute("chromadb.collection_name", collectionName);
            span.SetAttribute("chromadb.document_count", request.Documents?.Count() ?? 0);
            if (request.Documents == null || !request.Documents.Any())
            {
                span.SetStatus(ActivityStatusCode.Error, "Documents are required");
                return BadRequest(ApiResponse<object>.Fail("Documents are required"));
            }

            try
            {
                await _chromaDbService.AddDocumentsAsync(collectionName, request.Documents, request.Ids, request.Metadatas);
                // Return 200 OK with wrapper instead of 201 Created for consistency, or use Created with wrapper
                return Ok(ApiResponse<object>.Ok(new { message = "Documents added successfully" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add documents to collection: {CollectionName}", collectionName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Query documents from a collection
    /// 从集合查询文档
    /// </summary>
    /// <param name="collectionName">Collection name</param>
    /// <param name="request">Query request</param>
    /// <returns>Query results</returns>
    [HttpPost("collections/{collectionName}/query")]
    [SwaggerOperation(
        Summary = "Query documents from a collection",
        Description = "Queries documents from the specified ChromaDB collection based on vector similarity or metadata.",
        OperationId = "QueryDocuments",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<QueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<QueryResponse>>> QueryDocuments(string collectionName, [FromBody] QueryRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.QueryDocuments"))
        {
            span.SetAttribute("chromadb.collection_name", collectionName);
            span.SetAttribute("chromadb.query_text_count", request.QueryTexts?.Count() ?? 0);
            if (request.QueryTexts == null || !request.QueryTexts.Any())
            {
                span.SetStatus(ActivityStatusCode.Error, "Query texts are required");
                return BadRequest(ApiResponse<QueryResponse>.Fail("Query texts are required"));
            }

            try
            {
                var result = await _chromaDbService.QueryAsync(collectionName, request.QueryTexts, request.NResults, request.Where);
                span.SetAttribute("chromadb.result_count", result.Documents?.Count() ?? 0);
                return Ok(ApiResponse<QueryResponse>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query collection: {CollectionName}", collectionName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<QueryResponse>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get documents from a collection
    /// 从集合获取文档
    /// </summary>
    /// <param name="collectionName">Collection name</param>
    /// <param name="ids">Document IDs (optional)</param>
    /// <returns>Documents</returns>
    [HttpGet("collections/{collectionName}/documents")]
    [SwaggerOperation(
        Summary = "Get documents from a collection",
        Description = "Retrieves specific documents from the ChromaDB collection by their IDs.",
        OperationId = "GetDocuments",
        Tags = new[] { "ChromaDB" }
    )]
    [ProducesResponseType(typeof(ApiResponse<GetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetResponse>>> GetDocuments(string collectionName, [FromQuery] string[]? ids = null)
    {
        using (var span = _telemetryProvider.StartSpan("ChromaDbController.GetDocuments"))
        {
            span.SetAttribute("chromadb.collection_name", collectionName);
            span.SetAttribute("chromadb.id_count", ids?.Count() ?? 0);
            try
            {
                var result = await _chromaDbService.GetDocumentsAsync(collectionName, ids);
                span.SetAttribute("chromadb.document_count", result.Documents?.Count() ?? 0);
                return Ok(ApiResponse<GetResponse>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get documents from collection: {CollectionName}", collectionName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<GetResponse>.Fail(ex.Message));
            }
        }
    }
}

// Request/Response DTOs
public class CreateCollectionRequest
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}


public class QueryRequest
{
    public IEnumerable<string> QueryTexts { get; set; } = new List<string>();
    public int NResults { get; set; } = 10;
    public Dictionary<string, object>? Where { get; set; }
}

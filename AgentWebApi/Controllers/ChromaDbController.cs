using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services;
using ChromaDB.Client.Models;

namespace AgentWebApi.Controllers;

/// <summary>
/// Controller for ChromaDB operations
/// ChromaDB 操作控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChromaDbController : ControllerBase
{
    private readonly IChromaDbService _chromaDbService;
    private readonly ILogger<ChromaDbController> _logger;

    public ChromaDbController(IChromaDbService chromaDbService, ILogger<ChromaDbController> logger)
    {
        _chromaDbService = chromaDbService ?? throw new ArgumentNullException(nameof(chromaDbService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all collections
    /// 获取所有集合
    /// </summary>
    /// <returns>List of collections</returns>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(IEnumerable<Collection>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Collection>>> GetCollections()
    {
        try
        {
            var collections = await _chromaDbService.ListCollectionsAsync();
            return Ok(collections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collections");
            return StatusCode(500, new { error = "Failed to retrieve collections", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new collection
    /// 创建新集合
    /// </summary>
    /// <param name="request">Collection creation request</param>
    /// <returns>Created collection</returns>
    [HttpPost("collections")]
    [ProducesResponseType(typeof(Collection), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Collection>> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Collection name is required" });
        }

        try
        {
            var collection = await _chromaDbService.CreateCollectionAsync(request.Name, request.Metadata);
            return CreatedAtAction(nameof(GetCollection), new { name = request.Name }, collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create collection: {CollectionName}", request.Name);
            return StatusCode(500, new { error = "Failed to create collection", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific collection
    /// 获取特定集合
    /// </summary>
    /// <param name="name">Collection name</param>
    /// <returns>Collection details</returns>
    [HttpGet("collections/{name}")]
    [ProducesResponseType(typeof(Collection), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Collection>> GetCollection(string name)
    {
        try
        {
            var collection = await _chromaDbService.GetCollectionAsync(name);
            return Ok(collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection: {CollectionName}", name);
            return StatusCode(500, new { error = "Failed to retrieve collection", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a collection
    /// 删除集合
    /// </summary>
    /// <param name="name">Collection name</param>
    /// <returns>Success status</returns>
    [HttpDelete("collections/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCollection(string name)
    {
        try
        {
            var success = await _chromaDbService.DeleteCollectionAsync(name);
            if (success)
            {
                return NoContent();
            }
            return NotFound(new { error = "Collection not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete collection: {CollectionName}", name);
            return StatusCode(500, new { error = "Failed to delete collection", details = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddDocuments(string collectionName, [FromBody] AddDocumentsRequest request)
    {
        if (request.Documents == null || !request.Documents.Any())
        {
            return BadRequest(new { error = "Documents are required" });
        }

        try
        {
            await _chromaDbService.AddDocumentsAsync(collectionName, request.Documents, request.Ids, request.Metadatas);
            return Created($"api/chromadb/collections/{collectionName}/documents", new { message = "Documents added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add documents to collection: {CollectionName}", collectionName);
            return StatusCode(500, new { error = "Failed to add documents", details = ex.Message });
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
    [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QueryResponse>> QueryDocuments(string collectionName, [FromBody] QueryRequest request)
    {
        if (request.QueryTexts == null || !request.QueryTexts.Any())
        {
            return BadRequest(new { error = "Query texts are required" });
        }

        try
        {
            var result = await _chromaDbService.QueryAsync(collectionName, request.QueryTexts, request.NResults, request.Where);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query collection: {CollectionName}", collectionName);
            return StatusCode(500, new { error = "Failed to query documents", details = ex.Message });
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
    [ProducesResponseType(typeof(GetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetResponse>> GetDocuments(string collectionName, [FromQuery] string[]? ids = null)
    {
        try
        {
            var result = await _chromaDbService.GetDocumentsAsync(collectionName, ids);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents from collection: {CollectionName}", collectionName);
            return StatusCode(500, new { error = "Failed to get documents", details = ex.Message });
        }
    }
}

// Request/Response DTOs
public class CreateCollectionRequest
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AddDocumentsRequest
{
    public IEnumerable<string> Documents { get; set; } = new List<string>();
    public IEnumerable<string>? Ids { get; set; }
    public IEnumerable<Dictionary<string, object>>? Metadatas { get; set; }
}

public class QueryRequest
{
    public IEnumerable<string> QueryTexts { get; set; } = new List<string>();
    public int NResults { get; set; } = 10;
    public Dictionary<string, object>? Where { get; set; }
}


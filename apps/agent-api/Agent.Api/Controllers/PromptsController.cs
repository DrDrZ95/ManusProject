namespace Agent.Api.Controllers;

/// <summary>
/// Prompts management controller
/// 提示词管理控制器
/// 
/// 提供动态提示词模板库的管理和访问功能
/// Provides management and access functionality for dynamic prompt template library
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class PromptsController : ControllerBase
{
    private readonly IPromptsService _promptsService;
    private readonly ILogger<PromptsController> _logger;

    public PromptsController(IPromptsService promptsService, ILogger<PromptsController> logger)
    {
        _promptsService = promptsService ?? throw new ArgumentNullException(nameof(promptsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all available prompt categories
    /// 获取所有可用的提示词类别
    /// </summary>
    /// <returns>List of categories - 类别列表</returns>
    [HttpGet("categories")]
    [SwaggerOperation(
        Summary = "Get all available prompt categories",
        Description = "Retrieves a list of all available prompt categories.",
        OperationId = "GetPromptCategories",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCategories()
    {
        try
        {
            _logger.LogInformation("Getting all prompt categories");
            var categories = await _promptsService.GetCategoriesAsync();
            return Ok(ApiResponse<List<string>>.Ok(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt categories");
            return StatusCode(500, ApiResponse<List<string>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get prompts by category
    /// 根据类别获取提示词
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <returns>List of prompts - 提示词列表</returns>
    [HttpGet("category/{category}")]
    [SwaggerOperation(
        Summary = "Get prompts by category",
        Description = "Retrieves all prompts within a specific category.",
        OperationId = "GetPromptsByCategory",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<PromptTemplate>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<PromptTemplate>>>> GetPromptsByCategory(string category)
    {
        try
        {
            _logger.LogInformation("Getting prompts for category: {Category}", category);
            var prompts = await _promptsService.GetPromptsByCategoryAsync(category);
            return Ok(ApiResponse<List<PromptTemplate>>.Ok(prompts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompts for category: {Category}", category);
            return StatusCode(500, ApiResponse<List<PromptTemplate>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get specific prompt template
    /// 获取特定的提示词模板
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <param name="name">Prompt name - 提示词名称</param>
    /// <returns>Prompt template - 提示词模板</returns>
    [HttpGet("{category}/{name}")]
    [SwaggerOperation(
        Summary = "Get specific prompt template",
        Description = "Retrieves a specific prompt template by category and name.",
        OperationId = "GetPrompt",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<PromptTemplate>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PromptTemplate>>> GetPrompt(string category, string name)
    {
        try
        {
            _logger.LogInformation("Getting prompt: {Category}/{Name}", category, name);
            var prompt = await _promptsService.GetPromptAsync(category, name);
            
            if (prompt == null)
            {
                return NotFound(ApiResponse<PromptTemplate>.Fail($"Prompt not found: {category}/{name}"));
            }

            return Ok(ApiResponse<PromptTemplate>.Ok(prompt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt: {Category}/{Name}", category, name);
            return StatusCode(500, ApiResponse<PromptTemplate>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Search prompts by keywords
    /// 根据关键词搜索提示词
    /// </summary>
    /// <param name="keywords">Search keywords - 搜索关键词</param>
    /// <returns>List of matching prompts - 匹配的提示词列表</returns>
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Search prompts by keywords",
        Description = "Searches for prompts using keywords.",
        OperationId = "SearchPrompts",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<PromptTemplate>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<PromptTemplate>>>> SearchPrompts([FromQuery] string keywords)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return BadRequest(ApiResponse<List<PromptTemplate>>.Fail("Keywords parameter is required"));
            }

            _logger.LogInformation("Searching prompts with keywords: {Keywords}", keywords);
            var prompts = await _promptsService.SearchPromptsAsync(keywords);
            return Ok(ApiResponse<List<PromptTemplate>>.Ok(prompts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching prompts with keywords: {Keywords}", keywords);
            return StatusCode(500, ApiResponse<List<PromptTemplate>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Render prompt with variables
    /// 使用变量渲染提示词
    /// </summary>
    /// <param name="request">Render request - 渲染请求</param>
    /// <returns>Rendered prompt - 渲染后的提示词</returns>
    [HttpPost("render")]
    [SwaggerOperation(
        Summary = "Render prompt with variables",
        Description = "Renders a prompt template with the provided variables.",
        OperationId = "RenderPrompt",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> RenderPrompt([FromBody] RenderPromptRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<string>.Fail("Category and Name are required"));
            }

            _logger.LogInformation("Rendering prompt: {Category}/{Name}", request.Category, request.Name);
            
            var template = await _promptsService.GetPromptAsync(request.Category, request.Name);
            if (template == null)
            {
                return NotFound(ApiResponse<string>.Fail($"Prompt not found: {request.Category}/{request.Name}"));
            }

            var rendered = _promptsService.RenderPrompt(template, request.Variables ?? new Dictionary<string, object>());
            return Ok(ApiResponse<string>.Ok(rendered));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering prompt: {Category}/{Name}", request?.Category, request?.Name);
            return StatusCode(500, ApiResponse<string>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Save or update prompt template
    /// 保存或更新提示词模板
    /// </summary>
    /// <param name="template">Prompt template - 提示词模板</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Save or update prompt template",
        Description = "Saves a new prompt template or updates an existing one.",
        OperationId = "SavePrompt",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> SavePrompt([FromBody] PromptTemplate template)
    {
        try
        {
            if (template == null || string.IsNullOrWhiteSpace(template.Category) || string.IsNullOrWhiteSpace(template.Name))
            {
                return BadRequest(ApiResponse<bool>.Fail("Template with valid Category and Name is required"));
            }

            _logger.LogInformation("Saving prompt: {Category}/{Name}", template.Category, template.Name);
            var success = await _promptsService.SavePromptAsync(template);
            
            if (success)
            {
                return Ok(ApiResponse<bool>.Ok(true));
            }
            else
            {
                return StatusCode(500, ApiResponse<bool>.Fail("Failed to save prompt"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving prompt: {Category}/{Name}", template?.Category, template?.Name);
            return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Delete prompt template
    /// 删除提示词模板
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <param name="name">Prompt name - 提示词名称</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpDelete("{category}/{name}")]
    [SwaggerOperation(
        Summary = "Delete prompt template",
        Description = "Deletes a prompt template by category and name.",
        OperationId = "DeletePrompt",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePrompt(string category, string name)
    {
        try
        {
            _logger.LogInformation("Deleting prompt: {Category}/{Name}", category, name);
            var success = await _promptsService.DeletePromptAsync(category, name);
            
            if (success)
            {
                return Ok(ApiResponse<bool>.Ok(true));
            }
            else
            {
                return NotFound(ApiResponse<bool>.Fail($"Prompt not found: {category}/{name}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt: {Category}/{Name}", category, name);
            return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get all available tool types
    /// 获取所有可用的工具类型
    /// </summary>
    /// <returns>List of tool types - 工具类型列表</returns>
    [HttpGet("tools/types")]
    [SwaggerOperation(
        Summary = "Get all available tool types",
        Description = "Retrieves a list of all available tool types.",
        OperationId = "GetToolTypes",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetToolTypes()
    {
        try
        {
            _logger.LogInformation("Getting all tool types");
            var toolTypes = await _promptsService.GetToolTypesAsync();
            return Ok(ApiResponse<List<string>>.Ok(toolTypes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tool types");
            return StatusCode(500, ApiResponse<List<string>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get tool examples by type
    /// 根据类型获取工具示例
    /// </summary>
    /// <param name="toolType">Tool type - 工具类型</param>
    /// <returns>List of tool examples - 工具示例列表</returns>
    [HttpGet("tools/{toolType}")]
    [SwaggerOperation(
        Summary = "Get tool examples by type",
        Description = "Retrieves tool examples for a specific tool type.",
        OperationId = "GetToolExamples",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<ToolExample>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ToolExample>>>> GetToolExamples(string toolType)
    {
        try
        {
            _logger.LogInformation("Getting tool examples for type: {ToolType}", toolType);
            var examples = await _promptsService.GetToolExamplesAsync(toolType);
            return Ok(ApiResponse<List<ToolExample>>.Ok(examples));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tool examples for type: {ToolType}", toolType);
            return StatusCode(500, ApiResponse<List<ToolExample>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get prompt template library statistics
    /// 获取提示词模板库统计信息
    /// </summary>
    /// <returns>Library statistics - 库统计信息</returns>
    [HttpGet("statistics")]
    [SwaggerOperation(
        Summary = "Get prompt template library statistics",
        Description = "Retrieves statistics about the prompt template library.",
        OperationId = "GetPromptStatistics",
        Tags = new[] { "Prompts" }
    )]
    [ProducesResponseType(typeof(ApiResponse<PromptLibraryStatistics>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PromptLibraryStatistics>>> GetStatistics()
    {
        try
        {
            _logger.LogInformation("Getting prompt library statistics");
            
            var categories = await _promptsService.GetCategoriesAsync();
            var toolTypes = await _promptsService.GetToolTypesAsync();
            
            var statistics = new PromptLibraryStatistics
            {
                TotalCategories = categories.Count,
                TotalToolTypes = toolTypes.Count,
                CategoryDetails = new Dictionary<string, int>(),
                ToolTypeDetails = new Dictionary<string, int>()
            };

            // 统计每个类别的提示词数量 - Count prompts in each category
            foreach (var category in categories)
            {
                var prompts = await _promptsService.GetPromptsByCategoryAsync(category);
                statistics.CategoryDetails[category] = prompts.Count;
                statistics.TotalPrompts += prompts.Count;
            }

            // 统计每个工具类型的示例数量 - Count examples in each tool type
            foreach (var toolType in toolTypes)
            {
                var examples = await _promptsService.GetToolExamplesAsync(toolType);
                statistics.ToolTypeDetails[toolType] = examples.Count;
                statistics.TotalToolExamples += examples.Count;
            }

            return Ok(ApiResponse<PromptLibraryStatistics>.Ok(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt library statistics");
            return StatusCode(500, ApiResponse<PromptLibraryStatistics>.Fail("Internal server error"));
        }
    }
}

/// <summary>
/// Request model for rendering prompts
/// 渲染提示词的请求模型
/// </summary>
public class RenderPromptRequest
{
    /// <summary>
    /// Prompt category - 提示词类别
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Prompt name - 提示词名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template variables - 模板变量
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();
}

/// <summary>
/// Prompt library statistics model
/// 提示词库统计信息模型
/// </summary>
public class PromptLibraryStatistics
{
    /// <summary>
    /// Total number of categories - 总类别数
    /// </summary>
    public int TotalCategories { get; set; }

    /// <summary>
    /// Total number of prompts - 总提示词数
    /// </summary>
    public int TotalPrompts { get; set; }

    /// <summary>
    /// Total number of tool types - 总工具类型数
    /// </summary>
    public int TotalToolTypes { get; set; }

    /// <summary>
    /// Total number of tool examples - 总工具示例数
    /// </summary>
    public int TotalToolExamples { get; set; }

    /// <summary>
    /// Prompts count by category - 按类别统计的提示词数量
    /// </summary>
    public Dictionary<string, int> CategoryDetails { get; set; } = new();

    /// <summary>
    /// Examples count by tool type - 按工具类型统计的示例数量
    /// </summary>
    public Dictionary<string, int> ToolTypeDetails { get; set; } = new();
}

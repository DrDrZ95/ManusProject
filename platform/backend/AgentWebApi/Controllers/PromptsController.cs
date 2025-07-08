using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services.Prompts;
using AgentWebApi.Services.Telemetry;
using System.Diagnostics;

namespace AgentWebApi.Controllers;

/// <summary>
/// Professional prompts management controller
/// 专业提示词管理控制器
/// 
/// 提供AI-Agent系统中提示词模板和专业工具示例的API接口
/// Provides API endpoints for prompt templates and professional tool examples in AI-Agent system
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PromptsController : ControllerBase
{
    private readonly IPromptsService _promptsService;
    private readonly ILogger<PromptsController> _logger;
    private readonly IAgentTelemetryProvider _telemetryProvider;

    public PromptsController(IPromptsService promptsService, ILogger<PromptsController> logger, IAgentTelemetryProvider telemetryProvider)
    {
        _promptsService = promptsService ?? throw new ArgumentNullException(nameof(promptsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryProvider = telemetryProvider ?? throw new ArgumentNullException(nameof(telemetryProvider));
    }

    /// <summary>
    /// Get all available prompt categories
    /// 获取所有可用的提示词类别
    /// </summary>
    /// <returns>List of prompt categories - 提示词类别列表</returns>
    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.GetCategories"))
        {
            try
            {
                _logger.LogInformation("Getting all prompt categories");
                var categories = await _promptsService.GetCategoriesAsync();
                span.SetAttribute("prompt.category_count", categories.Count);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prompt categories");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Get prompts by category
    /// 根据类别获取提示词
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <returns>List of prompts in the category - 该类别下的提示词列表</returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<PromptTemplate>>> GetPromptsByCategory(string category)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.GetPromptsByCategory"))
        {
            span.SetAttribute("prompt.category", category);
            try
            {
                _logger.LogInformation("Getting prompts for category: {Category}", category);
                
                if (string.IsNullOrWhiteSpace(category))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Category cannot be empty");
                    return BadRequest(new { error = "Category cannot be empty" });
                }

                var prompts = await _promptsService.GetPromptsByCategoryAsync(category);
                span.SetAttribute("prompt.count", prompts.Count);
                return Ok(prompts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prompts for category: {Category}", category);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
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
    public async Task<ActionResult<PromptTemplate>> GetPrompt(string category, string name)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.GetPrompt"))
        {
            span.SetAttribute("prompt.category", category);
            span.SetAttribute("prompt.name", name);
            try
            {
                _logger.LogInformation("Getting prompt: {Category}/{Name}", category, name);
                
                if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(name))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Category and name cannot be empty");
                    return BadRequest(new { error = "Category and name cannot be empty" });
                }

                var prompt = await _promptsService.GetPromptAsync(category, name);
                if (prompt == null)
                {
                    span.SetStatus(ActivityStatusCode.Error, "Prompt not found");
                    return NotFound(new { error = "Prompt not found", category, name });
                }

                return Ok(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prompt: {Category}/{Name}", category, name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Render prompt with variables
    /// 使用变量渲染提示词
    /// </summary>
    /// <param name="request">Render request - 渲染请求</param>
    /// <returns>Rendered prompt - 渲染后的提示词</returns>
    [HttpPost("render")]
    public async Task<ActionResult<RenderPromptResponse>> RenderPrompt([FromBody] RenderPromptRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.RenderPrompt"))
        {
            span.SetAttribute("prompt.category", request.Category);
            span.SetAttribute("prompt.name", request.Name);
            try
            {
                _logger.LogInformation("Rendering prompt: {Category}/{Name}", request.Category, request.Name);
                
                if (string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.Name))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Category and name are required");
                    return BadRequest(new { error = "Category and name are required" });
                }

                // 获取提示词模板 - Get prompt template
                var template = await _promptsService.GetPromptAsync(request.Category, request.Name);
                if (template == null)
                {
                    span.SetStatus(ActivityStatusCode.Error, "Prompt template not found");
                    return NotFound(new { error = "Prompt template not found", request.Category, request.Name });
                }

                // 渲染提示词 - Render prompt
                var renderedPrompt = _promptsService.RenderPrompt(template, request.Variables ?? new Dictionary<string, object>());
                
                var response = new RenderPromptResponse
                {
                    RenderedPrompt = renderedPrompt,
                    Template = template,
                    Variables = request.Variables ?? new Dictionary<string, object>()
                };
                span.SetAttribute("prompt.rendered_length", renderedPrompt.Length);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering prompt: {Category}/{Name}", request.Category, request.Name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Search prompts by keywords
    /// 根据关键词搜索提示词
    /// </summary>
    /// <param name="keywords">Search keywords - 搜索关键词</param>
    /// <returns>List of matching prompts - 匹配的提示词列表</returns>
    [HttpGet("search")]
    public async Task<ActionResult<List<PromptTemplate>>> SearchPrompts([FromQuery] string keywords)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.SearchPrompts"))
        {
            span.SetAttribute("prompt.search_keywords", keywords);
            try
            {
                _logger.LogInformation("Searching prompts with keywords: {Keywords}", keywords);
                
                if (string.IsNullOrWhiteSpace(keywords))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Keywords cannot be empty");
                    return BadRequest(new { error = "Keywords cannot be empty" });
                }

                var prompts = await _promptsService.SearchPromptsAsync(keywords);
                span.SetAttribute("prompt.search_result_count", prompts.Count);
                return Ok(prompts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching prompts with keywords: {Keywords}", keywords);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Save or update prompt template
    /// 保存或更新提示词模板
    /// </summary>
    /// <param name="template">Prompt template - 提示词模板</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost]
    public async Task<ActionResult> SavePrompt([FromBody] PromptTemplate template)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.SavePrompt"))
        {
            span.SetAttribute("prompt.category", template.Category);
            span.SetAttribute("prompt.name", template.Name);
            try
            {
                _logger.LogInformation("Saving prompt: {Category}/{Name}", template.Category, template.Name);
                
                if (string.IsNullOrWhiteSpace(template.Category) || string.IsNullOrWhiteSpace(template.Name))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Category and name are required");
                    return BadRequest(new { error = "Category and name are required" });
                }

                var success = await _promptsService.SavePromptAsync(template);
                span.SetAttribute("prompt.save_success", success);
                if (success)
                {
                    return Ok(new { message = "Prompt saved successfully", category = template.Category, name = template.Name });
                }
                else
                {
                    span.SetStatus(ActivityStatusCode.Error, "Failed to save prompt");
                    return StatusCode(500, new { error = "Failed to save prompt" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving prompt: {Category}/{Name}", template.Category, template.Name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
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
    public async Task<ActionResult> DeletePrompt(string category, string name)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.DeletePrompt"))
        {
            span.SetAttribute("prompt.category", category);
            span.SetAttribute("prompt.name", name);
            try
            {
                _logger.LogInformation("Deleting prompt: {Category}/{Name}", category, name);
                
                if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(name))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Category and name cannot be empty");
                    return BadRequest(new { error = "Category and name cannot be empty" });
                }

                var success = await _promptsService.DeletePromptAsync(category, name);
                span.SetAttribute("prompt.delete_success", success);
                if (success)
                {
                    return Ok(new { message = "Prompt deleted successfully", category, name });
                }
                else
                {
                    span.SetStatus(ActivityStatusCode.Error, "Prompt not found");
                    return NotFound(new { error = "Prompt not found", category, name });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prompt: {Category}/{Name}", category, name);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Get all available tool types
    /// 获取所有可用的工具类型
    /// </summary>
    /// <returns>List of tool types - 工具类型列表</returns>
    [HttpGet("tools/types")]
    public async Task<ActionResult<List<string>>> GetToolTypes()
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.GetToolTypes"))
        {
            try
            {
                _logger.LogInformation("Getting all tool types");
                var toolTypes = await _promptsService.GetToolTypesAsync();
                span.SetAttribute("tool.type_count", toolTypes.Count);
                return Ok(toolTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tool types");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Get professional tool examples
    /// 获取专业工具示例
    /// </summary>
    /// <param name="toolType">Tool type - 工具类型</param>
    /// <returns>List of tool examples - 工具示例列表</returns>
    [HttpGet("tools/{toolType}")]
    public async Task<ActionResult<List<ToolExample>>> GetToolExamples(string toolType)
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.GetToolExamples"))
        {
            span.SetAttribute("tool.type", toolType);
            try
            {
                _logger.LogInformation("Getting tool examples for type: {ToolType}", toolType);
                
                if (string.IsNullOrWhiteSpace(toolType))
                {
                    span.SetStatus(ActivityStatusCode.Error, "Tool type cannot be empty");
                    return BadRequest(new { error = "Tool type cannot be empty" });
                }

                var examples = await _promptsService.GetToolExamplesAsync(toolType);
                span.SetAttribute("tool.example_count", examples.Count);
                return Ok(examples);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tool examples for type: {ToolType}", toolType);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Get comprehensive prompt and tool information
    /// 获取综合的提示词和工具信息
    /// </summary>
    /// <returns>Complete system information - 完整的系统信息</returns>
    [HttpGet("system-info")]
    public async Task<ActionResult<SystemInfoResponse>> GetSystemInfo()
    {
        using (var span = _telemetryProvider.StartSpan("PromptsController.GetSystemInfo"))
        {
            try
            {
                _logger.LogInformation("Getting comprehensive system information");
                
                var categories = await _promptsService.GetCategoriesAsync();
                var toolTypes = await _promptsService.GetToolTypesAsync();
                
                var response = new SystemInfoResponse
                {
                    PromptCategories = categories,
                    ToolTypes = toolTypes,
                    SystemName = "AI-Agent Professional Prompts System",
                    Version = "1.0.0",
                    Description = "Comprehensive prompt management and professional tool examples for AI-Agent system",
                    Features = new List<string>
                    {
                        "RAG-optimized prompts for document analysis",
                        "Workflow planning and task breakdown templates",
                        "Code generation and API documentation prompts",
                        "Professional tool examples for data analysis",
                        "Machine learning model development guides",
                        "Cybersecurity assessment templates",
                        "DevOps automation and containerization examples"
                    }
                };
                span.SetAttribute("system.prompt_category_count", categories.Count);
                span.SetAttribute("system.tool_type_count", toolTypes.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system information");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
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
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Response model for rendered prompts
/// 渲染提示词的响应模型
/// </summary>
public class RenderPromptResponse
{
    /// <summary>
    /// Rendered prompt text - 渲染后的提示词文本
    /// </summary>
    public string RenderedPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Original template - 原始模板
    /// </summary>
    public PromptTemplate Template { get; set; } = new();

    /// <summary>
    /// Variables used in rendering - 渲染中使用的变量
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();
}

/// <summary>
/// System information response model
/// 系统信息响应模型
/// </summary>
public class SystemInfoResponse
{
    /// <summary>
    /// Available prompt categories - 可用的提示词类别
    /// </summary>
    public List<string> PromptCategories { get; set; } = new();

    /// <summary>
    /// Available tool types - 可用的工具类型
    /// </summary>
    public List<string> ToolTypes { get; set; } = new();

    /// <summary>
    /// System name - 系统名称
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// System version - 系统版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// System description - 系统描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// System features - 系统特性
    /// </summary>
    public List<string> Features { get; set; } = new();
}



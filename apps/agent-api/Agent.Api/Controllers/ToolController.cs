namespace Agent.Api.Controllers;

using Agent.Core.Authorization;
using Agent.Core.Data.Entities;
using Agent.Core.Tools.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Tool Registry management controller
/// 工具注册中心管理控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class ToolController : ControllerBase
{
    private readonly IToolRegistryService _registryService;
    private readonly ISmartToolSelector _selector;
    private readonly ILogger<ToolController> _logger;

    public ToolController(
        IToolRegistryService registryService,
        ISmartToolSelector selector,
        ILogger<ToolController> logger)
    {
        _registryService = registryService;
        _selector = selector;
        _logger = logger;
    }

    /// <summary>
    /// Register a new tool
    /// 注册新工具
    /// </summary>
    [HttpPost]
    [RequirePermission("tool.create")]
    [SwaggerOperation(Summary = "Register a new tool", Tags = new[] { "ToolRegistry" })]
    public async Task<ActionResult<ToolMetadataEntity>> RegisterTool([FromBody] ToolMetadataEntity metadata)
    {
        try
        {
            var result = await _registryService.RegisterToolAsync(metadata);
            return CreatedAtAction(nameof(GetTool), new { version = "1.0", name = result.Name }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering tool");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get tool by name
    /// 获取工具信息
    /// </summary>
    [HttpGet("{name}")]
    [RequirePermission("tool.read")]
    [SwaggerOperation(Summary = "Get tool by name", Tags = new[] { "ToolRegistry" })]
    public async Task<ActionResult<ToolMetadataEntity>> GetTool(string name, [FromQuery] string? version = null)
    {
        var tool = await _registryService.GetToolAsync(name, version);
        if (tool == null) return NotFound();
        return Ok(tool);
    }

    /// <summary>
    /// Get all active tools
    /// 获取所有活跃工具
    /// </summary>
    [HttpGet]
    [RequirePermission("tool.read")]
    [SwaggerOperation(Summary = "Get all active tools", Tags = new[] { "ToolRegistry" })]
    public async Task<ActionResult<IEnumerable<ToolMetadataEntity>>> GetActiveTools()
    {
        var tools = await _registryService.GetActiveToolsAsync();
        return Ok(tools);
    }

    /// <summary>
    /// Recommend tools based on task
    /// 基于任务推荐工具
    /// </summary>
    [HttpPost("recommend")]
    [RequirePermission("tool.read")]
    [SwaggerOperation(Summary = "Recommend tools based on task", Tags = new[] { "ToolRegistry" })]
    public async Task<ActionResult<IEnumerable<ToolMetadataEntity>>> RecommendTools([FromBody] string taskDescription)
    {
        var tools = await _selector.RecommendToolsAsync(taskDescription);
        return Ok(tools);
    }

    /// <summary>
    /// Trigger hot-load
    /// 触发工具热加载
    /// </summary>
    [HttpPost("hot-load")]
    [RequirePermission("system.admin")]
    [SwaggerOperation(Summary = "Trigger hot-load", Tags = new[] { "ToolRegistry" })]
    public async Task<IActionResult> HotLoad()
    {
        await _registryService.HotLoadToolsAsync();
        return Ok("Hot-load initiated");
    }
}

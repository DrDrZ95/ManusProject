namespace Agent.Api.Controllers;

/// <summary>
/// eBPF Detective Controller
/// eBPF 侦探控制器
/// 
/// Provides eBPF-related API endpoints for system monitoring and diagnostics
/// 提供eBPF相关的API端点，用于系统监控和诊断
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class eBPFController : ControllerBase
{
    private readonly IeBPFDetectiveService _ebpfService;
    private readonly ILogger<eBPFController> _logger;

    public eBPFController(IeBPFDetectiveService ebpfService, ILogger<eBPFController> logger)
    {
        _ebpfService = ebpfService;
        _logger = logger;
    }

    /// <summary>
    /// Run a specified bpftrace script
    /// 运行指定的bpftrace脚本
    /// </summary>
    /// <param name="scriptName">Script name - 脚本名称</param>
    /// <param name="args">Script arguments - 脚本参数</param>
    /// <returns>Script output - 脚本输出</returns>
    [HttpGet("script/{scriptName}")]
    [SwaggerOperation(
        Summary = "Run bpftrace script",
        Description = "Executes a specified bpftrace script with optional arguments.",
        OperationId = "RunBpftraceScript",
        Tags = new[] { "eBPF" }
    )]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> RunScript(string scriptName, [FromQuery] string args = "")
    {
        try
        {
            _logger.LogInformation("Running eBPF script: {ScriptName} with args: {Args}", scriptName, args);
            var output = await _ebpfService.RunBpftraceScriptAsync(scriptName, args);
            return Ok(ApiResponse<string>.Ok(output));
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "eBPF script not found: {ScriptName}", scriptName);
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to run eBPF script: {ScriptName}", scriptName);
            return StatusCode(500, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while running eBPF script: {ScriptName}", scriptName);
            return StatusCode(500, ApiResponse<string>.Fail("An unexpected error occurred."));
        }
    }

    /// <summary>
    /// Get current CPU usage
    /// 获取当前CPU使用率
    /// </summary>
    /// <returns>CPU usage percentage - CPU使用率百分比</returns>
    [HttpGet("cpu-usage")]
    [SwaggerOperation(
        Summary = "Get CPU usage",
        Description = "Retrieves the current system CPU usage percentage.",
        OperationId = "GetCpuUsage",
        Tags = new[] { "eBPF" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetCpuUsage()
    {
        try
        {
            var cpuUsage = await _ebpfService.GetCpuUsageAsync();
            return Ok(ApiResponse<object>.Ok(new { CpuUsage = cpuUsage }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage.");
            return StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve CPU usage."));
        }
    }

    /// <summary>
    /// Get current memory usage
    /// 获取当前内存使用率
    /// </summary>
    /// <returns>Memory usage percentage - 内存使用率百分比</returns>
    [HttpGet("memory-usage")]
    [SwaggerOperation(
        Summary = "Get memory usage",
        Description = "Retrieves the current system memory usage percentage.",
        OperationId = "GetMemoryUsage",
        Tags = new[] { "eBPF" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetMemoryUsage()
    {
        try
        {
            var memoryUsage = await _ebpfService.GetMemoryUsageAsync();
            return Ok(ApiResponse<object>.Ok(new { MemoryUsage = memoryUsage }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory usage.");
            return StatusCode(500, ApiResponse<object>.Fail("Failed to retrieve memory usage."));
        }
    }

    /// <summary>
    /// Monitor network activity (streaming output)
    /// 监控网络活动 (流式输出)
    /// </summary>
    /// <param name="durationSeconds">Monitoring duration (seconds) - 监控持续时间（秒）</param>
    /// <returns>Stream of network activity data - 网络活动数据流</returns>
    [HttpGet("monitor/network")]
    [SwaggerOperation(
        Summary = "Monitor network activity",
        Description = "Streams real-time network activity data for a specified duration.",
        OperationId = "MonitorNetworkActivity",
        Tags = new[] { "eBPF" }
    )]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<string> MonitorNetworkActivity([FromQuery] int durationSeconds = 10)
    {
        _logger.LogInformation("Monitoring network activity for {Duration} seconds.", durationSeconds);
        await foreach (var data in _ebpfService.MonitorNetworkActivityAsync(durationSeconds))
        {
            yield return data;
        }
    }

    /// <summary>
    /// Monitor process activity (streaming output)
    /// 监控进程活动 (流式输出)
    /// </summary>
    /// <param name="processName">Process name - 进程名称</param>
    /// <param name="durationSeconds">Monitoring duration (seconds) - 监控持续时间（秒）</param>
    /// <returns>Stream of process activity data - 进程活动数据流</returns>
    [HttpGet("monitor/process/{processName}")]
    [SwaggerOperation(
        Summary = "Monitor process activity",
        Description = "Streams real-time process activity data for a specified process.",
        OperationId = "MonitorProcessActivity",
        Tags = new[] { "eBPF" }
    )]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<string> MonitorProcessActivity(string processName, [FromQuery] int durationSeconds = 10)
    {
        _logger.LogInformation("Monitoring process activity for {ProcessName} for {Duration} seconds.", processName, durationSeconds);
        await foreach (var data in _ebpfService.MonitorProcessActivityAsync(processName, durationSeconds))
        {
            yield return data;
        }
    }
}


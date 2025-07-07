using AgentWebApi.eBPF.Detective;
using Microsoft.AspNetCore.Mvc;

namespace AgentWebApi.eBPF.Controllers;

/// <summary>
/// eBPF 侦探控制器 - eBPF Detective Controller
/// 提供eBPF相关的API端点 - Provides eBPF-related API endpoints
/// </summary>
[ApiController]
[Route("api/ebpf")]
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
    /// 运行指定的bpftrace脚本 - Run a specified bpftrace script
    /// </summary>
    /// <param name="scriptName">脚本名称 - Script name</param>
    /// <param name="args">脚本参数 - Script arguments</param>
    /// <returns>脚本输出 - Script output</returns>
    [HttpGet("script/{scriptName}")]
    public async Task<IActionResult> RunScript(string scriptName, [FromQuery] string args = "")
    {
        try
        {
            _logger.LogInformation("Running eBPF script: {ScriptName} with args: {Args}", scriptName, args);
            var output = await _ebpfService.RunBpftraceScriptAsync(scriptName, args);
            return Ok(output);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "eBPF script not found: {ScriptName}", scriptName);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to run eBPF script: {ScriptName}", scriptName);
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while running eBPF script: {ScriptName}", scriptName);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// 获取当前CPU使用率 - Get current CPU usage
    /// </summary>
    /// <returns>CPU使用率百分比 - CPU usage percentage</returns>
    [HttpGet("cpu-usage")]
    public async Task<IActionResult> GetCpuUsage()
    {
        try
        {
            var cpuUsage = await _ebpfService.GetCpuUsageAsync();
            return Ok(new { CpuUsage = cpuUsage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage.");
            return StatusCode(500, "Failed to retrieve CPU usage.");
        }
    }

    /// <summary>
    /// 获取当前内存使用率 - Get current memory usage
    /// </summary>
    /// <returns>内存使用率百分比 - Memory usage percentage</returns>
    [HttpGet("memory-usage")]
    public async Task<IActionResult> GetMemoryUsage()
    {
        try
        {
            var memoryUsage = await _ebpfService.GetMemoryUsageAsync();
            return Ok(new { MemoryUsage = memoryUsage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory usage.");
            return StatusCode(500, "Failed to retrieve memory usage.");
        }
    }

    /// <summary>
    /// 监控网络活动 (流式输出) - Monitor network activity (streaming output)
    /// </summary>
    /// <param name="durationSeconds">监控持续时间（秒） - Monitoring duration (seconds)</param>
    /// <returns>网络活动数据流 - Stream of network activity data</returns>
    [HttpGet("monitor/network")]
    public async IAsyncEnumerable<string> MonitorNetworkActivity([FromQuery] int durationSeconds = 10)
    {
        _logger.LogInformation("Monitoring network activity for {Duration} seconds.", durationSeconds);
        await foreach (var data in _ebpfService.MonitorNetworkActivityAsync(durationSeconds))
        {
            yield return data;
        }
    }

    /// <summary>
    /// 监控进程活动 (流式输出) - Monitor process activity (streaming output)
    /// </summary>
    /// <param name="processName">进程名称 - Process name</param>
    /// <param name="durationSeconds">监控持续时间（秒） - Monitoring duration (seconds)</param>
    /// <returns>进程活动数据流 - Stream of process activity data</returns>
    [HttpGet("monitor/process/{processName}")]
    public async IAsyncEnumerable<string> MonitorProcessActivity(string processName, [FromQuery] int durationSeconds = 10)
    {
        _logger.LogInformation("Monitoring process activity for {ProcessName} for {Duration} seconds.", processName, durationSeconds);
        await foreach (var data in _ebpfService.MonitorProcessActivityAsync(processName, durationSeconds))
        {
            yield return data;
        }
    }
}



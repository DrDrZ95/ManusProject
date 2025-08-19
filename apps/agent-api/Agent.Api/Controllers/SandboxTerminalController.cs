using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Agent.Api.Controllers;

/// <summary>
/// Sandbox Terminal Controller
/// 沙盒终端控制器
/// 
/// 提供本地沙盒环境的终端操作API
/// Provides terminal operation APIs for local sandbox environment
/// 
/// 基于AI-Agent项目的sandbox.py和terminal.py功能转换
/// Converted from AI-Agent project's sandbox.py and terminal.py functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SandboxTerminalController : ControllerBase
{
    private readonly ISandboxTerminalService _terminalService;
    private readonly ILogger<SandboxTerminalController> _logger;

    public SandboxTerminalController(
        ISandboxTerminalService terminalService,
        ILogger<SandboxTerminalController> logger)
    {
        _terminalService = terminalService;
        _logger = logger;
    }

    /// <summary>
    /// Execute a command in the sandbox environment
    /// 在沙盒环境中执行命令
    /// </summary>
    /// <param name="request">Command execution request - 命令执行请求</param>
    /// <returns>Command execution result - 命令执行结果</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(SandboxCommandResult), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult<SandboxCommandResult>> ExecuteCommand(
        [FromBody] ExecuteCommandRequest request)
    {
        try
        {
            _logger.LogInformation("Received command execution request: {Command}", request.Command);

            // 验证请求 - Validate request
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Command",
                    Detail = "Command cannot be empty",
                    Status = 400
                });
            }

            // 执行命令 - Execute command
            var result = await _terminalService.ExecuteCommandAsync(
                request.Command,
                request.WorkingDirectory,
                request.Timeout,
                HttpContext.RequestAborted);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command: {Command}", request.Command);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Command Execution Failed",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Execute a command with streaming output
    /// 执行命令并流式输出结果
    /// 
    /// 模拟AI-Agent的异步终端输出功能
    /// Simulates AI-Agent async terminal output functionality
    /// </summary>
    /// <param name="request">Streaming command request - 流式命令请求</param>
    /// <returns>Streaming command output - 流式命令输出</returns>
    [HttpPost("execute/stream")]
    [Produces("text/plain")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ExecuteCommandStream(
        [FromBody] ExecuteCommandRequest request)
    {
        try
        {
            _logger.LogInformation("Received streaming command request: {Command}", request.Command);

            // 验证请求 - Validate request
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Command",
                    Detail = "Command cannot be empty",
                    Status = 400
                });
            }

            // 设置流式响应 - Setup streaming response
            Response.ContentType = "text/plain; charset=utf-8";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            // 流式执行命令 - Stream command execution
            await foreach (var output in _terminalService.ExecuteCommandStreamAsync(
                request.Command,
                request.WorkingDirectory,
                request.Timeout,
                HttpContext.RequestAborted))
            {
                await Response.WriteAsync($"{output}\n", HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }

            return new ContentResult {
                StatusCode = StatusCodes.Status404NotFound,   // 对应 HTTP 204
                Content    = string.Empty,                     // NoContent 一般没内容
                ContentType = "text/plain; charset=utf-8"
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Streaming command was cancelled: {Command}", request.Command);
            return new ContentResult {
                StatusCode = StatusCodes.Status404NotFound,   // 对应 HTTP 204
                Content    = string.Empty,                     // NoContent 一般没内容
                ContentType = "text/plain; charset=utf-8"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute streaming command: {Command}", request.Command);
            await Response.WriteAsync($"Error: {ex.Message}\n");
            return new ContentResult {
                StatusCode = StatusCodes.Status404NotFound,   // 对应 HTTP 204
                Content    = string.Empty,                     // NoContent 一般没内容
                ContentType = "text/plain; charset=utf-8"
            };
        }
    }

    /// <summary>
    /// Get current working directory
    /// 获取当前工作目录
    /// </summary>
    /// <returns>Current working directory - 当前工作目录</returns>
    [HttpGet("workdir")]
    [ProducesResponseType(typeof(WorkingDirectoryResponse), 200)]
    public async Task<ActionResult<WorkingDirectoryResponse>> GetWorkingDirectory()
    {
        try
        {
            var workingDir = await _terminalService.GetWorkingDirectoryAsync();
            return Ok(new WorkingDirectoryResponse { WorkingDirectory = workingDir });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get working directory");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Failed to Get Working Directory",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Set working directory
    /// 设置工作目录
    /// </summary>
    /// <param name="request">Working directory request - 工作目录请求</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost("workdir")]
    [ProducesResponseType(typeof(SetWorkingDirectoryResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<ActionResult<SetWorkingDirectoryResponse>> SetWorkingDirectory(
        [FromBody] SetWorkingDirectoryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Path))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Path",
                    Detail = "Path cannot be empty",
                    Status = 400
                });
            }

            var success = await _terminalService.SetWorkingDirectoryAsync(request.Path);
            
            if (!success)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to Set Working Directory",
                    Detail = "The specified path is invalid or inaccessible",
                    Status = 400
                });
            }

            return Ok(new SetWorkingDirectoryResponse 
            { 
                Success = true, 
                WorkingDirectory = await _terminalService.GetWorkingDirectoryAsync() 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set working directory: {Path}", request.Path);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Failed to Set Working Directory",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Check if a command is safe to execute
    /// 检查命令是否安全可执行
    /// </summary>
    /// <param name="request">Command safety check request - 命令安全检查请求</param>
    /// <returns>Safety check result - 安全检查结果</returns>
    [HttpPost("check-command")]
    [ProducesResponseType(typeof(CommandSafetyResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public ActionResult<CommandSafetyResponse> CheckCommandSafety(
        [FromBody] CommandSafetyRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Command",
                    Detail = "Command cannot be empty",
                    Status = 400
                });
            }

            var isSafe = _terminalService.IsCommandSafe(request.Command);
            
            return Ok(new CommandSafetyResponse 
            { 
                IsSafe = isSafe,
                Command = request.Command,
                Message = isSafe ? "Command is safe to execute" : "Command contains potentially dangerous operations"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check command safety: {Command}", request.Command);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Safety Check Failed",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Get system information
    /// 获取系统信息
    /// </summary>
    /// <returns>System information - 系统信息</returns>
    [HttpGet("system-info")]
    [ProducesResponseType(typeof(SandboxSystemInfo), 200)]
    public async Task<ActionResult<SandboxSystemInfo>> GetSystemInfo()
    {
        try
        {
            var systemInfo = await _terminalService.GetSystemInfoAsync();
            return Ok(systemInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system information");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Failed to Get System Information",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Get terminal health status
    /// 获取终端健康状态
    /// </summary>
    /// <returns>Health status - 健康状态</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(TerminalHealthResponse), 200)]
    public async Task<ActionResult<TerminalHealthResponse>> GetHealth()
    {
        try
        {
            // 执行简单的健康检查命令 - Execute simple health check command
            var result = await _terminalService.ExecuteCommandAsync(
                "echo 'health-check'", 
                timeout: 5,
                cancellationToken: HttpContext.RequestAborted);

            var isHealthy = result.IsSuccess && result.StandardOutput.Contains("health-check");

            return Ok(new TerminalHealthResponse
            {
                IsHealthy = isHealthy,
                Status = isHealthy ? "Healthy" : "Unhealthy",
                LastChecked = DateTime.UtcNow,
                Details = isHealthy ? "Terminal service is responding normally" : "Terminal service is not responding properly"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Ok(new TerminalHealthResponse
            {
                IsHealthy = false,
                Status = "Unhealthy",
                LastChecked = DateTime.UtcNow,
                Details = $"Health check failed: {ex.Message}"
            });
        }
    }
}

#region Request/Response Models - 请求/响应模型

/// <summary>
/// Command execution request
/// 命令执行请求
/// </summary>
public class ExecuteCommandRequest
{
    /// <summary>
    /// Command to execute - 要执行的命令
    /// </summary>
    [Required]
    [StringLength(1000, ErrorMessage = "Command length cannot exceed 1000 characters")]
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Working directory (optional) - 工作目录（可选）
    /// </summary>
    [StringLength(500, ErrorMessage = "Working directory path cannot exceed 500 characters")]
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Timeout in seconds (default: 30) - 超时时间秒数（默认：30）
    /// </summary>
    [Range(1, 300, ErrorMessage = "Timeout must be between 1 and 300 seconds")]
    public int Timeout { get; set; } = 30;
}

/// <summary>
/// Working directory response
/// 工作目录响应
/// </summary>
public class WorkingDirectoryResponse
{
    /// <summary>
    /// Current working directory - 当前工作目录
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}

/// <summary>
/// Set working directory request
/// 设置工作目录请求
/// </summary>
public class SetWorkingDirectoryRequest
{
    /// <summary>
    /// Directory path - 目录路径
    /// </summary>
    [Required]
    [StringLength(500, ErrorMessage = "Path cannot exceed 500 characters")]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Set working directory response
/// 设置工作目录响应
/// </summary>
public class SetWorkingDirectoryResponse
{
    /// <summary>
    /// Success status - 成功状态
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Current working directory after change - 更改后的当前工作目录
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}

/// <summary>
/// Command safety check request
/// 命令安全检查请求
/// </summary>
public class CommandSafetyRequest
{
    /// <summary>
    /// Command to check - 要检查的命令
    /// </summary>
    [Required]
    [StringLength(1000, ErrorMessage = "Command length cannot exceed 1000 characters")]
    public string Command { get; set; } = string.Empty;
}

/// <summary>
/// Command safety check response
/// 命令安全检查响应
/// </summary>
public class CommandSafetyResponse
{
    /// <summary>
    /// Whether the command is safe - 命令是否安全
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// Original command - 原始命令
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Safety check message - 安全检查消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Terminal health response
/// 终端健康响应
/// </summary>
public class TerminalHealthResponse
{
    /// <summary>
    /// Whether the terminal is healthy - 终端是否健康
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Health status - 健康状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Last health check time - 最后健康检查时间
    /// </summary>
    public DateTime LastChecked { get; set; }

    /// <summary>
    /// Health check details - 健康检查详情
    /// </summary>
    public string Details { get; set; } = string.Empty;
}

#endregion


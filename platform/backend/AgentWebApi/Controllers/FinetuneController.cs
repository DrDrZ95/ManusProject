using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services.Finetune;
using AgentWebApi.Data.Entities;

namespace AgentWebApi.Controllers;

/// <summary>
/// Python.NET fine-tuning controller
/// Python.NET微调控制器
/// 
/// 提供AI-Agent系统中Python微调功能的API接口
/// Provides API endpoints for Python fine-tuning functionality in AI-Agent system
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinetuneController : ControllerBase
{
    private readonly IPythonFinetuneService _finetuneService;
    private readonly ILogger<FinetuneController> _logger;

    public FinetuneController(IPythonFinetuneService finetuneService, ILogger<FinetuneController> logger)
    {
        _finetuneService = finetuneService ?? throw new ArgumentNullException(nameof(finetuneService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Start a new fine-tuning job
    /// 启动新的微调任务
    /// </summary>
    /// <param name="request">Fine-tuning request - 微调请求</param>
    /// <returns>Job ID - 任务ID</returns>
    [HttpPost("start")]
    public async Task<ActionResult<StartFinetuneResponse>> StartFinetune([FromBody] FinetuneRequest request)
    {
        try
        {
            _logger.LogInformation("Starting fine-tuning job: {JobName} - 启动微调任务: {JobName}", request.JobName);
            
            // 验证请求参数 - Validate request parameters
            if (string.IsNullOrWhiteSpace(request.JobName))
            {
                return BadRequest(new { error = "Job name is required", message = "任务名称不能为空" });
            }

            if (string.IsNullOrWhiteSpace(request.DatasetPath))
            {
                return BadRequest(new { error = "Dataset path is required", message = "数据集路径不能为空" });
            }

            if (string.IsNullOrWhiteSpace(request.OutputDir))
            {
                return BadRequest(new { error = "Output directory is required", message = "输出目录不能为空" });
            }

            // 启动微调任务 - Start fine-tuning job
            var jobId = await _finetuneService.StartFinetuningAsync(request);
            
            var response = new StartFinetuneResponse
            {
                JobId = jobId,
                Message = "Fine-tuning job started successfully",
                ChineseMessage = "微调任务启动成功",
                Status = "queued"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start fine-tuning job: {JobName} - 启动微调任务失败: {JobName}", request.JobName);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Get fine-tuning job status
    /// 获取微调任务状态
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Job status - 任务状态</returns>
    [HttpGet("{jobId}/status")]
    public async Task<ActionResult<FinetuneJobStatusResponse>> GetJobStatus(string jobId)
    {
        try
        {
            _logger.LogInformation("Getting job status: {JobId} - 获取任务状态: {JobId}", jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BadRequest(new { error = "Job ID is required", message = "任务ID不能为空" });
            }

            var record = await _finetuneService.GetJobStatusAsync(jobId);
            if (record == null)
            {
                return NotFound(new { error = "Job not found", message = "任务未找到", jobId });
            }

            var response = new FinetuneJobStatusResponse
            {
                JobId = record.Id,
                JobName = record.JobName,
                Status = record.Status.ToString(),
                StatusDisplay = record.Status.GetDisplayName(),
                ChineseStatusDisplay = record.Status.GetChineseDisplayName(),
                Progress = record.Progress,
                CurrentEpoch = record.CurrentEpoch,
                TotalEpochs = record.TotalEpochs,
                CurrentLoss = record.CurrentLoss,
                BestLoss = record.BestLoss,
                EstimatedTimeRemaining = record.EstimatedTimeRemaining,
                CreatedAt = record.CreatedAt,
                StartedAt = record.StartedAt,
                CompletedAt = record.CompletedAt,
                ErrorMessage = record.ErrorMessage,
                OutputPath = record.OutputPath
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job status: {JobId} - 获取任务状态失败: {JobId}", jobId);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Cancel a running fine-tuning job
    /// 取消正在运行的微调任务
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost("{jobId}/cancel")]
    public async Task<ActionResult> CancelJob(string jobId)
    {
        try
        {
            _logger.LogInformation("Cancelling job: {JobId} - 取消任务: {JobId}", jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BadRequest(new { error = "Job ID is required", message = "任务ID不能为空" });
            }

            var success = await _finetuneService.CancelJobAsync(jobId);
            if (success)
            {
                return Ok(new { 
                    message = "Job cancelled successfully", 
                    chineseMessage = "任务取消成功",
                    jobId 
                });
            }
            else
            {
                return BadRequest(new { 
                    error = "Failed to cancel job", 
                    message = "Job may not be running or already completed",
                    chineseMessage = "任务可能未在运行或已完成",
                    jobId 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel job: {JobId} - 取消任务失败: {JobId}", jobId);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Get all fine-tuning jobs
    /// 获取所有微调任务
    /// </summary>
    /// <param name="status">Filter by status - 按状态过滤</param>
    /// <param name="limit">Maximum number of results - 最大结果数</param>
    /// <returns>List of jobs - 任务列表</returns>
    [HttpGet]
    public async Task<ActionResult<List<FinetuneJobSummary>>> GetJobs(
        [FromQuery] FinetuneStatus? status = null, 
        [FromQuery] int limit = 100)
    {
        try
        {
            _logger.LogInformation("Getting jobs with status: {Status}, limit: {Limit} - 获取任务列表，状态: {Status}, 限制: {Limit}", 
                status, limit);
            
            if (limit <= 0 || limit > 1000)
            {
                return BadRequest(new { error = "Limit must be between 1 and 1000", message = "限制必须在1到1000之间" });
            }

            var records = await _finetuneService.GetJobsAsync(status, limit);
            
            var jobs = records.Select(r => new FinetuneJobSummary
            {
                JobId = r.Id,
                JobName = r.JobName,
                BaseModel = r.BaseModel,
                Status = r.Status.ToString(),
                StatusDisplay = r.Status.GetDisplayName(),
                ChineseStatusDisplay = r.Status.GetChineseDisplayName(),
                Progress = r.Progress,
                CreatedAt = r.CreatedAt,
                StartedAt = r.StartedAt,
                CompletedAt = r.CompletedAt,
                CreatedBy = r.CreatedBy,
                Priority = r.Priority
            }).ToList();

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get jobs - 获取任务列表失败");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Get job logs
    /// 获取任务日志
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Job logs - 任务日志</returns>
    [HttpGet("{jobId}/logs")]
    public async Task<ActionResult<JobLogsResponse>> GetJobLogs(string jobId)
    {
        try
        {
            _logger.LogInformation("Getting job logs: {JobId} - 获取任务日志: {JobId}", jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BadRequest(new { error = "Job ID is required", message = "任务ID不能为空" });
            }

            var logs = await _finetuneService.GetJobLogsAsync(jobId);
            if (logs == null)
            {
                return NotFound(new { error = "Job not found", message = "任务未找到", jobId });
            }

            var response = new JobLogsResponse
            {
                JobId = jobId,
                Logs = logs,
                LogLines = logs.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job logs: {JobId} - 获取任务日志失败: {JobId}", jobId);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Validate Python environment
    /// 验证Python环境
    /// </summary>
    /// <returns>Environment validation result - 环境验证结果</returns>
    [HttpGet("environment/validate")]
    public async Task<ActionResult<PythonEnvironmentInfo>> ValidateEnvironment()
    {
        try
        {
            _logger.LogInformation("Validating Python environment - 验证Python环境");
            
            var envInfo = await _finetuneService.ValidatePythonEnvironmentAsync();
            return Ok(envInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Python environment - Python环境验证失败");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Get available models for fine-tuning
    /// 获取可用于微调的模型
    /// </summary>
    /// <returns>List of available models - 可用模型列表</returns>
    [HttpGet("models")]
    public async Task<ActionResult<List<string>>> GetAvailableModels()
    {
        try
        {
            _logger.LogInformation("Getting available models - 获取可用模型");
            
            var models = await _finetuneService.GetAvailableModelsAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available models - 获取可用模型失败");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Estimate training resources
    /// 估算训练资源
    /// </summary>
    /// <param name="request">Fine-tuning request - 微调请求</param>
    /// <returns>Resource estimation - 资源估算</returns>
    [HttpPost("estimate")]
    public async Task<ActionResult<ResourceEstimation>> EstimateResources([FromBody] FinetuneRequest request)
    {
        try
        {
            _logger.LogInformation("Estimating resources for job: {JobName} - 估算任务资源: {JobName}", request.JobName);
            
            if (string.IsNullOrWhiteSpace(request.DatasetPath))
            {
                return BadRequest(new { error = "Dataset path is required", message = "数据集路径不能为空" });
            }

            var estimation = await _finetuneService.EstimateResourcesAsync(request);
            return Ok(estimation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to estimate resources - 资源估算失败");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Update job progress (for internal use)
    /// 更新任务进度（内部使用）
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <param name="progress">Progress update - 进度更新</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost("{jobId}/progress")]
    public async Task<ActionResult> UpdateJobProgress(string jobId, [FromBody] FinetuneProgress progress)
    {
        try
        {
            _logger.LogInformation("Updating job progress: {JobId} - 更新任务进度: {JobId}", jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BadRequest(new { error = "Job ID is required", message = "任务ID不能为空" });
            }

            var success = await _finetuneService.UpdateJobProgressAsync(jobId, progress);
            if (success)
            {
                return Ok(new { 
                    message = "Progress updated successfully", 
                    chineseMessage = "进度更新成功",
                    jobId 
                });
            }
            else
            {
                return NotFound(new { error = "Job not found", message = "任务未找到", jobId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job progress: {JobId} - 更新任务进度失败: {JobId}", jobId);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }

    /// <summary>
    /// Get system information
    /// 获取系统信息
    /// </summary>
    /// <returns>System information - 系统信息</returns>
    [HttpGet("system-info")]
    public async Task<ActionResult<FinetuneSystemInfo>> GetSystemInfo()
    {
        try
        {
            _logger.LogInformation("Getting system information - 获取系统信息");
            
            var envInfo = await _finetuneService.ValidatePythonEnvironmentAsync();
            var models = await _finetuneService.GetAvailableModelsAsync();
            
            var systemInfo = new FinetuneSystemInfo
            {
                SystemName = "AI-Agent Python Fine-tuning System",
                Version = "1.0.0",
                PythonEnvironment = envInfo,
                AvailableModels = models,
                SupportedFeatures = new List<string>
                {
                    "Qwen3 model fine-tuning",
                    "Custom dataset support", 
                    "GPU acceleration",
                    "Progress tracking",
                    "Checkpoint saving",
                    "Resource estimation",
                    "Job cancellation"
                },
                DefaultConfiguration = new FinetuneRequest
                {
                    BaseModel = "Qwen/Qwen3-4B-Instruct",
                    Epochs = 3,
                    LearningRate = 2e-5,
                    BatchSize = 4,
                    MaxLength = 512,
                    SaveCheckpoints = true,
                    SaveSteps = 500,
                    EvalSteps = 500,
                    LoggingSteps = 100,
                    WarmupSteps = 100,
                    WeightDecay = 0.01,
                    Priority = 5
                }
            };

            return Ok(systemInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system information - 获取系统信息失败");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                chineseMessage = "服务器内部错误"
            });
        }
    }
}

#region Response Models - 响应模型

/// <summary>
/// Start fine-tune response model
/// 启动微调响应模型
/// </summary>
public class StartFinetuneResponse
{
    /// <summary>
    /// Job ID - 任务ID
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Response message - 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Chinese response message - 中文响应消息
    /// </summary>
    public string ChineseMessage { get; set; } = string.Empty;

    /// <summary>
    /// Job status - 任务状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Fine-tune job status response model
/// 微调任务状态响应模型
/// </summary>
public class FinetuneJobStatusResponse
{
    /// <summary>
    /// Job ID - 任务ID
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Job name - 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Job status - 任务状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status display name - 状态显示名称
    /// </summary>
    public string StatusDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Chinese status display name - 中文状态显示名称
    /// </summary>
    public string ChineseStatusDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage - 进度百分比
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Current epoch - 当前轮次
    /// </summary>
    public int CurrentEpoch { get; set; }

    /// <summary>
    /// Total epochs - 总轮次
    /// </summary>
    public int TotalEpochs { get; set; }

    /// <summary>
    /// Current loss - 当前损失
    /// </summary>
    public double? CurrentLoss { get; set; }

    /// <summary>
    /// Best loss - 最佳损失
    /// </summary>
    public double? BestLoss { get; set; }

    /// <summary>
    /// Estimated time remaining - 预计剩余时间
    /// </summary>
    public int? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Created timestamp - 创建时间戳
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Started timestamp - 开始时间戳
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Completed timestamp - 完成时间戳
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message - 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Output path - 输出路径
    /// </summary>
    public string? OutputPath { get; set; }
}

/// <summary>
/// Fine-tune job summary model
/// 微调任务摘要模型
/// </summary>
public class FinetuneJobSummary
{
    /// <summary>
    /// Job ID - 任务ID
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Job name - 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Base model - 基础模型
    /// </summary>
    public string BaseModel { get; set; } = string.Empty;

    /// <summary>
    /// Job status - 任务状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status display name - 状态显示名称
    /// </summary>
    public string StatusDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Chinese status display name - 中文状态显示名称
    /// </summary>
    public string ChineseStatusDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage - 进度百分比
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Created timestamp - 创建时间戳
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Started timestamp - 开始时间戳
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Completed timestamp - 完成时间戳
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Created by user - 创建用户
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Priority level - 优先级
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// Job logs response model
/// 任务日志响应模型
/// </summary>
public class JobLogsResponse
{
    /// <summary>
    /// Job ID - 任务ID
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Raw logs - 原始日志
    /// </summary>
    public string Logs { get; set; } = string.Empty;

    /// <summary>
    /// Log lines - 日志行
    /// </summary>
    public List<string> LogLines { get; set; } = new();
}

/// <summary>
/// Fine-tune system information model
/// 微调系统信息模型
/// </summary>
public class FinetuneSystemInfo
{
    /// <summary>
    /// System name - 系统名称
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// System version - 系统版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Python environment info - Python环境信息
    /// </summary>
    public PythonEnvironmentInfo PythonEnvironment { get; set; } = new();

    /// <summary>
    /// Available models - 可用模型
    /// </summary>
    public List<string> AvailableModels { get; set; } = new();

    /// <summary>
    /// Supported features - 支持的功能
    /// </summary>
    public List<string> SupportedFeatures { get; set; } = new();

    /// <summary>
    /// Default configuration - 默认配置
    /// </summary>
    public FinetuneRequest DefaultConfiguration { get; set; } = new();
}

#endregion


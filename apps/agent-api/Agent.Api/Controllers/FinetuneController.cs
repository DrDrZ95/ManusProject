namespace Agent.Api.Controllers;

/// <summary>
/// Python.NET fine-tuning controller
/// Python.NET微调控制器
/// 
/// 提供AI-Agent系统中Python微调功能的API接口
/// Provides API endpoints for Python fine-tuning functionality in AI-Agent system
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class FinetuneController : ControllerBase
{
    private readonly IPythonFinetuneService _finetuneService;
    private readonly ILogger<FinetuneController> _logger;
    private readonly IAgentTelemetryProvider _telemetryProvider;

    public FinetuneController(IPythonFinetuneService finetuneService, ILogger<FinetuneController> logger, IAgentTelemetryProvider telemetryProvider)
    {
        _finetuneService = finetuneService ?? throw new ArgumentNullException(nameof(finetuneService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryProvider = telemetryProvider ?? throw new ArgumentNullException(nameof(telemetryProvider));
    }

    /// <summary>
    /// Start a new fine-tuning job
    /// 启动新的微调任务
    /// </summary>
    /// <param name="request">Fine-tuning request - 微调请求</param>
    /// <returns>Job ID - 任务ID</returns>
    [HttpPost("start")]
    [SwaggerOperation(
        Summary = "Start a new fine-tuning job",
        Description = "Initiates a new model fine-tuning job with the specified parameters.",
        OperationId = "StartFinetune",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<StartFinetuneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StartFinetuneResponse>>> StartFinetune([FromBody] FinetuneRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.StartFinetune"))
        {
            span.SetAttribute("finetune.job_name", request.JobName);
            _logger.LogInformation("Starting fine-tuning job: {JobName} - 启动微调任务: {JobName}", request.JobName, request.JobName);
            
            // 验证请求参数 - Validate request parameters
            if (string.IsNullOrWhiteSpace(request.JobName))
            {
                span.SetStatus(ActivityStatusCode.Error, "Job name is required");
                return BadRequest(ApiResponse<StartFinetuneResponse>.Fail("Job name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.DatasetPath))
            {
                span.SetStatus(ActivityStatusCode.Error, "Dataset path is required");
                return BadRequest(ApiResponse<StartFinetuneResponse>.Fail("Dataset path is required"));
            }

            if (string.IsNullOrWhiteSpace(request.OutputDir))
            {
                span.SetStatus(ActivityStatusCode.Error, "Output directory is required");
                return BadRequest(ApiResponse<StartFinetuneResponse>.Fail("Output directory is required"));
            }

            try
            {
                // 启动微调任务 - Start fine-tuning job
                var jobId = await _finetuneService.StartFinetuningAsync(request);
                span.SetAttribute("finetune.job_id", jobId);
                span.SetAttribute("finetune.status", "queued");
                
                var response = new StartFinetuneResponse
                {
                    JobId = jobId,
                    Message = "Fine-tuning job started successfully",
                    ChineseMessage = "微调任务启动成功",
                    Status = "queued"
                };

                return Ok(ApiResponse<StartFinetuneResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start fine-tuning job: {JobName} - 启动微调任务失败: {JobName}", request.JobName, request.JobName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<StartFinetuneResponse>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get fine-tuning job status
    /// 获取微调任务状态
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Job status - 任务状态</returns>
    [HttpGet("{jobId}/status")]
    [SwaggerOperation(
        Summary = "Get fine-tuning job status",
        Description = "Retrieves the current status and progress of a fine-tuning job.",
        OperationId = "GetJobStatus",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<FinetuneJobStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FinetuneJobStatusResponse>>> GetJobStatus(string jobId)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.GetJobStatus"))
        {
            span.SetAttribute("finetune.job_id", jobId);
            _logger.LogInformation("Getting job status: {JobId} - 获取任务状态: {JobId}", jobId, jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                span.SetStatus(ActivityStatusCode.Error, "Job ID is required");
                return BadRequest(ApiResponse<FinetuneJobStatusResponse>.Fail("Job ID is required"));
            }

            try
            {
                var record = await _finetuneService.GetJobStatusAsync(jobId);
                if (record == null)
                {
                    span.SetStatus(ActivityStatusCode.Error, "Job not found");
                    return NotFound(ApiResponse<FinetuneJobStatusResponse>.Fail($"Job not found: {jobId}"));
                }
                span.SetAttribute("finetune.status", record.Status.ToString());
                span.SetAttribute("finetune.progress", record.Progress);

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
                    EstimatedTimeRemaining = record.EstimatedTimeRemaining.HasValue ? TimeSpan.FromSeconds(record.EstimatedTimeRemaining.Value) : null,
                    CreatedAt = record.CreatedAt,
                    StartedAt = record.StartedAt,
                    CompletedAt = record.CompletedAt,
                    ErrorMessage = record.ErrorMessage,
                    OutputPath = record.OutputPath
                };

                return Ok(ApiResponse<FinetuneJobStatusResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get job status: {JobId} - 获取任务状态失败: {JobId}", jobId, jobId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<FinetuneJobStatusResponse>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Cancel a running fine-tuning job
    /// 取消正在运行的微调任务
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost("{jobId}/cancel")]
    [SwaggerOperation(
        Summary = "Cancel a running fine-tuning job",
        Description = "Cancels a currently running fine-tuning job.",
        OperationId = "CancelJob",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> CancelJob(string jobId)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.CancelJob"))
        {
            span.SetAttribute("finetune.job_id", jobId);
            _logger.LogInformation("Cancelling job: {JobId} - 取消任务: {JobId}", jobId, jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                span.SetStatus(ActivityStatusCode.Error, "Job ID is required");
                return BadRequest(ApiResponse<object>.Fail("Job ID is required"));
            }

            try
            {
                var success = await _finetuneService.CancelJobAsync(jobId);
                span.SetAttribute("finetune.cancel_success", success);
                if (success)
                {
                    return Ok(ApiResponse<object>.Ok(new { 
                        message = "Job cancelled successfully", 
                        chineseMessage = "任务取消成功",
                        jobId 
                    }));
                }
                else
                {
                    span.SetStatus(ActivityStatusCode.Error, "Failed to cancel job");
                    return BadRequest(ApiResponse<object>.Fail("Failed to cancel job. Job may not be running or already completed."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel job: {JobId} - 取消任务失败: {JobId}", jobId, jobId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
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
    [SwaggerOperation(
        Summary = "Get all fine-tuning jobs",
        Description = "Retrieves a list of all fine-tuning jobs, optionally filtered by status.",
        OperationId = "GetJobs",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<FinetuneJobSummary>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FinetuneJobSummary>>>> GetJobs(
        [FromQuery] FinetuneStatus? status = null, 
        [FromQuery] int limit = 100)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.GetJobs"))
        {
            span.SetAttribute("finetune.filter_status", status?.ToString());
            span.SetAttribute("finetune.limit", limit);
            _logger.LogInformation("Getting jobs with status: {Status}, limit: {Limit} - 获取任务列表，状态: {Status}, 限制: {Limit}", 
                status, limit, status, limit);
            
            if (limit <= 0 || limit > 1000)
            {
                span.SetStatus(ActivityStatusCode.Error, "Limit must be between 1 and 1000");
                return BadRequest(ApiResponse<List<FinetuneJobSummary>>.Fail("Limit must be between 1 and 1000"));
            }

            try
            {
                var records = await _finetuneService.GetJobsAsync(status, limit);
                span.SetAttribute("finetune.job_count", records.Count);
                
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

                return Ok(ApiResponse<List<FinetuneJobSummary>>.Ok(jobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get jobs - 获取任务列表失败");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<List<FinetuneJobSummary>>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get job logs
    /// 获取任务日志
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Job logs - 任务日志</returns>
    [HttpGet("{jobId}/logs")]
    [SwaggerOperation(
        Summary = "Get job logs",
        Description = "Retrieves the execution logs for a specific fine-tuning job.",
        OperationId = "GetJobLogs",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<JobLogsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<JobLogsResponse>>> GetJobLogs(string jobId)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.GetJobLogs"))
        {
            span.SetAttribute("finetune.job_id", jobId);
            _logger.LogInformation("Getting job logs: {JobId} - 获取任务日志: {JobId}", jobId, jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                span.SetStatus(ActivityStatusCode.Error, "Job ID is required");
                return BadRequest(ApiResponse<JobLogsResponse>.Fail("Job ID is required"));
            }

            try
            {
                var logs = await _finetuneService.GetJobLogsAsync(jobId);
                if (logs == null)
                {
                    span.SetStatus(ActivityStatusCode.Error, "Job not found");
                    return NotFound(ApiResponse<JobLogsResponse>.Fail($"Job not found: {jobId}"));
                }
                span.SetAttribute("finetune.log_length", logs.Length);

                var response = new JobLogsResponse
                {
                    JobId = jobId,
                    Logs = logs?.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()
                };

                return Ok(ApiResponse<JobLogsResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get job logs: {JobId} - 获取任务日志失败: {JobId}", jobId, jobId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<JobLogsResponse>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Validate Python environment
    /// 验证Python环境
    /// </summary>
    /// <returns>Environment validation result - 环境验证结果</returns>
    [HttpGet("environment/validate")]
    [SwaggerOperation(
        Summary = "Validate Python environment",
        Description = "Validates the Python environment configuration and availability.",
        OperationId = "ValidateEnvironment",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<PythonEnvironmentInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PythonEnvironmentInfo>>> ValidateEnvironment()
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.ValidateEnvironment"))
        {
            _logger.LogInformation("Validating Python environment - 验证Python环境");
            
            try
            {
                var envInfo = await _finetuneService.ValidatePythonEnvironmentAsync();
                span.SetAttribute("python.version", envInfo.PythonVersion);
                span.SetAttribute("python.is_installed", envInfo.IsPythonInstalled);
                return Ok(ApiResponse<PythonEnvironmentInfo>.Ok(envInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Python environment - Python环境验证失败");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<PythonEnvironmentInfo>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Get available models for fine-tuning
    /// 获取可用于微调的模型
    /// </summary>
    /// <returns>List of available models - 可用模型列表</returns>
    [HttpGet("models")]
    [SwaggerOperation(
        Summary = "Get available models",
        Description = "Retrieves a list of base models available for fine-tuning.",
        OperationId = "GetAvailableModels",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableModels()
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.GetAvailableModels"))
        {
            _logger.LogInformation("Getting available models - 获取可用模型");
            
            try
            {
                var models = await _finetuneService.GetAvailableModelsAsync();
                span.SetAttribute("finetune.model_count", models.Count);
                return Ok(ApiResponse<List<string>>.Ok(models));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available models - 获取可用模型失败");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<List<string>>.Fail(ex.Message));
            }
        }
    }

    /// <summary>
    /// Estimate training resources
    /// 估算训练资源
    /// </summary>
    /// <param name="request">Fine-tuning request - 微调请求</param>
    /// <returns>Resource estimation - 资源估算</returns>
    [HttpPost("estimate")]
    [SwaggerOperation(
        Summary = "Estimate training resources",
        Description = "Estimates the required resources (GPU memory, time) for a fine-tuning job.",
        OperationId = "EstimateResources",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<ResourceEstimation>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ResourceEstimation>>> EstimateResources([FromBody] FinetuneRequest request)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.EstimateResources"))
        {
            span.SetAttribute("finetune.job_name", request.JobName);
            _logger.LogInformation("Estimating resources for job: {JobName} - 估算任务资源: {JobName}", request.JobName, request.JobName);
            
            if (string.IsNullOrWhiteSpace(request.DatasetPath))
            {
                span.SetStatus(ActivityStatusCode.Error, "Dataset path is required");
                return BadRequest(ApiResponse<ResourceEstimation>.Fail("Dataset path is required"));
            }

            try
            {
                var estimation = await _finetuneService.EstimateResourcesAsync(request);
                span.SetAttribute("finetune.estimated_gpu_memory", estimation.EstimatedGpuMemoryMb);
                span.SetAttribute("finetune.estimated_training_time", estimation.EstimatedTrainingTimeMinutes);
                return Ok(ApiResponse<ResourceEstimation>.Ok(estimation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to estimate resources - 资源估算失败");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<ResourceEstimation>.Fail(ex.Message));
            }
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
    [SwaggerOperation(
        Summary = "Update job progress",
        Description = "Updates the progress of a running fine-tuning job (Internal Use).",
        OperationId = "UpdateJobProgress",
        Tags = new[] { "Finetune" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateJobProgress(string jobId, [FromBody] FinetuneProgress progress)
    {
        using (var span = _telemetryProvider.StartSpan("FinetuneController.UpdateJobProgress"))
        {
            span.SetAttribute("finetune.job_id", jobId);
            span.SetAttribute("finetune.progress_percentage", progress.ProgressPercentage);
            _logger.LogInformation("Updating job progress: {JobId} - 更新任务进度: {JobId}", jobId, jobId);
            
            if (string.IsNullOrWhiteSpace(jobId))
            {
                span.SetStatus(ActivityStatusCode.Error, "Job ID is required");
                return BadRequest(ApiResponse<object>.Fail("Job ID is required"));
            }

            try
            {
                var success = await _finetuneService.UpdateJobProgressAsync(jobId, progress);
                span.SetAttribute("finetune.update_success", success);
                if (success)
                {
                    return Ok(ApiResponse<object>.Ok(new { 
                        message = "Progress updated successfully", 
                        chineseMessage = "进度更新成功",
                        jobId 
                    }));
                }
                else
                {
                    span.SetStatus(ActivityStatusCode.Error, "Job not found");
                    return NotFound(ApiResponse<object>.Fail($"Job not found: {jobId}"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update job progress: {JobId} - 更新任务进度失败: {JobId}", jobId, jobId);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}

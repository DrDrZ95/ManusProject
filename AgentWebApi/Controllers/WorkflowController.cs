using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services.Workflow;

namespace AgentWebApi.Controllers;

/// <summary>
/// Workflow management controller
/// 工作流管理控制器
/// 
/// 提供基于AI-Agent planning.py的工作流管理API
/// Provides workflow management API based on AI-Agent planning.py
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IWorkflowService workflowService,
        ILogger<WorkflowController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new workflow plan
    /// 创建新的工作流计划
    /// </summary>
    /// <param name="request">Plan creation request - 计划创建请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Created workflow plan - 创建的工作流计划</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WorkflowPlan), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkflowPlan>> CreatePlan(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 验证请求 - Validate request
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Plan title is required - 计划标题是必需的");
            }

            if (request.Steps == null || request.Steps.Count == 0)
            {
                return BadRequest("At least one step is required - 至少需要一个步骤");
            }

            var plan = await _workflowService.CreatePlanAsync(request, cancellationToken);
            
            _logger.LogInformation("Created workflow plan: {PlanId}", plan.Id);
            
            return CreatedAtAction(
                nameof(GetPlan),
                new { planId = plan.Id },
                plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow plan");
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Get workflow plan by ID
    /// 根据ID获取工作流计划
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Workflow plan - 工作流计划</returns>
    [HttpGet("{planId}")]
    [ProducesResponseType(typeof(WorkflowPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowPlan>> GetPlan(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _workflowService.GetPlanAsync(planId, cancellationToken);
            
            if (plan == null)
            {
                return NotFound($"Plan not found: {planId} - 计划未找到");
            }

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow plan: {PlanId}", planId);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>List of workflow plans - 工作流计划列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<WorkflowPlan>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WorkflowPlan>>> GetAllPlans(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plans = await _workflowService.GetAllPlansAsync(cancellationToken);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all workflow plans");
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Update step status in a plan
    /// 更新计划中的步骤状态
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="stepIndex">Step index - 步骤索引</param>
    /// <param name="request">Status update request - 状态更新请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPut("{planId}/steps/{stepIndex}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStepStatus(
        string planId,
        int stepIndex,
        [FromBody] UpdateStepStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _workflowService.UpdateStepStatusAsync(
                planId, stepIndex, request.Status, cancellationToken);

            if (!success)
            {
                return NotFound($"Plan or step not found - 计划或步骤未找到");
            }

            return Ok(new { success = true, message = "Step status updated - 步骤状态已更新" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step status: {PlanId}, {StepIndex}", planId, stepIndex);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Get current active step in a plan
    /// 获取计划中当前活动步骤
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Current step information - 当前步骤信息</returns>
    [HttpGet("{planId}/current-step")]
    [ProducesResponseType(typeof(WorkflowStep), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowStep>> GetCurrentStep(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var step = await _workflowService.GetCurrentStepAsync(planId, cancellationToken);
            
            if (step == null)
            {
                return NotFound($"No active step found for plan: {planId} - 计划中没有找到活动步骤");
            }

            return Ok(step);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current step: {PlanId}", planId);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Mark step as completed
    /// 标记步骤为已完成
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="stepIndex">Step index - 步骤索引</param>
    /// <param name="request">Complete step request - 完成步骤请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost("{planId}/steps/{stepIndex}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteStep(
        string planId,
        int stepIndex,
        [FromBody] CompleteStepRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _workflowService.CompleteStepAsync(
                planId, stepIndex, request.Result, cancellationToken);

            if (!success)
            {
                return NotFound($"Plan or step not found - 计划或步骤未找到");
            }

            return Ok(new { success = true, message = "Step completed - 步骤已完成" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step: {PlanId}, {StepIndex}", planId, stepIndex);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Generate todo list for a plan
    /// 为计划生成待办事项列表
    /// 
    /// 这是AI-Agent的核心功能，生成markdown格式的任务列表
    /// This is a core feature from AI-Agent, generating markdown format task lists
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Todo list content - 待办事项列表内容</returns>
    [HttpGet("{planId}/todo")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("text/markdown", "application/json")]
    public async Task<IActionResult> GenerateToDoList(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var todoContent = await _workflowService.GenerateToDoListAsync(planId, cancellationToken);
            
            // 根据Accept头返回不同格式 - Return different formats based on Accept header
            if (Request.Headers.Accept.Any(h => h.Contains("text/markdown")))
            {
                return Content(todoContent, "text/markdown");
            }
            
            return Ok(new { content = todoContent, planId });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating todo list: {PlanId}", planId);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Save todo list to file
    /// 将待办事项列表保存到文件
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="request">Save request - 保存请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpPost("{planId}/todo/save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveToDoListToFile(
        string planId,
        [FromBody] SaveToDoListRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest("File path is required - 文件路径是必需的");
            }

            var success = await _workflowService.SaveToDoListToFileAsync(
                planId, request.FilePath, cancellationToken);

            if (!success)
            {
                return NotFound($"Plan not found or save failed - 计划未找到或保存失败");
            }

            return Ok(new { 
                success = true, 
                message = "Todo list saved to file - 待办事项列表已保存到文件",
                filePath = request.FilePath 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving todo list to file: {PlanId}, {FilePath}", planId, request.FilePath);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Load todo list from file
    /// 从文件加载待办事项列表
    /// </summary>
    /// <param name="request">Load request - 加载请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Loaded plan ID - 加载的计划ID</returns>
    [HttpPost("todo/load")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoadToDoListFromFile(
        [FromBody] LoadToDoListRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest("File path is required - 文件路径是必需的");
            }

            var planId = await _workflowService.LoadToDoListFromFileAsync(
                request.FilePath, cancellationToken);

            if (planId == null)
            {
                return NotFound($"File not found or load failed - 文件未找到或加载失败");
            }

            return Ok(new { 
                success = true, 
                message = "Todo list loaded from file - 待办事项列表已从文件加载",
                planId,
                filePath = request.FilePath 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading todo list from file: {FilePath}", request.FilePath);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Get plan execution progress
    /// 获取计划执行进度
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Progress information - 进度信息</returns>
    [HttpGet("{planId}/progress")]
    [ProducesResponseType(typeof(WorkflowProgress), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowProgress>> GetProgress(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var progress = await _workflowService.GetProgressAsync(planId, cancellationToken);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress: {PlanId}", planId);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }

    /// <summary>
    /// Delete a workflow plan
    /// 删除工作流计划
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    [HttpDelete("{planId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlan(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _workflowService.DeletePlanAsync(planId, cancellationToken);

            if (!success)
            {
                return NotFound($"Plan not found: {planId} - 计划未找到");
            }

            return Ok(new { success = true, message = "Plan deleted - 计划已删除" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plan: {PlanId}", planId);
            return StatusCode(500, "Internal server error - 内部服务器错误");
        }
    }
}

#region Request/Response Models

/// <summary>
/// Update step status request
/// 更新步骤状态请求
/// </summary>
public class UpdateStepStatusRequest
{
    /// <summary>
    /// New status - 新状态
    /// </summary>
    public PlanStepStatus Status { get; set; }
}

/// <summary>
/// Complete step request
/// 完成步骤请求
/// </summary>
public class CompleteStepRequest
{
    /// <summary>
    /// Step execution result - 步骤执行结果
    /// </summary>
    public string? Result { get; set; }
}

/// <summary>
/// Save todo list request
/// 保存待办事项列表请求
/// </summary>
public class SaveToDoListRequest
{
    /// <summary>
    /// File path - 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Load todo list request
/// 加载待办事项列表请求
/// </summary>
public class LoadToDoListRequest
{
    /// <summary>
    /// File path - 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}

#endregion


namespace Agent.Api.Controllers;

/// <summary>
/// Workflow management controller
/// 工作流管理控制器
/// 
/// 提供基于AI-Agent planning.py的工作流管理API
/// Provides workflow management API based on AI-Agent planning.py
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
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
    [SwaggerOperation(
        Summary = "Create a new workflow plan",
        Description = "Creates a new workflow plan based on the provided title and steps.",
        OperationId = "CreateWorkflowPlan",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<WorkflowPlan>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<WorkflowPlan>>> CreatePlan(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 验证请求 - Validate request
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<object>.Fail("Plan title is required - 计划标题是必需的"));
            }

            if (request.Steps == null || request.Steps.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("At least one step is required - 至少需要一个步骤"));
            }

            var plan = await _workflowService.CreatePlanAsync(request, cancellationToken);

            _logger.LogInformation("Created workflow plan: {PlanId}", plan.Id);

            return CreatedAtAction(
                nameof(GetPlan),
                new { version = "1.0", planId = plan.Id },
                ApiResponse<WorkflowPlan>.Ok(plan, "Plan created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow plan");
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Get workflow plan by ID",
        Description = "Retrieves a specific workflow plan by its unique identifier.",
        OperationId = "GetWorkflowPlan",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<WorkflowPlan>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<WorkflowPlan>>> GetPlan(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _workflowService.GetPlanAsync(planId, cancellationToken);

            if (plan == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Plan not found: {planId} - 计划未找到"));
            }

            return Ok(ApiResponse<WorkflowPlan>.Ok(plan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow plan: {PlanId}", planId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
        }
    }

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>List of workflow plans - 工作流计划列表</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all workflow plans",
        Description = "Retrieves a list of all available workflow plans.",
        OperationId = "GetAllWorkflowPlans",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<List<WorkflowPlan>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<WorkflowPlan>>>> GetAllPlans(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plans = await _workflowService.GetAllPlansAsync(cancellationToken);
            return Ok(ApiResponse<List<WorkflowPlan>>.Ok(plans));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all workflow plans");
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Update step status",
        Description = "Updates the status of a specific step within a workflow plan.",
        OperationId = "UpdateWorkflowStepStatus",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStepStatus(
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
                return NotFound(ApiResponse<object>.Fail($"Plan or step not found - 计划或步骤未找到"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Step status updated - 步骤状态已更新"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step status: {PlanId}, {StepIndex}", planId, stepIndex);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Get current active step",
        Description = "Retrieves the currently active step in the specified workflow plan.",
        OperationId = "GetWorkflowCurrentStep",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<WorkflowStep>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<WorkflowStep>>> GetCurrentStep(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var step = await _workflowService.GetCurrentStepAsync(planId, cancellationToken);

            if (step == null)
            {
                return NotFound(ApiResponse<object>.Fail($"No active step found for plan: {planId} - 计划中没有找到活动步骤"));
            }

            return Ok(ApiResponse<WorkflowStep>.Ok(step));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current step: {PlanId}", planId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Mark step as completed",
        Description = "Marks a specific step as completed and provides the result.",
        OperationId = "CompleteWorkflowStep",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> CompleteStep(
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
                return NotFound(ApiResponse<object>.Fail($"Plan or step not found - 计划或步骤未找到"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Step completed - 步骤已完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step: {PlanId}, {StepIndex}", planId, stepIndex);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Generate todo list",
        Description = "Generates a markdown formatted todo list for the specified plan.",
        OperationId = "GenerateWorkflowTodoList",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
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

            return Ok(ApiResponse<object>.Ok(new { content = todoContent, planId }));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating todo list: {PlanId}", planId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Save todo list to file",
        Description = "Saves the generated todo list to a specified file path.",
        OperationId = "SaveWorkflowTodoList",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> SaveToDoListToFile(
        string planId,
        [FromBody] SaveToDoListRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail("File path is required - 文件路径是必需的"));
            }

            var success = await _workflowService.SaveToDoListToFileAsync(
                planId, request.FilePath, cancellationToken);

            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Plan not found or save failed - 计划未找到或保存失败"));
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                filePath = request.FilePath
            }, "Todo list saved to file - 待办事项列表已保存到文件"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving todo list to file: {PlanId}, {FilePath}", planId, request.FilePath);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Load todo list from file",
        Description = "Loads a todo list from a specified file path.",
        OperationId = "LoadWorkflowTodoList",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> LoadToDoListFromFile(
        [FromBody] LoadToDoListRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail("File path is required - 文件路径是必需的"));
            }

            var planId = await _workflowService.LoadToDoListFromFileAsync(
                request.FilePath, cancellationToken);

            if (planId == null)
            {
                return NotFound(ApiResponse<object>.Fail($"File not found or load failed - 文件未找到或加载失败"));
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                planId,
                filePath = request.FilePath
            }, "Todo list loaded from file - 待办事项列表已从文件加载"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading todo list from file: {FilePath}", request.FilePath);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Get plan progress",
        Description = "Retrieves the execution progress of the specified workflow plan.",
        OperationId = "GetWorkflowProgress",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<WorkflowProgress>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<WorkflowProgress>>> GetProgress(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var progress = await _workflowService.GetProgressAsync(planId, cancellationToken);
            return Ok(ApiResponse<WorkflowProgress>.Ok(progress));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress: {PlanId}", planId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
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
    [SwaggerOperation(
        Summary = "Delete workflow plan",
        Description = "Deletes a specified workflow plan.",
        OperationId = "DeleteWorkflowPlan",
        Tags = new[] { "Workflow" }
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> DeletePlan(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _workflowService.DeletePlanAsync(planId, cancellationToken);

            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Plan not found: {planId} - 计划未找到"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Plan deleted - 计划已删除"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plan: {PlanId}", planId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error - 内部服务器错误"));
        }
    }

    // --- Visualization & Debugging (可视化与调试) ---

    /// <summary>
    /// Update visual graph for a plan
    /// 更新工作流的可视化图形数据
    /// </summary>
    [HttpPut("{planId}/visual-graph")]
    [SwaggerOperation(Summary = "Update visual graph", Tags = new[] { "Workflow" })]
    public async Task<IActionResult> UpdateVisualGraph(
        string planId,
        [FromBody] UpdateVisualGraphRequest request,
        CancellationToken cancellationToken = default)
    {
        var success = await _workflowService.UpdateVisualGraphAsync(planId, request.VisualGraphJson, cancellationToken);
        if (!success) return NotFound(ApiResponse<object>.Fail("Plan not found"));
        return Ok(ApiResponse<object>.Ok(null, "Visual graph updated"));
    }

    /// <summary>
    /// Toggle breakpoint for a step
    /// 设置或取消步骤的断点
    /// </summary>
    [HttpPut("{planId}/step/{stepIndex}/breakpoint")]
    [SwaggerOperation(Summary = "Toggle breakpoint", Tags = new[] { "Workflow" })]
    public async Task<IActionResult> ToggleBreakpoint(
        string planId,
        int stepIndex,
        [FromBody] ToggleBreakpointRequest request,
        CancellationToken cancellationToken = default)
    {
        var success = await _workflowService.ToggleBreakpointAsync(planId, stepIndex, request.IsBreakpoint, cancellationToken);
        if (!success) return NotFound(ApiResponse<object>.Fail("Plan or step not found"));
        return Ok(ApiResponse<object>.Ok(null, "Breakpoint toggled"));
    }

    /// <summary>
    /// Get performance report for a plan
    /// 获取工作流性能报告
    /// </summary>
    [HttpGet("{planId}/performance")]
    [SwaggerOperation(Summary = "Get performance report", Tags = new[] { "Workflow" })]
    public async Task<ActionResult<ApiResponse<WorkflowPerformanceReport>>> GetPerformanceReport(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _workflowService.GetPerformanceReportAsync(planId, cancellationToken);
            return Ok(ApiResponse<WorkflowPerformanceReport>.Ok(report));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Export workflow as JSON
    /// 导出工作流为JSON
    /// </summary>
    [HttpGet("{planId}/export")]
    [SwaggerOperation(Summary = "Export workflow", Tags = new[] { "Workflow" })]
    public async Task<IActionResult> ExportWorkflow(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await _workflowService.ExportWorkflowAsync(planId, cancellationToken);
            return Content(json, "application/json");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Import workflow from JSON
    /// 导入工作流JSON
    /// </summary>
    [HttpPost("import")]
    [SwaggerOperation(Summary = "Import workflow", Tags = new[] { "Workflow" })]
    public async Task<ActionResult<ApiResponse<WorkflowPlan>>> ImportWorkflow(
        [FromBody] ImportWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _workflowService.ImportWorkflowAsync(request.JsonContent, cancellationToken);
            return Ok(ApiResponse<WorkflowPlan>.Ok(plan));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

#region Request/Response Models

/// <summary>
/// Update visual graph request
/// </summary>
public class UpdateVisualGraphRequest
{
    public string VisualGraphJson { get; set; } = string.Empty;
}

/// <summary>
/// Toggle breakpoint request
/// </summary>
public class ToggleBreakpointRequest
{
    public bool IsBreakpoint { get; set; }
}

/// <summary>
/// Import workflow request
/// </summary>
public class ImportWorkflowRequest
{
    public string JsonContent { get; set; } = string.Empty;
}

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


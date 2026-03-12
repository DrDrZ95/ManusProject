namespace Agent.Application.Services.Workflow;

/// <summary>
/// Workflow service implementation
/// 工作流服务实现
/// 
/// 基于AI-Agent项目的planning.py转换而来，实现了核心的工作流管理和todo文件交互功能
/// Converted from AI-Agent project's planning.py, implementing core workflow management and todo file interaction
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly ILogger<WorkflowService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly WorkflowOptions _options;
    private readonly IWorkflowRepository _repository;
    private readonly IWorkflowNotificationService _notificationService; // 新增：通知服务
    private readonly IAgentTraceService _agentTraceService;
    private readonly IServiceProvider _serviceProvider;

    // 状态机引擎实例管理 (State machine engine instance management)
    private readonly ConcurrentDictionary<Guid, IWorkflowEngine> _engines = new();

    public WorkflowService(
        ILogger<WorkflowService> logger,
        ILoggerFactory loggerFactory,
        IOptions<WorkflowOptions> options,
        IWorkflowRepository repository,
        IWorkflowNotificationService notificationService,
        IAgentTraceService agentTraceService,
        IServiceProvider serviceProvider) // 注入通知服务
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _options = options.Value;
        _repository = repository;
        _notificationService = notificationService;
        _agentTraceService = agentTraceService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取或创建工作流执行引擎实例 (Get or create a workflow execution engine instance)
    /// </summary>
    private async Task<IWorkflowEngine?> GetOrCreateEngineAsync(Guid planId, CancellationToken cancellationToken)
    {
        if (_engines.TryGetValue(planId, out var engine))
        {
            return engine;
        }

        var planEntity = await _repository.GetPlanByIdAsync(planId, cancellationToken);
        if (planEntity == null)
        {
            _logger.LogWarning("Plan not found for engine creation: {PlanId}", planId);
            return null;
        }

        // 解析持久化的上下文 (Parse persisted context)
        WorkflowContext? existingContext = null;
        if (!string.IsNullOrEmpty(planEntity.ExecutionContextJson))
        {
            try
            {
                existingContext = JsonSerializer.Deserialize<WorkflowContext>(planEntity.ExecutionContextJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize execution context for plan {PlanId}", planId);
            }
        }

        // 使用当前持久化的状态和上下文初始化引擎 (Initialize engine with current persisted state and context)
        var newEngine = new WorkflowExecutionEngine(
            planId,
            planEntity.CurrentState,
            _notificationService,
            _repository,
            _serviceProvider.GetRequiredService<ISemanticKernelService>(),
            _serviceProvider.GetRequiredService<ISmartToolSelector>(),
            _loggerFactory.CreateLogger<WorkflowExecutionEngine>(),
            _agentTraceService,
            existingContext);

        _engines[planId] = newEngine;
        return newEngine;
    }

    /// <inheritdoc />
    public async Task<WorkflowPlan> GeneratePlanFromGoalAsync(string goal, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating plan from goal: {Goal}", goal);

        var prompt = $@"基于以下目标生成一个结构化的工作流计划：
目标：{goal}

请返回一个 JSON 对象，包含以下结构：
{{
  ""title"": ""计划标题"",
  ""description"": ""计划描述"",
  ""steps"": [
    {{
      ""text"": ""步骤描述"",
      ""type"": ""task/tool"",
      ""dependsOn"": [],
      ""condition"": null
    }}
  ]
}}
请确保 JSON 格式正确且易于解析。";

        var response = await _serviceProvider.GetRequiredService<ISemanticKernelService>().ExecutePromptAsync(prompt);
        
        // 解析生成的 JSON 计划 (Parsing generated JSON plan)
        try 
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                response = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            var generatedRequest = JsonSerializer.Deserialize<CreatePlanRequest>(response, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (generatedRequest != null)
            {
                generatedRequest.UserGoal = goal;
                return await CreatePlanAsync(generatedRequest, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse LLM generated plan: {Response}", response);
        }

        // 回退逻辑 (Fallback logic)
        return await CreatePlanAsync(new CreatePlanRequest
        {
            Title = $"Auto Plan: {goal}",
            Description = $"Automatically generated plan for: {goal}",
            UserGoal = goal,
            Steps = new List<WorkflowStepDto>
            {
                new WorkflowStepDto { Text = goal, Type = "task" }
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Create a new workflow plan
    /// 创建新的工作流计划
    /// 
    /// 对应AI-Agent中的_create_initial_plan方法
    /// Corresponds to _create_initial_plan method in AI-Agent
    /// </summary>
    public async Task<WorkflowPlan> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = new WorkflowPlan
            {
                Title = request.Title,
                Description = request.Description,
                ExecutorKeys = request.ExecutorKeys,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = PlanStatus.InProgress
            };

            // If userGoal is provided, steps will be generated by the engine.
            if (string.IsNullOrEmpty(request.UserGoal))
            {
                for (int i = 0; i < request.Steps.Count; i++)
                {
                    var stepDto = request.Steps[i];
                    plan.Steps.Add(new WorkflowStep
                    {
                        Index = i,
                        Text = stepDto.Text,
                        Type = string.IsNullOrEmpty(stepDto.Type) ? ExtractStepType(stepDto.Text) : stepDto.Type,
                        Status = PlanStepStatus.NotStarted,
                        DependsOn = stepDto.DependsOn,
                        Condition = stepDto.Condition
                    });
                }
            }

            var planEntity = WorkflowPlanMappingExtensions.ToEntity(plan);
            var addedEntity = await _repository.AddPlanAsync(planEntity, cancellationToken);
            var resultPlan = WorkflowPlanMappingExtensions.ToModel(addedEntity);

            var context = new WorkflowContext(resultPlan.Id.GetHashCode());
            if (!string.IsNullOrEmpty(request.UserGoal))
            {
                context.InputParameters["userGoal"] = request.UserGoal;
            }

            var engine = new WorkflowExecutionEngine(
                resultPlan.Id,
                resultPlan.CurrentState,
                _notificationService,
                _repository,
                _serviceProvider.GetRequiredService<ISemanticKernelService>(),
                _serviceProvider.GetRequiredService<ISmartToolSelector>(),
                _loggerFactory.CreateLogger<WorkflowExecutionEngine>(),
                _agentTraceService,
                context);

            _engines[resultPlan.Id] = engine;
            await engine.TriggerEventAsync(WorkflowEvent.StartTask);

            _logger.LogInformation("Created workflow plan with ID: {PlanId}, Title: {Title}", resultPlan.Id, request.Title);

            return resultPlan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow plan: {Title}", request.Title);
            throw;
        }
    }

    /// <summary>
    /// Get workflow plan by ID
    /// 根据ID获取工作流计划
    /// </summary>
    public async Task<WorkflowPlan?> GetPlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(planId, out var id))
        {
            _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
            return null;
        }

        var entity = await _repository.GetPlanByIdAsync(id, cancellationToken);

        var model = WorkflowPlanMappingExtensions.ToModel(entity);

        // 如果存在，确保引擎已加载 (If exists, ensure engine is loaded)
        if (model != null && !_engines.ContainsKey(model.Id))
        {
            await GetOrCreateEngineAsync(model.Id, cancellationToken);
        }

        return model;
    }

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    public async Task<List<WorkflowPlan>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllPlansAsync(cancellationToken);

        var models = entities.Select(e => WorkflowPlanMappingExtensions.ToModel(e)).ToList();

        // 确保所有计划的引擎都已加载 (Ensure all plan engines are loaded)
        foreach (var model in models)
        {
            if (!_engines.ContainsKey(model.Id))
            {
                await GetOrCreateEngineAsync(model.Id, cancellationToken);
            }
        }

        return models;
    }


    // --- Workflow Execution Engine Control (工作流执行引擎控制) ---

    /// <summary>
    /// 启动工作流执行引擎 (Start the workflow execution engine)
    /// </summary>
    public async Task<bool> StartWorkflowAsync(string planId, CancellationToken cancellationToken = default)
    {
        return await TriggerStateTransitionAsync(planId, WorkflowEvent.StartTask, null, cancellationToken);
    }

    public async Task<bool> PauseWorkflowAsync(string planId, CancellationToken cancellationToken = default)
    {
        return await TriggerStateTransitionAsync(planId, WorkflowEvent.NeedIntervention, "User requested pause.", cancellationToken);
    }

    public async Task<bool> ResumeWorkflowAsync(string planId, CancellationToken cancellationToken = default)
    {
        return await TriggerStateTransitionAsync(planId, WorkflowEvent.UserResume, null, cancellationToken);
    }

    public async Task<bool> TerminateWorkflowAsync(string planId, CancellationToken cancellationToken = default)
    {
        return await TriggerStateTransitionAsync(planId, WorkflowEvent.UserTerminate, "User requested termination.", cancellationToken);
    }

    public async Task<bool> TriggerStateTransitionAsync(string planId, WorkflowEvent @event, string? interventionReason = null, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(planId, out var id))
        {
            _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
            return false;
        }

        var engine = await GetOrCreateEngineAsync(id, cancellationToken);
        if (engine == null)
        {
            return false;
        }

        try
        {
            await engine.TriggerEventAsync(@event, new ManualInterventionInfo { Reason = interventionReason });

            _logger.LogInformation("Workflow {PlanId} transitioned to state {State} via event {Event}",
                planId, engine.CurrentState, @event);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger state transition for plan {PlanId} with event {Event}", planId, @event);
            return false;
        }
    }

    private async Task PersistEngineStateAsync(Guid planId, WorkflowState state, string? interventionReason, CancellationToken cancellationToken)
    {
        await _repository.UpdatePlanStateAsync(planId, state, interventionReason, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<IWorkflowContext?> GetWorkflowContextAsync(string planId)
    {
        if (!Guid.TryParse(planId, out var id))
        {
            _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
            return null;
        }

        var engine = await GetOrCreateEngineAsync(id, default);
        return engine?.GetContext();
    }

    /// <summary>
    /// Update step status in a plan
    /// 更新计划中的步骤状态
    /// 
    /// 对应AI-Agent中的mark_step功能
    /// Corresponds to mark_step functionality in AI-Agent
    /// </summary>
    public async Task<bool> UpdateStepStatusAsync(string planId, int stepIndex, PlanStepStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id))
            {
                _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
                return false;
            }

            var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
            if (planEntity == null)
            {
                _logger.LogWarning("Plan not found: {PlanId}", planId);
                return false;
            }

            var stepEntity = planEntity.Steps.FirstOrDefault(s => s.Index == stepIndex);
            if (stepEntity == null)
            {
                _logger.LogWarning("Invalid step index: {StepIndex} for plan: {PlanId}", stepIndex, planId);
                return false;
            }

            // 更新时间戳 - Update timestamps
            DateTime? startedAt = null;
            DateTime? completedAt = null;
            if (status == PlanStepStatus.InProgress)
            {
                startedAt = DateTime.UtcNow;
            }
            else if (status == PlanStepStatus.Completed)
            {
                completedAt = DateTime.UtcNow;
            }

            // 使用 Repository 的专用方法更新状态 (Use Repository's dedicated method to update status)
            await _repository.UpdateStepStatusAndResultAsync(
                id,
                stepIndex,
                status,
                stepEntity.Result, // 保持 Result 不变
                stepEntity.Error,  // 保持 Error 不变
                startedAt,
                completedAt,
                cancellationToken);

            _logger.LogDebug("Updated step {StepIndex} status to {Status} for plan {PlanId}",
                stepIndex, status, planId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step status for plan: {PlanId}, step: {StepIndex}", planId, stepIndex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStepStatusAndResultAsync(
        string planId,
        int stepIndex,
        PlanStepStatus status,
        string? result = null,
        string? error = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id)) return false;

            await _repository.UpdateStepStatusAndResultAsync(
                id,
                stepIndex,
                status,
                result,
                error,
                null,
                null,
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step status and result for plan {PlanId}", planId);
            return false;
        }
    }

    /// <summary>
    /// Get current active step in a plan
    /// 获取计划中当前活动步骤
    /// 
    /// 对应AI-Agent中的_get_current_step_info方法
    /// Corresponds to _get_current_step_info method in AI-Agent
    /// </summary>
    public async Task<WorkflowStep?> GetCurrentStepAsync(string planId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(planId, out var id))
        {
            _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
            return null;
        }

        var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
        if (planEntity == null)
        {
            _logger.LogWarning("Plan not found: {PlanId}", planId);
            return null;
        }

        // 查找第一个活动步骤 - Find first active step
        var activeStepEntity = planEntity.Steps
                .OrderBy(s => s.Index)
                .FirstOrDefault(s => (s.Status == PlanStepStatus.NotStarted || s.Status == PlanStepStatus.InProgress));

        if (activeStepEntity != null)
            {
                // 如果步骤未开始，标记为进行中 - If step not started, mark as in progress
                if (activeStepEntity.Status == PlanStepStatus.NotStarted)
                {
                    // 自动将状态更新为 InProgress
                    await UpdateStepStatusAsync(planId, activeStepEntity.Index, PlanStepStatus.InProgress, cancellationToken);
                    // 触发状态机转换 (Trigger state machine transition)
                    await TriggerStateTransitionAsync(planId, WorkflowEvent.StartTask, null, cancellationToken);
                    // 重新获取更新后的实体 (Re-fetch the updated entity)
                    planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
                    activeStepEntity = planEntity?.Steps.FirstOrDefault(s => s.Index == activeStepEntity.Index);
                }

                return activeStepEntity.Adapt<WorkflowStep>();
            }

        // 没有找到活动步骤 - No active step found
        return null;
    }

    /// <summary>
    /// Mark step as completed and move to next
    /// 标记步骤为已完成并移动到下一步
    /// </summary>
    public async Task<bool> CompleteStepAsync(string planId, int stepIndex, string? result = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id))
            {
                _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
                return false;
            }

            var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
            var stepEntity = planEntity?.Steps.FirstOrDefault(s => s.Index == stepIndex);

            if (stepEntity == null)
            {
                _logger.LogWarning("Step not found: {StepIndex} for plan: {PlanId}", stepIndex, planId);
                return false;
            }

            // 1. 敏感标签检测 (Sensitive tag detection)
            if (stepEntity.Text.Contains("[SENSITIVE]") || stepEntity.Text.Contains("[PAYMENT]"))
            {
                await TriggerStateTransitionAsync(planId, WorkflowEvent.NeedIntervention, $"Step {stepIndex} contains sensitive tag and requires manual approval.", cancellationToken);
                _logger.LogWarning("Step {StepIndex} for plan {PlanId} triggered ManualIntervention due to sensitive tag.", stepIndex, planId);
                return false; // 阻止完成，等待人工干预
            }

            // 2. 更新状态为 Completed (Update status to Completed)
            var success = await UpdateStepStatusAsync(planId, stepIndex, PlanStepStatus.Completed, cancellationToken);

            // 3. 如果成功且有结果，则更新结果 (If successful and result exists, update result)
            if (success && !string.IsNullOrEmpty(result))
            {
                // 使用 Repository 的专用方法更新结果 (Use Repository's dedicated method to update result)
                await _repository.UpdateStepStatusAndResultAsync(
                    id,
                    stepIndex,
                    PlanStepStatus.Completed, // 确保状态是 Completed
                    result,
                    null,
                    null,
                    DateTime.UtcNow,
                    cancellationToken);
            }

            // 4. 触发状态机转换 (Trigger state machine transition)
            await TriggerStateTransitionAsync(planId, WorkflowEvent.StepComplete, null, cancellationToken);

            // 5. 重置失败次数 (Reset failure count)
            var plan = await GetPlanAsync(planId, cancellationToken);
            if (plan != null && plan.FailureCount > 0)
            {
                await _repository.UpdatePlanFailureCountAsync(id, 0, cancellationToken);
            }

            _logger.LogInformation("Completed step {StepIndex} for plan {PlanId}", stepIndex, planId);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step for plan: {PlanId}, step: {StepIndex}", planId, stepIndex);
            return false;
        }
    }

    /// <summary>
    /// Mark step as failed
    /// 标记步骤为失败
    /// </summary>
    public async Task<bool> FailStepAsync(string planId, int stepIndex, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id))
            {
                _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
                return false;
            }

            var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
            var stepEntity = planEntity?.Steps.FirstOrDefault(s => s.Index == stepIndex);

            if (stepEntity == null)
            {
                _logger.LogWarning("Step not found: {StepIndex} for plan: {PlanId}", stepIndex, planId);
                return false;
            }

            // 1. 更新状态为 Failed (Update status to Failed)
            var success = await UpdateStepStatusAsync(planId, stepIndex, PlanStepStatus.Failed, cancellationToken);

            // 2. 更新失败原因 (Update failure reason in result field)
            if (success)
            {
                // 使用 Repository 的专用方法更新结果 (Use Repository's dedicated method to update result)
                await _repository.UpdateStepStatusAndResultAsync(
                    id,
                    stepIndex,
                    PlanStepStatus.Failed,
                    null,
                    reason ?? "Step failed without specific reason.",
                    null,
                    DateTime.UtcNow,
                    cancellationToken);
            }

            // 3. 触发状态机转换 (Trigger state machine transition)
            await TriggerStateTransitionAsync(planId, WorkflowEvent.StepFailed, reason, cancellationToken);

            // 4. 失败次数检测 (Failure count detection)
            var plan = await GetPlanAsync(planId, cancellationToken);
            if (plan != null)
            {
                var newFailureCount = plan.FailureCount + 1;
                await _repository.UpdatePlanFailureCountAsync(id, newFailureCount, cancellationToken);

                if (newFailureCount >= 3)
                {
                    await TriggerStateTransitionAsync(planId, WorkflowEvent.NeedIntervention, "Three consecutive step failures detected. Manual intervention required.", cancellationToken);
                    _logger.LogError("Plan {PlanId} triggered ManualIntervention after 3 consecutive failures.", planId);
                }
            }

            _logger.LogWarning("Failed step {StepIndex} for plan {PlanId}. Reason: {Reason}", stepIndex, planId, reason);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing step for plan: {PlanId}, step: {StepIndex}", planId, stepIndex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SyncPlanFromToDoListAsync(string planId, string filePath, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(planId, out var id)) return false;

        try 
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Todo list file not found for sync: {FilePath}", filePath);
                return false;
            }

            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var updatedSteps = ParseToDoList(content);

            var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
            if (planEntity == null) return false;

            var plan = WorkflowPlanMappingExtensions.ToModel(planEntity);

            // 同步步骤状态 (Sync step statuses)
            foreach (var updatedStep in updatedSteps)
            {
                var existingStep = plan.Steps.FirstOrDefault(s => s.Index == updatedStep.Index);
                if (existingStep != null)
                {
                    existingStep.Status = updatedStep.Status;
                    existingStep.Text = updatedStep.Text;
                }
                else 
                {
                    plan.Steps.Add(updatedStep);
                }
            }

            await _repository.UpdatePlanStepsAsync(id, plan.Steps);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing plan from todo list file: {PlanId}", planId);
            return false;
        }
    }

    private List<WorkflowStep> ParseToDoList(string content)
    {
        var steps = new List<WorkflowStep>();
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int currentIndex = 0;

        foreach (var line in lines)
        {
            // 匹配 Markdown 任务列表格式 (Match Markdown task list format)
            // - [x] Task description
            // - [ ] Task description
            var match = Regex.Match(line, @"^\s*-\s*\[(x| )\]\s*(.*)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                bool isCompleted = match.Groups[1].Value.ToLower() == "x";
                string taskText = match.Groups[2].Value.Trim();

                steps.Add(new WorkflowStep
                {
                    Index = currentIndex++,
                    Text = taskText,
                    Status = isCompleted ? PlanStepStatus.Completed : PlanStepStatus.NotStarted,
                    Type = ExtractStepType(taskText)
                });
            }
        }

        return steps;
    }

    /// <summary>
    /// Generate todo list file content for a plan
    /// 为计划生成待办事项列表内容
    /// </summary>
    /// 这是AI-Agent中的核心功能，生成markdown格式的任务列表
    /// This is a core feature from AI-Agent, generating markdown format task lists
    /// </summary>
    public async Task<string> GenerateToDoListAsync(string planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await GetPlanAsync(planId, cancellationToken);
            if (plan == null)
            {
                throw new ArgumentException($"Plan not found: {planId}");
            }

            var todoContent = new List<string>
            {
                $"# {plan.Title}",
                "",
                $"**计划ID**: {plan.Id}",
                $"**描述**: {plan.Description}",
                "",
                "## 步骤列表 (Steps List)",
                ""
            };

            foreach (var step in plan.Steps)
            {
                var statusChar = step.Status.ToString();
                var resultLine = string.IsNullOrEmpty(step.Result) ? "" : $" (结果: {step.Result})";
                todoContent.Add($"- [{statusChar}] {step.Text}{resultLine}");
            }

            return string.Join(Environment.NewLine, todoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating todo list for plan: {PlanId}", planId);
            throw;
        }
    }

    /// <summary>
    /// Save the workflow plan as a Markdown todo list to a specified file path
    /// 将工作流计划作为Markdown待办事项列表保存到指定文件路径
    /// </summary>
    public async Task<bool> SaveToDoListToFileAsync(string planId, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var todoContent = await GenerateToDoListAsync(planId, cancellationToken);
            await System.IO.File.WriteAllTextAsync(filePath, todoContent, cancellationToken);
            _logger.LogInformation("Successfully saved todo list for plan {PlanId} to {FilePath}", planId, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving todo list for plan {PlanId} to {FilePath}", planId, filePath);
            return false;
        }
    }

    /// <summary>
    /// Load a workflow plan from a Markdown todo list file
    /// 从Markdown待办事项列表文件加载工作流计划
    /// </summary>
    public async Task<string?> LoadToDoListFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError("Todo list file not found at {FilePath}", filePath);
                return null;
            }

            var lines = await System.IO.File.ReadAllLinesAsync(filePath, cancellationToken);

            var title = lines.FirstOrDefault(l => l.StartsWith("# "))?.Substring(2).Trim() ?? "Loaded Plan";
            var description = lines.FirstOrDefault(l => l.StartsWith("**描述**: "))?.Substring(8).Trim() ?? "Loaded from file";

            var steps = lines.Where(l => l.StartsWith("- [")).Select(l =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(l, @"- \[.\] (.*?)( \(结果: .*\))?$");
                return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
            }).Where(s => !string.IsNullOrEmpty(s)).Select(s => new WorkflowStepDto { Text = s, Type = "task" }).ToList();

            var createRequest = new CreatePlanRequest
            {
                Title = title,
                Description = description,
                Steps = steps
            };

            var plan = await CreatePlanAsync(createRequest, cancellationToken);
            _logger.LogInformation("Successfully loaded plan {PlanId} from {FilePath}", plan.Id, filePath);
            return plan.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading todo list from {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Soft delete a workflow plan by marking its status as Deleted
    /// 通过将工作流计划的状态标记为“已删除”来软删除它
    /// </summary>
    public async Task<bool> DeletePlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id))
            {
                _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
                return false;
            }

            var success = await _repository.UpdatePlanStatusAsync(id, PlanStatus.Deleted, cancellationToken);
            if (success)
            {
                _logger.LogInformation("Soft deleted workflow plan with ID: {PlanId}", planId);
            }
            else
            {
                _logger.LogWarning("Failed to soft delete workflow plan with ID: {PlanId}", planId);
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting workflow plan: {PlanId}", planId);
            return false;
        }
    }

    /// <summary>
    /// Get the progress of a workflow plan, including percentage and estimated time remaining
    /// 获取工作流计划的进度，包括百分比和预计剩余时间
    /// </summary>
    public async Task<WorkflowProgress> GetProgressAsync(string planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(planId, cancellationToken);
        if (plan == null)
        {
            throw new ArgumentException($"Plan not found: {planId}");
        }

        var completedSteps = plan.Steps.Count(s => s.Status == PlanStepStatus.Completed);
        var totalSteps = plan.Steps.Count;

        if (totalSteps == 0)
        {
            return new WorkflowProgress { Percentage = 0, EstimatedTimeRemaining = TimeSpan.Zero };
        }

        var percentage = (double)completedSteps / totalSteps * 100;

        // 估算剩余时间 (Estimate remaining time)
        TimeSpan estimatedTimeRemaining = TimeSpan.Zero;
        if (completedSteps > 0)
        {
            var averageTimePerStep = plan.Steps
                .Where(s => s.Status == PlanStepStatus.Completed && s.StartedAt.HasValue && s.CompletedAt.HasValue)
                .Select(s => s.CompletedAt.Value - s.StartedAt.Value)
                .DefaultIfEmpty(TimeSpan.Zero)
                .Average(t => t.TotalSeconds);

            var remainingSteps = totalSteps - completedSteps;
            estimatedTimeRemaining = TimeSpan.FromSeconds(averageTimePerStep * remainingSteps);
        }

        return new WorkflowProgress
        {
            Percentage = Math.Round(percentage, 2),
            EstimatedTimeRemaining = estimatedTimeRemaining
        };
    }

    public Task<string> CreateWorkflowAsync(string llmResponse)
    {
        throw new NotImplementedException();
    }

    // --- Visualization & Debugging (可视化与调试) ---

    /// <inheritdoc />
    public async Task<bool> UpdateVisualGraphAsync(string planId, string visualGraphJson, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id)) return false;
            var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
            if (planEntity == null) return false;

            planEntity.VisualGraphJson = visualGraphJson;
            planEntity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdatePlanAsync(planEntity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating visual graph for plan {PlanId}", planId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ToggleBreakpointAsync(string planId, int stepIndex, bool isBreakpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(planId, out var id)) return false;
            var success = await _repository.UpdateStepBreakpointAsync(id, stepIndex, isBreakpoint, cancellationToken);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling breakpoint for plan {PlanId}, step {StepIndex}", planId, stepIndex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<WorkflowPerformanceReport> GetPerformanceReportAsync(string planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(planId, cancellationToken);
        if (plan == null) throw new ArgumentException($"Plan not found: {planId}");

        var report = new WorkflowPerformanceReport
        {
            PlanId = plan.Id,
            Title = plan.Title,
            StepMetrics = plan.Steps.ToDictionary(s => s.Index, s => new StepPerformanceMetric
            {
                Text = s.Text,
                Duration = (s.CompletedAt ?? DateTime.UtcNow) - (s.StartedAt ?? DateTime.UtcNow),
                IsSuccess = s.Status == PlanStepStatus.Completed,
                Cost = 0, // Placeholder
                IsBottleneck = false
            })
        };

        report.TotalDuration = report.StepMetrics.Values.Aggregate(TimeSpan.Zero, (sum, m) => sum + m.Duration);
        var completedCount = plan.Steps.Count(s => s.Status == PlanStepStatus.Completed);
        report.SuccessRate = plan.Steps.Count > 0 ? (double)completedCount / plan.Steps.Count : 0;

        // Simple bottleneck identification: steps taking > 20% of total time
        if (report.TotalDuration.TotalSeconds > 0)
        {
            foreach (var kvp in report.StepMetrics)
            {
                var metric = kvp.Value;
                if (metric.Duration.TotalSeconds / report.TotalDuration.TotalSeconds > 0.2)
                {
                    metric.IsBottleneck = true;
                    report.OptimizationSuggestions.Add($"Step {kvp.Key} ({metric.Text}) is a bottleneck. Consider parallelization or optimization.");
                }
            }
        }

        return report;
    }

    /// <inheritdoc />
    public async Task<string> ExportWorkflowAsync(string planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(planId, cancellationToken);
        if (plan == null) throw new ArgumentException($"Plan not found: {planId}");
        return JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <inheritdoc />
    public async Task<WorkflowPlan> ImportWorkflowAsync(string jsonContent, CancellationToken cancellationToken = default)
    {
        var plan = JsonSerializer.Deserialize<WorkflowPlan>(jsonContent);
        if (plan == null) throw new ArgumentException("Invalid JSON content");

        var createRequest = new CreatePlanRequest
        {
            Title = plan.Title,
            Description = plan.Description,
            Steps = plan.Steps.Select(s => new WorkflowStepDto { Text = s.Text, Type = s.Type, DependsOn = s.DependsOn, Condition = s.Condition }).ToList(),
            ExecutorKeys = plan.ExecutorKeys,
            Metadata = plan.Metadata
        };

        var newPlan = await CreatePlanAsync(createRequest, cancellationToken);

        // If the imported plan has visual data, update it
        if (!string.IsNullOrEmpty(plan.VisualGraphJson))
        {
            await UpdateVisualGraphAsync(newPlan.Id.ToString(), plan.VisualGraphJson, cancellationToken);
        }

        return await GetPlanAsync(newPlan.Id.ToString(), cancellationToken) ?? newPlan;
    }

    /// <summary>
    /// Helper method to extract step type from text (e.g., [CODE], [SEARCH])
    /// 辅助方法：从文本中提取步骤类型
    /// </summary>
    private string? ExtractStepType(string stepText)
    {
        var match = System.Text.RegularExpressions.Regex.Match(stepText, @"^\[(\w+)\]");
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }
}


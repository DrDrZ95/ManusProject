namespace Agent.Application.Services.Workflow;

/// <summary>
/// 工作流执行引擎 (Workflow Execution Engine)
/// 实现了状态机逻辑，负责管理工作流的状态转换和执行流程。
/// Implements the state machine logic, responsible for managing workflow state transitions and execution flow.
/// </summary>
public class WorkflowExecutionEngine : IWorkflowEngine
{
    private const string SkillsFolderName = "skills";
    private const string PlanPromptFile = "PLAN_GENERATION_PROMPT.md";
    private const string StepPromptFile = "STEP_EXECUTOR_PROMPT.md";
    private const string DynamicInjectionPromptFile = "DYNAMIC_INJECTION_PROMPT.md";
    private readonly StateMachine<WorkflowState, WorkflowEvent> _stateMachine;
    private readonly Guid _planId;
    private readonly IWorkflowNotificationService _notificationService;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly ISmartToolSelector _smartToolSelector;
    private readonly WorkflowContext _context;
    private readonly ILogger<WorkflowExecutionEngine> _logger;
    private readonly IAgentTraceService? _agentTraceService;
    private readonly Action<Guid>? _onFinalized;
    private DateTime? _stepStartTime;

    /// <inheritdoc />
    public WorkflowState CurrentState => _stateMachine.State;

    public WorkflowExecutionEngine(
        Guid planId,
        WorkflowState initialState,
        IWorkflowNotificationService notificationService,
        IWorkflowRepository workflowRepository,
        ISemanticKernelService semanticKernelService, // 注入
        ISmartToolSelector smartToolSelector, // 注入
        ILogger<WorkflowExecutionEngine> logger,
        IAgentTraceService? agentTraceService = null,
        WorkflowContext? existingContext = null,
        Action<Guid>? onFinalized = null)
    {
        _planId = planId;
        _notificationService = notificationService;
        _workflowRepository = workflowRepository;
        _semanticKernelService = semanticKernelService;
        _smartToolSelector = smartToolSelector;
        _logger = logger;
        _agentTraceService = agentTraceService;
        _context = existingContext ?? new WorkflowContext(planId.GetHashCode());
        _onFinalized = onFinalized;

        _stateMachine = new StateMachine<WorkflowState, WorkflowEvent>(
            () => _context.TaskId == 0 ? initialState : (WorkflowState)initialState,
            s => { /* State is managed via _context or persistence if needed */ });

        ConfigureStateMachine();
    }

    private void ConfigureStateMachine()
    {
        _stateMachine.Configure(WorkflowState.Idle)
            .Permit(WorkflowEvent.StartTask, WorkflowState.Initializing);

        _stateMachine.Configure(WorkflowState.Initializing)
            .OnEntryAsync(OnInitializingEntry)
            .Permit(WorkflowEvent.InitializationComplete, WorkflowState.Planning)
            .Permit(WorkflowEvent.InitializationFailed, WorkflowState.Failed);

        _stateMachine.Configure(WorkflowState.Planning)
            .OnEntryAsync(OnPlanningEntry)
            .Permit(WorkflowEvent.PlanReady, WorkflowState.Executing)
            .Permit(WorkflowEvent.NeedIntervention, WorkflowState.ManualIntervention)
            .Permit(WorkflowEvent.PlanningFailed, WorkflowState.Failed);

        _stateMachine.Configure(WorkflowState.Executing)
            .OnEntryAsync(OnExecutingEntry)
            .Permit(WorkflowEvent.StepComplete, WorkflowState.Planning)
            .Permit(WorkflowEvent.NeedIntervention, WorkflowState.ManualIntervention)
            .Permit(WorkflowEvent.ExecutionComplete, WorkflowState.Completed)
            .Permit(WorkflowEvent.StepFailed, WorkflowState.Failed);

        _stateMachine.Configure(WorkflowState.ManualIntervention)
            .OnEntryAsync(OnManualInterventionEntry)
            .OnExitAsync(OnManualInterventionExit)
            .Permit(WorkflowEvent.UserResume, WorkflowState.Executing)
            .Permit(WorkflowEvent.UserModify, WorkflowState.Planning)
            .Permit(WorkflowEvent.UserTerminate, WorkflowState.Terminated);

        _stateMachine.Configure(WorkflowState.Completed)
            .OnEntryAsync(() => HandleFinalState(WorkflowState.Completed))
            .Permit(WorkflowEvent.StartTask, WorkflowState.Initializing);

        _stateMachine.Configure(WorkflowState.Failed)
            .OnEntryAsync(() => HandleFinalState(WorkflowState.Failed))
            .Permit(WorkflowEvent.StartTask, WorkflowState.Initializing);

        _stateMachine.Configure(WorkflowState.Terminated)
            .OnEntryAsync(() => HandleFinalState(WorkflowState.Terminated))
            .Permit(WorkflowEvent.StartTask, WorkflowState.Initializing);

        // Global transition monitoring
        _stateMachine.OnTransitionedAsync(OnTransitioned);
    }

    private async Task OnTransitioned(StateMachine<WorkflowState, WorkflowEvent>.Transition transition)
    {
        _logger.LogInformation("[WorkflowEngine] State changed from {OldState} to {NewState} via event {Trigger}.",
            transition.Source, transition.Destination, transition.Trigger);

        _context.ExecutionHistory.Add($"[{DateTime.UtcNow:O}] {transition.Source} --({transition.Trigger})--> {transition.Destination}");

        // Handle step performance recording
        if (transition.Source == WorkflowState.Executing)
        {
            if (transition.Trigger == WorkflowEvent.StepComplete)
            {
                await RecordStepPerformanceAsync(true);
                _context.CurrentStepIndex++; // Move to next step index after completion
            }
            else if (transition.Trigger == WorkflowEvent.StepFailed)
            {
                await RecordStepPerformanceAsync(false);
            }
        }

        if (_agentTraceService != null)
        {
            var sessionId = _context.InputParameters.TryGetValue("sessionId", out var rawSessionId) && rawSessionId is string sid && !string.IsNullOrWhiteSpace(sid)
                ? sid
                : "workflow";

            var metadata = new Dictionary<string, object>
            {
                ["planId"] = _planId.ToString(),
                ["sourceState"] = transition.Source.ToString(),
                ["destinationState"] = transition.Destination.ToString(),
                ["trigger"] = transition.Trigger.ToString()
            };

            var trace = new AgentTrace
            {
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow,
                Type = TraceType.Result,
                Data = new Dictionary<string, object>
                {
                    ["planId"] = _planId.ToString(),
                    ["state"] = transition.Destination.ToString()
                },
                Metadata = metadata
            };

            await _agentTraceService.RecordTraceAsync(trace);
        }

        // Persistence
        await SaveStateAndContextAsync();

        // Notification
        await _notificationService.BroadcastStateChange(
            _planId.ToString(),
            new StateChangeNotificationDto(
                _planId.ToString(),
                transition.Source.ToString(),
                transition.Destination.ToString(),
                DateTime.UtcNow
            )
        );
    }

    private async Task SaveStateAndContextAsync()
    {
        try
        {
            var contextJson = JsonSerializer.Serialize(_context);
            await _workflowRepository.UpdatePlanStateAsync(_planId, _stateMachine.State, _context.InterventionInfo?.Reason);
            await _workflowRepository.UpdatePlanContextAsync(_planId, contextJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist workflow state and context for plan {PlanId}", _planId);
        }
    }

    private async Task OnInitializingEntry()
    {
        _logger.LogInformation("Workflow {PlanId} initializing...", _planId);
        // Initialization logic would go here
        // For now, auto-complete if successful
        await TriggerEventAsync(WorkflowEvent.InitializationComplete);
    }

    private async Task OnPlanningEntry()
    {
        _logger.LogInformation("Workflow {PlanId} planning...", _planId);

        try
        {
            // 如果已有步骤，则直接进入执行
            var plan = await _workflowRepository.GetPlanByIdAsync(_planId, default);
            if (plan?.Steps.Any() == true)
            {
                await TriggerEventAsync(WorkflowEvent.PlanReady);
                return;
            }

            string? userGoal = null;
            if (_context.InputParameters.TryGetValue("user_objective", out var uo) && uo is string uoStr && !string.IsNullOrWhiteSpace(uoStr))
            {
                userGoal = uoStr;
            }
            else if (_context.InputParameters.TryGetValue("userGoal", out var ug) && ug is string ugStr && !string.IsNullOrWhiteSpace(ugStr))
            {
                userGoal = ugStr;
            }

            if (string.IsNullOrWhiteSpace(userGoal))
            {
                throw new InvalidOperationException("User goal is missing in workflow context.");
            }

            var template = ReadSkillTemplate(PlanPromptFile);
            var sessionId = _context.InputParameters.TryGetValue("sessionId", out var sidObj) && sidObj is string sidStr && !string.IsNullOrWhiteSpace(sidStr)
                ? sidStr
                : _planId.ToString();
            var contextSummary = BuildContextSummary();
            var timestamp = DateTime.UtcNow.ToString("O");

            var planningPrompt = template
                .Replace("{{user_objective}}", userGoal)
                .Replace("{{context}}", contextSummary)
                .Replace("{{session_id}}", sessionId)
                .Replace("{{timestamp}}", timestamp);

            var planResponse = await _semanticKernelService.GetChatCompletionAsync(planningPrompt);

            var json = ExtractJson(planResponse);
            var root = JsonDocument.Parse(json).RootElement;

            if (!root.TryGetProperty("steps", out var stepsEl) || stepsEl.ValueKind != JsonValueKind.Array || stepsEl.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("LLM failed to generate a valid plan.");
            }

            var steps = new List<WorkflowStep>();
            foreach (var s in stepsEl.EnumerateArray())
            {
                var text = s.TryGetProperty("text", out var te) ? te.GetString() ?? "" : "";
                var type = s.TryGetProperty("type", out var ty) ? ty.GetString() ?? "" : "";
                var dependsOn = new List<int>();
                if (s.TryGetProperty("depends_on", out var dep) && dep.ValueKind == JsonValueKind.Array)
                {
                    foreach (var di in dep.EnumerateArray())
                    {
                        if (di.TryGetInt32(out var idx)) dependsOn.Add(idx);
                    }
                }
                steps.Add(new WorkflowStep
                {
                    Index = s.TryGetProperty("index", out var ix) && ix.TryGetInt32(out var vi) ? vi : steps.Count,
                    Text = text,
                    Type = string.IsNullOrWhiteSpace(type) ? "task" : type,
                    DependsOn = dependsOn,
                    Status = PlanStepStatus.NotStarted
                });
            }

            var workflowSteps = steps.OrderBy(s => s.Index).Select((s, i) =>
            {
                s.Index = i;
                return s;
            }).ToList();

            await _workflowRepository.UpdatePlanStepsAsync(_planId, workflowSteps);

            if (workflowSteps.All(s => s.Status == PlanStepStatus.Completed || s.Status == PlanStepStatus.Skipped))
            {
                await TriggerEventAsync(WorkflowEvent.ExecutionComplete);
            }
            else
            {
                await TriggerEventAsync(WorkflowEvent.PlanReady);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Planning failed for workflow {PlanId}", _planId);
            await TriggerEventAsync(WorkflowEvent.PlanningFailed, new WorkflowError { Message = ex.Message });
        }
    }

    private async Task OnExecutingEntry()
    {
        _logger.LogInformation("Workflow {PlanId} executing step {StepIndex}...", _planId, _context.CurrentStepIndex);
        _stepStartTime = DateTime.UtcNow;

        try
        {
            var planEntity = await _workflowRepository.GetPlanByIdAsync(_planId, default);
            if (planEntity == null)
            {
                await TriggerEventAsync(WorkflowEvent.ExecutionComplete);
                return;
            }

            var plan = WorkflowPlanMappingExtensions.ToModel(planEntity);
            if (_context.CurrentStepIndex >= plan.Steps.Count)
            {
                await TriggerEventAsync(WorkflowEvent.ExecutionComplete);
                return;
            }

            var currentStep = plan.Steps.First(s => s.Index == _context.CurrentStepIndex);

            // 检查前置依赖 (Check prerequisites)
            if (currentStep.DependsOn != null && currentStep.DependsOn.Any())
            {
                foreach (var depIndex in currentStep.DependsOn)
                {
                    var depStep = plan.Steps.FirstOrDefault(s => s.Index == depIndex);
                    if (depStep != null && depStep.Status != PlanStepStatus.Completed)
                    {
                        _logger.LogInformation("Step {StepIndex} waiting for prerequisite {PrereqIndex}", currentStep.Index, depIndex);
                        // 如果前置未完成，且不是正在运行，则可能需要干预或跳过 (Wait or handle appropriately)
                    }
                }
            }

            // 检查执行条件 (Check execution condition)
            if (!string.IsNullOrEmpty(currentStep.Condition))
            {
                bool shouldExecute = await EvaluateConditionAsync(currentStep.Condition);
                if (!shouldExecute)
                {
                    _logger.LogInformation("Step {StepIndex} condition not met, skipping: {Condition}", currentStep.Index, currentStep.Condition);
                    await _workflowRepository.UpdateStepStatusAndResultAsync(_planId, currentStep.Index, PlanStepStatus.Skipped, "Condition not met", null, _stepStartTime, DateTime.UtcNow);
                    await TriggerEventAsync(WorkflowEvent.StepComplete);
                    return;
                }
            }

            await _workflowRepository.UpdateStepStatusAndResultAsync(_planId, currentStep.Index, PlanStepStatus.InProgress, null, null, _stepStartTime, default);

            var execTemplate = ReadSkillTemplate(StepPromptFile);
            var completedSummary = BuildCompletedSummary(plan);
            var recommendedTools = await _smartToolSelector.RecommendToolsAsync(currentStep.Text, 5);
            if (plan.ExecutorKeys != null && plan.ExecutorKeys.Any())
            {
                recommendedTools = recommendedTools.Where(t =>
                    plan.ExecutorKeys.Contains(t.PluginName) ||
                    plan.ExecutorKeys.Contains(t.FunctionName) ||
                    plan.ExecutorKeys.Contains($"{t.PluginName}.{t.FunctionName}"));
            }
            var toolManifest = string.Join("\n", recommendedTools.Select(t => $"- {t.PluginName}.{t.FunctionName}: {t.Description}"));

            var execPrompt = execTemplate
                .Replace("{{task_id}}", currentStep.Index.ToString())
                .Replace("{{task_text}}", currentStep.Text)
                .Replace("{{task_type}}", string.IsNullOrWhiteSpace(currentStep.Type) ? "task" : currentStep.Type)
                .Replace("{{expected_output}}", "")
                .Replace("{{phase_name}}", "Main")
                .Replace("{{completed_tasks_summary}}", completedSummary)
                .Replace("{{tool_manifest}}", toolManifest);

            var execResponse = await _semanticKernelService.GetChatCompletionAsync(execPrompt);
            var execJson = ExtractJson(execResponse);
            using var doc = JsonDocument.Parse(execJson);
            var root = doc.RootElement;

            var status = root.TryGetProperty("status", out var st) ? st.GetString() ?? "FAILED" : "FAILED";
            var toolUsed = root.TryGetProperty("tool_used", out var tu) ? tu.GetString() : null;
            var args = new Dictionary<string, object>();
            if (root.TryGetProperty("tool_arguments", out var ta) && ta.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in ta.EnumerateObject())
                {
                    args[p.Name] = p.Value.ValueKind switch
                    {
                        JsonValueKind.String => p.Value.GetString()!,
                        JsonValueKind.Number => p.Value.ToString(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => p.Value.ToString()
                    };
                }
            }
            var resultSummary = root.TryGetProperty("result_summary", out var rs) ? rs.GetString() ?? "" : "";
            var outputArtifact = root.TryGetProperty("output_artifact", out var oa) ? oa.GetString() : null;
            var inject = root.TryGetProperty("should_inject_tasks", out var si) && si.ValueKind == JsonValueKind.True;
            var injectReason = root.TryGetProperty("injection_reason", out var ir) ? ir.GetString() : null;

            string finalResult = outputArtifact ?? resultSummary;

            if (!string.IsNullOrWhiteSpace(toolUsed))
            {
                var parts = toolUsed.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var plugin = parts[0];
                    var func = parts[1];
                    var selected = recommendedTools.FirstOrDefault(t =>
                        string.Equals(t.PluginName, plugin, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(t.FunctionName, func, StringComparison.OrdinalIgnoreCase));
                    if (selected != null)
                    {
                        try
                        {
                            var toolResult = await _semanticKernelService.InvokeFunctionAsync<string>(selected.PluginName, selected.FunctionName, args);
                            if (!string.IsNullOrWhiteSpace(toolResult))
                            {
                                finalResult = string.IsNullOrWhiteSpace(finalResult) ? toolResult : $"{finalResult}\n{toolResult}";
                            }
                        }
                        catch (Exception tex)
                        {
                            _logger.LogWarning(tex, "Tool invocation failed for {Tool}", toolUsed);
                        }
                    }
                }
            }

            _context.IntermediateResults[currentStep.Index.ToString()] = finalResult;

            if (inject && !string.IsNullOrWhiteSpace(injectReason))
            {
                await TryDynamicInjectionAsync(plan, currentStep.Index, finalResult, injectReason);
            }

            if (string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                await _workflowRepository.UpdateStepStatusAndResultAsync(_planId, currentStep.Index, PlanStepStatus.Completed, finalResult, null, _stepStartTime, DateTime.UtcNow);
                await TriggerEventAsync(WorkflowEvent.StepComplete);
            }
            else if (string.Equals(status, "NEEDS_RETRY", StringComparison.OrdinalIgnoreCase))
            {
                await _workflowRepository.UpdateStepStatusAndResultAsync(_planId, currentStep.Index, PlanStepStatus.InProgress, finalResult, null, _stepStartTime, default);
                await TriggerEventAsync(WorkflowEvent.NeedIntervention, new ManualInterventionInfo
                {
                    Reason = "Step requires retry based on executor output",
                    ProposedAction = currentStep.Text,
                    AgentThoughts = resultSummary
                });
            }
            else
            {
                await _workflowRepository.UpdateStepStatusAndResultAsync(_planId, currentStep.Index, PlanStepStatus.Failed, null, resultSummary, _stepStartTime, DateTime.UtcNow);
                await TriggerEventAsync(WorkflowEvent.StepFailed);
            }
        }
        catch (Exception ex)
        {
            await HandleExecutionErrorAsync(ex, "Failed to execute step");
        }
    }

    private async Task<bool> EvaluateConditionAsync(string condition)
    {
        // 简化的条件评估逻辑 (Simplified condition evaluation)
        // 在生产环境中应使用表达式解析器 (Use expression parser in production)
        try 
        {
            if (condition.ToLower().Contains("success")) return true;
            // 检查中间结果 (Check intermediate results)
            foreach (var res in _context.IntermediateResults)
            {
                if (condition.Contains($"step[{res.Key}].result"))
                {
                    var val = res.Value?.ToString() ?? "";
                    if (condition.Contains("=="))
                    {
                        var expected = condition.Split("==")[1].Trim(' ', '\'', '\"');
                        return val == expected;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating condition: {Condition}", condition);
        }
        return true; // 默认执行 (Execute by default)
    }

    private void ExtractAndRecordThinking(string content)
    {
        var match = Regex.Match(content, @"<thinking>(.*?)</thinking>", RegexOptions.Singleline);
        if (match.Success)
        {
            var thinking = match.Groups[1].Value.Trim();
            _context.ThinkingHistory.Add(thinking);
            _logger.LogInformation("[Thinking] {Thinking}", thinking);
        }
    }

    private async Task InjectDynamicTasksAsync(string executionResult)
    {
        _logger.LogInformation("Injecting dynamic tasks based on result: {Result}", executionResult);
        try 
        {
            var planEntity = await _workflowRepository.GetPlanByIdAsync(_planId, default);
            if (planEntity == null) return;

            var plan = WorkflowPlanMappingExtensions.ToModel(planEntity);

            // 简单模拟注入 (Simple mock injection)
            var newStep = new WorkflowStep 
            { 
                Index = plan.Steps.Count,
                Text = "Dynamic Follow-up Task",
                Type = "task",
                Status = PlanStepStatus.NotStarted
            };

            plan.Steps.Add(newStep);
            await _workflowRepository.UpdatePlanStepsAsync(_planId, plan.Steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject dynamic tasks");
        }
    }

    private async Task HandleExecutionErrorAsync(Exception ex, string message)
    {
        var error = new WorkflowError
        {
            Code = "EXECUTION_ERROR",
            Message = $"{message}: {ex.Message}",
            StackTrace = ex.StackTrace,
            Timestamp = DateTime.UtcNow,
            Suggestion = GetDiagnosticSuggestion(ex)
        };

        _context.Errors.Add(error);
        _logger.LogError(ex, "[WorkflowEngine] {Message} for Plan {PlanId}", message, _planId);

        // Check for automatic recovery (e.g., retry)
        if (ShouldRetry(error))
        {
            _logger.LogInformation("[WorkflowEngine] Attempting automatic retry for Plan {PlanId}", _planId);
            // In a real implementation, we might decrement a retry counter and stay in Executing or move back to Planning
            // For now, let's trigger StepFailed if retries are exhausted
        }

        // If failure count is too high, move to ManualIntervention instead of Failed
        // This allows for "Rollback" or "Skip" recovery strategies via user action
        if (_context.Errors.Count >= 3)
        {
            await TriggerEventAsync(WorkflowEvent.NeedIntervention, new ManualInterventionInfo
            {
                Reason = "Multiple consecutive errors detected. Manual intervention required for recovery."
            });
        }
        else
        {
            await TriggerEventAsync(WorkflowEvent.StepFailed);
        }
    }

    public async Task InjectDynamicTasksAsync(IEnumerable<string> taskDescriptions)
    {
        var plan = await _workflowRepository.GetPlanByIdAsync(_planId, default);
        if (plan == null) return;

        var newSteps = taskDescriptions.Select(desc => new WorkflowStep
        {
            Text = desc,
            Status = PlanStepStatus.NotStarted,
            // Index will be set by the repository
        }).ToList();

        await _workflowRepository.AddStepsToPlanAsync(_planId, newSteps);

        // Notify UI about the plan update
        var updatedPlan = await _workflowRepository.GetPlanByIdAsync(_planId, default);
        await _notificationService.BroadcastPlanUpdate(_planId.ToString(), updatedPlan.Adapt<WorkflowPlan>());
    }

    private async Task RecordStepPerformanceAsync(bool success)
    {
        if (!_stepStartTime.HasValue) return;

        var endTime = DateTime.UtcNow;
        var duration = endTime - _stepStartTime.Value;

        // Simple cost estimation placeholder (e.g., based on duration or step type)
        double estimatedCost = 0.002; // Placeholder cost in USD

        var performanceData = new Dictionary<string, object>
        {
            { "DurationMs", duration.TotalMilliseconds },
            { "StartTime", _stepStartTime.Value },
            { "EndTime", endTime },
            { "Success", success },
            { "Cost", estimatedCost },
            { "MachineName", Environment.MachineName }
        };

        try
        {
            // Update step with performance data and CompletedAt
            await _workflowRepository.UpdateStepPerformanceDataAsync(
                _planId,
                _context.CurrentStepIndex,
                JsonSerializer.Serialize(performanceData),
                endTime,
                default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record performance data for Plan {PlanId}, Step {StepIndex}", _planId, _context.CurrentStepIndex);
        }
        finally
        {
            _stepStartTime = null;
        }
    }

    private bool ShouldRetry(WorkflowError error)
    {
        // Custom logic to determine if an error is retryable
        // e.g., transient network errors, rate limits, etc.
        return error.Message.Contains("timeout") || error.Message.Contains("rate limit");
    }

    private string GetDiagnosticSuggestion(Exception ex)
    {
        if (ex is TimeoutException) return "Check network connectivity or increase timeout settings.";
        if (ex.Message.Contains("auth")) return "Verify API keys and permissions.";
        if (ex.Message.Contains("not found")) return "Check if the requested resource exists.";
        return "Review the error stack trace and logs for more details.";
    }

    private async Task OnManualInterventionEntry()
    {
        var reason = _context.InterventionInfo?.Reason ?? "Manual intervention required.";
        _logger.LogWarning("[WorkflowEngine] MANUAL INTERVENTION REQUIRED for {PlanId}. Reason: {Reason}", _planId, reason);

        var notification = new InterventionNotificationDto(
            _planId.ToString(),
            reason,
            JsonSerializer.Serialize(new
            {
                _context.InputParameters,
                _context.IntermediateResults,
                History = _context.ToolCallHistory.TakeLast(5)
            }),
            "Resume, Modify, or Terminate the workflow.",
            30 // 30 minutes timeout
        );

        await _notificationService.NotifyInterventionRequired(_planId.ToString(), notification);
    }

    private Task OnManualInterventionExit()
    {
        _context.InterventionInfo = null;
        return Task.CompletedTask;
    }

    private async Task HandleFinalState(WorkflowState state)
    {
        _logger.LogInformation("Workflow {PlanId} reached final state: {State}", _planId, state);
        var planStatus = state switch
        {
            WorkflowState.Completed => PlanStatus.Completed,
            WorkflowState.Failed => PlanStatus.Failed,
            WorkflowState.Terminated => PlanStatus.Deleted, // Or a separate Terminated status
            _ => PlanStatus.InProgress
        };
        await _workflowRepository.UpdatePlanStatusAsync(_planId, planStatus);
        _onFinalized?.Invoke(_planId);
    }

    /// <inheritdoc />
    public async Task TriggerEventAsync(WorkflowEvent workflowEvent, object? data = null)
    {
        if (!_stateMachine.CanFire(workflowEvent))
        {
            _logger.LogWarning("[WorkflowEngine] Cannot fire event {Event} from current state {State}", workflowEvent, _stateMachine.State);
            return;
        }

        // Handle data if provided
        if (data is ManualInterventionInfo interventionInfo)
        {
            _context.InterventionInfo = interventionInfo;
        }
        else if (data is WorkflowError error)
        {
            _context.Errors.Add(error);
        }

        await _stateMachine.FireAsync(workflowEvent);
    }

    /// <inheritdoc />
    public WorkflowContext GetContext() => _context;

    private static string ReadSkillTemplate(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var current = new DirectoryInfo(baseDir);
        while (current != null)
        {
            var skillsPath = Path.Combine(current.FullName, SkillsFolderName, fileName);
            if (File.Exists(skillsPath))
            {
                return File.ReadAllText(skillsPath);
            }
            current = current.Parent;
        }
        return "";
    }

    private string BuildContextSummary()
    {
        var sb = new StringBuilder();
        if (_context.IntermediateResults.Count > 0)
        {
            sb.AppendLine("Recent intermediate results:");
            foreach (var kv in _context.IntermediateResults.TakeLast(5))
            {
                sb.AppendLine($"- Step[{kv.Key}]: {kv.Value}");
            }
        }
        if (_context.ToolCallHistory.Count > 0)
        {
            sb.AppendLine("Recent tool calls:");
            foreach (var rec in _context.ToolCallHistory.TakeLast(5))
            {
                var tool = $"{rec.PluginName}.{rec.FunctionName}";
                var args = rec.Arguments != null ? JsonSerializer.Serialize(rec.Arguments) : "{}";
                var res = string.IsNullOrWhiteSpace(rec.Result) ? (rec.Success ? "OK" : rec.ErrorMessage ?? "Error") : rec.Result;
                sb.AppendLine($"- {tool} {args} => {res}");
            }
        }
        return sb.ToString();
    }

    private static string ExtractJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("Empty LLM response");
        var startFence = content.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (startFence >= 0)
        {
            var after = startFence + "```json".Length;
            var endFence = content.IndexOf("```", after, StringComparison.OrdinalIgnoreCase);
            if (endFence > after) return content.Substring(after, endFence - after).Trim();
        }
        var bs = content.IndexOf('{');
        var be = content.LastIndexOf('}');
        if (bs >= 0 && be > bs) return content.Substring(bs, be - bs + 1).Trim();
        throw new InvalidOperationException("No JSON block found in LLM response");
    }

    private static string BuildCompletedSummary(WorkflowPlan plan)
    {
        var sb = new StringBuilder();
        var completed = plan.Steps.Where(s => s.Status == PlanStepStatus.Completed).OrderBy(s => s.Index).TakeLast(5);
        foreach (var s in completed)
        {
            var res = string.IsNullOrWhiteSpace(s.Result) ? "OK" : s.Result;
            sb.AppendLine($"- [{s.Index}] {s.Text} -> {res}");
        }
        return sb.ToString();
    }

    private async Task TryDynamicInjectionAsync(WorkflowPlan plan, int completedStepIndex, string completedResult, string injectionReason)
    {
        var template = ReadSkillTemplate(DynamicInjectionPromptFile);
        if (string.IsNullOrWhiteSpace(template)) return;

        var remaining = string.Join("\n",
            plan.Steps
                .Where(s => s.Status == PlanStepStatus.NotStarted || s.Status == PlanStepStatus.InProgress)
                .OrderBy(s => s.Index)
                .Select(s => $"- [{s.Index}] {s.Text}"));

        var prompt = template
            .Replace("{{completed_task_id}}", completedStepIndex.ToString())
            .Replace("{{completed_task_result_summary}}", completedResult)
            .Replace("{{injection_reason}}", injectionReason)
            .Replace("{{remaining_tasks_list}}", remaining)
            .Replace("{{current_task_index}}", completedStepIndex.ToString());

        var response = await _semanticKernelService.GetChatCompletionAsync(prompt);
        var json = ExtractJson(response);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var shouldInject = root.TryGetProperty("inject", out var inj) && inj.ValueKind == JsonValueKind.True;
        if (!shouldInject) return;

        var totalInjected = plan.Steps.Count(s => s.Text.Contains("[INJECTED]", StringComparison.OrdinalIgnoreCase));
        if (totalInjected >= 5) return;

        var afterIndex = completedStepIndex;
        if (root.TryGetProperty("inject_after_index", out var ia) && ia.TryGetInt32(out var parsedAfter))
        {
            afterIndex = parsedAfter;
        }

        if (!root.TryGetProperty("injected_tasks", out var tasksEl) || tasksEl.ValueKind != JsonValueKind.Array) return;

        var injectedSteps = new List<WorkflowStep>();
        foreach (var t in tasksEl.EnumerateArray().Take(3))
        {
            if (totalInjected + injectedSteps.Count >= 5) break;

            var text = t.TryGetProperty("text", out var te) ? te.GetString() ?? "" : "";
            var type = t.TryGetProperty("type", out var ty) ? ty.GetString() ?? "" : "";
            var condition = t.TryGetProperty("condition", out var ce) ? ce.GetString() : null;

            var dependsOn = new List<int>();
            if (t.TryGetProperty("depends_on", out var dep) && dep.ValueKind == JsonValueKind.Array)
            {
                foreach (var di in dep.EnumerateArray())
                {
                    if (di.ValueKind == JsonValueKind.Number && di.TryGetInt32(out var idx)) dependsOn.Add(idx);
                    else if (di.ValueKind == JsonValueKind.String && int.TryParse(di.GetString(), out var sidx)) dependsOn.Add(sidx);
                }
            }

            injectedSteps.Add(new WorkflowStep
            {
                Text = text,
                Type = string.IsNullOrWhiteSpace(type) ? "task" : type,
                Condition = condition,
                DependsOn = dependsOn,
                Status = PlanStepStatus.NotStarted
            });
        }

        if (injectedSteps.Count == 0) return;

        var insertionIndex = Math.Clamp(afterIndex + 1, 0, plan.Steps.Count);
        plan.Steps.InsertRange(insertionIndex, injectedSteps);
        for (var i = 0; i < plan.Steps.Count; i++)
        {
            plan.Steps[i].Index = i;
        }

        await _workflowRepository.UpdatePlanStepsAsync(_planId, plan.Steps);

        if (_context.CurrentStepIndex > afterIndex)
        {
            _context.CurrentStepIndex += injectedSteps.Count;
        }

        var updatedPlan = await _workflowRepository.GetPlanByIdAsync(_planId, default);
        if (updatedPlan != null)
        {
            await _notificationService.BroadcastPlanUpdate(_planId.ToString(), updatedPlan.Adapt<WorkflowPlan>());
        }
    }
}


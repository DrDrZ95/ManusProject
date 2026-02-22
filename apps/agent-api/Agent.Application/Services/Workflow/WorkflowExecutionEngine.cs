namespace Agent.Application.Services.Workflow;

/// <summary>
/// 工作流执行引擎 (Workflow Execution Engine)
/// 实现了状态机逻辑，负责管理工作流的状态转换和执行流程。
/// Implements the state machine logic, responsible for managing workflow state transitions and execution flow.
/// </summary>
public class WorkflowExecutionEngine : IWorkflowEngine
{
    private readonly StateMachine<WorkflowState, WorkflowEvent> _stateMachine;
    private readonly Guid _planId;
    private readonly IWorkflowNotificationService _notificationService;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly WorkflowContext _context;
    private readonly ILogger<WorkflowExecutionEngine> _logger;
    private readonly IAgentTraceService? _agentTraceService;
    private DateTime? _stepStartTime;

    /// <inheritdoc />
    public WorkflowState CurrentState => _stateMachine.State;

    public WorkflowExecutionEngine(
        Guid planId,
        WorkflowState initialState,
        IWorkflowNotificationService notificationService,
        IWorkflowRepository workflowRepository,
        ILogger<WorkflowExecutionEngine> logger,
        IAgentTraceService? agentTraceService = null,
        WorkflowContext? existingContext = null)
    {
        _planId = planId;
        _notificationService = notificationService;
        _workflowRepository = workflowRepository;
        _logger = logger;
        _agentTraceService = agentTraceService;
        _context = existingContext ?? new WorkflowContext(planId.GetHashCode());

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
        // Planning logic would go here
    }

    private async Task OnExecutingEntry()
    {
        _logger.LogInformation("Workflow {PlanId} executing step {StepIndex}...", _planId, _context.CurrentStepIndex);
        _stepStartTime = DateTime.UtcNow;

        try
        {
            // Update step status to InProgress and set StartedAt
            await _workflowRepository.UpdateStepStatusAndResultAsync(
                _planId,
                _context.CurrentStepIndex,
                PlanStepStatus.InProgress,
                null,
                null,
                _stepStartTime,
                default);

            // In a real implementation, the actual execution logic would be triggered here.
            // For now, we assume the step is executed by an external service which will trigger StepComplete/StepFailed.
        }
        catch (Exception ex)
        {
            await HandleExecutionErrorAsync(ex, "Failed to start step execution");
        }
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
}


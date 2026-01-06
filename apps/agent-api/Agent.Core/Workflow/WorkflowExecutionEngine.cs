namespace Agent.Core.Workflow;

/// <summary>
/// 工作流执行引擎 (Workflow Execution Engine)
/// 实现了状态机逻辑，负责管理工作流的状态转换和执行流程。
/// Implements the state machine logic, responsible for managing workflow state transitions and execution flow.
/// </summary>
public class WorkflowExecutionEngine : IWorkflowEngine
{
    private WorkflowState _currentState = WorkflowState.Idle;
    private readonly WorkflowContext _context;
    private readonly Dictionary<WorkflowState, Dictionary<WorkflowEvent, WorkflowState>> _transitionMap;

    /// <inheritdoc />
    public WorkflowState CurrentState => _currentState;

    public WorkflowExecutionEngine(long taskId)
    {
        _context = new WorkflowContext(taskId);
        _transitionMap = BuildTransitionMap();
    }

    /// <summary>
    /// 构建状态转换映射表 (Builds the state transition map)
    /// 严格遵循文档中定义的状态转换图 (Strictly follows the state transition diagram defined in the documentation).
    /// </summary>
    private static Dictionary<WorkflowState, Dictionary<WorkflowEvent, WorkflowState>> BuildTransitionMap()
    {
        return new Dictionary<WorkflowState, Dictionary<WorkflowEvent, WorkflowState>>
        {
            {
                WorkflowState.Idle, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    { WorkflowEvent.StartTask, WorkflowState.Initializing }
                }
            },
            {
                WorkflowState.Initializing, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    { WorkflowEvent.InitializationComplete, WorkflowState.Planning },
                    { WorkflowEvent.InitializationFailed, WorkflowState.Failed }
                }
            },
            {
                WorkflowState.Planning, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    { WorkflowEvent.PlanReady, WorkflowState.Executing },
                    { WorkflowEvent.NeedIntervention, WorkflowState.ManualIntervention },
                    { WorkflowEvent.PlanningFailed, WorkflowState.Failed }
                }
            },
            {
                WorkflowState.Executing, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    { WorkflowEvent.StepComplete, WorkflowState.Planning }, // Step complete leads to next planning/refinement
                    { WorkflowEvent.NeedIntervention, WorkflowState.ManualIntervention },
                    { WorkflowEvent.ExecutionComplete, WorkflowState.Completed },
                    { WorkflowEvent.StepFailed, WorkflowState.Failed } // Assuming StepFailed is unrecoverable for simplicity here
                }
            },
            {
                WorkflowState.ManualIntervention, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    { WorkflowEvent.UserResume, WorkflowState.Executing },
                    { WorkflowEvent.UserModify, WorkflowState.Planning },
                    { WorkflowEvent.UserTerminate, WorkflowState.Terminated }
                }
            },
            {
                WorkflowState.Completed, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    // Completed tasks can be reset to Idle for a new task
                    { WorkflowEvent.StartTask, WorkflowState.Initializing }
                }
            },
            {
                WorkflowState.Failed, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    // Failed tasks can be reset to Idle for a new task
                    { WorkflowEvent.StartTask, WorkflowState.Initializing }
                }
            },
            {
                WorkflowState.Terminated, new Dictionary<WorkflowEvent, WorkflowState>
                {
                    // Terminated tasks can be reset to Idle for a new task
                    { WorkflowEvent.StartTask, WorkflowState.Initializing }
                }
            }
        };
    }

    /// <inheritdoc />
    public async Task TriggerEventAsync(WorkflowEvent workflowEvent, object? data = null)
    {
        // 1. 检查当前状态是否有对应的事件转换 (Check if the current state has a valid transition for the event)
        if (!_transitionMap.TryGetValue(_currentState, out var transitions) ||
            !transitions.TryGetValue(workflowEvent, out var nextState))
        {
            // 记录无效的转换尝试 (Log invalid transition attempt)
            Console.WriteLine($"[WorkflowEngine] Invalid transition attempt from {_currentState} with event {workflowEvent}.");
            return;
        }

        // 2. 执行当前状态的退出逻辑 (Execute exit logic for the current state)
        await OnStateExitAsync(_currentState, workflowEvent, data);

        // 3. 更新状态 (Update the state)
        _currentState = nextState;
        _context.ExecutionHistory.Add($"State Transition: {workflowEvent} -> {_currentState}");
        Console.WriteLine($"[WorkflowEngine] State changed to {_currentState} via event {workflowEvent}.");

        // 4. 执行新状态的进入逻辑 (Execute entry logic for the new state)
        await OnStateEnterAsync(_currentState, workflowEvent, data);
    }

    /// <summary>
    /// 状态进入逻辑 (State Entry Logic)
    /// </summary>
    private Task OnStateEnterAsync(WorkflowState state, WorkflowEvent triggerEvent, object? data)
    {
        switch (state)
        {
            case WorkflowState.Initializing:
                // 启动初始化任务 (Start initialization task)
                // 实际应用中会启动一个后台任务来加载配置、工具等 (In a real app, a background task would start to load config, tools, etc.)
                break;
            case WorkflowState.ManualIntervention:
                // 准备人工干预信息 (Prepare manual intervention info)
                if (data is ManualInterventionInfo info)
                {
                    _context.InterventionInfo = info;
                }
                // 通知用户 (Notify user via SignalR)
                Console.WriteLine($"[WorkflowEngine] MANUAL INTERVENTION REQUIRED. Reason: {_context.InterventionInfo?.Reason}");
                break;
            case WorkflowState.Completed:
                // 任务完成后的清理和通知 (Cleanup and notification after task completion)
                break;
            case WorkflowState.Failed:
                // 任务失败后的记录和通知 (Logging and notification after task failure)
                break;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 状态退出逻辑 (State Exit Logic)
    /// </summary>
    private Task OnStateExitAsync(WorkflowState state, WorkflowEvent triggerEvent, object? data)
    {
        switch (state)
        {
            case WorkflowState.ManualIntervention:
                // 清除干预信息 (Clear intervention info)
                _context.InterventionInfo = null;
                break;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public WorkflowContext GetContext() => _context;
}

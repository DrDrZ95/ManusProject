namespace Agent.Core.Workflow;

/// <summary>
/// 工作流执行引擎的状态 (States of the Workflow Execution Engine)
/// </summary>
public enum WorkflowState
{
    /// <summary>
    /// 引擎空闲，等待新的工作流任务或用户指令 (Engine is idle, waiting for a new workflow task or user command).
    /// </summary>
    Idle,

    /// <summary>
    /// 任务初始化阶段 (Task initialization phase).
    /// </summary>
    Initializing,

    /// <summary>
    /// Agent 正在制定或修正执行计划 (Agent is formulating or refining the execution plan).
    /// </summary>
    Planning,

    /// <summary>
    /// 正在执行计划中的一个步骤 (Executing a step in the plan).
    /// </summary>
    Executing,

    /// <summary>
    /// 暂停执行，等待用户输入、确认或修正指令 (Execution is paused, waiting for user input, confirmation, or corrected instructions).
    /// </summary>
    ManualIntervention,

    /// <summary>
    /// 任务成功完成 (The task has been completed successfully).
    /// </summary>
    Completed,

    /// <summary>
    /// 任务执行失败，无法恢复 (The task execution failed and cannot be recovered).
    /// </summary>
    Failed,

    /// <summary>
    /// 任务被用户或系统显式终止 (The task was explicitly terminated by the user or the system).
    /// </summary>
    Terminated
}

/// <summary>
/// 触发状态转换的事件 (Events that trigger state transitions)
/// </summary>
public enum WorkflowEvent
{
    /// <summary>
    /// 启动一个新的任务 (Start a new task).
    /// </summary>
    StartTask,

    /// <summary>
    /// 初始化完成 (Initialization complete).
    /// </summary>
    InitializationComplete,

    /// <summary>
    /// 初始化失败 (Initialization failed).
    /// </summary>
    InitializationFailed,

    /// <summary>
    /// 计划制定完成 (Plan formulation complete).
    /// </summary>
    PlanReady,

    /// <summary>
    /// 计划制定失败 (Plan formulation failed).
    /// </summary>
    PlanningFailed,

    /// <summary>
    /// 执行步骤完成 (Execution step complete).
    /// </summary>
    StepComplete,

    /// <summary>
    /// 执行步骤失败 (Execution step failed).
    /// </summary>
    StepFailed,

    /// <summary>
    /// 任务执行完成 (Task execution complete).
    /// </summary>
    ExecutionComplete,

    /// <summary>
    /// 任务执行失败 (Task execution failed).
    /// </summary>
    ExecutionFailed,

    /// <summary>
    /// 需要人工干预 (Manual intervention is required).
    /// </summary>
    NeedIntervention,

    /// <summary>
    /// 用户选择继续执行 (User chooses to resume execution).
    /// </summary>
    UserResume,

    /// <summary>
    /// 用户选择修改计划 (User chooses to modify the plan).
    /// </summary>
    UserModify,

    /// <summary>
    /// 用户选择终止任务 (User chooses to terminate the task).
    /// </summary>
    UserTerminate,

    /// <summary>
    /// 系统超时终止任务 (System timeout terminates the task).
    /// </summary>
    SystemTimeout
}

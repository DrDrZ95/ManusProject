namespace Agent.Application.Services.Workflow;

/// <summary>
/// 工作流执行引擎接口 (Workflow Execution Engine Interface)
/// 定义了工作流引擎的核心功能，包括状态管理和事件触发。
/// Defines the core functionality of the workflow engine, including state management and event triggering.
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// 当前工作流的状态 (The current state of the workflow).
    /// </summary>
    WorkflowState CurrentState { get; }

    /// <summary>
    /// 触发一个事件，导致状态转换 (Triggers an event, causing a state transition).
    /// </summary>
    /// <param name="workflowEvent">要触发的事件 (The event to be triggered).</param>
    /// <param name="data">事件携带的数据 (Data carried by the event).</param>
    /// <returns>异步任务 (Asynchronous task).</returns>
    Task TriggerEventAsync(WorkflowEvent workflowEvent, object? data = null);

    /// <summary>
    /// 获取当前工作流的上下文信息 (Gets the context information of the current workflow).
    /// </summary>
    /// <returns>工作流上下文模型 (Workflow context model).</returns>
    WorkflowContext GetContext();
}


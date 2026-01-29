namespace Agent.Core.Workflow;

/// <summary>
/// 工作流上下文接口 (Workflow Context Interface)
/// 包含工作流执行所需的所有运行时数据。
/// Contains all runtime data required for workflow execution.
/// </summary>
public interface IWorkflowContext
{
    /// <summary>
    /// 任务的唯一标识符 (Unique identifier for the task).
    /// </summary>
    long TaskId { get; }

    /// <summary>
    /// 当前的执行计划 (The current execution plan).
    /// </summary>
    string? CurrentPlan { get; set; }

    /// <summary>
    /// 历史执行记录 (Historical execution records).
    /// </summary>
    List<string> ExecutionHistory { get; }

    /// <summary>
    /// 人工干预所需的信息 (Information required for manual intervention).
    /// </summary>
    ManualInterventionInfo? InterventionInfo { get; set; }
}
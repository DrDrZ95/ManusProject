namespace Agent.Core.Workflow.Models;

/// <summary>
/// 工作流上下文模型 (Workflow Context Model)
/// 包含工作流执行所需的所有运行时数据。
/// </summary>
public class WorkflowContext : IWorkflowContext
{
    /// <inheritdoc />
    public long TaskId { get; }

    /// <inheritdoc />
    public string? CurrentPlan { get; set; }

    /// <inheritdoc />
    public List<string> ExecutionHistory { get; } = new();

    /// <inheritdoc />
    public ManualInterventionInfo? InterventionInfo { get; set; }

    public WorkflowContext(long taskId)
    {
        TaskId = taskId;
    }
}

/// <summary>
/// 人工干预信息模型 (Manual Intervention Information Model)
/// </summary>
public class ManualInterventionInfo
{
    /// <summary>
    /// 触发干预的原因 (Reason for triggering intervention).
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Agent 暂停前计划执行的下一步操作 (The next action the Agent planned to execute before pausing).
    /// </summary>
    public string ProposedAction { get; set; } = string.Empty;

    /// <summary>
    /// Agent 的内部思考过程 (Agent's internal thought process).
    /// </summary>
    public string AgentThoughts { get; set; } = string.Empty;
}

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
    /// 当前执行的步骤索引 (Current execution step index).
    /// </summary>
    int CurrentStepIndex { get; set; }

    /// <summary>
    /// 历史执行记录 (Historical execution records).
    /// </summary>
    List<string> ExecutionHistory { get; }

    /// <summary>
    /// 人工干预所需的信息 (Information required for manual intervention).
    /// </summary>
    ManualInterventionInfo? InterventionInfo { get; set; }

    /// <summary>
    /// 输入参数 (Input parameters).
    /// </summary>
    Dictionary<string, object> InputParameters { get; }

    /// <summary>
    /// 中间结果 (Intermediate results).
    /// </summary>
    Dictionary<string, object> IntermediateResults { get; }

    /// <summary>
    /// 工具调用历史 (Tool call history).
    /// </summary>
    List<ToolCallRecord> ToolCallHistory { get; }

    /// <summary>
    /// 错误信息 (Error information).
    /// </summary>
    List<WorkflowError> Errors { get; }

    /// <summary>
    /// 当前版本号 (Current version number).
    /// </summary>
    int Version { get; set; }

    /// <summary>
    /// 创建上下文快照 (Create a snapshot of the context).
    /// </summary>
    WorkflowContextSnapshot CreateSnapshot();
}

/// <summary>
/// 工具调用记录 (Tool call record).
/// </summary>
public class ToolCallRecord
{
    public string PluginName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 工作流错误记录 (Workflow error record).
/// </summary>
public class WorkflowError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? StepId { get; set; }
    public string? Suggestion { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRecoverable { get; set; }
}

/// <summary>
/// 工作流上下文快照 (Workflow context snapshot).
/// </summary>
public class WorkflowContextSnapshot
{
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> IntermediateResults { get; set; } = new();
    public string? CurrentPlan { get; set; }
}


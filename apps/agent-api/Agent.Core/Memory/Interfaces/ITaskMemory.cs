namespace Agent.Core.Memory.Interfaces;

/// <summary>
/// Task memory interface (Workflow Context)
/// 任务记忆接口（工作流上下文）
/// </summary>
public interface ITaskMemory
{
    /// <summary>
    /// Save or update task execution context
    /// 保存或更新任务执行上下文
    /// </summary>
    Task SaveContextAsync(TaskExecutionContextEntity context);

    /// <summary>
    /// Get task execution context
    /// 获取任务执行上下文
    /// </summary>
    Task<TaskExecutionContextEntity?> GetContextAsync(string workflowId, string? stepId = null);

    /// <summary>
    /// Record a tool call in the task history
    /// 在任务历史中记录工具调用
    /// </summary>
    Task RecordToolCallAsync(string workflowId, string toolName, string parameters, string result);
}


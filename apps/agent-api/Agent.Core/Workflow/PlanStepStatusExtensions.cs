namespace Agent.Core.Workflow;

/// <summary>
/// Extension methods for PlanStepStatus
/// PlanStepStatus的扩展方法
/// </summary>
public static class PlanStepStatusExtensions
{
    /// <summary>
    /// Get status marker symbol
    /// 获取状态标记符号
    /// 
    /// 对应AI-Agent中的get_status_marks方法
    /// Corresponds to get_status_marks method in AI-Agent
    /// </summary>
    /// <param name="status">Status - 状态</param>
    /// <returns>Marker symbol - 标记符号</returns>
    public static string GetMarker(this PlanStepStatus status)
    {
        return status switch
        {
            PlanStepStatus.NotStarted => "[ ]",
            PlanStepStatus.InProgress => "[→]",
            PlanStepStatus.Completed => "[✓]",
            PlanStepStatus.Blocked => "[!]",
            _ => "[ ]"
        };
    }

    /// <summary>
    /// Check if status is active (not started or in progress)
    /// 检查状态是否为活动状态（未开始或进行中）
    /// </summary>
    /// <param name="status">Status - 状态</param>
    /// <returns>True if active - 如果活动则为true</returns>
    public static bool IsActive(this PlanStepStatus status)
    {
        return status == PlanStepStatus.NotStarted || status == PlanStepStatus.InProgress;
    }

    /// <summary>
    /// Convert to string representation
    /// 转换为字符串表示
    /// </summary>
    /// <param name="status">Status - 状态</param>
    /// <returns>String representation - 字符串表示</returns>
    public static string ToStringValue(this PlanStepStatus status)
    {
        return status switch
        {
            PlanStepStatus.NotStarted => "not_started",
            PlanStepStatus.InProgress => "in_progress",
            PlanStepStatus.Completed => "completed",
            PlanStepStatus.Blocked => "blocked",
            _ => "not_started"
        };
    }

    /// <summary>
    /// Parse from string value
    /// 从字符串值解析
    /// </summary>
    /// <param name="value">String value - 字符串值</param>
    /// <returns>Parsed status - 解析的状态</returns>
    public static PlanStepStatus FromStringValue(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "not_started" => PlanStepStatus.NotStarted,
            "in_progress" => PlanStepStatus.InProgress,
            "completed" => PlanStepStatus.Completed,
            "blocked" => PlanStepStatus.Blocked,
            _ => PlanStepStatus.NotStarted
        };
    }
}


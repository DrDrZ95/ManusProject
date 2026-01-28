namespace Agent.Core.Notifications;

/// <summary>
/// 人工干预通知数据传输对象 (Intervention Notification Data Transfer Object)
/// 用于向前端或操作员推送需要人工介入的通知
/// </summary>
public record InterventionNotificationDto(
    /// <summary>
    /// 工作流计划ID (Workflow Plan ID)
    /// </summary>
    string PlanId,
    
    /// <summary>
    /// 干预原因 (Reason for Intervention)
    /// </summary>
    string Reason,
    
    /// <summary>
    /// 当前上下文，例如最近的步骤、日志摘要等 (Current context, e.g., recent steps, log summary)
    /// </summary>
    string CurrentContext,
    
    /// <summary>
    /// 建议的操作，例如批准、拒绝、修改 (Suggested operation, e.g., approve, reject, modify)
    /// </summary>
    string SuggestedOperation,
    
    /// <summary>
    /// 超时时间（分钟），超过此时间将自动执行默认操作 (Timeout in minutes, after which default action is taken)
    /// </summary>
    int TimeoutMinutes
);

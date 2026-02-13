namespace Agent.Core.Notifications;

/// <summary>
/// 状态变更通知数据传输对象 (State Change Notification Data Transfer Object)
/// 用于向前端或订阅者广播工作流状态的变更
/// </summary>
public record StateChangeNotificationDto(
    /// <summary>
    /// 工作流计划ID (Workflow Plan ID)
    /// </summary>
    string PlanId,

    /// <summary>
    /// 旧状态 (Old State)
    /// </summary>
    string OldState,

    /// <summary>
    /// 新状态 (New State)
    /// </summary>
    string NewState,

    /// <summary>
    /// 状态变更时间戳 (State Change Timestamp)
    /// </summary>
    DateTime Timestamp
);

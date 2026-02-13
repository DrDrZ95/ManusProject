namespace Agent.Application.Hubs;

/// <summary>
/// 工作流实时通知中心 (Workflow Real-time Notification Hub)
/// 继承自 SignalR 的 Hub，用于处理客户端连接和实时消息推送
/// </summary>
public class WorkflowHub : Hub
{
    // 客户端调用的方法，用于加入工作流通知组
    // Client-callable method to join a workflow notification group
    public async Task JoinWorkflowGroup(string planId)
    {
        // 将连接添加到以 planId 命名的组中
        // Add the connection to the group named after planId
        await Groups.AddToGroupAsync(Context.ConnectionId, planId);
        // 记录日志 (Log the action)
        Console.WriteLine($"Connection {Context.ConnectionId} joined group {planId}");
    }

    // 客户端调用的方法，用于离开工作流通知组
    // Client-callable method to leave a workflow notification group
    public async Task LeaveWorkflowGroup(string planId)
    {
        // 将连接从组中移除
        // Remove the connection from the group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, planId);
        // 记录日志 (Log the action)
        Console.WriteLine($"Connection {Context.ConnectionId} left group {planId}");
    }

    /// <summary>
    /// 向特定工作流组广播状态变更 (Broadcast state change to a specific workflow group)
    /// </summary>
    /// <param name="planId">工作流ID (Workflow ID)</param>
    /// <param name="notification">状态变更通知DTO (State Change Notification DTO)</param>
    /// <returns></returns>
    public async Task BroadcastStateChange(string planId, StateChangeNotificationDto notification)
    {
        // 向 planId 组中的所有客户端发送 "ReceiveStateChange" 消息
        // Send "ReceiveStateChange" message to all clients in the planId group
        await Clients.Group(planId).SendAsync("ReceiveStateChange", notification);
    }

    /// <summary>
    /// 向特定工作流组广播人工干预通知 (Broadcast manual intervention notification to a specific workflow group)
    /// </summary>
    /// <param name="planId">工作流ID (Workflow ID)</param>
    /// <param name="notification">干预通知DTO (Intervention Notification DTO)</param>
    /// <returns></returns>
    public async Task NotifyInterventionRequired(string planId, InterventionNotificationDto notification)
    {
        // 向 planId 组中的所有客户端发送 "ReceiveInterventionRequired" 消息
        // Send "ReceiveInterventionRequired" message to all clients in the planId group
        await Clients.Group(planId).SendAsync("ReceiveInterventionRequired", notification);
    }

    // TODO: 实现基于用户ID的推送，需要集成身份验证系统 (Implement user ID based push, requires authentication system integration)
    // public async Task NotifyUser(string userId, string message) { ... }
}

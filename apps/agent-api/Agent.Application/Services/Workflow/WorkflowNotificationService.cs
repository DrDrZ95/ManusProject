namespace Agent.Application.Services.Workflow;

/// <summary>
/// SignalR 通知服务实现 (SignalR Notification Service Implementation)
/// </summary>
public class WorkflowNotificationService : IWorkflowNotificationService
{
    private readonly IHubContext<WorkflowHub> _hubContext;

    public WorkflowNotificationService(IHubContext<WorkflowHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyInterventionRequired(string planId, InterventionNotificationDto notification)
    {
        // 直接调用 HubContext 的 Clients.Group 方法进行推送
        // Directly call HubContext's Clients.Group method for push notification
        await _hubContext.Clients.Group(planId).SendAsync("ReceiveInterventionRequired", notification);
    }

    public async Task BroadcastStateChange(string planId, StateChangeNotificationDto notification)
    {
        // 直接调用 HubContext 的 Clients.Group 方法进行推送
        // Directly call HubContext's Clients.Group method for push notification
        await _hubContext.Clients.Group(planId).SendAsync("ReceiveStateChange", notification);
    }
}
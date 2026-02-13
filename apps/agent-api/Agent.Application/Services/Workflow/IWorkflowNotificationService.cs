namespace Agent.Application.Services.Workflow;

/// <summary>
/// SignalR 通知服务接口 (SignalR Notification Service Interface)
/// 抽象出通知逻辑，方便在 WorkflowExecutionEngine 中注入和使用
/// </summary>
public interface IWorkflowNotificationService
{
    Task NotifyInterventionRequired(string planId, InterventionNotificationDto notification);
    Task BroadcastStateChange(string planId, StateChangeNotificationDto notification);
}

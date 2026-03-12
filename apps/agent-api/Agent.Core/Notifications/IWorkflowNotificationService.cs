namespace Agent.Core.Notifications;

public interface IWorkflowNotificationService
{
    Task BroadcastStateChange(string planId, object notification);
    Task NotifyInterventionRequired(string planId, object notification);
    Task BroadcastPlanUpdate(string planId, object plan);
}

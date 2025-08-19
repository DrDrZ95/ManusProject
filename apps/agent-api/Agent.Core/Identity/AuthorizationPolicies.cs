namespace Agent.Core.Identity;

/// <summary>
/// Authorization policies for SignalR and other services
/// SignalR和其他服务的授权策略
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy for general SignalR access
    /// 一般SignalR访问策略
    /// </summary>
    public const string SignalRAccess = "SignalRAccess";

    /// <summary>
    /// Policy for RAG (Retrieval Augmented Generation) access
    /// RAG（检索增强生成）访问策略
    /// </summary>
    public const string RagAccess = "RagAccess";

    /// <summary>
    /// Policy for fine-tuning access
    /// 微调访问策略
    /// </summary>
    public const string FinetuneAccess = "FinetuneAccess";

    /// <summary>
    /// Policy for admin access
    /// 管理员访问策略
    /// </summary>
    public const string AdminAccess = "AdminAccess";

    /// <summary>
    /// Policy for user management access
    /// 用户管理访问策略
    /// </summary>
    public const string UserManagementAccess = "UserManagementAccess";

    /// <summary>
    /// Policy for system monitoring access
    /// 系统监控访问策略
    /// </summary>
    public const string MonitoringAccess = "MonitoringAccess";
}


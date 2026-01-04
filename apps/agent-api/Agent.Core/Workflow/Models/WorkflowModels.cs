using System.ComponentModel;

namespace Agent.Core.Workflow.Models;

// --------------------------------------------------------------------------------
// Models from old /Services/Workflow/Models/
// --------------------------------------------------------------------------------

/// <summary>
/// Plan step status enumeration
/// 计划步骤状态枚举
/// </summary>
public enum PlanStepStatus
{
    /// <summary>
    /// Not started - 未开始
    /// </summary>
    [Description("NotStarted")]
    NotStarted,

    /// <summary>
    /// In progress - 进行中
    /// </summary>
    [Description("InProgress")]
    InProgress,

    /// <summary>
    /// Completed - 已完成
    /// </summary>
    [Description("Completed")]
    Completed,

    /// <summary>
    /// Blocked - 已阻塞
    /// </summary>
    [Description("Blocked")]
    Blocked
}

/// <summary>
/// Workflow plan model
/// 工作流计划模型
/// </summary>
public class WorkflowPlan
{
    /// <summary>
    /// Plan ID - 计划ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Plan title - 计划标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Plan description - 计划描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Plan steps - 计划步骤
    /// </summary>
    public List<WorkflowStep> Steps { get; set; } = new();

    /// <summary>
    /// Step statuses - 步骤状态
    /// </summary>
    public List<PlanStepStatus> StepStatuses { get; set; } = new();

    /// <summary>
    /// Creation time - 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last updated time - 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Plan metadata - 计划元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Current step index - 当前步骤索引
    /// </summary>
    public int? CurrentStepIndex { get; set; }

    /// <summary>
    /// Executor agent keys - 执行器代理键
    /// </summary>
    public List<string> ExecutorKeys { get; set; } = new();

    /// <summary>
    /// 状态
    /// </summary>
    public PlanStatus Status { get; set; }
}

/// <summary>
/// Workflow step model
/// 工作流步骤模型
/// </summary>
public class WorkflowStep
{
    /// <summary>
    /// Step index - 步骤索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Step text/description - 步骤文本/描述
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Step type/category - 步骤类型/类别
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Step status - 步骤状态
    /// </summary>
    public PlanStepStatus Status { get; set; } = PlanStepStatus.NotStarted;

    /// <summary>
    /// Step execution result - 步骤执行结果
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Step start time - 步骤开始时间
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Step completion time - 步骤完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Step metadata - 步骤元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Create plan request model
/// 创建计划请求模型
/// </summary>
public class CreatePlanRequest
{
    /// <summary>
    /// Plan title - 计划标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Plan description - 计划描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Plan steps - 计划步骤
    /// </summary>
    public List<string> Steps { get; set; } = new();

    /// <summary>
    /// Executor agent keys - 执行器代理键
    /// </summary>
    public List<string> ExecutorKeys { get; set; } = new();

    /// <summary>
    /// Plan metadata - 计划元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Workflow progress model
/// 工作流进度模型
/// </summary>
public class WorkflowProgress
{
    /// <summary>
    /// Plan ID - 计划ID
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Total steps count - 总步骤数
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Completed steps count - 已完成步骤数
    /// </summary>
    public int CompletedSteps { get; set; }

    /// <summary>
    /// In progress steps count - 进行中步骤数
    /// </summary>
    public int InProgressSteps { get; set; }

    /// <summary>
    /// Blocked steps count - 阻塞步骤数
    /// </summary>
    public int BlockedSteps { get; set; }

    /// <summary>
    /// Progress percentage (0-100) - 进度百分比（0-100）
    /// </summary>
    public double Percentage { get; set; }

/// <summary>
    /// Progress percentage (0-100) - 进度百分比（0-100）
    /// </summary>
    public double ProgressPercentage => TotalSteps > 0 ? (double)CompletedSteps / TotalSteps * 100 : 0;

    /// <summary>
    /// Current step index - 当前步骤索引
    /// </summary>
    public int? CurrentStepIndex { get; set; }

    /// <summary>
    /// Is plan completed - 计划是否完成
    /// </summary>
    public bool IsCompleted => CompletedSteps == TotalSteps && TotalSteps > 0;

    /// <summary>
    /// Has blocked steps - 是否有阻塞步骤
    /// </summary>
    public bool HasBlockedSteps => BlockedSteps > 0;
    
    /// <summary>
    /// Estimated time remaining - 预计剩余时间
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}

using Agent.Core.Data.Entities;
using System.ComponentModel;
using System.Text.Json;

namespace Agent.Core.Workflow;

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

// --------------------------------------------------------------------------------
// WorkflowPlan.cs existing content starts here
// --------------------------------------------------------------------------------


/// <summary>
/// 工作流计划业务模型 (Workflow Plan Business Model)
/// </summary>
public class WorkflowPlan
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ExecutorKeys { get; set; } = new List<string>();
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public PlanStatus Status { get; set; }
    
    /// <summary>
    /// 当前工作流状态 (Current Workflow State)
    /// </summary>
    public WorkflowState CurrentState { get; set; } = WorkflowState.Idle;

    /// <summary>
    /// 人工干预原因 (Reason for Manual Intervention)
    /// </summary>
    public string? InterventionReason { get; set; }

    /// <summary>
    /// 连续失败次数 (Consecutive Failure Count)
    /// </summary>
    public int FailureCount { get; set; } = 0;

    public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    
    /// <summary>
    /// 当前步骤索引 (Current Step Index)
    /// </summary>
    public int? CurrentStepIndex { get; set; }
}

/// <summary>
/// 工作流步骤业务模型 (Workflow Step Business Model)
/// </summary>
public class WorkflowStep
{
    public Guid Id { get; set; }
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public PlanStepStatus Status { get; set; }
    public string? Result { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// 步骤元数据 (Step Metadata)
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// 实体和模型转换扩展方法 (Entity and Model Conversion Extensions)
/// </summary>
public static class WorkflowPlanExtensions
{
    public static WorkflowPlanEntity ToEntity(this WorkflowPlan model)
    {
        return new WorkflowPlanEntity
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            Status = model.Status,
            CurrentState = model.CurrentState, // 新增状态字段
            InterventionReason = model.InterventionReason, // 新增干预原因
            FailureCount = model.FailureCount, // 新增失败次数
            Metadata = JsonSerializer.Serialize(model.Metadata),
            ExecutorKeys = JsonSerializer.Serialize(model.ExecutorKeys),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            Steps = model.Steps.Select(s => new WorkflowStepEntity
            {
                Id = s.Id,
                Index = s.Index,
                Text = s.Text,
                Type = s.Type,
                Status = s.Status,
                Result = s.Result,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt
            }).ToList()
        };
    }

    public static WorkflowPlan ToModel(this WorkflowPlanEntity entity)
    {
        return new WorkflowPlan
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            CurrentState = entity.CurrentState, // 新增状态字段
            InterventionReason = entity.InterventionReason, // 新增干预原因
            FailureCount = entity.FailureCount, // 新增失败次数
            Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata ?? "{}") ?? new Dictionary<string, object>(),
            ExecutorKeys = JsonSerializer.Deserialize<List<string>>(entity.ExecutorKeys ?? "[]") ?? new List<string>(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Steps = entity.Steps.Select(s => new WorkflowStep
            {
                Id = s.Id,
                Index = s.Index,
                Text = s.Text,
                Type = s.Type,
                Status = s.Status,
                Result = s.Result,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt
            }).OrderBy(s => s.Index).ToList()
        };
    }
}

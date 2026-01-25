using Agent.Core.Data.Entities;
using System.Text.Json;

namespace Agent.Core.Workflow;

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

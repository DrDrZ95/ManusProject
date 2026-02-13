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
    Blocked,

    /// <summary>
    /// Failed - 失败
    /// </summary>
    [Description("Failed")]
    Failed
}

/// <summary>
/// 工作流计划业务模型 (Workflow Plan Business Model)
/// </summary>
public class WorkflowPlan
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Plan steps - 计划步骤
    /// </summary>
    public List<WorkflowStep> Steps { get; set; } = new();

    /// <summary>
    /// Step statuses - 步骤状态（历史字段，兼容旧模型）
    /// </summary>
    public List<PlanStepStatus> StepStatuses { get; set; } = new();

    /// <summary>
    /// Current step index - 当前步骤索引（历史字段，兼容旧模型）
    /// </summary>
    public int? CurrentStepIndex { get; set; }

    public List<string> ExecutorKeys { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

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

    /// <summary>
    /// 可视化图形数据 (Visual Graph Data)
    /// </summary>
    public string? VisualGraphJson { get; set; }
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
    /// Step metadata - 步骤元数据（来自旧模型补齐）
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Performance metrics - 性能指标
    /// </summary>
    public Dictionary<string, object> PerformanceData { get; set; } = new();

    /// <summary>
    /// Is breakpoint set - 是否设置了断点
    /// </summary>
    public bool IsBreakpoint { get; set; }
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

/// <summary>
/// Workflow performance report model
/// 工作流性能报告模型
/// </summary>
public class WorkflowPerformanceReport
{
    public string PlanId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public double SuccessRate { get; set; }
    public List<StepPerformanceMetric> StepMetrics { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
}

/// <summary>
/// Step performance metric model
/// 步骤性能指标模型
/// </summary>
public class StepPerformanceMetric
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public double Cost { get; set; }
    public PlanStepStatus Status { get; set; }
    public bool IsBottleneck { get; set; }
}

/// <summary>
/// 实体和模型转换扩展方法 (Entity and Model Conversion Extensions)
/// </summary>
public static class WorkflowPlanExtensions
{
    public static WorkflowPlan ToModel(this WorkflowPlanEntity entity)
    {
        var model = new WorkflowPlan
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            // ExecutorKeys 和 Metadata 假设在 Entity 中是 string，需要反序列化，这里简化处理
            // In a real app, Metadata/ExecutorKeys would need JSON deserialization
            ExecutorKeys = new List<string>(),
            Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Status = entity.Status,
            Steps = entity.Steps.Select(s => s.ToModel()).ToList(),
            StepStatuses = entity.Steps.Select(s => s.Status).ToList(),
            CurrentStepIndex = entity.Steps.FirstOrDefault(s => s.Status.IsActive())?.Index,
            VisualGraphJson = entity.VisualGraphJson
        };
        return model;
    }

    public static WorkflowStep ToModel(this WorkflowStepEntity entity)
    {
        return new WorkflowStep
        {
            Index = entity.Index,
            Text = entity.Text,
            Type = entity.Type,
            Status = entity.Status,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            Result = entity.Result,
            Metadata = string.IsNullOrEmpty(entity.Metadata) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata) ?? new Dictionary<string, object>(),
            PerformanceData = string.IsNullOrEmpty(entity.PerformanceDataJson) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.PerformanceDataJson) ?? new Dictionary<string, object>(),
            IsBreakpoint = entity.IsBreakpoint
        };
    }

    // --- Model to Entity (业务模型到实体) ---

    public static WorkflowPlanEntity ToEntity(this WorkflowPlan model)
    {
        var entity = new WorkflowPlanEntity
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            // ExecutorKeys 和 Metadata 假设在 Entity 中是 string，需要序列化，这里简化处理
            Metadata = JsonSerializer.Serialize(model.Metadata),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            Status = model.Status,
            VisualGraphJson = model.VisualGraphJson,
            Steps = model.Steps.Select(s => s.ToEntity(model.Id)).ToList()
        };
        return entity;
    }

    public static WorkflowStepEntity ToEntity(this WorkflowStep model, Guid planId)
    {
        return new WorkflowStepEntity
        {
            PlanId = planId,
            Index = model.Index,
            Text = model.Text,
            Type = model.Type,
            Status = model.Status,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt,
            Result = model.Result,
            Metadata = JsonSerializer.Serialize(model.Metadata),
            PerformanceDataJson = JsonSerializer.Serialize(model.PerformanceData),
            IsBreakpoint = model.IsBreakpoint
        };
    }
}


namespace Agent.Core.Data.Entities;

/// <summary>
/// 工作流计划实体 (Workflow Plan Entity)
/// 对应一个完整的任务计划，包含多个步骤。
/// </summary>
[Table("workflow_plans")]
public class WorkflowPlanEntity
{
    /// <summary>
    /// 计划ID (Plan ID)
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// 计划标题 (Plan Title)
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 计划描述 (Plan Description)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 计划状态 (Plan Status)
    /// 聚合状态，例如 NotStarted, InProgress, Completed, Failed
    /// </summary>
    public PlanStatus Status { get; set; } = PlanStatus.NotStarted;

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
    /// 元数据 (Metadata)
    /// 存储额外的JSON格式数据，例如执行器键、用户ID等
    /// </summary>
    public string Metadata { get; set; } = "{}"; // 存储为JSON字符串

    /// <summary>
    /// 执行上下文 (Execution Context)
    /// 存储 WorkflowContext 的 JSON 序列化数据
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? ExecutionContextJson { get; set; }

    [Column(TypeName = "jsonb")]
    public string? ExecutorKeys { get; set; }

    /// <summary>
    /// 可视化图形数据 (Visual Graph Data)
    /// 存储 React Flow 的节点和连线信息
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? VisualGraphJson { get; set; }

    /// <summary>
    /// 创建时间 (Created At)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间 (Updated At)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性 (Navigation Property)
    /// <summary>
    /// 包含的步骤列表 (List of steps included in the plan)
    /// </summary>
    public ICollection<WorkflowStepEntity> Steps { get; set; } = new List<WorkflowStepEntity>();
}

/// <summary>
/// 计划的聚合状态 (Aggregated status of the plan)
/// </summary>
public enum PlanStatus
{
    NotStarted,
    InProgress,
    Deleted,
    Completed,
    Failed
}


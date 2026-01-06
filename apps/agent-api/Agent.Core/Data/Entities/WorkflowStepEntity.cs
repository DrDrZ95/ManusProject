namespace Agent.Core.Data.Entities;

/// <summary>
/// 工作流步骤实体 (Workflow Step Entity)
/// 对应计划中的一个具体步骤。
/// </summary>
[Table("WorkflowSteps")]
public class WorkflowStepEntity
{
    /// <summary>
    /// 步骤ID (Step ID)
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// 所属计划ID (Parent Plan ID)
    /// 外键
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// 步骤索引 (Step Index)
    /// 在计划中的顺序，从0开始
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 步骤文本 (Step Text)
    /// 具体的任务描述
    /// </summary>
    [Required]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 步骤类型 (Step Type)
    /// 例如 SEARCH, CODE, WRITE 等
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }

    /// <summary>
    /// 步骤状态 (Step Status)
    /// 例如 NotStarted, InProgress, Completed, Blocked
    /// </summary>
    public PlanStepStatus Status { get; set; } = PlanStepStatus.NotStarted;

    /// <summary>
    /// 开始时间 (Started At)
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 完成时间 (Completed At)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 步骤执行结果 (Step Execution Result)
    /// 存储步骤执行后的输出或总结
    /// </summary>
    public string? Result { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    // 导航属性 (Navigation Property)
    /// <summary>
    /// 所属的工作流计划 (The parent workflow plan)
    /// </summary>
    [ForeignKey(nameof(PlanId))]
    public WorkflowPlanEntity Plan { get; set; } = null!;
}

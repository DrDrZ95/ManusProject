using System.Text.Json;

namespace Agent.Core.Services.Workflow;

/// <summary>
/// Interface for workflow management operations
/// 工作流管理操作接口
/// 
/// 基于AI-Agent项目的planning.py转换而来
/// Converted from AI-Agent project's planning.py
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Create a new workflow plan
    /// 创建新的工作流计划
    /// </summary>
    /// <param name="request">Plan creation request - 计划创建请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Created plan information - 创建的计划信息</returns>
    Task<WorkflowPlan> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow plan by ID
    /// 根据ID获取工作流计划
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Workflow plan or null if not found - 工作流计划或null（如果未找到）</returns>
    Task<WorkflowPlan?> GetPlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>List of workflow plans - 工作流计划列表</returns>
    Task<List<WorkflowPlan>> GetAllPlansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update step status in a plan
    /// 更新计划中的步骤状态
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="stepIndex">Step index - 步骤索引</param>
    /// <param name="status">New status - 新状态</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> UpdateStepStatusAsync(string planId, int stepIndex, PlanStepStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current active step in a plan
    /// 获取计划中当前活动步骤
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Current step information or null if no active step - 当前步骤信息或null（如果没有活动步骤）</returns>
    Task<WorkflowStep?> GetCurrentStepAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark step as completed and move to next
    /// 标记步骤为已完成并移动到下一步
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="stepIndex">Step index - 步骤索引</param>
    /// <param name="result">Step execution result - 步骤执行结果</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> CompleteStepAsync(string planId, int stepIndex, string? result = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate todo list file content for a plan
    /// 为计划生成待办事项列表文件内容
    /// 
    /// 这是AI-Agent中的核心功能，用于生成可视化的任务列表
    /// This is a core feature from AI-Agent for generating visual task lists
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Todo list content - 待办事项列表内容</returns>
    Task<string> GenerateToDoListAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save todo list to file
    /// 将待办事项列表保存到文件
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="filePath">File path - 文件路径</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> SaveToDoListToFileAsync(string planId, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load todo list from file and update plan status
    /// 从文件加载待办事项列表并更新计划状态
    /// </summary>
    /// <param name="filePath">File path - 文件路径</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Loaded plan ID or null if failed - 加载的计划ID或null（如果失败）</returns>
    Task<string?> LoadToDoListFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a workflow plan
    /// 删除工作流计划
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> DeletePlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get plan execution progress
    /// 获取计划执行进度
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Progress information - 进度信息</returns>
    Task<WorkflowProgress> GetProgressAsync(string planId, CancellationToken cancellationToken = default);

    Task<string> CreateWorkflowAsync(string llmResponse);
}

/// <summary>
/// Plan step status enumeration
/// 计划步骤状态枚举
/// 
/// 对应AI-Agent中的PlanStepStatus
/// Corresponds to PlanStepStatus in AI-Agent
/// </summary>
public enum PlanStepStatus
{
    /// <summary>
    /// Not started - 未开始
    /// </summary>
    NotStarted,

    /// <summary>
    /// In progress - 进行中
    /// </summary>
    InProgress,

    /// <summary>
    /// Completed - 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// Blocked - 已阻塞
    /// </summary>
    Blocked
}

/// <summary>
/// Extension methods for PlanStepStatus
/// PlanStepStatus的扩展方法
/// </summary>
public static class PlanStepStatusExtensions
{
    /// <summary>
    /// Get status marker symbol
    /// 获取状态标记符号
    /// 
    /// 对应AI-Agent中的get_status_marks方法
    /// Corresponds to get_status_marks method in AI-Agent
    /// </summary>
    /// <param name="status">Status - 状态</param>
    /// <returns>Marker symbol - 标记符号</returns>
    public static string GetMarker(this PlanStepStatus status)
    {
        return status switch
        {
            PlanStepStatus.NotStarted => "[ ]",
            PlanStepStatus.InProgress => "[→]",
            PlanStepStatus.Completed => "[✓]",
            PlanStepStatus.Blocked => "[!]",
            _ => "[ ]"
        };
    }

    /// <summary>
    /// Check if status is active (not started or in progress)
    /// 检查状态是否为活动状态（未开始或进行中）
    /// </summary>
    /// <param name="status">Status - 状态</param>
    /// <returns>True if active - 如果活动则为true</returns>
    public static bool IsActive(this PlanStepStatus status)
    {
        return status == PlanStepStatus.NotStarted || status == PlanStepStatus.InProgress;
    }

    /// <summary>
    /// Convert to string representation
    /// 转换为字符串表示
    /// </summary>
    /// <param name="status">Status - 状态</param>
    /// <returns>String representation - 字符串表示</returns>
    public static string ToStringValue(this PlanStepStatus status)
    {
        return status switch
        {
            PlanStepStatus.NotStarted => "not_started",
            PlanStepStatus.InProgress => "in_progress",
            PlanStepStatus.Completed => "completed",
            PlanStepStatus.Blocked => "blocked",
            _ => "not_started"
        };
    }

    /// <summary>
    /// Parse from string value
    /// 从字符串值解析
    /// </summary>
    /// <param name="value">String value - 字符串值</param>
    /// <returns>Parsed status - 解析的状态</returns>
    public static PlanStepStatus FromStringValue(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "not_started" => PlanStepStatus.NotStarted,
            "in_progress" => PlanStepStatus.InProgress,
            "completed" => PlanStepStatus.Completed,
            "blocked" => PlanStepStatus.Blocked,
            _ => PlanStepStatus.NotStarted
        };
    }
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
    public string Id { get; set; } = string.Empty;

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
}


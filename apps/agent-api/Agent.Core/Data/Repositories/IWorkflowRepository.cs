namespace Agent.Core.Data.Repositories;

/// <summary>
/// 工作流数据仓储接口 (Workflow Data Repository Interface)
/// 负责工作流计划和步骤的持久化操作。
/// </summary>
public interface IWorkflowRepository
{
    // --- CRUD Operations (增删改查操作) ---

    /// <summary>
    /// 添加新的工作流计划 (Add a new workflow plan)
    /// </summary>
    Task<WorkflowPlanEntity> AddPlanAsync(WorkflowPlanEntity plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取工作流计划 (Get a workflow plan by ID)
    /// </summary>
    Task<WorkflowPlanEntity?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作流计划 (Update a workflow plan)
    /// </summary>
    Task UpdatePlanAsync(WorkflowPlanEntity plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作流计划的状态 (Update a status of workflow plan)
    /// </summary>
    Task<bool> UpdatePlanStatusAsync(Guid planId, PlanStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作流计划的状态机状态和干预原因 (Update the state machine state and intervention reason of a workflow plan)
    /// </summary>
    Task<bool> UpdatePlanStateAsync(Guid planId, WorkflowState state, string? interventionReason, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作流计划的执行上下文 (Update the execution context of a workflow plan)
    /// </summary>
    Task<bool> UpdatePlanContextAsync(Guid planId, string contextJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作流计划的连续失败次数 (Update the consecutive failure count of a workflow plan)
    /// </summary>
    Task<bool> UpdatePlanFailureCountAsync(Guid planId, int failureCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除工作流计划 (Delete a workflow plan)
    /// </summary>
    Task DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default);

    // --- Complex Queries (复杂查询) ---

    /// <summary>
    /// 获取所有工作流计划 (Get all workflow plans)
    /// </summary>
    Task<List<WorkflowPlanEntity>> GetAllPlansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据状态过滤并分页获取工作流计划 (Get workflow plans filtered by status and with pagination)
    /// </summary>
    Task<List<WorkflowPlanEntity>> GetPlansByStatusAsync(
        PlanStatus status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取计划中的所有步骤 (Get all steps for a plan)
    /// </summary>
    Task<List<WorkflowStepEntity>> GetStepsByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据计划ID和步骤索引获取特定步骤 (Get a specific step by plan ID and step index)
    /// </summary>
    Task<WorkflowStepEntity?> GetStepByPlanIdAndIndexAsync(
        Guid planId,
        int stepIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新单个步骤的状态和结果 (Update the status and result of a single step)
    /// </summary>
    Task UpdateStepStatusAndResultAsync(
        Guid planId,
        int stepIndex,
        PlanStepStatus status,
        string? result,
        string? error,
        DateTime? startedAt,
        DateTime? completedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新步骤的性能数据 (Update the performance data for a step)
    /// </summary>
    Task UpdateStepPerformanceDataAsync(
        Guid planId,
        int stepIndex,
        string performanceDataJson,
        DateTime? completedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新步骤的断点设置 (Update the breakpoint setting for a step)
    /// </summary>
    Task<bool> UpdateStepBreakpointAsync(
        Guid planId,
        int stepIndex,
        bool isBreakpoint,
        CancellationToken cancellationToken = default);
}


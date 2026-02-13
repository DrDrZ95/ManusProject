namespace Agent.Core.Data.Repositories;

/// <summary>
/// Specialized repository for workflow plans
/// 工作流计划的专用仓储
/// </summary>
public interface IWorkflowPlanRepository : IRepository<WorkflowPlanEntity, string>
{
    /// <summary>
    /// Get plans with their steps - 获取包含步骤的计划
    /// </summary>
    Task<List<WorkflowPlanEntity>> GetPlansWithStepsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get plan with steps by ID - 根据ID获取包含步骤的计划
    /// </summary>
    Task<WorkflowPlanEntity?> GetPlanWithStepsAsync(string planId, CancellationToken cancellationToken = default);
}


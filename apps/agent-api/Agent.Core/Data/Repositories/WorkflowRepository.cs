namespace Agent.Core.Data.Repositories;

/// <summary>
/// 工作流数据仓储实现 (Workflow Data Repository Implementation)
/// 
/// 注意: 这是一个模拟的内存实现，用于演示 Repository 模式。
/// 在实际应用中，应注入 Entity Framework Core 的 DbContext 来实现真正的数据库操作。
/// </summary>
public class WorkflowRepository : IWorkflowRepository
{
    // 模拟数据库上下文 (Simulated Database Context)
    // 实际应用中，这里应该是 private readonly AgentDbContext _context;
    private readonly ConcurrentDictionary<Guid, WorkflowPlanEntity> _plans = new();
    private readonly ConcurrentDictionary<Guid, WorkflowStepEntity> _steps = new();

    // --- CRUD Operations (增删改查操作) ---

    /// <summary>
    /// 添加新的工作流计划 (Add a new workflow plan)
    /// </summary>
    public Task<WorkflowPlanEntity> AddPlanAsync(WorkflowPlanEntity plan, CancellationToken cancellationToken = default)
    {
        plan.Id = Guid.NewGuid();
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        _plans[plan.Id] = plan;

        foreach (var step in plan.Steps)
        {
            step.Id = Guid.NewGuid();
            step.PlanId = plan.Id;
            _steps[step.Id] = step;
        }

        return Task.FromResult(plan);
    }

    /// <summary>
    /// 根据ID获取工作流计划 (Get a workflow plan by ID)
    /// </summary>
    public Task<WorkflowPlanEntity?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: return _context.WorkflowPlans.Include(p => p.Steps).FirstOrDefaultAsync(p => p.Id == planId);
        _plans.TryGetValue(planId, out var plan);
        if (plan != null)
        {
            // 模拟Include(Steps)
            plan.Steps = _steps.Values.Where(s => s.PlanId == planId).OrderBy(s => s.Index).ToList();
        }
        return Task.FromResult(plan);
    }

    /// <summary>
    /// 更新工作流计划 (Update a workflow plan)
    /// </summary>
    public async Task UpdatePlanAsync(WorkflowPlanEntity plan, CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: _context.WorkflowPlans.Update(plan); await _context.SaveChangesAsync();
        if (_plans.ContainsKey(plan.Id))
        {
            plan.UpdatedAt = DateTime.UtcNow;
            _plans[plan.Id] = plan;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// 更新工作流计划的状态 (Update a status of workflow plan)
    /// </summary>
    public async Task<bool> UpdatePlanStatusAsync(Guid planId, PlanStatus status, CancellationToken cancellationToken = default)
    {
        if (_plans.ContainsKey(planId))
        {
            WorkflowPlanEntity plan = _plans[planId];
            plan.Status = status;
            plan.UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 更新工作流计划的状态机状态和干预原因 (Update the state machine state and intervention reason of a workflow plan)
    /// </summary>
    public Task<bool> UpdatePlanStateAsync(Guid planId, WorkflowState state, string? interventionReason, CancellationToken cancellationToken = default)
    {
        if (_plans.ContainsKey(planId))
        {
            WorkflowPlanEntity plan = _plans[planId];
            plan.CurrentState = state;
            plan.InterventionReason = interventionReason;
            plan.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// 更新工作流计划的执行上下文 (Update the execution context of a workflow plan)
    /// </summary>
    public Task<bool> UpdatePlanContextAsync(Guid planId, string contextJson, CancellationToken cancellationToken = default)
    {
        if (_plans.ContainsKey(planId))
        {
            WorkflowPlanEntity plan = _plans[planId];
            plan.ExecutionContextJson = contextJson;
            plan.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// 更新工作流计划的连续失败次数 (Update the consecutive failure count of a workflow plan)
    /// </summary>
    public Task<bool> UpdatePlanFailureCountAsync(Guid planId, int failureCount, CancellationToken cancellationToken = default)
    {
        if (_plans.ContainsKey(planId))
        {
            WorkflowPlanEntity plan = _plans[planId];
            plan.FailureCount = failureCount;
            plan.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// 删除工作流计划 (Delete a workflow plan)
    /// </summary>
    public Task DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: _context.WorkflowPlans.Remove(plan); await _context.SaveChangesAsync();
        _plans.TryRemove(planId, out _);
        // 模拟级联删除步骤
        var stepsToRemove = _steps.Where(s => s.Value.PlanId == planId).ToList();
        foreach (var step in stepsToRemove)
        {
            _steps.TryRemove(step.Key, out _);
        }
        return Task.CompletedTask;
    }

    // --- Complex Queries (复杂查询) ---

    /// <summary>
    /// 获取所有工作流计划 (Get all workflow plans)
    /// </summary>
    public Task<List<WorkflowPlanEntity>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: return _context.WorkflowPlans.OrderByDescending(p => p.CreatedAt).ToListAsync();
        var plans = _plans.Values.OrderByDescending(p => p.CreatedAt).ToList();
        return Task.FromResult(plans);
    }

    /// <summary>
    /// 根据状态过滤并分页获取工作流计划 (Get workflow plans filtered by status and with pagination)
    /// </summary>
    public Task<List<WorkflowPlanEntity>> GetPlansByStatusAsync(
        PlanStatus status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: return _context.WorkflowPlans.Where(p => p.Status == status).Skip(...).Take(...).ToListAsync();
        var plans = _plans.Values
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(plans);
    }

    /// <summary>
    /// 获取计划中的所有步骤 (Get all steps for a plan)
    /// </summary>
    public Task<List<WorkflowStepEntity>> GetStepsByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: return _context.WorkflowSteps.Where(s => s.PlanId == planId).OrderBy(s => s.Index).ToListAsync();
        var steps = _steps.Values.Where(s => s.PlanId == planId).OrderBy(s => s.Index).ToList();
        return Task.FromResult(steps);
    }

    /// <summary>
    /// 根据计划ID和步骤索引获取特定步骤 (Get a specific step by plan ID and step index)
    /// </summary>
    public Task<WorkflowStepEntity?> GetStepByPlanIdAndIndexAsync(
        Guid planId,
        int stepIndex,
        CancellationToken cancellationToken = default)
    {
        // 实际EF Core实现: return _context.WorkflowSteps.FirstOrDefaultAsync(s => s.PlanId == planId && s.Index == stepIndex);
        var step = _steps.Values.FirstOrDefault(s => s.PlanId == planId && s.Index == stepIndex);
        return Task.FromResult(step);
    }

    /// <summary>
    /// 更新单个步骤的状态和结果 (Update the status and result of a single step)
    /// </summary>
    public Task UpdateStepStatusAndResultAsync(
        Guid planId,
        int stepIndex,
        PlanStepStatus status,
        string? result,
        string? error,
        DateTime? startedAt,
        DateTime? completedAt,
        CancellationToken cancellationToken = default)
    {
        var step = _steps.Values.FirstOrDefault(s => s.PlanId == planId && s.Index == stepIndex);
        if (step != null)
        {
            step.Status = status;
            step.Result = result;
            step.Error = error;
            step.StartedAt = startedAt ?? step.StartedAt;
            step.CompletedAt = completedAt ?? step.CompletedAt;

            if (_plans.TryGetValue(planId, out var plan))
            {
                plan.UpdatedAt = DateTime.UtcNow;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateStepPerformanceDataAsync(
        Guid planId,
        int stepIndex,
        string performanceDataJson,
        DateTime? completedAt,
        CancellationToken cancellationToken = default)
    {
        var step = _steps.Values.FirstOrDefault(s => s.PlanId == planId && s.Index == stepIndex);
        if (step != null)
        {
            step.PerformanceDataJson = performanceDataJson;
            step.CompletedAt = completedAt ?? step.CompletedAt;

            if (_plans.TryGetValue(planId, out var plan))
            {
                plan.UpdatedAt = DateTime.UtcNow;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> UpdateStepBreakpointAsync(
        Guid planId,
        int stepIndex,
        bool isBreakpoint,
        CancellationToken cancellationToken = default)
    {
        var step = _steps.Values.FirstOrDefault(s => s.PlanId == planId && s.Index == stepIndex);
        if (step != null)
        {
            step.IsBreakpoint = isBreakpoint;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

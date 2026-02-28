namespace Agent.Core.Interfaces;

/// <summary>
/// Service for managing token budgets and tracking usage costs
/// Token 预算管理与消耗成本追踪服务
/// </summary>
public interface ITokenBudgetService
{
    /// <summary>
    /// Records the cost of an LLM interaction and updates the budget.
    /// 记录 LLM 交互成本并更新预算。
    /// </summary>
    Task RecordUsageAsync(string userId, double costUsd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the user has exceeded their budget for the current month.
    /// 检查用户本月是否已超出预算。
    /// </summary>
    Task<bool> IsWithinBudgetAsync(string userId, double budgetLimit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total cost consumed by the user in the current month.
    /// 获取用户本月累计消耗的总成本。
    /// </summary>
    Task<double> GetCurrentMonthCostAsync(string userId, CancellationToken cancellationToken = default);
}

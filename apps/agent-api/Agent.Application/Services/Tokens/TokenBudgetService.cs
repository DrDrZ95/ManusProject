namespace Agent.Application.Services.Tokens;

/// <summary>
/// Redis-based implementation of ITokenBudgetService for tracking user token usage and budgets.
/// 基于 Redis 的 ITokenBudgetService 实现，用于追踪用户 Token 使用量和预算。
/// </summary>
public class TokenBudgetService : ITokenBudgetService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<TokenBudgetService> _logger;
    private const string BudgetKeyPrefix = "budget:user:";

    public TokenBudgetService(IConnectionMultiplexer redis, ILogger<TokenBudgetService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RecordUsageAsync(string userId, double costUsd, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId)) return;

        try
        {
            var db = _redis.GetDatabase();
            var key = GetMonthKey(userId);

            // Use INCRBYFLOAT for atomic update of floating point costs
            await db.StringIncrementAsync(key, costUsd);
            
            // Set expiration to 60 days to ensure it stays long enough for monthly tracking
            await db.KeyExpireAsync(key, TimeSpan.FromDays(60));
            
            _logger.LogDebug("Recorded usage for user {UserId}: ${CostUsd}. Total monthly cost updated.", userId, costUsd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record usage in Redis for user {UserId}", userId);
        }
    }

    public async Task<bool> IsWithinBudgetAsync(string userId, double budgetLimit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId)) return true;

        var currentCost = await GetCurrentMonthCostAsync(userId, cancellationToken);
        return currentCost < budgetLimit;
    }

    public async Task<double> GetCurrentMonthCostAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId)) return 0;

        try
        {
            var db = _redis.GetDatabase();
            var key = GetMonthKey(userId);
            var value = await db.StringGetAsync(key);

            if (value.HasValue && double.TryParse(value, out var cost))
            {
                return cost;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current month cost from Redis for user {UserId}", userId);
        }

        return 0;
    }

    private string GetMonthKey(string userId)
    {
        // Format: budget:user:{userId}:{yyyy-MM}
        return $"{BudgetKeyPrefix}{userId}:{DateTime.UtcNow:yyyy-MM}";
    }
}

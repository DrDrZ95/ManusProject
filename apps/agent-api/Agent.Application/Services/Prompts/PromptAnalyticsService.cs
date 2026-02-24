namespace Agent.Application.Services.Prompts;

public class PromptAnalyticsService : IPromptAnalyticsService
{
    private readonly IRepository<PromptExecutionLogEntity, Guid> _executionLogs;
    private readonly IRepository<PromptTemplateEntity, Guid> _templates;
    private readonly ILogger<PromptAnalyticsService> _logger;

    public PromptAnalyticsService(
        IRepository<PromptExecutionLogEntity, Guid> executionLogs,
        IRepository<PromptTemplateEntity, Guid> templates,
        ILogger<PromptAnalyticsService> logger)
    {
        _executionLogs = executionLogs;
        _templates = templates;
        _logger = logger;
    }

    public async Task LogExecutionAsync(PromptExecutionMetrics metrics, CancellationToken cancellationToken = default)
    {
        var entity = new PromptExecutionLogEntity
        {
            Id = metrics.ExecutionId ?? Guid.NewGuid(),
            PromptTemplateId = metrics.PromptTemplateId,
            TokensUsed = metrics.TokensUsed,
            ResponseTime = metrics.ResponseTimeMs,
            UserFeedback = metrics.UserFeedback,
            QualityScore = metrics.QualityScore,
            CostUsd = metrics.CostUsd,
            ExecutedAt = metrics.ExecutedAt == default ? DateTime.UtcNow : metrics.ExecutedAt
        };

        await _executionLogs.AddAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<PromptVariantStatistics>> GetVariantStatisticsAsync(
        Guid promptTemplateId,
        string? experimentId = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _executionLogs.FindAsync(
            x => x.PromptTemplateId == promptTemplateId,
            cancellationToken);

        var template = await _templates.FirstOrDefaultAsync(x => x.Id == promptTemplateId, cancellationToken);

        var grouped = logs
            .GroupBy(_ => new
            {
                ExperimentId = experimentId ?? template?.ExperimentId,
                VariantName = template?.VariantName
            })
            .Select(g =>
            {
                var executions = g.ToList();

                var executionCount = executions.Count;
                if (executionCount == 0)
                {
                    return null;
                }

                var avgResponse = executions.Average(x => x.ResponseTime);
                var avgQuality = executions.Where(x => x.QualityScore.HasValue).DefaultIfEmpty().Average(x => (double)(x?.QualityScore ?? 0));
                var avgFeedback = executions.Where(x => x.UserFeedback.HasValue).DefaultIfEmpty().Average(x => (double)(x?.UserFeedback ?? 0));
                var avgCost = executions.Where(x => x.CostUsd.HasValue).DefaultIfEmpty().Average(x => (double)(x?.CostUsd ?? 0));

                var successCount = executions.Count(x =>
                    (x.QualityScore ?? 0) >= 0.7m ||
                    (x.UserFeedback ?? 0) >= 4);

                return new PromptVariantStatistics
                {
                    PromptTemplateId = promptTemplateId,
                    ExperimentId = g.Key.ExperimentId,
                    VariantName = g.Key.VariantName,
                    ExecutionCount = executionCount,
                    AverageResponseTimeMs = avgResponse,
                    AverageQualityScore = avgQuality,
                    AverageUserFeedback = avgFeedback,
                    AverageCostUsd = avgCost,
                    SuccessRate = executionCount > 0 ? (double)successCount / executionCount : 0
                };
            })
            .Where(x => x != null)
            .Cast<PromptVariantStatistics>()
            .ToList();

        return grouped;
    }

    public async Task<AbTestResult> RunAbTestAsync(
        Guid promptTemplateId,
        string experimentId,
        string controlVariant,
        string treatmentVariant,
        string primaryMetric = "quality_score",
        CancellationToken cancellationToken = default)
    {
        var stats = await GetVariantStatisticsAsync(promptTemplateId, experimentId, cancellationToken);

        var control = stats.FirstOrDefault(x => string.Equals(x.VariantName, controlVariant, StringComparison.OrdinalIgnoreCase));
        var treatment = stats.FirstOrDefault(x => string.Equals(x.VariantName, treatmentVariant, StringComparison.OrdinalIgnoreCase));

        if (control == null || treatment == null)
        {
            throw new InvalidOperationException("Both control and treatment variants must have statistics.");
        }

        double controlValue;
        double treatmentValue;

        switch (primaryMetric)
        {
            case "success_rate":
                controlValue = control.SuccessRate;
                treatmentValue = treatment.SuccessRate;
                break;
            case "response_time":
                controlValue = control.AverageResponseTimeMs;
                treatmentValue = treatment.AverageResponseTimeMs;
                break;
            case "cost_usd":
                controlValue = control.AverageCostUsd;
                treatmentValue = treatment.AverageCostUsd;
                break;
            default:
                controlValue = control.AverageQualityScore;
                treatmentValue = treatment.AverageQualityScore;
                break;
        }

        var pValue = ApproximatePValue(controlValue, treatmentValue, control.ExecutionCount, treatment.ExecutionCount);

        return new AbTestResult
        {
            ExperimentId = experimentId,
            ControlVariant = controlVariant,
            TreatmentVariant = treatmentVariant,
            PrimaryMetric = primaryMetric,
            ControlMetricValue = controlValue,
            TreatmentMetricValue = treatmentValue,
            PValue = pValue,
            IsStatisticallySignificant = pValue <= 0.05
        };
    }

    public async Task<IReadOnlyList<string>> GetOptimizationSuggestionsAsync(
        Guid promptTemplateId,
        CancellationToken cancellationToken = default)
    {
        var stats = await GetVariantStatisticsAsync(promptTemplateId, null, cancellationToken);
        var current = stats.FirstOrDefault();

        if (current == null)
        {
            return Array.Empty<string>();
        }

        var suggestions = new List<string>();

        if (current.SuccessRate < 0.8)
        {
            suggestions.Add("尝试增加 few-shot 示例，覆盖常见失败用例。");
        }

        if (current.AverageResponseTimeMs > 5000)
        {
            suggestions.Add("考虑收紧 Prompt 上下文长度，减少无关信息。");
        }

        if (current.AverageCostUsd > 0.01)
        {
            suggestions.Add("尝试拆分复杂任务，使用多轮交互减少单次调用上下文。");
        }

        if (current.AverageQualityScore < 0.8 && current.AverageUserFeedback < 4)
        {
            suggestions.Add("根据失败样本补充负面示例和澄清性指令。");
        }

        return suggestions;
    }

    private static double ApproximatePValue(double control, double treatment, int nControl, int nTreatment)
    {
        if (nControl <= 1 || nTreatment <= 1)
        {
            return 1.0;
        }

        var diff = treatment - control;
        var pooledVariance = (control * (1 - control) / nControl) + (treatment * (1 - treatment) / nTreatment);

        if (pooledVariance <= 0)
        {
            return 1.0;
        }

        var z = diff / Math.Sqrt(pooledVariance);

        var p = 2 * (1 - NormalCdf(Math.Abs(z)));
        return p;
    }

    private static double NormalCdf(double x)
    {
        return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
    }

    private static double Erf(double x)
    {
        var sign = Math.Sign(x);
        x = Math.Abs(x);

        var a1 = 0.254829592;
        var a2 = -0.284496736;
        var a3 = 1.421413741;
        var a4 = -1.453152027;
        var a5 = 1.061405429;
        var p = 0.3275911;

        var t = 1.0 / (1.0 + p * x);
        var y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return sign * y;
    }
}

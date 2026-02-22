namespace Agent.Application.Services.Prompts;

public interface IPromptAnalyticsService
{
    Task LogExecutionAsync(PromptExecutionMetrics metrics, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptVariantStatistics>> GetVariantStatisticsAsync(
        Guid promptTemplateId,
        string? experimentId = null,
        CancellationToken cancellationToken = default);

    Task<AbTestResult> RunAbTestAsync(
        Guid promptTemplateId,
        string experimentId,
        string controlVariant,
        string treatmentVariant,
        string primaryMetric = "quality_score",
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetOptimizationSuggestionsAsync(
        Guid promptTemplateId,
        CancellationToken cancellationToken = default);
}


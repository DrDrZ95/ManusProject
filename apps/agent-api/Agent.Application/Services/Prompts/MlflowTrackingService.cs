namespace Agent.Application.Services.Prompts;

public interface IMlflowTrackingService
{
    Task TrackPromptExperimentAsync(
        string experimentId,
        string variantName,
        PromptVariantStatistics statistics,
        CancellationToken cancellationToken = default);
}

public class MlflowTrackingService : IMlflowTrackingService
{
    private readonly HttpClient _httpClient;
    private readonly MlflowOptions _options;
    private readonly ILogger<MlflowTrackingService> _logger;

    public MlflowTrackingService(
        HttpClient httpClient,
        IOptions<MlflowOptions> options,
        ILogger<MlflowTrackingService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_options.TrackingUri))
        {
            _httpClient.BaseAddress = new Uri(_options.TrackingUri);
        }
    }

    public async Task TrackPromptExperimentAsync(
        string experimentId,
        string variantName,
        PromptVariantStatistics statistics,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.TrackingUri))
        {
            return;
        }

        try
        {
            var payload = new
            {
                experiment_id = experimentId,
                run_name = variantName,
                metrics = new Dictionary<string, double>
                {
                    ["avg_response_time_ms"] = statistics.AverageResponseTimeMs,
                    ["avg_quality_score"] = statistics.AverageQualityScore,
                    ["avg_user_feedback"] = statistics.AverageUserFeedback,
                    ["avg_cost_usd"] = statistics.AverageCostUsd,
                    ["success_rate"] = statistics.SuccessRate
                },
                tags = new Dictionary<string, string>
                {
                    ["prompt_template_id"] = statistics.PromptTemplateId.ToString(),
                    ["variant_name"] = variantName
                }
            };

            await _httpClient.PostAsJsonAsync("api/2.0/mlflow/runs/log-mlflow-prompt", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track prompt experiment to MLflow");
        }
    }
}


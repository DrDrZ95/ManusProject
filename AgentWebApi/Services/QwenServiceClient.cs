namespace AgentWebApi.Services.Qwen;

public interface IQwenServiceClient
{
    Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}

public class QwenServiceClient : IQwenServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QwenServiceClient> _logger;
    private const string QwenApiUrl = "http://localhost:2025/generate"; // Qwen3 Python service URL

    public QwenServiceClient(HttpClient httpClient, ILogger<QwenServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var requestPayload = new QwenGenerationRequest
        {
            Prompt = prompt,
            MaxLength = 512, // Default, can be made configurable
            Temperature = 0.7f,
            TopP = 0.9f,
            TopK = 50,
            NumReturnSequences = 1
        };

        try
        {
            _logger.LogInformation("Sending request to Qwen service at {Url} with prompt: {Prompt}", QwenApiUrl, prompt);
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(QwenApiUrl, requestPayload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var qwenResponse = await response.Content.ReadFromJsonAsync<QwenGenerationResponse>(cancellationToken: cancellationToken);
                if (qwenResponse != null && qwenResponse.GeneratedTexts.Any())
                {
                    _logger.LogInformation("Received successful response from Qwen service.");
                    return qwenResponse.GeneratedTexts.First();
                }
                _logger.LogWarning("Qwen service returned success but no generated texts.");
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error calling Qwen service. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception while calling Qwen service.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling Qwen service.");
            return null;
        }
    }
}

using System.Text.Json.Serialization;

namespace Agent.Core.Services.Qwen;

public class QwenGenerationRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("max_length")]
    public int MaxLength { get; set; } = 512;

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;

    [JsonPropertyName("top_p")]
    public float TopP { get; set; } = 0.9f;

    [JsonPropertyName("top_k")]
    public int TopK { get; set; } = 50;

    [JsonPropertyName("num_return_sequences")]
    public int NumReturnSequences { get; set; } = 1;
}

public class QwenGenerationResponse
{
    [JsonPropertyName("generated_texts")]
    public List<string> GeneratedTexts { get; set; } = new List<string>();

    // We can ignore the "parameters" part of the response for now if not needed
}


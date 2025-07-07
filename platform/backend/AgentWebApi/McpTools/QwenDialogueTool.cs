using System.ComponentModel;
using AgentWebApi.Services.Qwen;
using ModelContextProtocol.Models.Contexts;
using ModelContextProtocol.Models.Primitives;
using ModelContextProtocol.Models.Tools;

namespace AgentWebApi.McpTools;

public class QwenDialogueTool : ITool
{
    private readonly IQwenServiceClient _qwenServiceClient;
    private readonly ILogger<QwenDialogueTool> _logger;

    public QwenDialogueTool(IQwenServiceClient qwenServiceClient, ILogger<QwenDialogueTool> logger)
    {
        _qwenServiceClient = qwenServiceClient;
        _logger = logger;
    }

    public string Name => "QwenDialogue";
    public string Description => "Engages in a dialogue with the Qwen3 AI model.";

    public async Task<ToolOutput> ExecuteAsync(ToolInput toolInput, CancellationToken cancellationToken = default)
    {
        if (toolInput.Parameters == null || !toolInput.Parameters.TryGetValue("prompt", out var promptValue) || promptValue?.Value is not string userPrompt || string.IsNullOrWhiteSpace(userPrompt))
        {
            _logger.LogWarning("QwenDialogueTool: Prompt parameter is missing or invalid.");
            return new ToolOutput
            {
                IsSuccessful = false,
                ErrorMessage = "Prompt parameter is required and must be a non-empty string.",
                Results = new Dictionary<string, McpPrimitive> { { "response", new McpString("Error: Prompt not provided.") } }
            };
        }

        _logger.LogInformation("QwenDialogueTool: Executing with prompt - {UserPrompt}", userPrompt);

        try
        {
            var qwenResponse = await _qwenServiceClient.GenerateTextAsync(userPrompt, cancellationToken);

            if (qwenResponse != null)
            {
                _logger.LogInformation("QwenDialogueTool: Successfully received response from Qwen service.");
                return new ToolOutput
                {
                    IsSuccessful = true,
                    Results = new Dictionary<string, McpPrimitive>
                    {
                        { "response", new McpString(qwenResponse) }
                    }
                };
            }
            else
            {
                _logger.LogError("QwenDialogueTool: Failed to get a response from Qwen service.");
                return new ToolOutput
                {
                    IsSuccessful = false,
                    ErrorMessage = "Failed to get a response from the Qwen AI model.",
                    Results = new Dictionary<string, McpPrimitive> { { "response", new McpString("Error: No response from AI.") } }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QwenDialogueTool: An error occurred while executing the tool.");
            return new ToolOutput
            {
                IsSuccessful = false,
                ErrorMessage = $"An unexpected error occurred: {ex.Message}",
                Results = new Dictionary<string, McpPrimitive> { { "response", new McpString($"Error: {ex.Message}") } }
            };
        }
    }

    public ToolDefinition GetDefinition()
    {
        return new ToolDefinition
        {
            Name = Name,
            Description = Description,
            InputSchema = new McpSchema
            (
                Type = McpSchemaType.Object,
                Properties = new Dictionary<string, McpSchema>
                {
                    { "prompt", new McpSchema(McpSchemaType.String, "The user's message or question to the Qwen3 AI model.") }
                },
                Required = new List<string> { "prompt" }
            ),
            OutputSchema = new McpSchema
            (
                Type = McpSchemaType.Object,
                Properties = new Dictionary<string, McpSchema>
                {
                    { "response", new McpSchema(McpSchemaType.String, "The AI model's response.") }
                }
            )
        };
    }
}

namespace Agent.Application.Services.Prompts;

public class CompositePromptTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseTemplateId { get; set; } = string.Empty;
    public List<string> IncludeTemplateIds { get; set; } = new();
    public List<PromptVariable> Variables { get; set; } = new();
    public List<string> Examples { get; set; } = new();
    public string OutputSchemaJson { get; set; } = string.Empty;
    public int EstimatedTokenCost { get; set; }
}

public class CompositePromptRequest
{
    public CompositePromptTemplate Template { get; set; } = new();
    public Dictionary<string, object> Variables { get; set; } = new();
    public string? Context { get; set; }
    public int MaxTokens { get; set; } = 2048;
}


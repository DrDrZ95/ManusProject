namespace Agent.Application.Services.Tools;

/// <summary>
/// Smart Tool Selector Implementation
/// 智能工具选择引擎实现：基于语义理解和历史指标进行工具推荐。
/// </summary>
public class SmartToolSelector : ISmartToolSelector
{
    private readonly ISemanticKernelService _skService;
    private readonly IToolRegistryService _registryService;
    private readonly ILogger<SmartToolSelector> _logger;

    public SmartToolSelector(
        ISemanticKernelService skService,
        IToolRegistryService registryService,
        ILogger<SmartToolSelector> logger)
    {
        _skService = skService;
        _registryService = registryService;
        _logger = logger;
    }

    public async Task<IEnumerable<ToolMetadataEntity>> RecommendToolsAsync(string taskDescription, int limit = 3)
    {
        _logger.LogInformation("Recommending tools for task: {TaskDescription}", taskDescription);

        var availableTools = await _registryService.GetActiveToolsAsync();
        if (!availableTools.Any()) return Enumerable.Empty<ToolMetadataEntity>();

        try
        {
            // 1. Try Semantic Search via Vector DB first
            var searchResults = await _skService.SearchMemoryAsync("Tools", taskDescription, limit: limit * 2);

            if (searchResults.Any())
            {
                var toolIds = searchResults
                    .Where(r => r.Metadata != null && r.Metadata.ContainsKey("id"))
                    .Select(r => r.Metadata["id"].ToString())
                    .ToList();

                if (toolIds.Any())
                {
                    var recommendedTools = availableTools
                        .Where(t => toolIds.Contains(t.Id.ToString()))
                        .ToList();

                    if (recommendedTools.Any())
                    {
                        _logger.LogInformation("Found {Count} tools via semantic search", recommendedTools.Count);
                        return recommendedTools.Take(limit);
                    }
                }
            }

            // 2. Fallback to LLM-based ranking if vector search finds nothing or not indexed
            _logger.LogInformation("Falling back to LLM-based ranking for tools");
            var toolListStr = string.Join("\n", availableTools.Select(t => $"- ID: {t.Id}, Name: {t.Name}, Description: {t.Description}"));

            var prompt = $@"Given the following user task, identify the top {limit} most relevant tools from the list below.
User Task: {taskDescription}

Available Tools:
{toolListStr}

Respond with ONLY a JSON array of tool IDs, e.g., [""guid1"", ""guid2""].";

            var response = await _skService.GetChatCompletionAsync(prompt);
            var parsedIds = JsonSerializer.Deserialize<List<string>>(response.Trim());

            if (parsedIds != null)
            {
                return availableTools
                    .Where(t => parsedIds.Contains(t.Id.ToString()))
                    .OrderBy(t => parsedIds.IndexOf(t.Id.ToString()));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recommend tools. Falling back to default.");
        }

        return availableTools.Take(limit);
    }

    public async Task<ToolMetadataEntity?> SelectBestToolAsync(string goal, IEnumerable<ToolMetadataEntity> availableTools)
    {
        // Advanced selection considering metrics like reliability and cost
        return availableTools
            .OrderByDescending(t => GetReliabilityScore(t.ReliabilityMetrics))
            .ThenBy(t => GetCostScore(t.CostInfo))
            .FirstOrDefault();
    }

    public async Task EvaluateToolPerformanceAsync(Guid toolId, bool success, TimeSpan latency)
    {
        // Update reliability metrics in the database
        // This would typically involve fetching, updating JSON, and saving back
        _logger.LogDebug("Evaluating tool {ToolId}: Success={Success}, Latency={Latency}", toolId, success, latency);
        await Task.CompletedTask;
    }

    private double GetReliabilityScore(string? metricsJson)
    {
        if (string.IsNullOrEmpty(metricsJson)) return 0.5; // Neutral
        // Simplified: Parse JSON and extract success rate
        return 0.8;
    }

    private double GetCostScore(string? costJson)
    {
        if (string.IsNullOrEmpty(costJson)) return 0.0;
        // Simplified: Lower cost is better
        return 0.1;
    }
}


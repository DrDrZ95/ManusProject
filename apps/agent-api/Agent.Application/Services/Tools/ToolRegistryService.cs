namespace Agent.Application.Services.Tools;


/// <summary>
/// Tool Registry Service Implementation
/// 工具注册中心实现：负责元数据持久化和内核插件动态管理。
/// </summary>
public class ToolRegistryService : IToolRegistryService
{
    private readonly IRepository<ToolMetadataEntity, Guid> _toolRepo;
    private readonly Kernel _kernel;
    private readonly ILogger<ToolRegistryService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ISemanticKernelService _skService;

    public ToolRegistryService(
        IRepository<ToolMetadataEntity, Guid> toolRepo,
        Kernel kernel,
        ILogger<ToolRegistryService> logger,
        IHttpClientFactory httpClientFactory,
        ISemanticKernelService skService)
    {
        _toolRepo = toolRepo;
        _kernel = kernel;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _skService = skService;
    }

    public async Task<ToolMetadataEntity> RegisterToolAsync(ToolMetadataEntity metadata)
    {
        _logger.LogInformation("Registering new tool: {ToolName} v{Version}", metadata.Name, metadata.Version);

        metadata.CreatedAt = DateTime.UtcNow;
        metadata.UpdatedAt = DateTime.UtcNow;

        var result = await _toolRepo.AddAsync(metadata);

        // Index tool for semantic search
        await IndexToolAsync(result);

        // Trigger hot load if it's a direct plugin or API
        if (metadata.IsEnabled)
        {
            await HotLoadToolsAsync();
        }

        return result;
    }

    public async Task<ToolMetadataEntity> UpdateToolAsync(ToolMetadataEntity metadata)
    {
        _logger.LogInformation("Updating tool: {ToolName} (ID: {ToolId})", metadata.Name, metadata.Id);

        metadata.UpdatedAt = DateTime.UtcNow;
        var result = await _toolRepo.UpdateAsync(metadata);

        // Re-index tool
        await IndexToolAsync(result);

        await HotLoadToolsAsync();

        return result;
    }

    private async Task IndexToolAsync(ToolMetadataEntity tool)
    {
        try
        {
            _logger.LogInformation("Indexing tool for semantic search: {ToolName}", tool.Name);
            var text = $"{tool.Name}: {tool.Description}";
            var metadata = new Dictionary<string, object>
            {
                { "id", tool.Id.ToString() },
                { "name", tool.Name },
                { "version", tool.Version },
                { "type", tool.Type }
            };

            await _skService.SaveMemoryAsync("Tools", text, tool.Id.ToString(), metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index tool: {ToolName}", tool.Name);
        }
    }

    public async Task SetToolStatusAsync(Guid toolId, bool isEnabled)
    {
        var tool = await _toolRepo.GetByIdAsync(toolId);
        if (tool != null)
        {
            tool.IsEnabled = isEnabled;
            tool.UpdatedAt = DateTime.UtcNow;
            await _toolRepo.UpdateAsync(tool);
            await HotLoadToolsAsync();
        }
    }

    public async Task<IEnumerable<ToolMetadataEntity>> GetActiveToolsAsync()
    {
        return await _toolRepo.FindAsync(t => t.IsEnabled);
    }

    public async Task<ToolMetadataEntity?> GetToolAsync(string name, string? version = null)
    {
        var versions = await _toolRepo.FindAsync(t => t.Name == name);
        if (!versions.Any()) return null;

        if (string.IsNullOrEmpty(version))
        {
            // Handle Grayscale Release logic here:
            // Pick based on ReleaseWeight
            var rand = new Random().NextDouble();
            var cumulativeWeight = 0.0;
            var sortedVersions = versions.OrderByDescending(v => v.Version).ToList();

            foreach (var v in sortedVersions)
            {
                cumulativeWeight += v.ReleaseWeight;
                if (rand <= cumulativeWeight) return v;
            }

            return sortedVersions.First();
        }

        return versions.FirstOrDefault(t => t.Version == version);
    }

    public async Task<IEnumerable<ToolMetadataEntity>> GetToolVersionsAsync(string name)
    {
        return await _toolRepo.FindAsync(t => t.Name == name);
    }

    public async Task HotLoadToolsAsync()
    {
        _logger.LogInformation("Hot-loading tool plugins into Semantic Kernel...");

        var activeTools = await GetActiveToolsAsync();

        foreach (var tool in activeTools)
        {
            try
            {
                if (tool.Type == "WebAPI" && !string.IsNullOrEmpty(tool.Configuration))
                {
                    var config = JsonSerializer.Deserialize<WebApiConfig>(tool.Configuration);
                    if (config != null && !string.IsNullOrEmpty(config.OpenApiUrl))
                    {
                        _logger.LogInformation("Loading WebAPI tool from OpenAPI: {ToolName} URL: {Url}", tool.Name, config.OpenApiUrl);

                        // Using Semantic Kernel OpenAPI plugin loader
                        await _kernel.ImportPluginFromOpenApiAsync(
                            pluginName: tool.Name,
                            uri: new Uri(config.OpenApiUrl),
                            executionParameters: new OpenApiFunctionExecutionParameters
                            {
                                HttpClient = _httpClient,
                                IgnoreNonCompliantErrors = true
                            }
                        );
                    }
                }
                else if (tool.Type == "Composite" && !string.IsNullOrEmpty(tool.Configuration))
                {
                    _logger.LogInformation("Loading Composite tool: {ToolName}", tool.Name);

                    var compositeConfig = JsonSerializer.Deserialize<CompositeToolConfig>(tool.Configuration);
                    if (compositeConfig != null)
                    {
                        // Create a composite function from the steps
                        var compositeFunction = KernelFunctionFactory.CreateFromMethod(
                            async (Kernel kernel, KernelArguments args) =>
                            {
                                var context = new Dictionary<string, object?>();
                                // Initialize context with input arguments
                                foreach (var arg in args)
                                {
                                    context[arg.Key] = arg.Value;
                                }

                                object? lastResult = null;

                                foreach (var step in compositeConfig.Steps)
                                {
                                    _logger.LogDebug("Executing step: {PluginName}.{FunctionName}", step.PluginName, step.FunctionName);

                                    var stepArgs = new KernelArguments();
                                    foreach (var mapping in step.InputMapping)
                                    {
                                        // Support mapping from context variables or literals
                                        if (mapping.Value.StartsWith("$") && context.TryGetValue(mapping.Value.Substring(1), out var contextVal))
                                        {
                                            stepArgs[mapping.Key] = contextVal;
                                        }
                                        else
                                        {
                                            stepArgs[mapping.Key] = mapping.Value;
                                        }
                                    }

                                    var result = await kernel.InvokeAsync(step.PluginName, step.FunctionName, stepArgs);
                                    lastResult = result.GetValue<object>();

                                    if (!string.IsNullOrEmpty(step.OutputVariable))
                                    {
                                        context[step.OutputVariable] = lastResult;
                                    }
                                }

                                return lastResult;
                            },
                            functionName: tool.Name,
                            description: tool.Description
                        );

                        _kernel.Plugins.Add(KernelPluginFactory.CreateFromFunctions(tool.Name, new[] { compositeFunction }));
                    }
                }
                else if (tool.Type == "Plugin")
                {
                    _logger.LogDebug("Plugin tool {ToolName} is expected to be registered via standard DI or assembly loading", tool.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to hot-load tool: {ToolName}", tool.Name);
            }
        }
    }

    private class WebApiConfig
    {
        public string? OpenApiUrl { get; set; }
        public string? AuthType { get; set; }
        public string? ApiKey { get; set; }
    }

    private class CompositeToolConfig
    {
        public List<ToolStep> Steps { get; set; } = new();
    }

    private class ToolStep
    {
        public string PluginName { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public Dictionary<string, string> InputMapping { get; set; } = new();
        public string? OutputVariable { get; set; }
    }
}

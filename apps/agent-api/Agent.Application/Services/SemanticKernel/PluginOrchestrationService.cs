namespace Agent.Application.Services.SemanticKernel;

public class PluginOrchestrationService : IPluginOrchestrationService
{
    private readonly Kernel _kernel;
    private readonly IPermissionService _permissionService;
    private readonly IPrometheusService _prometheusService;
    private readonly ITokenBudgetService _budgetService;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<PluginOrchestrationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAgentTelemetryProvider _telemetryProvider;

    public PluginOrchestrationService(
        Kernel kernel,
        IPermissionService permissionService,
        IPrometheusService prometheusService,
        ITokenBudgetService budgetService,
        SemanticKernelOptions options,
        ILogger<PluginOrchestrationService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAgentTelemetryProvider telemetryProvider)
    {
        _kernel = kernel;
        _permissionService = permissionService;
        _prometheusService = prometheusService;
        _budgetService = budgetService;
        _options = options;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _telemetryProvider = telemetryProvider;

        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(1),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retrying tool call. Attempt: {Attempt}, Error: {Error}", args.AttemptNumber, args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromMinutes(1),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _logger.LogError("Circuit breaker opened for tool calls. Duration: {Duration}", args.BreakDuration);
                    return default;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(60))
            .Build();
    }

    public async Task<string> InvokeFunctionAsync(string plugName, string functionName, Dictionary<string, object>? arguments = null, string? fallbackPlugin = null, string? fallbackFunction = null)
    {
        var stopwatch = Stopwatch.StartNew();
        using var span = _telemetryProvider.StartSpan($"Tool.{plugName}.{functionName}");
        span.SetAttribute("tool.plugin", plugName);
        span.SetAttribute("tool.function", functionName);

        try
        {
            _logger.LogInformation("Invoking function: {FunctionName}", functionName);
            var userId = GetCurrentSessionId();
            if (!await _budgetService.IsWithinBudgetAsync(userId, _options.ToolBudgetLimit))
            {
                throw new InvalidOperationException($"Tool call budget exceeded for user {userId}");
            }

            await CheckFunctionPermissionAsync(functionName);

            var kernelArguments = new KernelArguments();
            if (arguments != null) foreach (var kvp in arguments) kernelArguments[kvp.Key] = kvp.Value;

            var resultValue = await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                try
                {
                    var result = await _kernel.InvokeAsync(plugName, functionName, kernelArguments, ct);
                    _prometheusService.IncrementToolCallCounter(plugName, functionName, "success");
                    return result.ToString();
                }
                catch (Exception ex) when (!string.IsNullOrEmpty(fallbackPlugin) && !string.IsNullOrEmpty(fallbackFunction))
                {
                    _logger.LogWarning(ex, "Tool call failed, falling back to {FallbackPlugin}.{FallbackFunction}", fallbackPlugin, fallbackFunction);
                    _prometheusService.IncrementToolCallCounter(plugName, functionName, "fallback");
                    var fallbackResult = await _kernel.InvokeAsync(fallbackPlugin, fallbackFunction, kernelArguments, ct);
                    return fallbackResult.ToString();
                }
            });

            _prometheusService.ObserveToolCallDuration(plugName, functionName, stopwatch.Elapsed.TotalSeconds);
            
            // Use configured tool cost
            var toolCost = _options.ToolCosts.TryGetValue(functionName, out var cost) ? cost : 
                          (_options.ToolCosts.TryGetValue("default", out var defaultCost) ? defaultCost : 0.0001);
            
            await _budgetService.RecordUsageAsync(userId, toolCost);
            return resultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke function: {FunctionName}", functionName);
            _prometheusService.IncrementToolCallCounter(plugName, functionName, "error");
            span.RecordException(ex);
            span.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally { stopwatch.Stop(); }
    }

    public async Task<T> InvokeFunctionAsync<T>(string plugName, string functionName, Dictionary<string, object>? arguments = null, string? fallbackPlugin = null, string? fallbackFunction = null)
    {
        var stopwatch = Stopwatch.StartNew();
        using var span = _telemetryProvider.StartSpan($"Tool.{plugName}.{functionName}");
        span.SetAttribute("tool.plugin", plugName);
        span.SetAttribute("tool.function", functionName);

        try
        {
            _logger.LogInformation("Invoking function: {FunctionName} with return type: {ReturnType}", functionName, typeof(T).Name);
            var userId = GetCurrentSessionId();
            if (!await _budgetService.IsWithinBudgetAsync(userId, _options.ToolBudgetLimit))
            {
                throw new InvalidOperationException($"Tool call budget exceeded for user {userId}");
            }

            await CheckFunctionPermissionAsync(functionName);

            var kernelArguments = new KernelArguments();
            if (arguments != null) foreach (var kvp in arguments) kernelArguments[kvp.Key] = kvp.Value;

            var resultValue = await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                try
                {
                    var result = await _kernel.InvokeAsync(plugName, functionName, kernelArguments, ct);
                    _prometheusService.IncrementToolCallCounter(plugName, functionName, "success");
                    return result;
                }
                catch (Exception ex) when (!string.IsNullOrEmpty(fallbackPlugin) && !string.IsNullOrEmpty(fallbackFunction))
                {
                    _logger.LogWarning(ex, "Tool call failed, falling back to {FallbackPlugin}.{FallbackFunction}", fallbackPlugin, fallbackFunction);
                    _prometheusService.IncrementToolCallCounter(plugName, functionName, "fallback");
                    return await _kernel.InvokeAsync(fallbackPlugin, fallbackFunction, kernelArguments, ct);
                }
            });

            _prometheusService.ObserveToolCallDuration(plugName, functionName, stopwatch.Elapsed.TotalSeconds);
            
            var toolCost = _options.ToolCosts.TryGetValue(functionName, out var cost) ? cost : 
                          (_options.ToolCosts.TryGetValue("default", out var defaultCost) ? defaultCost : 0.0001);
            
            await _budgetService.RecordUsageAsync(userId, toolCost);

            if (resultValue is T typedResult) return typedResult;
            if (typeof(T) == typeof(string)) return (T)(object)resultValue.ToString();
            throw new InvalidCastException($"Cannot convert result to type {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke function: {FunctionName}", functionName);
            _prometheusService.IncrementToolCallCounter(plugName, functionName, "error");
            span.RecordException(ex);
            span.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally { stopwatch.Stop(); }
    }

    public void AddPlugin(object plugin, string? pluginName = null)
    {
        var name = pluginName ?? plugin.GetType().Name;
        _kernel.Plugins.AddFromObject(plugin, name);
    }

    public void AddPluginFromType<T>(string? pluginName = null) where T : class, new()
    {
        var name = pluginName ?? typeof(T).Name;
        _kernel.Plugins.AddFromObject(new T(), name);
    }

    public IEnumerable<string> GetAvailableFunctions()
    {
        var functions = new List<string>();
        foreach (var plugin in _kernel.Plugins)
        {
            foreach (var function in plugin) functions.Add($"{plugin.Name}.{function.Name}");
        }
        return functions;
    }

    private string GetCurrentSessionId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId) ? "system" : userId;
    }

    private async Task CheckFunctionPermissionAsync(string functionName)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        string requiredPermission = functionName switch
        {
            var f when f.StartsWith("KubernetesPlanner.") || f.StartsWith("IstioPlanner.") => "system.admin",
            var f when f.StartsWith("PostgreSQLPlanner.") || f.StartsWith("ClickHousePlanner.") => "system.config",
            _ => "tool.execute"
        };

        if (!await _permissionService.UserHasPermissionAsync(userId, requiredPermission))
        {
            throw new UnauthorizedAccessException($"User does not have permission '{requiredPermission}' to invoke {functionName}");
        }
    }
}

namespace Agent.Core.Gateway;

/// <summary>
/// 熔断器中间件 - AI-Agent Circuit Breaker Middleware
/// 实现熔断器模式以防止级联故障 - Implements circuit breaker pattern to prevent cascading failures
/// </summary>
public class CircuitBreakerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CircuitBreakerMiddleware> _logger;
    private readonly CircuitBreakerOptions _options;
    private readonly Dictionary<string, ResiliencePipeline> _pipelines;

    public CircuitBreakerMiddleware(
        RequestDelegate next,
        ILogger<CircuitBreakerMiddleware> logger,
        IOptions<CircuitBreakerOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
        _pipelines = new Dictionary<string, ResiliencePipeline>();
        
        // 初始化熔断器管道 - Initialize circuit breaker pipelines
        InitializePipelines();
    }

    /// <summary>
    /// 处理HTTP请求 - Process HTTP request
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var pipelineKey = GetPipelineKey(path);
        
        if (!_pipelines.TryGetValue(pipelineKey, out var pipeline))
        {
            // 如果没有匹配的管道，直接传递请求 - If no matching pipeline, pass through
            await _next(context);
            return;
        }

        try
        {
            // 使用熔断器执行请求 - Execute request with circuit breaker
            await pipeline.ExecuteAsync(async (cancellationToken) =>
            {
                await _next(context);
                
                // 检查响应状态码 - Check response status code
                if (context.Response.StatusCode >= 500)
                {
                    throw new HttpRequestException($"Server error: {context.Response.StatusCode}");
                }
            }, context.RequestAborted);
        }
        catch (BrokenCircuitException ex)
        {
            // 熔断器开启时的处理 - Handle when circuit breaker is open
            _logger.LogWarning("Circuit breaker is open for {PipelineKey}: {Message}", 
                pipelineKey, ex.Message);
            
            context.Response.StatusCode = 503; // Service Unavailable
            await context.Response.WriteAsync("Service temporarily unavailable due to circuit breaker");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in circuit breaker middleware for {PipelineKey}", pipelineKey);
            throw;
        }
    }

    /// <summary>
    /// 初始化熔断器管道 - Initialize circuit breaker pipelines
    /// </summary>
    private void InitializePipelines()
    {
        // AI服务熔断器 - AI Service Circuit Breaker
        _pipelines["ai-service"] = CreatePipeline(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5, // 50%失败率触发熔断 - 50% failure rate triggers circuit breaker
            MinimumThroughput = 10, // 最少10个请求 - Minimum 10 requests
            SamplingDuration = TimeSpan.FromSeconds(30), // 30秒采样周期 - 30 second sampling period
            BreakDuration = TimeSpan.FromSeconds(60), // 熔断持续60秒 - Circuit break for 60 seconds
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        });

        // RAG服务熔断器 - RAG Service Circuit Breaker
        _pipelines["rag-service"] = CreatePipeline(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.6, // RAG服务容错性更高 - Higher fault tolerance for RAG service
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(45),
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        });

        // ChromaDB熔断器 - ChromaDB Circuit Breaker
        _pipelines["chromadb"] = CreatePipeline(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.7, // 数据库服务更宽松的策略 - More lenient policy for database service
            MinimumThroughput = 3,
            SamplingDuration = TimeSpan.FromMinutes(1),
            BreakDuration = TimeSpan.FromSeconds(45),
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        });

        // SignalR熔断器 - SignalR Circuit Breaker
        _pipelines["signalr"] = CreatePipeline(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.4, // 实时通信需要更严格的策略 - Real-time communication needs stricter policy
            MinimumThroughput = 15,
            SamplingDuration = TimeSpan.FromSeconds(20),
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        });
    }

    /// <summary>
    /// 创建弹性管道 - Create resilience pipeline
    /// </summary>
    private ResiliencePipeline CreatePipeline(CircuitBreakerStrategyOptions circuitBreakerOptions)
    {
        return new ResiliencePipelineBuilder()
            .AddCircuitBreaker(circuitBreakerOptions)
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
            })
            .AddTimeout(TimeSpan.FromSeconds(30)) // 30秒超时 - 30 second timeout
            .Build();
    }

    /// <summary>
    /// 根据请求路径获取管道键 - Get pipeline key based on request path
    /// </summary>
    private string GetPipelineKey(string path)
    {
        return path switch
        {
            var p when p.StartsWith("/api/semantickernel") => "ai-service",
            var p when p.StartsWith("/sk/") => "ai-service",
            var p when p.StartsWith("/api/rag") => "rag-service",
            var p when p.StartsWith("/rag/") => "rag-service",
            var p when p.StartsWith("/api/chromadb") => "chromadb",
            var p when p.StartsWith("/chromadb/") => "chromadb",
            var p when p.StartsWith("/hubs/") => "signalr",
            _ => "default"
        };
    }
}

/// <summary>
/// 熔断器配置选项 - Circuit Breaker Configuration Options
/// </summary>
public class CircuitBreakerOptions
{
    public const string SectionName = "CircuitBreaker";

    /// <summary>
    /// 是否启用熔断器 - Whether to enable circuit breaker
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 默认失败率阈值 - Default failure rate threshold
    /// </summary>
    public double DefaultFailureRatio { get; set; } = 0.5;

    /// <summary>
    /// 默认最小吞吐量 - Default minimum throughput
    /// </summary>
    public int DefaultMinimumThroughput { get; set; } = 10;

    /// <summary>
    /// 默认采样持续时间 - Default sampling duration
    /// </summary>
    public TimeSpan DefaultSamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 默认熔断持续时间 - Default break duration
    /// </summary>
    public TimeSpan DefaultBreakDuration { get; set; } = TimeSpan.FromSeconds(60);
}


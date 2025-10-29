
namespace Agent.Api.Extensions;

/// <summary>
/// Python.NET fine-tuning extensions for dependency injection
/// Python.NET微调扩展，用于依赖注入
/// 
/// 为AI-Agent系统提供Python微调功能的服务注册
/// Provides service registration for Python fine-tuning functionality in AI-Agent system
/// </summary>
public static class PythonFinetuneExtensions
{
    /// <summary>
    /// Add Python.NET fine-tuning services
    /// 添加Python.NET微调服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddPythonFinetune(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册微调记录仓储 - Register finetune record repository
        services.AddScoped<IRepository<FinetuneRecordEntity, string>, Repository<FinetuneRecordEntity, string>>();
        
        // 注册Python微调服务 - Register Python finetune service
        services.AddScoped<IPythonFinetuneService, PythonFinetuneService>();
        
        // 配置Python.NET设置 - Configure Python.NET settings
        services.Configure<PythonFinetuneOptions>(configuration.GetSection("Python"));
        
        // 添加内存缓存（如果尚未添加） - Add memory cache if not already added
        services.AddMemoryCache();
        
        return services;
    }

    /// <summary>
    /// Add Python.NET fine-tuning services with custom options
    /// 使用自定义选项添加Python.NET微调服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configureOptions">Options configuration - 选项配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddPythonFinetune(this IServiceCollection services, Action<PythonFinetuneOptions> configureOptions)
    {
        // 注册微调记录仓储 - Register finetune record repository
        services.AddScoped<IRepository<FinetuneRecordEntity, string>, Repository<FinetuneRecordEntity, string>>();
        
        // 注册Python微调服务 - Register Python finetune service
        services.AddScoped<IPythonFinetuneService, PythonFinetuneService>();
        
        // 配置选项 - Configure options
        services.Configure(configureOptions);
        
        // 添加内存缓存（如果尚未添加） - Add memory cache if not already added
        services.AddMemoryCache();
        
        return services;
    }

    /// <summary>
    /// Add health checks for Python.NET fine-tuning
    /// 为Python.NET微调添加健康检查
    /// </summary>
    /// <param name="builder">Health checks builder - 健康检查构建器</param>
    /// <returns>Health checks builder - 健康检查构建器</returns>
    public static IServiceCollection AddPythonFinetuneHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PythonFinetuneHealthCheck>("python_finetune", tags: new[] { "python", "finetune" });
        
        return services;
    }
}

/// <summary>
/// Python.NET fine-tuning configuration options
/// Python.NET微调配置选项
/// </summary>
public class PythonFinetuneOptions
{
    /// <summary>
    /// Python executable path - Python可执行文件路径
    /// </summary>
    public string ExecutablePath { get; set; } = "python";

    /// <summary>
    /// Python home directory - Python主目录
    /// </summary>
    public string? Home { get; set; }

    /// <summary>
    /// AI-Agent project path - AI-Agent项目路径
    /// </summary>
    public string ProjectPath { get; set; } = "/home/ubuntu/ai-agent";

    /// <summary>
    /// Maximum concurrent jobs - 最大并发任务数
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = 2;

    /// <summary>
    /// Job timeout in minutes - 任务超时时间（分钟）
    /// </summary>
    public int JobTimeoutMinutes { get; set; } = 1440; // 24 hours

    /// <summary>
    /// Enable GPU support - 启用GPU支持
    /// </summary>
    public bool EnableGpu { get; set; } = true;

    /// <summary>
    /// Default output directory - 默认输出目录
    /// </summary>
    public string DefaultOutputDir { get; set; } = "/tmp/finetune_outputs";

    /// <summary>
    /// Log retention days - 日志保留天数
    /// </summary>
    public int LogRetentionDays { get; set; } = 30;

    /// <summary>
    /// Enable automatic cleanup - 启用自动清理
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;
}

/// <summary>
/// Python.NET fine-tuning health check
/// Python.NET微调健康检查
/// </summary>
public class PythonFinetuneHealthCheck : IHealthCheck
{
    private readonly IPythonFinetuneService _finetuneService;
    private readonly ILogger<PythonFinetuneHealthCheck> _logger;

    public PythonFinetuneHealthCheck(IPythonFinetuneService finetuneService, ILogger<PythonFinetuneHealthCheck> logger)
    {
        _finetuneService = finetuneService ?? throw new ArgumentNullException(nameof(finetuneService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check health status
    /// 检查健康状态
    /// </summary>
    /// <param name="context">Health check context - 健康检查上下文</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Health check result - 健康检查结果</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing Python.NET fine-tuning health check - 执行Python.NET微调健康检查");

            // 验证Python环境 - Validate Python environment
            var envInfo = await _finetuneService.ValidatePythonEnvironmentAsync();
            
            var data = new Dictionary<string, object>
            {
                ["python_version"] = envInfo.PythonVersion,
                ["python_path"] = envInfo.PythonPath,
                ["is_valid"] = envInfo.IsValid,
                ["installed_packages_count"] = envInfo.InstalledPackages.Count,
                ["missing_packages_count"] = envInfo.MissingPackages.Count,
                ["available_gpus_count"] = envInfo.AvailableGpus.Count,
                ["error_messages_count"] = envInfo.ErrorMessages.Count
            };

            if (envInfo.IsValid)
            {
                _logger.LogInformation("Python.NET fine-tuning health check passed - Python.NET微调健康检查通过");
                return HealthCheckResult.Healthy("Python.NET fine-tuning environment is healthy", data);
            }
            else
            {
                var errorMessage = string.Join("; ", envInfo.ErrorMessages);
                var missingPackages = string.Join(", ", envInfo.MissingPackages);
                
                _logger.LogWarning("Python.NET fine-tuning health check failed - Python.NET微调健康检查失败: {ErrorMessage}", errorMessage);
                
                return HealthCheckResult.Degraded(
                    $"Python.NET fine-tuning environment has issues. Missing packages: {missingPackages}. Errors: {errorMessage}", 
                    data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python.NET fine-tuning health check error - Python.NET微调健康检查错误");
            
            return HealthCheckResult.Unhealthy(
                "Python.NET fine-tuning health check failed with exception", 
                ex, 
                new Dictionary<string, object> { ["exception"] = ex.Message });
        }
    }
}


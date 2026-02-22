namespace Agent.Api.Extensions;

/// <summary>
/// Prompts service extensions for dependency injection
/// 提示词服务的依赖注入扩展
/// 
/// 提供AI-Agent系统中提示词管理服务的配置和注册
/// Provides configuration and registration for prompt management services in AI-Agent system
/// </summary>
public static class PromptsExtensions
{
    /// <summary>
    /// Add prompts services to the service collection
    /// 将提示词服务添加到服务集合
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddPromptsServices(this IServiceCollection services)
    {
        services.AddSingleton<IPromptsService, PromptsService>();
        services.AddScoped<IPromptAnalyticsService, PromptAnalyticsService>();
        services.AddHttpClient<IMlflowTrackingService, MlflowTrackingService>();

        // 添加内存缓存支持 - Add memory cache support
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Add prompts services with custom configuration
    /// 使用自定义配置添加提示词服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configureOptions">Configuration action - 配置操作</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddPromptsServices(
        this IServiceCollection services,
        Action<PromptsOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<IPromptsService, PromptsService>();
        services.AddScoped<IPromptAnalyticsService, PromptAnalyticsService>();
        services.AddHttpClient<IMlflowTrackingService, MlflowTrackingService>();

        // 添加内存缓存支持 - Add memory cache support
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Configure prompts for the application
    /// 为应用程序配置提示词
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <returns>Application builder for chaining - 用于链式调用的应用程序构建器</returns>
    public static IApplicationBuilder UsePrompts(this IApplicationBuilder app)
    {
        // 验证提示词服务是否正确注册 - Verify prompts service is properly registered
        using var scope = app.ApplicationServices.CreateScope();
        var promptsService = scope.ServiceProvider.GetService<IPromptsService>();

        if (promptsService == null)
        {
            throw new InvalidOperationException(
                "Prompts service not registered. Please call AddPromptsServices() in ConfigureServices.");
        }

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IPromptsService>>();
        logger.LogInformation("AI-Agent Prompts system initialized successfully");

        return app;
    }
}

/// <summary>
/// Prompts configuration options
/// 提示词配置选项
/// </summary>
public class PromptsOptions
{
    /// <summary>
    /// Enable caching for prompts - 启用提示词缓存
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache expiration time - 缓存过期时间
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Maximum number of cached prompts - 最大缓存提示词数量
    /// </summary>
    public int MaxCachedPrompts { get; set; } = 1000;

    /// <summary>
    /// Enable prompt validation - 启用提示词验证
    /// </summary>
    public bool EnableValidation { get; set; } = true;

    /// <summary>
    /// Custom prompt templates directory - 自定义提示词模板目录
    /// </summary>
    public string? CustomTemplatesDirectory { get; set; }

    /// <summary>
    /// Enable automatic template loading - 启用自动模板加载
    /// </summary>
    public bool EnableAutoTemplateLoading { get; set; } = false;

    /// <summary>
    /// Supported template file extensions - 支持的模板文件扩展名
    /// </summary>
    public List<string> SupportedExtensions { get; set; } = new() { ".txt", ".md", ".json" };
}


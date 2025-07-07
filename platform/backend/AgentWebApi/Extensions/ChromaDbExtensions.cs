using ChromaDB.Client;

namespace AgentWebApi.Extensions;

/// <summary>
/// Extension methods for configuring ChromaDB services
/// 配置 ChromaDB 服务的扩展方法
/// </summary>
public static class ChromaDbExtensions
{
    /// <summary>
    /// Add ChromaDB services to the service collection
    /// 向服务集合添加 ChromaDB 服务
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddChromaDb(this IServiceCollection services, IConfiguration configuration)
    {
        // Get ChromaDB configuration
        var chromaDbUrl = configuration.GetConnectionString("ChromaDb") ?? "http://localhost:8000";
        
        // Register ChromaDB client as singleton
        services.AddSingleton<ChromaDBClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ChromaDBClient>>();
            logger.LogInformation("Initializing ChromaDB client with URL: {ChromaDbUrl}", chromaDbUrl);
            
            return new ChromaDBClient(chromaDbUrl);
        });
        
        // Register ChromaDB service
        services.AddScoped<Services.IChromaDbService, Services.ChromaDbService>();
        
        return services;
    }
    
    /// <summary>
    /// Add ChromaDB services with custom configuration
    /// 使用自定义配置添加 ChromaDB 服务
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddChromaDb(this IServiceCollection services, Action<ChromaDbOptions> configureOptions)
    {
        var options = new ChromaDbOptions();
        configureOptions(options);
        
        // Register ChromaDB client as singleton
        services.AddSingleton<ChromaDBClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ChromaDBClient>>();
            logger.LogInformation("Initializing ChromaDB client with URL: {ChromaDbUrl}", options.Url);
            
            return new ChromaDBClient(options.Url);
        });
        
        // Register ChromaDB service
        services.AddScoped<Services.IChromaDbService, Services.ChromaDbService>();
        
        return services;
    }
}

/// <summary>
/// ChromaDB configuration options
/// ChromaDB 配置选项
/// </summary>
public class ChromaDbOptions
{
    /// <summary>
    /// ChromaDB server URL
    /// ChromaDB 服务器 URL
    /// </summary>
    public string Url { get; set; } = "http://localhost:8000";
    
    /// <summary>
    /// Connection timeout in seconds
    /// 连接超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum retry attempts
    /// 最大重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}


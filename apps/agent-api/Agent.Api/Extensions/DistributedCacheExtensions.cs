namespace Agent.Api.Extensions;

/// <summary>
/// 分布式缓存扩展类
/// </summary>
public static class DistributedCacheExtensions
{
    /// <summary>
    /// 添加Redis分布式缓存支持
    /// </summary>
    /// <param name="services">IServiceCollection实例</param>
    /// <param name="configuration">IConfiguration实例</param>
    /// <returns>IServiceCollection实例</returns>
    public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Get<string>();

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new ArgumentNullException("Redis:ConnectionString", "Redis connection string cannot be null or empty.");
        }

        // 1. 添加IDistributedCache服务
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "AgentApi:";
        });

        // 2. 注册ConnectionMultiplexer为单例，并优雅处理启动连接失败
        try
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        }
        catch (Exception ex)
        {
            // 在启动阶段捕获连接异常，输出黄色加粗提示，但不阻塞程序启动
            ExternalComponentLogger.LogConnectionError("Redis Startup", ex, "主程序将继续启动，但 Redis 缓存功能将不可用。请确认 Redis 服务状态。");
            
            // 注册一个空的或处于失败状态的单例，防止后续依赖注入失败
            // 更好的做法是注册一个 Lazy 实例或者使用 NullObject 模式
            services.AddSingleton<IConnectionMultiplexer>(sp => 
            {
                try 
                {
                    return ConnectionMultiplexer.Connect(redisConnectionString);
                }
                catch
                {
                    // 如果后续依然连接失败，由业务代码处理异常或降级
                    return null!; 
                }
            });
        }

        return services;
    }
}


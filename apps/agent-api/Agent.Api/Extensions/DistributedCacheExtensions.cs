using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Agent.Api.Extensions
{
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

            // 2. 注册ConnectionMultiplexer为单例，以便在应用程序中共享连接
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

            return services;
        }
    }
}

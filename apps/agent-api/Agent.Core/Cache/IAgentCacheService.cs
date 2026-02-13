namespace Agent.Core.Cache;

/// <summary>
/// 统一的 Agent 缓存服务接口 (Unified Agent Cache Service Interface)
/// 
/// 采用两级缓存设计：
/// L1: In-Memory Cache (内存缓存) - 速度快，容量小，适用于短期、高频访问数据。
/// L2: Distributed Cache (分布式缓存，如 Redis) - 速度较慢，容量大，适用于长期、跨服务共享数据。
/// </summary>
public interface IAgentCacheService
{
    /// <summary>
    /// 尝试从缓存中获取数据，如果不存在则执行工厂函数并存入缓存 (Try to get data from cache, execute factory and set if not found)
    /// </summary>
    /// <typeparam name="T">数据类型 (Data type)</typeparam>
    /// <param name="key">缓存键 (Cache key)</param>
    /// <param name="factory">数据生成工厂函数 (Data generation factory function)</param>
    /// <param name="memoryTtl">L1 内存缓存的 TTL (TTL for L1 memory cache)</param>
    /// <param name="distributedTtl">L2 分布式缓存的 TTL (TTL for L2 distributed cache)</param>
    /// <param name="cancellationToken">取消令牌 (Cancellation token)</param>
    /// <returns>缓存或生成的数据 (Cached or generated data)</returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? memoryTtl = null,
        TimeSpan? distributedTtl = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 尝试从缓存中获取数据 (Try to get data from cache)
    /// </summary>
    /// <typeparam name="T">数据类型 (Data type)</typeparam>
    /// <param name="key">缓存键 (Cache key)</param>
    /// <param name="cancellationToken">取消令牌 (Cancellation token)</param>
    /// <returns>缓存中的数据，如果不存在则为 null (Cached data, or null if not found)</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 将数据存入缓存 (Set data into cache)
    /// </summary>
    /// <typeparam name="T">数据类型 (Data type)</typeparam>
    /// <param name="key">缓存键 (Cache key)</param>
    /// <param name="value">要缓存的值 (Value to cache)</param>
    /// <param name="memoryTtl">L1 内存缓存的 TTL (TTL for L1 memory cache)</param>
    /// <param name="distributedTtl">L2 分布式缓存的 TTL (TTL for L2 distributed cache)</param>
    /// <param name="cancellationToken">取消令牌 (Cancellation token)</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? memoryTtl = null,
        TimeSpan? distributedTtl = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 从缓存中移除数据 (Remove data from cache)
    /// </summary>
    /// <param name="key">缓存键 (Cache key)</param>
    /// <param name="cancellationToken">取消令牌 (Cancellation token)</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

namespace Agent.Core.Cache;

/// <summary>
/// Agent 缓存服务实现 (Agent Cache Service Implementation)
/// 
/// 实现了两级缓存策略：L1 内存缓存 (IMemoryCache) 和 L2 分布式缓存 (IDistributedCache/Redis)。
/// </summary>
public class AgentCacheService : IAgentCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<AgentCacheService> _logger;
    private readonly CacheOptions _options;

    public AgentCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<AgentCacheService> logger,
        IOptions<CacheOptions> options)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// 尝试从缓存中获取数据，如果不存在则执行工厂函数并存入缓存
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? memoryTtl = null,
        TimeSpan? distributedTtl = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // 1. 尝试从 L1 内存缓存获取 (Try to get from L1 Memory Cache)
        if (_memoryCache.TryGetValue(key, out T? result))
        {
            _logger.LogDebug("Cache Hit (L1): {Key}", key);
            return result!;
        }

        // 2. 尝试从 L2 分布式缓存获取 (Try to get from L2 Distributed Cache)
        var distributedValue = await _distributedCache.GetAsync(key, cancellationToken);
        if (distributedValue != null)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(distributedValue);
                result = JsonSerializer.Deserialize<T>(json);

                if (result != null)
                {
                    _logger.LogDebug("Cache Hit (L2): {Key}", key);
                    // L2 命中后，回填到 L1 (Refill L1 after L2 hit)
                    SetMemoryCache(key, result, memoryTtl);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize value from distributed cache for key: {Key}", key);
                // 反序列化失败，清除 L2 缓存 (Clear L2 cache on deserialization failure)
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
        }

        // 3. 缓存未命中，执行工厂函数 (Cache Miss, execute factory)
        _logger.LogDebug("Cache Miss: {Key}. Executing factory function.", key);
        result = await factory();

        // 4. 存入缓存 (Set into cache)
        if (result != null)
        {
            await SetAsync(key, result, memoryTtl, distributedTtl, cancellationToken);
        }

        return result!;
    }

    /// <summary>
    /// 尝试从缓存中获取数据
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        // 1. 尝试从 L1 内存缓存获取
        if (_memoryCache.TryGetValue(key, out T? result))
        {
            return result;
        }

        // 2. 尝试从 L2 分布式缓存获取
        var distributedValue = await _distributedCache.GetAsync(key, cancellationToken);
        if (distributedValue != null)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(distributedValue);
                result = JsonSerializer.Deserialize<T>(json);

                if (result != null)
                {
                    // L2 命中后，回填到 L1
                    SetMemoryCache(key, result, null); // 使用默认 TTL 回填
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize value from distributed cache for key: {Key}", key);
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
        }

        return null;
    }

    /// <summary>
    /// 将数据存入缓存
    /// </summary>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? memoryTtl = null,
        TimeSpan? distributedTtl = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // 1. 存入 L1 内存缓存 (Set into L1 Memory Cache)
        SetMemoryCache(key, value, memoryTtl);

        // 2. 存入 L2 分布式缓存 (Set into L2 Distributed Cache)
        await SetDistributedCache(key, value, distributedTtl, cancellationToken);
    }

    /// <summary>
    /// 从缓存中移除数据
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // 1. 移除 L1 内存缓存 (Remove from L1 Memory Cache)
        _memoryCache.Remove(key);

        // 2. 移除 L2 分布式缓存 (Remove from L2 Distributed Cache)
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    // --- Private Helpers ---

    private void SetMemoryCache<T>(string key, T value, TimeSpan? ttl) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultMemoryTtl
        };
        _memoryCache.Set(key, value, options);
    }

    private async Task SetDistributedCache<T>(string key, T value, TimeSpan? ttl, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultDistributedTtl
            };

            await _distributedCache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize or set value to distributed cache for key: {Key}", key);
        }
    }
}

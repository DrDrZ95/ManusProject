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
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<AgentCacheService> _logger;
    private readonly CacheOptions _options;

    public AgentCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        IConnectionMultiplexer redisConnection,
        ILogger<AgentCacheService> logger,
        IOptions<CacheOptions> options)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _redisConnection = redisConnection;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// 尝试从缓存中获取数据，如果不存在则执行工厂函数并存入缓存
    /// </summary>
    /// <summary>
    /// 尝试从缓存中获取数据，如果不存在则执行工厂函数并存入缓存
    /// 实现了分布式锁机制防止缓存击穿，以及熔断降级逻辑。
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
        try
        {
            var distributedValue = await _distributedCache.GetAsync(key, cancellationToken);
            if (distributedValue != null)
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
        }
        catch (Exception ex)
        {
            // 熔断降级：分布式缓存故障时，记录错误并继续尝试从数据库/原始源获取
            _logger.LogWarning(ex, "Distributed cache failure for key: {Key}. Degrading to factory.", key);
        }

        // 3. 缓存未命中，使用分布式锁防止缓存击穿 (Cache Miss, use distributed lock to prevent breakdown)
        var lockKey = $"lock:{key}";
        var lockToken = Guid.NewGuid().ToString();
        var db = _redisConnection.GetDatabase();
        
        // 尝试获取锁，超时时间 10 秒
        bool lockAcquired = false;
        try
        {
            lockAcquired = await db.LockTakeAsync(lockKey, lockToken, TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to acquire distributed lock for key: {Key}. Proceeding without lock.", key);
        }

        if (lockAcquired)
        {
            try
            {
                // 双重检查：获得锁后再次检查缓存，防止在等待锁期间已有其他线程填补了缓存
                result = await GetAsync<T>(key, cancellationToken);
                if (result != null) return result;

                _logger.LogDebug("Cache Miss: {Key}. Executing factory function with lock.", key);
                result = await factory();

                if (result != null)
                {
                    await SetAsync(key, result, memoryTtl, distributedTtl, cancellationToken);
                }
            }
            finally
            {
                await db.LockReleaseAsync(lockKey, lockToken);
            }
        }
        else
        {
            // 如果获取锁失败（例如 Redis 挂了或锁竞争异常激烈），则直接执行工厂函数
            _logger.LogInformation("Proceeding without lock for key: {Key} (lock not acquired).", key);
            result = await factory();
            if (result != null)
            {
                await SetAsync(key, result, memoryTtl, distributedTtl, cancellationToken);
            }
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
        try
        { 
            var json = JsonSerializer.Serialize(value);
            var data = System.Text.Encoding.UTF8.GetBytes(json);

            var ttl = distributedTtl ?? _options.DefaultDistributedTtl;

            // 添加随机抖动，防止大量缓存同时过期导致雪崩 (Add jitter to prevent cache avalanche)
            var jitterMinutes = new Random().Next(_options.JitterMinMinutes, _options.JitterMaxMinutes);
            var finalTtl = ttl.Add(TimeSpan.FromMinutes(jitterMinutes));

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = finalTtl
            };

            await _distributedCache.SetAsync(key, data, options, cancellationToken);
        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "Failed to set value into distributed cache for key: {Key}", key);
            // 这里不抛出异常，实现降级：L2 失败不影响 L1 和业务流程
        }
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

    /// <summary>
    /// 根据前缀移除缓存
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // 1. 移除 L1 内存缓存
        // 注意：IMemoryCache 不直接支持按前缀删除。
        // 这里采用的方法是：在分布式缓存中删除，并通过后续的机制或 TTL 自然失效。
        // 进阶做法：可以遍历内存缓存的 Key (如果使用了自定义的 Key 跟踪)
        RemoveL1ByPrefix(prefix);

        // 2. 移除 L2 分布式缓存 (Redis)
        await RemoveL2ByPrefixAsync(prefix, cancellationToken);
    }

    private void RemoveL1ByPrefix(string prefix)
    {
        // 由于 IMemoryCache 没有公开获取所有 Key 的 API，
        // 且反射性能较差且不稳定，建议在生产环境中使用统一的过期策略或 Key 追踪。
        // 此处通过反射尝试清理（仅限演示/小型应用，高性能场景需优化）
        try
        {
            var field = typeof(MemoryCache).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var entries = field.GetValue(_memoryCache) as dynamic;
                if (entries != null)
                {
                    var keysToRemove = new List<object>();
                    foreach (var entry in entries)
                    {
                        var key = entry.Key.ToString();
                        if (key.StartsWith(prefix))
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _memoryCache.Remove(key);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove L1 cache by prefix using reflection. This is expected if IMemoryCache implementation changed.");
        }
    }

    private async Task RemoveL2ByPrefixAsync(string prefix, CancellationToken cancellationToken)
    {
        try
        {
            // 获取所有 Redis 节点
            var endpoints = _redisConnection.GetEndPoints();
            var instanceName = "AgentApi:"; // 对应 DistributedCacheExtensions 中的配置
            var pattern = $"{instanceName}{prefix}*";

            foreach (var endpoint in endpoints)
            {
                var server = _redisConnection.GetServer(endpoint);
                // 使用 SCAN 命令避免阻塞 Redis
                var keys = server.Keys(pattern: pattern, pageSize: 1000).ToArray();

                if (keys.Length > 0)
                {
                    var db = _redisConnection.GetDatabase();
                    await db.KeyDeleteAsync(keys);
                    _logger.LogInformation("Removed {Count} keys from Redis by prefix: {Prefix}", keys.Length, prefix);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove L2 cache by prefix: {Prefix}", prefix);
        }
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


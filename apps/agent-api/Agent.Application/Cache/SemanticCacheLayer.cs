namespace Agent.Application.Cache;

/// <summary>
/// 语义相似度缓存层 (Semantic Similarity Cache Layer)
/// </summary>
public class SemanticCacheLayer
{
    private readonly IAgentCacheService _cacheService;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SemanticCacheLayer> _logger;
    private readonly SemanticCacheOptions _options;
    private readonly SemanticKernelOptions _skOptions;

    private const string RedisIndexKey = "cache:semantic:index";

    public SemanticCacheLayer(
        IAgentCacheService cacheService,
        ITextEmbeddingGenerationService embeddingService,
        IConnectionMultiplexer redis,
        ILogger<SemanticCacheLayer> logger,
        IOptions<SemanticCacheOptions> options,
        SemanticKernelOptions skOptions)
    {
        _cacheService = cacheService;
        _embeddingService = embeddingService;
        _redis = redis;
        _logger = logger;
        _options = options.Value;
        _skOptions = skOptions;
    }

    /// <summary>
    /// 获取语义相似缓存结果
    /// </summary>
    public async Task<T> GetSemanticAsync<T>(
        string queryText,
        Func<Task<T>> factory,
        float? similarityThreshold = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled)
        {
            return await factory();
        }

        var threshold = similarityThreshold ?? _options.SimilarityThreshold;

        try
        {
            // 1. 生成查询向量 (手动实现 7 天缓存逻辑以避免循环依赖)
            var queryEmbedding = await GenerateEmbeddingWithCacheAsync(queryText);

            // 2. 获取最近的 N 条索引记录
            var indexEntries = await GetIndexEntriesAsync();

            // 3. 计算余弦相似度 (SIMD 优化)
            SemanticCacheEntry? bestMatch = null;
            float maxSimilarity = -1f;

            foreach (var entry in indexEntries)
            {
                float similarity = CalculateCosineSimilarity(queryEmbedding, entry.Embedding);
                if (similarity > threshold && similarity > maxSimilarity)
                {
                    maxSimilarity = similarity;
                    bestMatch = entry;
                }
            }

            // 4. 如果命中相似缓存
            if (bestMatch != null)
            {
                _logger.LogInformation("Semantic Cache Hit: Query '{Query}' matched with '{Match}' (Similarity: {Similarity:P2})", 
                    queryText, bestMatch.QueryText, maxSimilarity);
                
                var cachedResult = await _cacheService.GetAsync<T>(bestMatch.CacheKey, cancellationToken);
                if (cachedResult != null)
                {
                    return cachedResult;
                }
                
                _logger.LogWarning("Semantic index points to missing cache key: {Key}. Proceeding to factory.", bestMatch.CacheKey);
            }

            // 5. 未命中，执行工厂函数
            var result = await factory();

            // 6. 存入底层缓存 (设置 1 小时 TTL 作为默认值)
            var newCacheKey = $"semantic:data:{Guid.NewGuid():N}";
            await _cacheService.SetAsync(newCacheKey, result, memoryTtl: TimeSpan.FromHours(1), distributedTtl: TimeSpan.FromHours(1), cancellationToken: cancellationToken);

            // 7. 更新语义索引
            await UpdateIndexAsync(new SemanticCacheEntry
            {
                QueryText = queryText,
                Embedding = queryEmbedding,
                CacheKey = newCacheKey,
                CreatedAt = DateTime.UtcNow
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SemanticCacheLayer for query: {Query}", queryText);
            return await factory();
        }
    }

    private async Task<float[]> GenerateEmbeddingWithCacheAsync(string text)
    {
        var contentHash = SecurityHelper.GetSha256Hash(text);
        var cacheKey = $"embedding:{_skOptions.EmbeddingModel}:{contentHash}";

        return await _cacheService.GetOrCreateAsync<float[]>(
            cacheKey,
            async () =>
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
                return embedding.ToArray();
            },
            memoryTtl: TimeSpan.FromDays(7),
            distributedTtl: TimeSpan.FromDays(7)
        );
    }

    private async Task<List<SemanticCacheEntry>> GetIndexEntriesAsync()
    {
        var db = _redis.GetDatabase();
        var data = await db.StringGetAsync(RedisIndexKey);
        
        if (data.IsNullOrEmpty)
        {
            return new List<SemanticCacheEntry>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<SemanticCacheEntry>>(data!) ?? new List<SemanticCacheEntry>();
        }
        catch
        {
            return new List<SemanticCacheEntry>();
        }
    }

    private async Task UpdateIndexAsync(SemanticCacheEntry newEntry)
    {
        var db = _redis.GetDatabase();
        
        // 使用 Redis 锁确保索引更新的原子性 (由于是全量存取，需要简单锁)
        var lockKey = "lock:cache:semantic:index";
        var lockToken = Guid.NewGuid().ToString();
        
        if (await db.LockTakeAsync(lockKey, lockToken, TimeSpan.FromSeconds(5)))
        {
            try
            {
                var entries = await GetIndexEntriesAsync();
                
                // 添加新记录并保持最近 N 条
                entries.Insert(0, newEntry);
                if (entries.Count > _options.MaxIndexSize)
                {
                    entries = entries.Take(_options.MaxIndexSize).ToList();
                }

                var json = JsonSerializer.Serialize(entries);
                await db.StringSetAsync(RedisIndexKey, json, TimeSpan.FromDays(30));
            }
            finally
            {
                await db.LockReleaseAsync(lockKey, lockToken);
            }
        }
    }

    /// <summary>
    /// 使用 SIMD 优化计算余弦相似度
    /// </summary>
    private float CalculateCosineSimilarity(float[] v1, float[] v2)
    {
        if (v1.Length != v2.Length) return 0;

        int count = Vector<float>.Count;
        float dot = 0;
        float mag1 = 0;
        float mag2 = 0;

        int i = 0;
        for (; i <= v1.Length - count; i += count)
        {
            var va = new Vector<float>(v1, i);
            var vb = new Vector<float>(v2, i);
            dot += Vector.Dot(va, vb);
            mag1 += Vector.Dot(va, va);
            mag2 += Vector.Dot(vb, vb);
        }

        for (; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        float denominator = MathF.Sqrt(mag1) * MathF.Sqrt(mag2);
        return denominator == 0 ? 0 : dot / denominator;
    }
}

public class SemanticCacheEntry
{
    public string QueryText { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string CacheKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

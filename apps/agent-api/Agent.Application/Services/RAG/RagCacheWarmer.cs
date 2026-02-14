using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Agent.Application.Services.RAG;

/// <summary>
/// RAG 缓存预热服务实现
/// </summary>
public class RagCacheWarmer : IRagCacheWarmer
{
    private readonly IRagService _ragService;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RagCacheWarmer> _logger;
    private const string HotQueriesKeyPrefix = "rag:hot_queries:";

    public RagCacheWarmer(
        IRagService ragService,
        IConnectionMultiplexer redis,
        ILogger<RagCacheWarmer> logger)
    {
        _ragService = ragService;
        _redis = redis;
        _logger = logger;
    }

    public async Task RecordQueryAccessAsync(string collectionName, RagQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{HotQueriesKeyPrefix}{collectionName}";
            var queryJson = JsonSerializer.Serialize(query);
            
            // 使用 Redis 有序集合 (Sorted Set) 记录查询频率
            await db.SortedSetIncrementAsync(key, queryJson, 1);
            
            // 限制记录数量，只保留前 100 个热点查询
            if (await db.SortedSetLengthAsync(key) > 100)
            {
                await db.SortedSetRemoveRangeByRankAsync(key, 0, -101);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record query access for warmup in collection {CollectionName}", collectionName);
        }
    }

    public async Task WarmupHotQueriesAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cache warmup for collection: {CollectionName}", collectionName);

        try
        {
            var db = _redis.GetDatabase();
            var key = $"{HotQueriesKeyPrefix}{collectionName}";
            
            // 获取前 20 个热点查询
            var hotQueries = await db.SortedSetRangeByRankWithScoresAsync(key, 0, 19, Order.Descending);

            foreach (var entry in hotQueries)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var query = JsonSerializer.Deserialize<RagQuery>(entry.Element!);
                    if (query != null)
                    {
                        _logger.LogDebug("Warming up query: {QueryText} (Score: {Score})", query.Text, entry.Score);
                        // 触发检索逻辑，内部会自动写入缓存
                        await _ragService.HybridRetrievalAsync(collectionName, query);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to warmup a specific query in collection {CollectionName}", collectionName);
                }
            }

            _logger.LogInformation("Finished cache warmup for collection: {CollectionName}. Processed {Count} queries.", 
                collectionName, hotQueries.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to warmup hot queries for collection {CollectionName}", collectionName);
        }
    }
}

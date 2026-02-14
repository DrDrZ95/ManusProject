namespace Agent.Application.Services.RAG;

/// <summary>
/// RAG 缓存预热服务接口
/// </summary>
public interface IRagCacheWarmer
{
    /// <summary>
    /// 预热指定集合的热点查询
    /// </summary>
    Task WarmupHotQueriesAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 记录一次查询访问，用于热点识别
    /// </summary>
    Task RecordQueryAccessAsync(string collectionName, RagQuery query, CancellationToken cancellationToken = default);
}

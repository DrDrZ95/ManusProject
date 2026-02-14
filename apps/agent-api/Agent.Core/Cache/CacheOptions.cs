namespace Agent.Core.Cache;

/// <summary>
/// 缓存配置选项 (Cache Configuration Options)
/// 用于配置L1内存缓存和L2分布式缓存的TTL（Time-To-Live）和其他参数。
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// 配置节名称 (Configuration section name)
    /// </summary>
    public const string Cache = "Cache";

    /// <summary>
    /// L1 内存缓存的默认 TTL (Default TTL for L1 In-Memory Cache)
    /// </summary>
    public TimeSpan DefaultMemoryTtl { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// L2 分布式缓存的默认 TTL (Default TTL for L2 Distributed Cache)
    /// </summary>
    public TimeSpan DefaultDistributedTtl { get; set; } = TimeSpan.FromHours(1);

    // --- L1 Cache TTLs (内存缓存 TTL) ---

    /// <summary>
    /// 嵌入结果缓存 TTL (Embedding Results Cache TTL) - 24 小时
    /// </summary>
    public TimeSpan EmbeddingResultsTtl { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// 热门 RAG 查询结果缓存 TTL (Popular RAG Query Results Cache TTL) - 1 小时
    /// </summary>
    public TimeSpan PopularRagQueryTtl { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// LLM Prompt-Response 对缓存 TTL (LLM Prompt-Response Pairs Cache TTL) - 30 分钟
    /// </summary>
    public TimeSpan LlmPromptResponseTtl { get; set; } = TimeSpan.FromMinutes(30);

    // --- L2 Cache TTLs (分布式缓存 TTL) ---

    /// <summary>
    /// 工作流执行历史缓存 TTL (Workflow Execution History Cache TTL) - 7 天
    /// </summary>
    public TimeSpan WorkflowHistoryTtl { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// 用户会话状态缓存 TTL (User Session States Cache TTL) - 24 小时
    /// </summary>
    public TimeSpan UserSessionTtl { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// 文档向量元数据缓存 TTL (Document Vectors Metadata Cache TTL) - 永久（由业务逻辑控制清除）
    /// </summary>
    public TimeSpan DocumentVectorMetadataTtl { get; set; } = TimeSpan.FromDays(365 * 10); // 模拟永久

    /// <summary>
    /// 缓存抖动范围 (Cache Jitter Range)
    /// 用于防止缓存雪崩，为分布式缓存 TTL 添加一个随机偏移量。
    /// 默认 5-15 分钟。
    /// </summary>
    public int JitterMinMinutes { get; set; } = 5;

    /// <summary>
    /// 缓存抖动范围 (Cache Jitter Range)
    /// </summary>
    public int JitterMaxMinutes { get; set; } = 15;
}


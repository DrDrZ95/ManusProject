using AgentWebApi.Services.RAG;

namespace AgentWebApi.Extensions;

/// <summary>
/// RAG service extensions for dependency injection
/// RAG服务的依赖注入扩展
/// </summary>
public static class RagExtensions
{
    /// <summary>
    /// Add RAG services to the service collection
    /// 将RAG服务添加到服务集合
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddRagServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register RAG service - 注册RAG服务
        services.AddScoped<IRagService, RagService>();

        // Configure RAG options from configuration - 从配置中配置RAG选项
        services.Configure<RagServiceOptions>(configuration.GetSection("RAG"));

        return services;
    }
}

/// <summary>
/// RAG service configuration options
/// RAG服务配置选项
/// </summary>
public class RagServiceOptions
{
    /// <summary>
    /// Default chunk size for document splitting - 文档分割的默认块大小
    /// </summary>
    public int DefaultChunkSize { get; set; } = 1000;

    /// <summary>
    /// Default chunk overlap - 默认块重叠
    /// </summary>
    public int DefaultChunkOverlap { get; set; } = 200;

    /// <summary>
    /// Default retrieval strategy - 默认检索策略
    /// </summary>
    public string DefaultRetrievalStrategy { get; set; } = "Hybrid";

    /// <summary>
    /// Default number of results to retrieve - 默认检索结果数量
    /// </summary>
    public int DefaultTopK { get; set; } = 5;

    /// <summary>
    /// Default minimum similarity threshold - 默认最小相似度阈值
    /// </summary>
    public float DefaultMinSimilarity { get; set; } = 0.7f;

    /// <summary>
    /// Enable re-ranking by default - 默认启用重排序
    /// </summary>
    public bool EnableReRanking { get; set; } = true;

    /// <summary>
    /// Default hybrid retrieval weights - 默认混合检索权重
    /// </summary>
    public HybridWeightsOptions DefaultWeights { get; set; } = new();

    /// <summary>
    /// Maximum context length for generation - 生成的最大上下文长度
    /// </summary>
    public int MaxContextLength { get; set; } = 4000;

    /// <summary>
    /// Default generation temperature - 默认生成温度
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Default maximum tokens for generation - 默认生成的最大令牌数
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1000;
}

/// <summary>
/// Hybrid retrieval weights configuration
/// 混合检索权重配置
/// </summary>
public class HybridWeightsOptions
{
    /// <summary>
    /// Vector similarity weight - 向量相似度权重
    /// </summary>
    public float VectorWeight { get; set; } = 0.6f;

    /// <summary>
    /// Keyword matching weight - 关键词匹配权重
    /// </summary>
    public float KeywordWeight { get; set; } = 0.3f;

    /// <summary>
    /// Semantic similarity weight - 语义相似度权重
    /// </summary>
    public float SemanticWeight { get; set; } = 0.1f;
}


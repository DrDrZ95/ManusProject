namespace Agent.Core.Memory.Interfaces;

/// <summary>
/// Long-term memory interface (Structured & Vector)
/// 长期记忆接口（结构化和向量）
/// </summary>
public interface ILongTermMemory
{
    /// <summary>
    /// Save a structured memory (fact, preference, etc.)
    /// 保存结构化记忆（事实、偏好等）
    /// </summary>
    Task SaveStructuredMemoryAsync(StructuredMemoryEntity memory);

    /// <summary>
    /// Search structured memories
    /// 搜索结构化记忆
    /// </summary>
    Task<List<StructuredMemoryEntity>> SearchStructuredMemoryAsync(string userId, string query, string? type = null);

    /// <summary>
    /// Retrieve relevant knowledge using vector search
    /// 使用向量搜索检索相关知识
    /// </summary>
    Task<List<string>> RetrieveRelevantKnowledgeAsync(string query, int limit = 5, double minRelevance = 0.7);

    /// <summary>
    /// Archive less important memories
    /// 归档不重要的记忆
    /// </summary>
    Task ArchiveMemoriesAsync(string userId, double importanceThreshold = 0.2);
}


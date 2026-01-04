namespace Agent.Core.Services.RAG;

/// <summary>
/// RAG document representation
/// RAG文档表示
/// </summary>
public class RagDocument
{
    /// <summary>
    /// Document unique identifier - 文档唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document title - 文档标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Document content - 文档内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Document summary - 文档摘要
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Document metadata - 文档元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Document chunks for better retrieval - 用于更好检索的文档块
    /// </summary>
    public List<RagDocumentChunk> Chunks { get; set; } = new();

    /// <summary>
    /// Document creation time - 文档创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Document last update time - 文档最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

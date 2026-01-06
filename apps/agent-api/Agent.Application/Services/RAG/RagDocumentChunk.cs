namespace Agent.Application.Services.RAG;


/// <summary>
/// RAG document chunk for granular retrieval
/// 用于细粒度检索的RAG文档块
/// </summary>
public class RagDocumentChunk
{
    /// <summary>
    /// Chunk unique identifier - 块唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Parent document ID - 父文档ID
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Chunk content - 块内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Chunk position in document - 块在文档中的位置
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Chunk metadata - 块元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Pre-computed embedding - 预计算的嵌入
    /// </summary>
    public float[]? Embedding { get; set; }
}

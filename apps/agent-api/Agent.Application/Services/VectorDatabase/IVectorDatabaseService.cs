namespace Agent.Application.Services.VectorDatabase;

/// <summary>
/// Universal vector database interface for multimodal AI applications
/// 多模态AI应用的通用向量数据库接口
/// </summary>
public interface IVectorDatabaseService
{
    // Collection Management - 集合管理
    Task<VectorCollection> CreateCollectionAsync(string name, VectorCollectionOptions? config = null);
    Task<VectorCollection> GetCollectionAsync(string name);
    Task<bool> DeleteCollectionAsync(string name);
    Task<IEnumerable<VectorCollection>> ListCollectionsAsync();

    // Document Operations - 文档操作
    Task AddDocumentsAsync(string collectionId, IEnumerable<VectorDocument> documents);
    Task<IEnumerable<VectorDocument>> GetDocumentsAsync(string collectionName, IEnumerable<string>? ids = null, VectorFilter? filter = null);
    Task UpdateDocumentsAsync(string collectionId, IEnumerable<VectorDocument> documents);
    Task DeleteDocumentsAsync(string collectionId, IEnumerable<string>? ids = null, VectorFilter? filter = null);

    // Vector Search - 向量搜索
    Task<VectorSearchResult> SearchAsync(string collectionName, VectorSearchRequest request);
    Task<VectorSearchResult> SearchByTextAsync(string collectionName, string text, VectorSearchOptions? options = null);
    Task<VectorSearchResult> SearchByEmbeddingAsync(string collectionName, float[] embedding, VectorSearchOptions? options = null);

    // Cross-Modal Search - 跨模态搜索 (Using local file paths as requested by user)
    /// <summary>
    /// Performs a cross-modal search using an image file path.
    /// 使用图像文件路径执行跨模态搜索。
    /// </summary>
    Task<VectorSearchResult> SearchByImageAsync(string collectionName, string imagePath, VectorSearchOptions? options = null);

    /// <summary>
    /// Performs a voice search using an audio file path.
    /// 使用音频文件路径执行语音搜索。
    /// </summary>
    Task<VectorSearchResult> SearchByAudioAsync(string collectionName, string audioPath, VectorSearchOptions? options = null);

}


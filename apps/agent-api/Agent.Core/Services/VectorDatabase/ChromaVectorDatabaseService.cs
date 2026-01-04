namespace Agent.Core.Services.VectorDatabase;

/// <summary>
/// ChromaDB implementation of universal vector database service
/// ChromaDB通用向量数据库服务的实现
/// </summary>
public class ChromaVectorDatabaseService : IVectorDatabaseService
{
    private readonly IChromaClient _client;
    private readonly ILogger<ChromaVectorDatabaseService> _logger;
    private readonly IImageEmbeddingService _imageEmbeddingService;
    private readonly IAudioEmbeddingService _audioEmbeddingService;
    private readonly ISpeechToTextService _speechToTextService;
    private readonly IAgentCacheService _cacheService;
    private readonly VectorDatabaseOptions _options;

    public ChromaVectorDatabaseService(
        IChromaClient client,
        ILogger<ChromaVectorDatabaseService> logger,
        IImageEmbeddingService imageEmbeddingService,
        IAudioEmbeddingService audioEmbeddingService,
        ISpeechToTextService speechToTextService,
        IAgentCacheService cacheService,
        IOptions<VectorDatabaseOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _imageEmbeddingService = imageEmbeddingService ?? throw new ArgumentNullException(nameof(imageEmbeddingService));
        _audioEmbeddingService = audioEmbeddingService ?? throw new ArgumentNullException(nameof(audioEmbeddingService));
        _speechToTextService = speechToTextService ?? throw new ArgumentNullException(nameof(speechToTextService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    #region Collection Management - 集合管理

    public async Task<VectorCollection> CreateCollectionAsync(string name, VectorCollectionOptions? config = null)
    {
        await _client.CreateCollectionAsync(name);
        await _cacheService.RemoveAsync($"vector:collections:{name}");
        return new VectorCollection { Name = name };
    }

    public async Task<VectorCollection> GetCollectionAsync(string name)
    {
        var cacheKey = $"vector:collection:{name}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            _logger.LogInformation("Cache Miss: Getting vector collection: {CollectionName}", name);
            
            var zeroEmbedding = new ReadOnlyMemory<float>(Enumerable.Repeat(0f, 0).ToArray());
            var queryResult = await _client.QueryEmbeddingsAsync(
                collectionId: name,
                queryEmbeddings: new[] { zeroEmbedding },  // 单查询向量
                nResults: 100000,  // 设大值，确保覆盖集合（可根据你的集合规模调整）
                include: Array.Empty<string>()  // 不返回 documents/metadatas/embeddings/distances，减少数据量
            );
            
            int count = queryResult.Ids?[0]?.Count ?? 0;
            return new VectorCollection
            {
                Name = name,
                DocumentCount = count,
                CreatedAt = DateTime.UtcNow, // Placeholder
                UpdatedAt = DateTime.UtcNow  // Placeholder
            };
        }, distributedTtl: _options.DocumentVectorMetadataTtl);
    }

    public async Task<bool> DeleteCollectionAsync(string name)
    {
        await _client.DeleteCollectionAsync(name);
        await _cacheService.RemoveAsync($"vector:collection:{name}");
        await _cacheService.RemoveAsync($"vector:collections");
        return true;
    }

    public async Task<IEnumerable<VectorCollection>> ListCollectionsAsync()
    {
        var cacheKey = "vector:collections";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            _logger.LogInformation("Cache Miss: Listing all vector collections.");
            var collections = _client.ListCollectionsAsync();
            // 不推荐
            return await collections.Select(c => new VectorCollection { Name = c.ToString() }).ToListAsync();

        }, distributedTtl: _options.DocumentVectorMetadataTtl);
    }

    #endregion

    #region Document Management - 文档管理

    public async Task AddDocumentsAsync(string collectionName, IEnumerable<VectorDocument> documents)
    {
        var collection = await _client.GetCollectionAsync(collectionName);
        var docs = documents.ToList();
        var ids = docs.Select(d => d.Id).ToArray();
        ReadOnlyMemory<float>[] embeddings = docs
            .Select(d => (ReadOnlyMemory<float>)d.Embedding)
            .ToArray();
        var metadatas = docs.Select(d => d.Metadata).ToArray();
        await _client.UpsertEmbeddingsAsync(collectionName, ids, embeddings, metadatas);
    }

    public async Task<IEnumerable<VectorDocument>> GetDocumentsAsync(string collectionId, IEnumerable<string>? ids = null, VectorFilter? filter = null)
    {
        var collection = await _client.GetCollectionAsync(collectionId);
        //var results = await _client.QueryEmbeddingsAsync(collectionId, ids?.ToArray());
        //return results.Select(r => new VectorDocument { Id = r.Id, Metadata = r.Metadata, Embedding = r.Embedding });
        return null;
    }

    public async Task UpdateDocumentsAsync(string collectionName, IEnumerable<VectorDocument> documents)
    {
        var collection = await _client.GetCollectionAsync(collectionName);
        var docs = documents.ToList();
        var ids = docs.Select(d => d.Id).ToArray();
        ReadOnlyMemory<float>[] embeddings = docs
            .Select(d => (ReadOnlyMemory<float>)d.Embedding)
            .ToArray();
        var metadatas = docs.Select(d => d.Metadata).ToArray();
        await _client.UpsertEmbeddingsAsync(collectionName, ids, embeddings, metadatas);
    }

    public async Task DeleteDocumentsAsync(string collectionId, IEnumerable<string> ids, VectorFilter? filter = null)
    {
        var collection = await _client.GetCollectionAsync(collectionId);
        await _client.DeleteEmbeddingsAsync(collectionId, ids.ToArray());
    }

    #endregion

    #region Search - 搜索

    public async Task<VectorSearchResult> SearchAsync(string collectionName, VectorSearchRequest request)
    {
        var collection = await _client.GetCollectionAsync(collectionName);
        //var results = await _client.QueryEmbeddingsAsync(request.Embeddings, request.Options.Limit, request.Filter?.ToDictionary());
        return new VectorSearchResult
        {
            //Results = results.SelectMany(r => r.Select(i => new VectorDocument { Id = i.Id, Metadata = i.Metadata, Embedding = i.Embedding }))
        };
    }

    public async Task<VectorSearchResult> SearchByTextAsync(string collectionName, string text, VectorSearchOptions? options = null)
    {
        // This requires an embedding service, which is not directly available here.
        // This method should be implemented in a higher-level service (e.g., RagService).
        throw new NotSupportedException("SearchByTextAsync should be implemented in a higher-level service that handles embedding generation.");
    }

    public async Task<VectorSearchResult> SearchByEmbeddingAsync(string collectionName, float[] embedding, VectorSearchOptions? options = null)
    {
        var collection = await _client.GetCollectionAsync(collectionName);
        //var results = await _client.QueryEmbeddingsAsync(collectionName, options?.Limit ?? 10, options?.Filter?.ToDictionary());
        return new VectorSearchResult
        {
            //Results = results.SelectMany(r => r.Select(i => new VectorDocument { Id = i.Id, Metadata = i.Metadata, Embedding = i.Embedding }))
        };
    }

    public async Task<VectorSearchResult> SearchByImageAsync(string collectionName, string imagePath, VectorSearchOptions? options = null)
    {
        var embedding = await _imageEmbeddingService.GenerateImageEmbeddingAsync(imagePath);
        return await SearchByEmbeddingAsync(collectionName, embedding, options);
    }

    public async Task<VectorSearchResult> SearchByAudioAsync(string collectionName, string audioPath, VectorSearchOptions? options = null)
    {
        var embedding = await _audioEmbeddingService.GenerateAudioEmbeddingAsync(audioPath);
        return await SearchByEmbeddingAsync(collectionName, embedding, options);
    }

    #endregion
}

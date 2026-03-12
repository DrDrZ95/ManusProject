using Microsoft.SemanticKernel.Embeddings;

namespace Agent.Application.Services.SemanticKernel;

public class EmbeddingAndMemoryService : IEmbeddingAndMemoryService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorDatabaseService _vectorDatabase;
    private readonly IAgentCacheService _cacheService;
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<EmbeddingAndMemoryService> _logger;

    public EmbeddingAndMemoryService(
        ITextEmbeddingGenerationService embeddingService,
        IVectorDatabaseService vectorDatabase,
        IAgentCacheService cacheService,
        SemanticKernelOptions options,
        ILogger<EmbeddingAndMemoryService> logger)
    {
        _embeddingService = embeddingService;
        _vectorDatabase = vectorDatabase;
        _cacheService = cacheService;
        _options = options;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var contentHash = SecurityHelper.GetSha256Hash(text);
        var cacheKey = $"embedding:{_options.EmbeddingModel}:{contentHash}";

        return await _cacheService.GetOrCreateAsync<float[]>(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache Miss (Embedding): Generating embedding for text length: {TextLength}", text.Length);
                var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
                _logger.LogInformation("Embedding generated successfully, dimension: {Dimension}", embedding.Length);
                return embedding.ToArray();
            },
            memoryTtl: TimeSpan.FromDays(7),
            distributedTtl: TimeSpan.FromDays(7)
        );
    }

    public async Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts)
    {
        try
        {
            var textList = texts.ToList();
            _logger.LogInformation("Generating embeddings for {Count} texts", textList.Count);
            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(textList);
            var result = embeddings.Select(e => e.ToArray()).ToList();
            _logger.LogInformation("Embeddings generated successfully for {Count} texts", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings");
            throw;
        }
    }

    public async Task SaveMemoryAsync(string collectionName, string text, string id, Dictionary<string, object>? metadata = null)
    {
        try
        {
            _logger.LogInformation("Saving memory to collection: {CollectionName}, ID: {Id}", collectionName, id);
            var embedding = await GenerateEmbeddingAsync(text);
            var document = new VectorDocument
            {
                Id = id,
                Content = text,
                Embedding = embedding,
                Metadata = metadata ?? new Dictionary<string, object>(),
                Modality = Modality.Text
            };

            try { await _vectorDatabase.GetCollectionAsync(collectionName); }
            catch
            {
                await _vectorDatabase.CreateCollectionAsync(collectionName, new VectorCollectionOptions
                {
                    EmbeddingDimension = embedding.Length,
                    DistanceMetric = DistanceMetric.Cosine,
                    SupportedModalities = new HashSet<Modality> { Modality.Text }
                });
            }

            await _vectorDatabase.AddDocumentsAsync(collectionName, new[] { document });
            _logger.LogInformation("Memory saved successfully to collection: {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save memory to collection: {CollectionName}", collectionName);
            throw;
        }
    }

    public async Task<IEnumerable<MemoryResult>> SearchMemoryAsync(string collectionName, string query, int limit = 10, float minRelevance = 0.7f)
    {
        try
        {
            _logger.LogInformation("Searching memory in collection: {CollectionName}, query length: {QueryLength}", collectionName, query.Length);
            var searchResult = await _vectorDatabase.SearchByTextAsync(collectionName, query, new VectorSearchOptions
            {
                MaxResults = limit,
                MinSimilarity = minRelevance,
                IncludeContent = true,
                IncludeMetadata = true
            });

            var memoryResults = searchResult.Matches.Select(match => new MemoryResult
            {
                Id = match.Id,
                Text = match.Content ?? string.Empty,
                Relevance = match.Score,
                Metadata = match.Metadata
            }).ToList();

            _logger.LogInformation("Memory search completed, found {Count} results", memoryResults.Count);
            return memoryResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search memory in collection: {CollectionName}", collectionName);
            throw;
        }
    }

    public async Task RemoveMemoryAsync(string collectionName, string id)
    {
        try
        {
            _logger.LogInformation("Removing memory from collection: {CollectionName}, ID: {Id}", collectionName, id);
            await _vectorDatabase.DeleteDocumentsAsync(collectionName, new[] { id });
            _logger.LogInformation("Memory removed successfully from collection: {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove memory from collection: {CollectionName}", collectionName);
            throw;
        }
    }
}

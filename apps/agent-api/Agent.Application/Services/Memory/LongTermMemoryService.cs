namespace Agent.Application.Services.Memory;

/// <summary>
/// Long-term memory implementation using Vector Database and DB
/// </summary>
public class LongTermMemoryService : ILongTermMemory
{
    private readonly IVectorDatabaseService _vectorDb;
    private readonly IRepository<StructuredMemoryEntity, Guid> _structuredRepo;
    private readonly ISemanticKernelService _semanticKernel;
    private readonly ILogger<LongTermMemoryService> _logger;

    public LongTermMemoryService(
        IVectorDatabaseService vectorDb,
        IRepository<StructuredMemoryEntity, Guid> structuredRepo,
        ISemanticKernelService semanticKernel,
        ILogger<LongTermMemoryService> logger)
    {
        _vectorDb = vectorDb;
        _structuredRepo = structuredRepo;
        _semanticKernel = semanticKernel;
        _logger = logger;
    }

    public async Task SaveStructuredMemoryAsync(StructuredMemoryEntity memory)
    {
        await _structuredRepo.AddAsync(memory);

        // Also index in vector DB for semantic search if content is substantial
        if (!string.IsNullOrWhiteSpace(memory.Content))
        {
            try
            {
                var embedding = await _semanticKernel.GenerateEmbeddingAsync(memory.Content);

                var doc = new VectorDocument
                {
                    Id = memory.Id.ToString(),
                    Content = memory.Content,
                    Embedding = embedding,
                    Metadata = new Dictionary<string, object>
                    {
                        { "UserId", memory.UserId },
                        { "Type", memory.Type },
                        { "Importance", memory.ImportanceScore }
                    }
                };

                // Assuming "knowledge_base" is the default collection or derived from userId
                await _vectorDb.AddDocumentsAsync("knowledge_base", new[] { doc });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index structured memory in vector DB");
            }
        }
    }

    public async Task<List<StructuredMemoryEntity>> SearchStructuredMemoryAsync(string userId, string query, string? type = null)
    {
        // Simple DB search (exact match or like)
        // In real world, this should use Vector Search + DB Filtering
        // For now, simple implementation
        return await _structuredRepo.FindAsync(m =>
            (m.UserId == userId) &&
            (type == null || m.Type == type) &&
            (m.Content.Contains(query)));
    }

    public async Task<List<string>> RetrieveRelevantKnowledgeAsync(string query, int limit = 5, double minRelevance = 0.7)
    {
        // Use Vector DB search
        // Need to define collection name, e.g., "global_knowledge" or user specific
        string collectionName = "knowledge_base";

        try
        {
            var result = await _vectorDb.SearchByTextAsync(collectionName, query, new VectorSearchOptions { TopK = limit, MinSimilarity = (float)minRelevance });

            return result.Matches
                .Where(m => !string.IsNullOrEmpty(m.Content))
                .Select(m => m.Content!)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve knowledge");
            return new List<string>();
        }
    }

    public async Task ArchiveMemoriesAsync(string userId, double importanceThreshold = 0.2)
    {
        // Find low importance memories and move them or mark them
        // Placeholder
        await Task.CompletedTask;
    }
}


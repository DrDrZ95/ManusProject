using System.Diagnostics;
using ChromaDB.Client;
using ChromaDB.Client.Models;

namespace AgentWebApi.Services.VectorDatabase;

/// <summary>
/// ChromaDB implementation of universal vector database service
/// ChromaDB通用向量数据库服务的实现
/// </summary>
public class ChromaVectorDatabaseService : IVectorDatabaseService
{
    private readonly ChromaDBClient _client;
    private readonly ILogger<ChromaVectorDatabaseService> _logger;

    public ChromaVectorDatabaseService(ChromaDBClient client, ILogger<ChromaVectorDatabaseService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Collection Management - 集合管理

    /// <summary>
    /// Create a new vector collection
    /// 创建新的向量集合
    /// </summary>
    public async Task<VectorCollection> CreateCollectionAsync(string name, VectorCollectionConfig? config = null)
    {
        try
        {
            _logger.LogInformation("Creating vector collection: {CollectionName}", name);

            var metadata = config?.Metadata ?? new Dictionary<string, object>();
            
            // Add configuration metadata - 添加配置元数据
            if (config != null)
            {
                metadata["embedding_dimension"] = config.EmbeddingDimension ?? 384;
                metadata["distance_metric"] = config.DistanceMetric.ToString();
                metadata["supported_modalities"] = string.Join(",", config.SupportedModalities);
            }

            var chromaCollection = await _client.CreateCollectionAsync(name, metadata);

            var vectorCollection = new VectorCollection
            {
                Name = chromaCollection.Name,
                Id = chromaCollection.Id,
                DocumentCount = 0,
                Config = config,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully created vector collection: {CollectionName}", name);
            return vectorCollection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create vector collection: {CollectionName}", name);
            throw;
        }
    }

    /// <summary>
    /// Get an existing vector collection
    /// 获取现有的向量集合
    /// </summary>
    public async Task<VectorCollection> GetCollectionAsync(string name)
    {
        try
        {
            _logger.LogInformation("Getting vector collection: {CollectionName}", name);

            var chromaCollection = await _client.GetCollectionAsync(name);
            
            // Get collection count - 获取集合计数
            var countResult = await chromaCollection.CountAsync();

            var vectorCollection = new VectorCollection
            {
                Name = chromaCollection.Name,
                Id = chromaCollection.Id,
                DocumentCount = countResult,
                CreatedAt = DateTime.UtcNow, // ChromaDB doesn't provide creation time
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully retrieved vector collection: {CollectionName}", name);
            return vectorCollection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vector collection: {CollectionName}", name);
            throw;
        }
    }

    /// <summary>
    /// Delete a vector collection
    /// 删除向量集合
    /// </summary>
    public async Task<bool> DeleteCollectionAsync(string name)
    {
        try
        {
            _logger.LogInformation("Deleting vector collection: {CollectionName}", name);

            await _client.DeleteCollectionAsync(name);

            _logger.LogInformation("Successfully deleted vector collection: {CollectionName}", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete vector collection: {CollectionName}", name);
            return false;
        }
    }

    /// <summary>
    /// List all vector collections
    /// 列出所有向量集合
    /// </summary>
    public async Task<IEnumerable<VectorCollection>> ListCollectionsAsync()
    {
        try
        {
            _logger.LogInformation("Listing all vector collections");

            var chromaCollections = await _client.ListCollectionsAsync();
            var vectorCollections = new List<VectorCollection>();

            foreach (var chromaCollection in chromaCollections)
            {
                var countResult = await chromaCollection.CountAsync();
                
                vectorCollections.Add(new VectorCollection
                {
                    Name = chromaCollection.Name,
                    Id = chromaCollection.Id,
                    DocumentCount = countResult,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation("Successfully listed {Count} vector collections", vectorCollections.Count);
            return vectorCollections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list vector collections");
            throw;
        }
    }

    #endregion

    #region Document Operations - 文档操作

    /// <summary>
    /// Add documents to a vector collection
    /// 向向量集合添加文档
    /// </summary>
    public async Task AddDocumentsAsync(string collectionName, IEnumerable<VectorDocument> documents)
    {
        try
        {
            _logger.LogInformation("Adding {Count} documents to collection: {CollectionName}", 
                documents.Count(), collectionName);

            var collection = await _client.GetCollectionAsync(collectionName);
            
            var ids = documents.Select(d => d.Id).ToList();
            var contents = documents.Select(d => d.Content).ToList();
            var embeddings = documents.Where(d => d.Embedding != null)
                                   .Select(d => d.Embedding!.Select(f => (double)f).ToList())
                                   .ToList();
            var metadatas = documents.Select(d => ConvertMetadata(d)).ToList();

            // Add documents with or without embeddings - 添加带有或不带有嵌入的文档
            if (embeddings.Any())
            {
                await collection.AddAsync(ids, embeddings, metadatas, contents);
            }
            else
            {
                await collection.AddAsync(ids, null, metadatas, contents);
            }

            _logger.LogInformation("Successfully added {Count} documents to collection: {CollectionName}", 
                documents.Count(), collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add documents to collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Get documents from a vector collection
    /// 从向量集合获取文档
    /// </summary>
    public async Task<IEnumerable<VectorDocument>> GetDocumentsAsync(string collectionName, 
        IEnumerable<string>? ids = null, VectorFilter? filter = null)
    {
        try
        {
            _logger.LogInformation("Getting documents from collection: {CollectionName}", collectionName);

            var collection = await _client.GetCollectionAsync(collectionName);
            var whereClause = ConvertFilter(filter);
            
            var result = await collection.GetAsync(ids, whereClause);
            var documents = new List<VectorDocument>();

            for (int i = 0; i < result.Ids.Count; i++)
            {
                var document = new VectorDocument
                {
                    Id = result.Ids[i],
                    Content = result.Documents?[i] ?? string.Empty,
                    Embedding = result.Embeddings?[i]?.Select(d => (float)d).ToArray(),
                    Metadata = result.Metadatas?[i]
                };

                // Extract modality from metadata - 从元数据中提取模态
                if (document.Metadata?.TryGetValue("modality", out var modalityValue) == true)
                {
                    if (Enum.TryParse<Modality>(modalityValue.ToString(), out var modality))
                    {
                        document.Modality = modality;
                    }
                }

                documents.Add(document);
            }

            _logger.LogInformation("Successfully retrieved {Count} documents from collection: {CollectionName}", 
                documents.Count, collectionName);
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents from collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Update documents in a vector collection
    /// 更新向量集合中的文档
    /// </summary>
    public async Task UpdateDocumentsAsync(string collectionName, IEnumerable<VectorDocument> documents)
    {
        try
        {
            _logger.LogInformation("Updating {Count} documents in collection: {CollectionName}", 
                documents.Count(), collectionName);

            var collection = await _client.GetCollectionAsync(collectionName);
            
            var ids = documents.Select(d => d.Id).ToList();
            var contents = documents.Select(d => d.Content).ToList();
            var embeddings = documents.Where(d => d.Embedding != null)
                                   .Select(d => d.Embedding!.Select(f => (double)f).ToList())
                                   .ToList();
            var metadatas = documents.Select(d => ConvertMetadata(d)).ToList();

            if (embeddings.Any())
            {
                await collection.UpdateAsync(ids, embeddings, metadatas, contents);
            }
            else
            {
                await collection.UpdateAsync(ids, null, metadatas, contents);
            }

            _logger.LogInformation("Successfully updated {Count} documents in collection: {CollectionName}", 
                documents.Count(), collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update documents in collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Delete documents from a vector collection
    /// 从向量集合删除文档
    /// </summary>
    public async Task DeleteDocumentsAsync(string collectionName, IEnumerable<string>? ids = null, 
        VectorFilter? filter = null)
    {
        try
        {
            _logger.LogInformation("Deleting documents from collection: {CollectionName}", collectionName);

            var collection = await _client.GetCollectionAsync(collectionName);
            var whereClause = ConvertFilter(filter);

            await collection.DeleteAsync(ids, whereClause);

            _logger.LogInformation("Successfully deleted documents from collection: {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete documents from collection: {CollectionName}", collectionName);
            throw;
        }
    }

    #endregion

    #region Vector Search - 向量搜索

    /// <summary>
    /// Perform vector search with custom request
    /// 使用自定义请求执行向量搜索
    /// </summary>
    public async Task<VectorSearchResult> SearchAsync(string collectionName, VectorSearchRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Performing vector search in collection: {CollectionName}", collectionName);

            var collection = await _client.GetCollectionAsync(collectionName);
            var options = request.Options ?? new VectorSearchOptions();
            var whereClause = ConvertFilter(request.Filter);

            QueryResponse result;

            if (request.QueryEmbedding != null)
            {
                // Search by embedding - 通过嵌入搜索
                var queryEmbeddings = new List<IEnumerable<double>>
                {
                    request.QueryEmbedding.Select(f => (double)f)
                };
                result = await collection.QueryAsync(queryEmbeddings, options.MaxResults, whereClause);
            }
            else if (!string.IsNullOrEmpty(request.QueryText))
            {
                // Search by text - 通过文本搜索
                var queryTexts = new List<string> { request.QueryText };
                result = await collection.QueryAsync(queryTexts, options.MaxResults, whereClause);
            }
            else
            {
                throw new ArgumentException("Either QueryText or QueryEmbedding must be provided");
            }

            stopwatch.Stop();

            var searchResult = ConvertToVectorSearchResult(result, options, stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation("Vector search completed in {ElapsedMs}ms with {Count} results", 
                stopwatch.ElapsedMilliseconds, searchResult.Matches.Count());
            
            return searchResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to perform vector search in collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Search by text query
    /// 通过文本查询搜索
    /// </summary>
    public async Task<VectorSearchResult> SearchByTextAsync(string collectionName, string text, 
        VectorSearchOptions? options = null)
    {
        var request = new VectorSearchRequest
        {
            QueryText = text,
            Options = options
        };
        return await SearchAsync(collectionName, request);
    }

    /// <summary>
    /// Search by embedding vector
    /// 通过嵌入向量搜索
    /// </summary>
    public async Task<VectorSearchResult> SearchByEmbeddingAsync(string collectionName, float[] embedding, 
        VectorSearchOptions? options = null)
    {
        var request = new VectorSearchRequest
        {
            QueryEmbedding = embedding,
            Options = options
        };
        return await SearchAsync(collectionName, request);
    }

    #endregion

    #region Multimodal Support - 多模态支持

    /// <summary>
    /// Search by image data (placeholder for future implementation)
    /// 通过图像数据搜索（未来实现的占位符）
    /// </summary>
    public async Task<VectorSearchResult> SearchByImageAsync(string collectionName, byte[] imageData, 
        VectorSearchOptions? options = null)
    {
        // TODO: Implement image embedding generation
        // 待实现：图像嵌入生成
        _logger.LogWarning("Image search not yet implemented, returning empty results");
        
        return new VectorSearchResult
        {
            Matches = new List<VectorSearchMatch>(),
            TotalMatches = 0,
            ExecutionTimeMs = 0,
            SearchMetadata = new Dictionary<string, object>
            {
                ["search_type"] = "image",
                ["status"] = "not_implemented"
            }
        };
    }

    /// <summary>
    /// Search by audio data (placeholder for future implementation)
    /// 通过音频数据搜索（未来实现的占位符）
    /// </summary>
    public async Task<VectorSearchResult> SearchByAudioAsync(string collectionName, byte[] audioData, 
        VectorSearchOptions? options = null)
    {
        // TODO: Implement audio embedding generation
        // 待实现：音频嵌入生成
        _logger.LogWarning("Audio search not yet implemented, returning empty results");
        
        return new VectorSearchResult
        {
            Matches = new List<VectorSearchMatch>(),
            TotalMatches = 0,
            ExecutionTimeMs = 0,
            SearchMetadata = new Dictionary<string, object>
            {
                ["search_type"] = "audio",
                ["status"] = "not_implemented"
            }
        };
    }

    /// <summary>
    /// Multimodal search (placeholder for future implementation)
    /// 多模态搜索（未来实现的占位符）
    /// </summary>
    public async Task<VectorSearchResult> SearchMultimodalAsync(string collectionName, 
        MultimodalSearchRequest request)
    {
        // TODO: Implement multimodal fusion search
        // 待实现：多模态融合搜索
        _logger.LogWarning("Multimodal search not yet implemented, falling back to text search");
        
        if (!string.IsNullOrEmpty(request.Text))
        {
            return await SearchByTextAsync(collectionName, request.Text, request.Options);
        }
        
        return new VectorSearchResult
        {
            Matches = new List<VectorSearchMatch>(),
            TotalMatches = 0,
            ExecutionTimeMs = 0,
            SearchMetadata = new Dictionary<string, object>
            {
                ["search_type"] = "multimodal",
                ["status"] = "not_implemented"
            }
        };
    }

    #endregion

    #region Helper Methods - 辅助方法

    /// <summary>
    /// Convert VectorDocument metadata to ChromaDB format
    /// 将VectorDocument元数据转换为ChromaDB格式
    /// </summary>
    private Dictionary<string, object> ConvertMetadata(VectorDocument document)
    {
        var metadata = document.Metadata ?? new Dictionary<string, object>();
        
        // Add modality information - 添加模态信息
        metadata["modality"] = document.Modality.ToString();
        
        // Add MIME type if available - 如果可用，添加MIME类型
        if (!string.IsNullOrEmpty(document.MimeType))
        {
            metadata["mime_type"] = document.MimeType;
        }
        
        // Add binary data indicator - 添加二进制数据指示器
        if (document.BinaryData != null)
        {
            metadata["has_binary_data"] = true;
            metadata["binary_data_size"] = document.BinaryData.Length;
        }

        return metadata;
    }

    /// <summary>
    /// Convert VectorFilter to ChromaDB where clause
    /// 将VectorFilter转换为ChromaDB的where子句
    /// </summary>
    private Dictionary<string, object>? ConvertFilter(VectorFilter? filter)
    {
        if (filter == null) return null;

        var whereClause = new Dictionary<string, object>();

        // Handle equality filters - 处理等值过滤器
        if (filter.Equals != null)
        {
            foreach (var kvp in filter.Equals)
            {
                whereClause[kvp.Key] = kvp.Value;
            }
        }

        // Handle not equals filters - 处理不等值过滤器
        if (filter.NotEquals != null)
        {
            foreach (var kvp in filter.NotEquals)
            {
                whereClause[$"{kvp.Key}"] = new Dictionary<string, object> { ["$ne"] = kvp.Value };
            }
        }

        // Handle in filters - 处理包含过滤器
        if (filter.In != null)
        {
            foreach (var kvp in filter.In)
            {
                whereClause[$"{kvp.Key}"] = new Dictionary<string, object> { ["$in"] = kvp.Value.ToList() };
            }
        }

        return whereClause.Any() ? whereClause : null;
    }

    /// <summary>
    /// Convert ChromaDB QueryResponse to VectorSearchResult
    /// 将ChromaDB QueryResponse转换为VectorSearchResult
    /// </summary>
    private VectorSearchResult ConvertToVectorSearchResult(QueryResponse result, VectorSearchOptions options, 
        long executionTimeMs)
    {
        var matches = new List<VectorSearchMatch>();

        if (result.Ids?.Any() == true)
        {
            for (int i = 0; i < result.Ids[0].Count; i++)
            {
                var match = new VectorSearchMatch
                {
                    Id = result.Ids[0][i],
                    Score = result.Distances?[0][i] ?? 0f,
                    Distance = result.Distances?[0][i] ?? 0f
                };

                // Add content if requested - 如果请求，添加内容
                if (options.IncludeContent && result.Documents?[0] != null && i < result.Documents[0].Count)
                {
                    match.Content = result.Documents[0][i];
                }

                // Add embeddings if requested - 如果请求，添加嵌入
                if (options.IncludeEmbeddings && result.Embeddings?[0] != null && i < result.Embeddings[0].Count)
                {
                    match.Embedding = result.Embeddings[0][i]?.Select(d => (float)d).ToArray();
                }

                // Add metadata if requested - 如果请求，添加元数据
                if (options.IncludeMetadata && result.Metadatas?[0] != null && i < result.Metadatas[0].Count)
                {
                    match.Metadata = result.Metadatas[0][i];
                    
                    // Extract modality from metadata - 从元数据中提取模态
                    if (match.Metadata?.TryGetValue("modality", out var modalityValue) == true)
                    {
                        if (Enum.TryParse<Modality>(modalityValue.ToString(), out var modality))
                        {
                            match.Modality = modality;
                        }
                    }
                }

                matches.Add(match);
            }
        }

        return new VectorSearchResult
        {
            Matches = matches,
            TotalMatches = matches.Count,
            ExecutionTimeMs = executionTimeMs,
            SearchMetadata = new Dictionary<string, object>
            {
                ["search_type"] = "vector",
                ["backend"] = "chromadb"
            }
        };
    }

    #endregion
}


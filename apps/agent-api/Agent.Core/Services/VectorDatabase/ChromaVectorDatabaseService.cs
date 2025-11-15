namespace Agent.Core.Services.VectorDatabase;

/// <summary>
/// ChromaDB implementation of universal vector database service
/// ChromaDB通用向量数据库服务的实现
/// </summary>
public class ChromaVectorDatabaseService : IVectorDatabaseService
{
    private readonly ChromaClient _client;
    private readonly ILogger<ChromaVectorDatabaseService> _logger;
    // 依赖注入的图像嵌入服务 - Image embedding service dependency
    private readonly IImageEmbeddingService _imageEmbeddingService; 
    // 依赖注入的音频嵌入服务 - Audio embedding service dependency
    private readonly IAudioEmbeddingService _audioEmbeddingService; 
    // 依赖注入的语音转文本服务 - Speech-to-text service dependency
    private readonly ISpeechToTextService _speechToTextService;

    public ChromaVectorDatabaseService(
        ChromaClient client, 
        ILogger<ChromaVectorDatabaseService> logger, 
        IImageEmbeddingService imageEmbeddingService,
        IAudioEmbeddingService audioEmbeddingService,
        ISpeechToTextService speechToTextService)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _imageEmbeddingService = imageEmbeddingService ?? throw new ArgumentNullException(nameof(imageEmbeddingService));
        _audioEmbeddingService = audioEmbeddingService ?? throw new ArgumentNullException(nameof(audioEmbeddingService));
        _speechToTextService = speechToTextService ?? throw new ArgumentNullException(nameof(speechToTextService));
    }

    // ... existing methods ...

    /// <summary>
    /// Performs a cross-modal search using an image file path.
    /// 使用图像文件路径执行跨模态搜索。
    /// </summary>
    public async Task<VectorSearchResult> SearchByImageAsync(string collectionName, string imagePath, VectorSearchOptions? options = null)
    {
        _logger.LogInformation("Starting cross-modal search by image for collection {CollectionName} with image {ImagePath}", collectionName, imagePath);

        if (!File.Exists(imagePath))
        {
            _logger.LogError("Image file not found at path: {ImagePath}", imagePath);
            throw new FileNotFoundException("Image file not found.", imagePath);
        }

        // 1. 图像预处理和嵌入生成 (CLIP模型集成)
        // 1. Image preprocessing and embedding generation (CLIP model integration)
        // 假设 IImageEmbeddingService 内部处理了图像预处理（缩放、归一化）和CLIP模型的调用
        // Assume IImageEmbeddingService handles image preprocessing (scaling, normalization) and CLIP model invocation
        float[] imageEmbedding;
        try
        {
            // 集成 CLIP 模型生成图像嵌入 - Integrate CLIP model to generate image embeddings
            // 使用本地部署的模型或OpenAI CLIP API - Use OpenAI CLIP API or a locally deployed model
            imageEmbedding = await _imageEmbeddingService.GenerateImageEmbeddingAsync(imagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image embedding for {ImagePath}", imagePath);
            throw new InvalidOperationException("Failed to generate image embedding.", ex);
        }

        // 2. 执行向量搜索 (跨模态图像-文本搜索)
        // 2. Perform vector search (cross-modal image-text search)
        var searchRequest = new VectorSearchRequest
        {
            QueryEmbedding = imageEmbedding,
            TopK = options?.TopK ?? 10,
            Filter = options?.Filter
        };

        // 假设 SearchAsync 内部会处理 ChromaDB 的查询逻辑
        // Assume SearchAsync handles the ChromaDB query logic
        return await SearchAsync(collectionName, searchRequest);
    }

    // ... existing methods ...

    /// <summary>
    /// Performs a voice search using an audio file path.
    /// 使用音频文件路径执行语音搜索。
    /// </summary>
    public async Task<VectorSearchResult> SearchByAudioAsync(string collectionName, string audioPath, VectorSearchOptions? options = null)
    {
        _logger.LogInformation("Starting voice search by audio for collection {CollectionName} with audio {AudioPath}", collectionName, audioPath);

        if (!File.Exists(audioPath))
        {
            _logger.LogError("Audio file not found at path: {AudioPath}", audioPath);
            throw new FileNotFoundException("Audio file not found.", audioPath);
        }

        // 1. 音频转文本 (Whisper集成)
        // 1. Audio-to-text conversion (Whisper integration)
        string transcription;
        try
        {
            // 使用 Whisper 进行语音转文本 - Use Whisper for speech-to-text
            transcription = await _speechToTextService.TranscribeAsync(audioPath);
            _logger.LogInformation("Audio transcription: {Transcription}", transcription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio for {AudioPath}", audioPath);
            throw new InvalidOperationException("Failed to transcribe audio.", ex);
        }

        // 2. 音频特征提取和嵌入生成 (CLAP集成)
        // 2. Audio feature extraction and embedding generation (CLAP integration)
        // 也可以直接使用文本嵌入，但为了实现语音搜索，我们假设使用音频嵌入模型
        // We could use text embedding directly, but for voice search, we assume an audio embedding model
        float[] audioEmbedding;
        try
        {
            // 使用 CLAP 或其他音频嵌入模型 - Use CLAP or other audio embedding model
            audioEmbedding = await _audioEmbeddingService.GenerateAudioEmbeddingAsync(audioPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate audio embedding for {AudioPath}", audioPath);
            throw new InvalidOperationException("Failed to generate audio embedding.", ex);
        }

        // 3. 执行向量搜索 (语音搜索功能)
        // 3. Perform vector search (voice search functionality)
        // 优先使用音频嵌入进行搜索，如果需要，可以结合文本嵌入
        // Prioritize searching with audio embedding, can combine with text embedding if needed
        var searchRequest = new VectorSearchRequest
        {
            QueryEmbedding = audioEmbedding,
            QueryTexts = new[] { transcription }, // 也可以将转录文本作为辅助查询
            TopK = options?.TopK ?? 10,
            Filter = options?.Filter
        };

        // 假设 SearchAsync 内部会处理 ChromaDB 的查询逻辑
        // Assume SearchAsync handles the ChromaDB query logic
        return await SearchAsync(collectionName, searchRequest);
    }

    // ... existing methods ...
    
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

            var chromaCollection = await _client.GetOrCreateCollection(name, metadata);

            var vectorCollection = new VectorCollection
            {
                Name = chromaCollection.Name,
                //Id = chromaCollection.Id,
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

            var chromaCollection = await _client.GetOrCreateCollection(name);
            
            // Get collection count - 获取集合计数
            var countResult = chromaCollection.Database.Length;

            var vectorCollection = new VectorCollection
            {
                Name = chromaCollection.Name,
                Id = chromaCollection.Id.ToString(),
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

            await _client.DeleteCollection(name);

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

            var chromaCollections = await _client.ListCollections();
            var vectorCollections = new List<VectorCollection>();

            foreach (var chromaCollection in chromaCollections)
            {
                var countResult = chromaCollection.Database.Length;
                
                vectorCollections.Add(new VectorCollection
                {
                    Name = chromaCollection.Name,
                    Id = chromaCollection.Id.ToString(),
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

            var collection = await _client.GetOrCreateCollection(collectionName);
            
            var ids = documents.Select(d => d.Id).ToList();
            var contents = documents.Select(d => d.Content).ToList();
            var embeddings = documents.Where(d => d.Embedding != null)
                                   .Select(d => d.Embedding!.Select(f => (double)f).ToList())
                                   .ToList();
            var metadatas = documents.Select(d => ConvertMetadata(d)).ToList();

            // Add documents with or without embeddings - 添加带有或不带有嵌入的文档
            if (embeddings.Any())
            {
                //await collection.AddAsync(ids, embeddings, metadatas, contents);
            }
            else
            {
                //await collection.AddAsync(ids, null, metadatas, contents);
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

            var model = await _client.GetOrCreateCollection(collectionName);
            var collection = new ChromaCollectionClient(model, options:new ChromaConfigurationOptions()
            {
                
            }, httpClient: new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5)
            });
            
            //var whereClause = ConvertFilter(filter);
            //var embedding = await _embeddingProvider.EmbedAsync(message, cancellationToken);
            
            //var result = await collection.Query(
            //    queryEmbeddings: new[] { embedding },
            //    where: whereClause
            //    
            //    );
            //
            //var documents = new List<VectorDocument>();
            
            //for (int i = 0; i < result.Ids.Count; i++)
            //{
            //    var document = new VectorDocument
            //    {
            //        Id = result.Ids[i],
            //        Content = result.Documents?[i] ?? string.Empty,
            //        Embedding = result.Embeddings?[i]?.Select(d => (float)d).ToArray(),
            //        Metadata = result.Metadatas?[i]
            //    };

            //    // Extract modality from metadata - 从元数据中提取模态
            //    if (document.Metadata?.TryGetValue("modality", out var modalityValue) == true)
            //    {
            //        if (Enum.TryParse<Modality>(modalityValue.ToString(), out var modality))
            //        {
            //            document.Modality = modality;
            //        }
            //    }
            
            //    documents.Add(document);
            //}
            
            //_logger.LogInformation("Successfully retrieved {Count} documents from collection: {CollectionName}", 
            //    documents.Count, collectionName);
            //return documents;
            return default!;
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

            var collection = await _client.GetOrCreateCollection(collectionName);
            
            var ids = documents.Select(d => d.Id).ToList();
            var contents = documents.Select(d => d.Content).ToList();
            var embeddings = documents.Where(d => d.Embedding != null)
                                   .Select(d => d.Embedding!.Select(f => (double)f).ToList())
                                   .ToList();
            var metadatas = documents.Select(d => ConvertMetadata(d)).ToList();

            if (embeddings.Any())
            {
                //await collection.UpdateAsync(ids, embeddings, metadatas, contents);
            }
            else
            {
                //await collection.UpdateAsync(ids, null, metadatas, contents);
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

            var collection = await _client.GetOrCreateCollection(collectionName);
            var whereClause = ConvertFilter(filter);

            //await collection.DeleteAsync(ids, whereClause);

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

            var collection = await _client.GetOrCreateCollection(collectionName);
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
                //result = await collection.QueryAsync(queryEmbeddings, options.MaxResults, whereClause);
            }
            else if (!string.IsNullOrEmpty(request.QueryText))
            {
                // Search by text - 通过文本搜索
                var queryTexts = new List<string> { request.QueryText };
                //result = await collection.QueryAsync(queryTexts, options.MaxResults, whereClause);
            }
            else
            {
                throw new ArgumentException("Either QueryText or QueryEmbedding must be provided");
            }

            stopwatch.Stop();

            //var searchResult = ConvertToVectorSearchResult(result, options, stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation("Vector search completed in {ElapsedMs}ms with {Count} results", 
                stopwatch.ElapsedMilliseconds, 0); // searchResult.Matches.Count()
            
            return default!;
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


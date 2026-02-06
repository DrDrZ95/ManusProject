namespace Agent.Application.Services.SemanticKernel;

/// <summary>
/// Semantic Kernel service implementation
/// 语义内核服务实现
/// </summary>
public class SemanticKernelService : ISemanticKernelService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorDatabaseService _vectorDatabase;
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<SemanticKernelService> _logger;
    private readonly IKubernetesPlanner _kubernetesPlanner;
    private readonly IIstioPlanner _istioPlanner;
    private readonly IPostgreSQLPlanner _postgreSqlPlanner;
    private readonly IClickHousePlanner _clickHousePlanner;
    private readonly IAgentCacheService _cacheService;

    public SemanticKernelService(
        Kernel kernel,
        IChatCompletionService chatService,
        ITextEmbeddingGenerationService embeddingService,
        IVectorDatabaseService vectorDatabase,
        SemanticKernelOptions options,
        ILogger<SemanticKernelService> logger,
        IAgentCacheService cacheService,
        IKubernetesPlanner kubernetesPlanner,
        IIstioPlanner istioPlanner,
        IPostgreSQLPlanner postgreSqlPlanner,
        IClickHousePlanner clickHousePlanner)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _vectorDatabase = vectorDatabase ?? throw new ArgumentNullException(nameof(vectorDatabase));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _kubernetesPlanner = kubernetesPlanner ?? throw new ArgumentNullException(nameof(kubernetesPlanner));
        _istioPlanner = istioPlanner ?? throw new ArgumentNullException(nameof(istioPlanner));
        _postgreSqlPlanner = postgreSqlPlanner ?? throw new ArgumentNullException(nameof(postgreSqlPlanner));
        _clickHousePlanner = clickHousePlanner ?? throw new ArgumentNullException(nameof(clickHousePlanner));

        // Register planners as plugins
        _kernel.Plugins.AddFromObject(_kubernetesPlanner, "KubernetesPlanner");
        _kernel.Plugins.AddFromObject(_istioPlanner, "IstioPlanner");
        _kernel.Plugins.AddFromObject(_postgreSqlPlanner, "PostgreSQLPlanner");
        _kernel.Plugins.AddFromObject(_clickHousePlanner, "ClickHousePlanner");
    }

    #region Chat Completion - 聊天完成

    /// <summary>
    /// Get chat completion response
    /// 获取聊天完成响应
    /// </summary>
    public async Task<string> GetChatCompletionAsync(string prompt, string? systemMessage = null)
    {
        try
        {
            _logger.LogInformation("Getting chat completion for prompt length: {PromptLength}", prompt.Length);

            var chatHistory = new ChatHistory();
            
            // Add system message if provided - 如果提供了系统消息，则添加
            if (!string.IsNullOrEmpty(systemMessage))
            {
                chatHistory.AddSystemMessage(systemMessage);
            }
            
            chatHistory.AddUserMessage(prompt);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature
            };

            var result = await _chatService.GetChatMessageContentAsync(chatHistory, executionSettings);
            
            _logger.LogInformation("Chat completion successful, response length: {ResponseLength}", 
                result.Content?.Length ?? 0);
            
            return result.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chat completion");
            throw;
        }
    }

    /// <summary>
    /// Get streaming chat completion response
    /// 获取流式聊天完成响应
    /// </summary>
    public async IAsyncEnumerable<string> GetStreamingChatCompletionAsync(string prompt, string? systemMessage = null)
    {
        _logger.LogInformation("Getting streaming chat completion for prompt length: {PromptLength}", prompt.Length);

        var chatHistory = new ChatHistory();
        
        if (!string.IsNullOrEmpty(systemMessage))
        {
            chatHistory.AddSystemMessage(systemMessage);
        }
        
        chatHistory.AddUserMessage(prompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature
        };

        await foreach (var content in GetStreamingResponseAsync(chatHistory, executionSettings))
        {
            yield return content;
        };
    }

    /// <summary>
    /// Get chat completion with conversation history
    /// 使用对话历史获取聊天完成
    /// </summary>
    public async Task<string> GetChatCompletionWithHistoryAsync(IEnumerable<ChatMessage> chatHistory)
    {
        try
        {
            _logger.LogInformation("Getting chat completion with {MessageCount} messages in history", 
                chatHistory.Count());

            var kernelChatHistory = new ChatHistory();
            
            foreach (var message in chatHistory)
            {
                switch (message.Role.ToLowerInvariant())
                {
                    case "system":
                        kernelChatHistory.AddSystemMessage(message.Content);
                        break;
                    case "user":
                        kernelChatHistory.AddUserMessage(message.Content);
                        break;
                    case "assistant":
                        kernelChatHistory.AddAssistantMessage(message.Content);
                        break;
                    default:
                        _logger.LogWarning("Unknown message role: {Role}", message.Role);
                        break;
                }
            }

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature
            };

            var result = await _chatService.GetChatMessageContentAsync(kernelChatHistory, executionSettings);
            
            _logger.LogInformation("Chat completion with history successful");
            return result.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chat completion with history");
            throw;
        }
    }

    #endregion

    #region Text Embeddings - 文本嵌入

    /// <summary>
    /// Generate embedding for a single text
    /// 为单个文本生成嵌入
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // 计算文本内容的 SHA256 哈希值作为内容标识 (Calculate SHA256 hash of text content as content identifier)
        var contentHash = SecurityHelper.GetSha256Hash(text);
        // 缓存键: embedding:{模型名称}:{内容哈希} (Cache key: embedding:{model_name}:{content_hash})
        var cacheKey = $"embedding:{_options.EmbeddingModel}:{contentHash}";
        
        // 使用 GetOrCreateAsync 尝试从缓存获取，否则生成并缓存 (Use GetOrCreateAsync to try cache, otherwise generate and cache)
        var embeddingArray = await _cacheService.GetOrCreateAsync<float[]>(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache Miss (Embedding): Generating embedding for text length: {TextLength}", text.Length);
                var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
                _logger.LogInformation("Embedding generated successfully, dimension: {Dimension}", embedding.Length);
                return embedding.ToArray();
            },
            memoryTtl: TimeSpan.FromDays(7), // 7天 TTL (7-day TTL)
            distributedTtl: TimeSpan.FromDays(7) // 7天 TTL (7-day TTL)
        );

        return embeddingArray;
    }

    /// <summary>
    /// Generate embeddings for multiple texts
    /// 为多个文本生成嵌入
    /// </summary>
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

    #endregion

    #region Kernel Functions - 内核函数

    /// <summary>
    /// Invoke a kernel function
    /// 调用内核函数
    /// </summary>
    public async Task<string> InvokeFunctionAsync(string plugName, string functionName, Dictionary<string, object>? arguments = null)
    {
        try
        {
            _logger.LogInformation("Invoking function: {FunctionName}", functionName);

            // Implement role-based access control here
            // For demonstration, let's assume a simple check based on function name
            // In a real application, you would get the user's role from the current context
            // and check against a predefined set of permissions.
            if (functionName.StartsWith("KubernetesPlanner.") || functionName.StartsWith("IstioPlanner."))
            {
                // Example: Only 'highest permission' role can access Kubernetes/Istio functions
                // This is a placeholder. You would replace 'CheckUserRole' with actual auth logic.
                // if (!CheckUserRole("highest permission"))
                // {
                //     throw new UnauthorizedAccessException($"User does not have 'highest permission' to invoke {functionName}");
                // }
                _logger.LogWarning("Access control check for {FunctionName}: Requires 'highest permission'. (Simulated)", functionName);
            }
            else if (functionName.StartsWith("PostgreSQLPlanner.") || functionName.StartsWith("ClickHousePlanner."))
            {
                // Example: Only 'DBA' role can access database functions
                // if (!CheckUserRole("DBA"))
                // {
                //     throw new UnauthorizedAccessException($"User does not have 'DBA' permission to invoke {functionName}");
                // }
                _logger.LogWarning("Access control check for {FunctionName}: Requires 'DBA' permission. (Simulated)", functionName);
            }

            var kernelArguments = new KernelArguments();
            if (arguments != null)
            {
                foreach (var kvp in arguments)
                {
                    kernelArguments[kvp.Key] = kvp.Value;
                }
            }

            var result = await _kernel.InvokeAsync(plugName, functionName, kernelArguments);
            
            _logger.LogInformation("Function {FunctionName} invoked successfully", functionName);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke function: {FunctionName}", functionName);
            throw;
        }
    }

    /// <summary>
    /// Invoke a kernel function with typed return value
    /// 调用内核函数并返回类型化的值
    /// </summary>
    public async Task<T> InvokeFunctionAsync<T>(string plugName, string functionName, Dictionary<string, object>? arguments = null)
    {
        try
        {
            _logger.LogInformation("Invoking function: {FunctionName} with return type: {ReturnType}", 
                functionName, typeof(T).Name);

            // Implement role-based access control here (similar to the non-generic InvokeFunctionAsync)
            if (functionName.StartsWith("KubernetesPlanner.") || functionName.StartsWith("IstioPlanner."))
            {
                _logger.LogWarning("Access control check for {FunctionName}: Requires 'highest permission'. (Simulated)", functionName);
            }
            else if (functionName.StartsWith("PostgreSQLPlanner.") || functionName.StartsWith("ClickHousePlanner."))
            {
                _logger.LogWarning("Access control check for {FunctionName}: Requires 'DBA' permission. (Simulated)", functionName);
            }

            var kernelArguments = new KernelArguments();
            if (arguments != null)
            {
                foreach (var kvp in arguments)
                {
                    kernelArguments[kvp.Key] = kvp.Value;
                }
            }
            
            var result = await _kernel.InvokeAsync(plugName, functionName, kernelArguments);
            
            _logger.LogInformation("Function {FunctionName} invoked successfully", functionName);
            
            if (result is T typedResult)
            {
                return typedResult;
            }
            
            // Try to convert the result - 尝试转换结果
            if (typeof(T) == typeof(string))
            {
                return (T)(object)result.ToString();
            }
            
            throw new InvalidCastException($"Cannot convert result to type {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke function: {FunctionName}", functionName);
            throw;
        }
    }

    #endregion

    #region Plugin Management - 插件管理

    /// <summary>
    /// Add a plugin instance to the kernel
    /// 向内核添加插件实例
    /// </summary>
    public void AddPlugin(object plugin, string? pluginName = null)
    {
        try
        {
            var name = pluginName ?? plugin.GetType().Name;
            _logger.LogInformation("Adding plugin: {PluginName}", name);

            _kernel.Plugins.AddFromObject(plugin, name);
            
            _logger.LogInformation("Plugin {PluginName} added successfully", name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add plugin: {PluginName}", pluginName);
            throw;
        }
    }

    /// <summary>
    /// Add a plugin from type to the kernel
    /// 从类型向内核添加插件
    /// </summary>
    public void AddPluginFromType<T>(string? pluginName = null) where T : class, new()
    {
        try
        {
            var name = pluginName ?? typeof(T).Name;
            _logger.LogInformation("Adding plugin from type: {PluginType}", typeof(T).Name);

            var plugin = new T();
            _kernel.Plugins.AddFromObject(plugin, name);
            
            _logger.LogInformation("Plugin {PluginName} added successfully from type {PluginType}", 
                name, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add plugin from type: {PluginType}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Get list of available functions
    /// 获取可用函数列表
    /// </summary>
    public IEnumerable<string> GetAvailableFunctions()
    {
        try
        {
            var functions = new List<string>();
            
            foreach (var plugin in _kernel.Plugins)
            {
                foreach (var function in plugin)
                {
                    functions.Add($"{plugin.Name}.{function.Name}");
                }
            }
            
            _logger.LogInformation("Retrieved {Count} available functions", functions.Count);
            return functions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available functions");
            throw;
        }
    }

    #endregion

    #region Memory Operations - 记忆操作

    /// <summary>
    /// Save text to memory with vector database
    /// 使用向量数据库将文本保存到记忆中
    /// </summary>
    public async Task SaveMemoryAsync(string collectionName, string text, string id, 
        Dictionary<string, object>? metadata = null)
    {
        try
        {
            _logger.LogInformation("Saving memory to collection: {CollectionName}, ID: {Id}", collectionName, id);

            // Generate embedding for the text - 为文本生成嵌入
            var embedding = await GenerateEmbeddingAsync(text);

            // Create vector document - 创建向量文档
            var document = new VectorDocument
            {
                Id = id,
                Content = text,
                Embedding = embedding,
                Metadata = metadata ?? new Dictionary<string, object>(),
                Modality = Modality.Text
            };

            // Ensure collection exists - 确保集合存在
            try
            {
                await _vectorDatabase.GetCollectionAsync(collectionName);
            }
            catch
            {
                // Create collection if it doesn't exist - 如果集合不存在则创建
                await _vectorDatabase.CreateCollectionAsync(collectionName, new VectorCollectionOptions
                {
                    EmbeddingDimension = embedding.Length,
                    DistanceMetric = DistanceMetric.Cosine,
                    SupportedModalities = new HashSet<Modality> { Modality.Text }
                });
            }

            // Add document to collection - 将文档添加到集合
            await _vectorDatabase.AddDocumentsAsync(collectionName, new[] { document });
            
            _logger.LogInformation("Memory saved successfully to collection: {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save memory to collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Search memory using vector similarity
    /// 使用向量相似性搜索记忆
    /// </summary>
    public async Task<IEnumerable<MemoryResult>> SearchMemoryAsync(string collectionName, string query, 
        int limit = 10, float minRelevance = 0.7f)
    {
        try
        {
            _logger.LogInformation("Searching memory in collection: {CollectionName}, query length: {QueryLength}", 
                collectionName, query.Length);

            // Search using vector database - 使用向量数据库搜索
            var searchResult = await _vectorDatabase.SearchByTextAsync(collectionName, query, 
                new VectorSearchOptions
                {
                    MaxResults = limit,
                    MinSimilarity = minRelevance,
                    IncludeContent = true,
                    IncludeMetadata = true
                });

            // Convert to memory results - 转换为记忆结果
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

    /// <summary>
    /// Remove memory from collection
    /// 从集合中删除记忆
    /// </summary>
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

    public Task<string> ExecutePromptAsync(string initialLlmInteractionFor)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Private Helper Methods - 私有辅助方法

    /// <summary>
    /// Get streaming response from chat service
    /// 从聊天服务获取流式响应
    /// </summary>
    private async IAsyncEnumerable<string> GetStreamingResponseAsync(ChatHistory chatHistory, 
        OpenAIPromptExecutionSettings executionSettings,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var streamingResult = _chatService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, 
            cancellationToken: cancellationToken);

        await foreach (var content in streamingResult.WithCancellation(cancellationToken))
        {
            if (!string.IsNullOrEmpty(content.Content))
            {
                yield return content.Content;
            }
        }
    }

    #endregion
}



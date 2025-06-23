using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentWebApi.Services.VectorDatabase;
using AgentWebApi.Services.SemanticKernel;

namespace AgentWebApi.Services.RAG;

/// <summary>
/// RAG service implementation with hybrid retrieval capabilities
/// 具有混合检索能力的RAG服务实现
/// </summary>
public class RagService : IRagService
{
    private readonly IVectorDatabaseService _vectorDb;
    private readonly ISemanticKernelService _semanticKernel;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IVectorDatabaseService vectorDb,
        ISemanticKernelService semanticKernel,
        ILogger<RagService> logger)
    {
        _vectorDb = vectorDb ?? throw new ArgumentNullException(nameof(vectorDb));
        _semanticKernel = semanticKernel ?? throw new ArgumentNullException(nameof(semanticKernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Document Management - 文档管理

    /// <summary>
    /// Add document to RAG collection with automatic chunking
    /// 将文档添加到RAG集合并自动分块
    /// </summary>
    public async Task<string> AddDocumentAsync(string collectionName, RagDocument document)
    {
        try
        {
            _logger.LogInformation("Adding document {DocumentId} to collection {CollectionName}", 
                document.Id, collectionName);

            // 1. Split document into chunks - 将文档分割成块
            var chunks = await SplitDocumentIntoChunksAsync(document);
            
            // 2. Generate embeddings for chunks - 为块生成嵌入
            var vectorDocuments = new List<VectorDocument>();
            
            foreach (var chunk in chunks)
            {
                var embedding = await _semanticKernel.GenerateEmbeddingAsync(chunk.Content);
                chunk.Embedding = embedding;

                // Create vector document for each chunk - 为每个块创建向量文档
                var vectorDoc = new VectorDocument
                {
                    Id = chunk.Id,
                    Content = chunk.Content,
                    Embedding = embedding,
                    Metadata = new Dictionary<string, object>
                    {
                        ["document_id"] = document.Id,
                        ["document_title"] = document.Title,
                        ["chunk_position"] = chunk.Position,
                        ["chunk_type"] = "content",
                        ["created_at"] = document.CreatedAt.ToString("O"),
                        ["updated_at"] = document.UpdatedAt.ToString("O")
                    },
                    Modality = Modality.Text
                };

                // Add document metadata to chunk metadata - 将文档元数据添加到块元数据
                foreach (var kvp in document.Metadata)
                {
                    vectorDoc.Metadata[$"doc_{kvp.Key}"] = kvp.Value;
                }

                vectorDocuments.Add(vectorDoc);
            }

            // 3. Add document summary as a special chunk - 将文档摘要作为特殊块添加
            if (!string.IsNullOrEmpty(document.Summary))
            {
                var summaryEmbedding = await _semanticKernel.GenerateEmbeddingAsync(document.Summary);
                var summaryDoc = new VectorDocument
                {
                    Id = $"{document.Id}_summary",
                    Content = document.Summary,
                    Embedding = summaryEmbedding,
                    Metadata = new Dictionary<string, object>
                    {
                        ["document_id"] = document.Id,
                        ["document_title"] = document.Title,
                        ["chunk_type"] = "summary",
                        ["created_at"] = document.CreatedAt.ToString("O")
                    },
                    Modality = Modality.Text
                };
                vectorDocuments.Add(summaryDoc);
            }

            // 4. Store in vector database - 存储到向量数据库
            await _vectorDb.AddDocumentsAsync(collectionName, vectorDocuments);

            // 5. Update document with generated chunks - 用生成的块更新文档
            document.Chunks = chunks;

            _logger.LogInformation("Successfully added document {DocumentId} with {ChunkCount} chunks", 
                document.Id, chunks.Count);

            return document.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add document {DocumentId} to collection {CollectionName}", 
                document.Id, collectionName);
            throw;
        }
    }

    /// <summary>
    /// Get documents from RAG collection
    /// 从RAG集合获取文档
    /// </summary>
    public async Task<IEnumerable<RagDocument>> GetDocumentsAsync(string collectionName, IEnumerable<string>? ids = null)
    {
        try
        {
            _logger.LogInformation("Getting documents from collection {CollectionName}", collectionName);

            var filter = new VectorFilter();
            if (ids != null)
            {
                filter.In = new Dictionary<string, IEnumerable<object>>
                {
                    ["document_id"] = ids.Cast<object>()
                };
            }

            var vectorDocs = await _vectorDb.GetDocumentsAsync(collectionName, null, filter);
            
            // Group by document ID and reconstruct RAG documents - 按文档ID分组并重构RAG文档
            var documentGroups = vectorDocs.GroupBy(d => d.Metadata?["document_id"]?.ToString() ?? "unknown");
            var ragDocuments = new List<RagDocument>();

            foreach (var group in documentGroups)
            {
                var firstDoc = group.First();
                var ragDoc = new RagDocument
                {
                    Id = group.Key,
                    Title = firstDoc.Metadata?["document_title"]?.ToString() ?? "",
                    CreatedAt = DateTime.TryParse(firstDoc.Metadata?["created_at"]?.ToString(), out var created) 
                        ? created : DateTime.UtcNow,
                    UpdatedAt = DateTime.TryParse(firstDoc.Metadata?["updated_at"]?.ToString(), out var updated) 
                        ? updated : DateTime.UtcNow
                };

                // Reconstruct chunks - 重构块
                var chunks = group.Where(d => d.Metadata?["chunk_type"]?.ToString() == "content")
                                 .OrderBy(d => int.TryParse(d.Metadata?["chunk_position"]?.ToString(), out var pos) ? pos : 0)
                                 .Select(d => new RagDocumentChunk
                                 {
                                     Id = d.Id,
                                     DocumentId = ragDoc.Id,
                                     Content = d.Content,
                                     Position = int.TryParse(d.Metadata?["chunk_position"]?.ToString(), out var pos) ? pos : 0,
                                     Embedding = d.Embedding,
                                     Metadata = d.Metadata ?? new Dictionary<string, object>()
                                 }).ToList();

                ragDoc.Chunks = chunks;
                ragDoc.Content = string.Join("\n\n", chunks.Select(c => c.Content));

                // Get summary if available - 如果可用，获取摘要
                var summaryDoc = group.FirstOrDefault(d => d.Metadata?["chunk_type"]?.ToString() == "summary");
                if (summaryDoc != null)
                {
                    ragDoc.Summary = summaryDoc.Content;
                }

                ragDocuments.Add(ragDoc);
            }

            _logger.LogInformation("Retrieved {DocumentCount} documents from collection {CollectionName}", 
                ragDocuments.Count, collectionName);

            return ragDocuments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents from collection {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Update document in RAG collection
    /// 更新RAG集合中的文档
    /// </summary>
    public async Task UpdateDocumentAsync(string collectionName, RagDocument document)
    {
        try
        {
            _logger.LogInformation("Updating document {DocumentId} in collection {CollectionName}", 
                document.Id, collectionName);

            // Delete existing document chunks - 删除现有文档块
            await DeleteDocumentAsync(collectionName, document.Id);

            // Add updated document - 添加更新的文档
            document.UpdatedAt = DateTime.UtcNow;
            await AddDocumentAsync(collectionName, document);

            _logger.LogInformation("Successfully updated document {DocumentId}", document.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document {DocumentId}", document.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete document from RAG collection
    /// 从RAG集合删除文档
    /// </summary>
    public async Task DeleteDocumentAsync(string collectionName, string documentId)
    {
        try
        {
            _logger.LogInformation("Deleting document {DocumentId} from collection {CollectionName}", 
                documentId, collectionName);

            var filter = new VectorFilter
            {
                Equals = new Dictionary<string, object>
                {
                    ["document_id"] = documentId
                }
            };

            await _vectorDb.DeleteDocumentsAsync(collectionName, null, filter);

            _logger.LogInformation("Successfully deleted document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Get document count in collection
    /// 获取集合中的文档数量
    /// </summary>
    public async Task<int> GetDocumentCountAsync(string collectionName)
    {
        try
        {
            var collection = await _vectorDb.GetCollectionAsync(collectionName);
            return collection.DocumentCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document count for collection {CollectionName}", collectionName);
            throw;
        }
    }

    #endregion

    #region Hybrid Retrieval - 混合检索

    /// <summary>
    /// Hybrid retrieval combining vector, keyword, and semantic search
    /// 结合向量、关键词和语义搜索的混合检索
    /// </summary>
    public async Task<RagRetrievalResult> HybridRetrievalAsync(string collectionName, RagQuery query)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Performing hybrid retrieval in collection {CollectionName} for query: {Query}", 
                collectionName, query.Text);

            // 1. Parallel execution of different retrieval strategies - 并行执行不同的检索策略
            var vectorTask = VectorRetrievalAsync(collectionName, query.Text, query.TopK * 2);
            var keywordTask = KeywordRetrievalAsync(collectionName, query.Text, query.TopK * 2);
            var semanticTask = SemanticRetrievalAsync(collectionName, query.Text, query.TopK * 2);

            await Task.WhenAll(vectorTask, keywordTask, semanticTask);

            var vectorResults = await vectorTask;
            var keywordResults = await keywordTask;
            var semanticResults = await semanticTask;

            // 2. Merge and score results using hybrid weights - 使用混合权重合并和评分结果
            var weights = query.Weights ?? new HybridRetrievalWeights();
            var mergedResults = MergeRetrievalResults(vectorResults, keywordResults, semanticResults, weights);

            // 3. Apply re-ranking if enabled - 如果启用，应用重排序
            if (query.ReRanking?.Enabled == true)
            {
                mergedResults = await ApplyReRankingAsync(query.Text, mergedResults, query.ReRanking);
            }

            // 4. Apply filters and limit results - 应用过滤器并限制结果
            var filteredResults = ApplyFiltersAndLimit(mergedResults, query);

            stopwatch.Stop();

            var result = new RagRetrievalResult
            {
                Chunks = filteredResults,
                TotalMatches = filteredResults.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Strategy = RetrievalStrategy.Hybrid,
                Metadata = new Dictionary<string, object>
                {
                    ["vector_results"] = vectorResults.TotalMatches,
                    ["keyword_results"] = keywordResults.TotalMatches,
                    ["semantic_results"] = semanticResults.TotalMatches,
                    ["weights"] = JsonSerializer.Serialize(weights),
                    ["reranking_enabled"] = query.ReRanking?.Enabled ?? false
                }
            };

            _logger.LogInformation("Hybrid retrieval completed in {ElapsedMs}ms with {ResultCount} results", 
                stopwatch.ElapsedMilliseconds, result.TotalMatches);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to perform hybrid retrieval");
            throw;
        }
    }

    /// <summary>
    /// Vector similarity retrieval
    /// 向量相似度检索
    /// </summary>
    public async Task<RagRetrievalResult> VectorRetrievalAsync(string collectionName, string query, int topK = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Performing vector retrieval for query: {Query}", query);

            // Generate query embedding - 生成查询嵌入
            var queryEmbedding = await _semanticKernel.GenerateEmbeddingAsync(query);

            // Search using vector similarity - 使用向量相似度搜索
            var searchResult = await _vectorDb.SearchByEmbeddingAsync(collectionName, queryEmbedding, 
                new VectorSearchOptions
                {
                    MaxResults = topK,
                    IncludeContent = true,
                    IncludeMetadata = true,
                    IncludeEmbeddings = false
                });

            stopwatch.Stop();

            // Convert to RAG chunks - 转换为RAG块
            var ragChunks = searchResult.Matches.Select(match => new RagRetrievedChunk
            {
                Chunk = new RagDocumentChunk
                {
                    Id = match.Id,
                    DocumentId = match.Metadata?["document_id"]?.ToString() ?? "",
                    Content = match.Content ?? "",
                    Position = int.TryParse(match.Metadata?["chunk_position"]?.ToString(), out var pos) ? pos : 0,
                    Metadata = match.Metadata ?? new Dictionary<string, object>()
                },
                Score = match.Score,
                VectorScore = match.Score,
                KeywordScore = 0f,
                SemanticScore = 0f
            }).ToList();

            return new RagRetrievalResult
            {
                Chunks = ragChunks,
                TotalMatches = ragChunks.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Strategy = RetrievalStrategy.Vector,
                Metadata = new Dictionary<string, object>
                {
                    ["embedding_dimension"] = queryEmbedding.Length,
                    ["similarity_metric"] = "cosine"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to perform vector retrieval");
            throw;
        }
    }

    /// <summary>
    /// Keyword-based retrieval using TF-IDF and BM25
    /// 使用TF-IDF和BM25的基于关键词的检索
    /// </summary>
    public async Task<RagRetrievalResult> KeywordRetrievalAsync(string collectionName, string query, int topK = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Performing keyword retrieval for query: {Query}", query);

            // Extract keywords from query - 从查询中提取关键词
            var keywords = ExtractKeywords(query);
            
            // Get all documents for keyword matching - 获取所有文档进行关键词匹配
            var allDocs = await _vectorDb.GetDocumentsAsync(collectionName);
            
            // Calculate keyword scores using BM25 algorithm - 使用BM25算法计算关键词分数
            var scoredChunks = new List<RagRetrievedChunk>();
            
            foreach (var doc in allDocs)
            {
                var keywordScore = CalculateBM25Score(doc.Content, keywords);
                
                if (keywordScore > 0)
                {
                    scoredChunks.Add(new RagRetrievedChunk
                    {
                        Chunk = new RagDocumentChunk
                        {
                            Id = doc.Id,
                            DocumentId = doc.Metadata?["document_id"]?.ToString() ?? "",
                            Content = doc.Content,
                            Position = int.TryParse(doc.Metadata?["chunk_position"]?.ToString(), out var pos) ? pos : 0,
                            Metadata = doc.Metadata ?? new Dictionary<string, object>()
                        },
                        Score = keywordScore,
                        VectorScore = 0f,
                        KeywordScore = keywordScore,
                        SemanticScore = 0f,
                        HighlightedContent = HighlightKeywords(doc.Content, keywords)
                    });
                }
            }

            // Sort by keyword score and take top K - 按关键词分数排序并取前K个
            var topResults = scoredChunks.OrderByDescending(c => c.KeywordScore)
                                       .Take(topK)
                                       .ToList();

            stopwatch.Stop();

            return new RagRetrievalResult
            {
                Chunks = topResults,
                TotalMatches = topResults.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Strategy = RetrievalStrategy.Keyword,
                Metadata = new Dictionary<string, object>
                {
                    ["keywords"] = string.Join(", ", keywords),
                    ["algorithm"] = "BM25"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to perform keyword retrieval");
            throw;
        }
    }

    /// <summary>
    /// Semantic retrieval using cross-encoder re-ranking
    /// 使用交叉编码器重排序的语义检索
    /// </summary>
    public async Task<RagRetrievalResult> SemanticRetrievalAsync(string collectionName, string query, int topK = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Performing semantic retrieval for query: {Query}", query);

            // First get candidates using vector search - 首先使用向量搜索获取候选
            var vectorResults = await VectorRetrievalAsync(collectionName, query, topK * 3);
            
            // Apply semantic scoring using cross-encoder approach - 使用交叉编码器方法应用语义评分
            var semanticChunks = new List<RagRetrievedChunk>();
            
            foreach (var chunk in vectorResults.Chunks)
            {
                // Calculate semantic similarity using prompt-based approach - 使用基于提示的方法计算语义相似度
                var semanticScore = await CalculateSemanticSimilarityAsync(query, chunk.Chunk.Content);
                
                chunk.SemanticScore = semanticScore;
                chunk.Score = semanticScore; // Use semantic score as primary score
                
                semanticChunks.Add(chunk);
            }

            // Sort by semantic score and take top K - 按语义分数排序并取前K个
            var topResults = semanticChunks.OrderByDescending(c => c.SemanticScore)
                                         .Take(topK)
                                         .ToList();

            stopwatch.Stop();

            return new RagRetrievalResult
            {
                Chunks = topResults,
                TotalMatches = topResults.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Strategy = RetrievalStrategy.Semantic,
                Metadata = new Dictionary<string, object>
                {
                    ["semantic_model"] = "cross-encoder-simulation",
                    ["candidate_count"] = vectorResults.TotalMatches
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to perform semantic retrieval");
            throw;
        }
    }

    #endregion

    #region RAG Generation - RAG生成

    /// <summary>
    /// Generate response using RAG with retrieved context
    /// 使用RAG和检索到的上下文生成响应
    /// </summary>
    public async Task<RagResponse> GenerateResponseAsync(string collectionName, RagGenerationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Generating RAG response for query: {Query}", request.Query);

            // 1. Retrieve relevant context - 检索相关上下文
            var retrievalQuery = request.RetrievalOptions ?? new RagQuery
            {
                Text = request.Query,
                Strategy = RetrievalStrategy.Hybrid,
                TopK = 5
            };

            var retrievalResult = await HybridRetrievalAsync(collectionName, retrievalQuery);

            // 2. Build context from retrieved chunks - 从检索到的块构建上下文
            var context = BuildContextFromChunks(retrievalResult.Chunks);

            // 3. Create system prompt with context - 使用上下文创建系统提示
            var systemPrompt = BuildSystemPrompt(request.SystemPrompt, context, request.IncludeSources);

            // 4. Generate response using Semantic Kernel - 使用Semantic Kernel生成响应
            var generationOptions = request.GenerationOptions ?? new RagGenerationOptions();
            
            string response;
            if (request.ConversationHistory?.Any() == true)
            {
                // Use conversation history - 使用对话历史
                var chatHistory = request.ConversationHistory.ToList();
                chatHistory.Insert(0, new ChatMessage { Role = "system", Content = systemPrompt });
                chatHistory.Add(new ChatMessage { Role = "user", Content = request.Query });
                
                response = await _semanticKernel.GetChatCompletionWithHistoryAsync(chatHistory);
            }
            else
            {
                // Single turn conversation - 单轮对话
                response = await _semanticKernel.GetChatCompletionAsync(request.Query, systemPrompt);
            }

            // 5. Calculate confidence score - 计算置信度分数
            var confidenceScore = CalculateConfidenceScore(retrievalResult.Chunks, response);

            stopwatch.Stop();

            var ragResponse = new RagResponse
            {
                Response = response,
                Sources = request.IncludeSources ? retrievalResult.Chunks : new List<RagRetrievedChunk>(),
                RetrievalResult = retrievalResult,
                ConfidenceScore = confidenceScore,
                GenerationTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object>
                {
                    ["context_length"] = context.Length,
                    ["source_count"] = retrievalResult.Chunks.Count,
                    ["retrieval_strategy"] = retrievalResult.Strategy.ToString(),
                    ["generation_model"] = "semantic-kernel"
                }
            };

            _logger.LogInformation("RAG response generated in {ElapsedMs}ms with confidence {Confidence}", 
                stopwatch.ElapsedMilliseconds, confidenceScore);

            return ragResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to generate RAG response");
            throw;
        }
    }

    /// <summary>
    /// Generate streaming response using RAG
    /// 使用RAG生成流式响应
    /// </summary>
    public async Task<IAsyncEnumerable<string>> GenerateStreamingResponseAsync(string collectionName, RagGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating streaming RAG response for query: {Query}", request.Query);

            // Retrieve context first - 首先检索上下文
            var retrievalQuery = request.RetrievalOptions ?? new RagQuery
            {
                Text = request.Query,
                Strategy = RetrievalStrategy.Hybrid,
                TopK = 5
            };

            var retrievalResult = await HybridRetrievalAsync(collectionName, retrievalQuery);
            var context = BuildContextFromChunks(retrievalResult.Chunks);
            var systemPrompt = BuildSystemPrompt(request.SystemPrompt, context, request.IncludeSources);

            // Generate streaming response - 生成流式响应
            return await _semanticKernel.GetStreamingChatCompletionAsync(request.Query, systemPrompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate streaming RAG response");
            throw;
        }
    }

    #endregion

    #region Enterprise Scenarios - 企业场景

    /// <summary>
    /// Enterprise Q&A with domain-specific knowledge
    /// 具有领域特定知识的企业问答
    /// </summary>
    public async Task<RagResponse> EnterpriseQAAsync(string knowledgeBase, string question, RagOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Processing enterprise Q&A for knowledge base: {KnowledgeBase}", knowledgeBase);

            var ragOptions = options ?? new RagOptions();
            
            // Build enterprise-specific retrieval query - 构建企业特定的检索查询
            var retrievalQuery = new RagQuery
            {
                Text = question,
                Strategy = RetrievalStrategy.Hybrid,
                TopK = 8,
                Filters = ragOptions.DomainFilters,
                Weights = new HybridRetrievalWeights
                {
                    VectorWeight = 0.5f,
                    KeywordWeight = 0.4f,
                    SemanticWeight = 0.1f
                },
                ReRanking = new ReRankingOptions
                {
                    Enabled = true,
                    MaxResults = 20
                }
            };

            // Create enterprise-specific system prompt - 创建企业特定的系统提示
            var systemPrompt = $@"
你是一个专业的企业知识助手。请基于提供的企业知识库内容回答用户问题。

回答要求：
1. 准确性：确保答案基于提供的知识内容，不要编造信息
2. 专业性：使用专业术语和企业语言风格
3. 完整性：提供全面的答案，包括相关的背景信息
4. 引用：{(ragOptions.EnableCitation ? "在答案中标注信息来源" : "无需标注来源")}
5. 语言：使用{ragOptions.Language}回答

如果知识库中没有相关信息，请明确说明并建议用户联系相关部门。

知识库内容：
{{CONTEXT}}

用户问题：{question}
";

            var generationRequest = new RagGenerationRequest
            {
                Query = question,
                SystemPrompt = systemPrompt,
                RetrievalOptions = retrievalQuery,
                GenerationOptions = new RagGenerationOptions
                {
                    MaxTokens = ragOptions.MaxContextLength,
                    Temperature = 0.3, // Lower temperature for enterprise accuracy
                    Format = ResponseFormat.Text
                },
                IncludeSources = ragOptions.EnableCitation
            };

            return await GenerateResponseAsync(knowledgeBase, generationRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process enterprise Q&A");
            throw;
        }
    }

    /// <summary>
    /// Document summarization with customizable options
    /// 具有可自定义选项的文档摘要
    /// </summary>
    public async Task<RagResponse> DocumentSummarizationAsync(string collectionName, string documentId, RagSummaryOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Summarizing document {DocumentId} in collection {CollectionName}", 
                documentId, collectionName);

            var summaryOptions = options ?? new RagSummaryOptions();

            // Retrieve document chunks - 检索文档块
            var documents = await GetDocumentsAsync(collectionName, new[] { documentId });
            var document = documents.FirstOrDefault();
            
            if (document == null)
            {
                throw new ArgumentException($"Document {documentId} not found");
            }

            // Build summarization prompt based on options - 根据选项构建摘要提示
            var lengthInstruction = summaryOptions.Length switch
            {
                SummaryLength.Short => "简短摘要（1-2段）",
                SummaryLength.Medium => "中等长度摘要（3-4段）",
                SummaryLength.Long => "详细摘要（5-6段）",
                SummaryLength.Detailed => "全面详细的摘要（多段落，包含所有要点）",
                _ => "中等长度摘要"
            };

            var styleInstruction = summaryOptions.Style switch
            {
                SummaryStyle.Executive => "执行摘要风格，突出关键决策点和业务影响",
                SummaryStyle.Technical => "技术摘要风格，包含技术细节和专业术语",
                SummaryStyle.Informative => "信息性摘要风格，客观描述主要内容",
                SummaryStyle.Casual => "通俗易懂的摘要风格，避免专业术语",
                _ => "信息性摘要风格"
            };

            var systemPrompt = $@"
你是一个专业的文档摘要助手。请为以下文档创建{lengthInstruction}。

摘要要求：
1. 风格：{styleInstruction}
2. 要点：{(summaryOptions.IncludeKeyPoints ? "包含关键要点列表" : "以段落形式呈现")}
3. 受众：{summaryOptions.TargetAudience ?? "一般读者"}
4. 保持客观性和准确性
5. 突出文档的主要价值和核心信息

文档标题：{document.Title}
文档内容：
{document.Content}
";

            var generationRequest = new RagGenerationRequest
            {
                Query = "请为这个文档创建摘要",
                SystemPrompt = systemPrompt,
                GenerationOptions = new RagGenerationOptions
                {
                    MaxTokens = 2000,
                    Temperature = 0.5,
                    Format = ResponseFormat.Text
                },
                IncludeSources = false
            };

            // Generate summary without retrieval (we already have the document) - 生成摘要而不检索（我们已经有文档）
            var response = await _semanticKernel.GetChatCompletionAsync(
                generationRequest.Query, 
                generationRequest.SystemPrompt);

            return new RagResponse
            {
                Response = response,
                Sources = new List<RagRetrievedChunk>(),
                GenerationTimeMs = 0,
                Metadata = new Dictionary<string, object>
                {
                    ["document_id"] = documentId,
                    ["summary_length"] = summaryOptions.Length.ToString(),
                    ["summary_style"] = summaryOptions.Style.ToString(),
                    ["original_length"] = document.Content.Length
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize document {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Multi-document analysis and comparison
    /// 多文档分析和比较
    /// </summary>
    public async Task<RagResponse> MultiDocumentAnalysisAsync(string collectionName, IEnumerable<string> documentIds, string analysisQuery)
    {
        try
        {
            _logger.LogInformation("Performing multi-document analysis for {DocumentCount} documents", 
                documentIds.Count());

            // Retrieve all specified documents - 检索所有指定的文档
            var documents = await GetDocumentsAsync(collectionName, documentIds);
            var documentList = documents.ToList();

            if (!documentList.Any())
            {
                throw new ArgumentException("No documents found for analysis");
            }

            // Build analysis context from all documents - 从所有文档构建分析上下文
            var analysisContext = new StringBuilder();
            foreach (var doc in documentList)
            {
                analysisContext.AppendLine($"=== 文档：{doc.Title} ===");
                analysisContext.AppendLine(doc.Content);
                analysisContext.AppendLine();
            }

            var systemPrompt = $@"
你是一个专业的文档分析师。请基于提供的多个文档进行分析和比较。

分析要求：
1. 综合分析：整合多个文档的信息
2. 比较对比：识别文档间的异同点
3. 关联性：找出文档间的关联和依赖关系
4. 洞察发现：提供深入的分析洞察
5. 结构化：使用清晰的结构组织答案

文档数量：{documentList.Count}
分析查询：{analysisQuery}

文档内容：
{analysisContext}
";

            var generationRequest = new RagGenerationRequest
            {
                Query = analysisQuery,
                SystemPrompt = systemPrompt,
                GenerationOptions = new RagGenerationOptions
                {
                    MaxTokens = 3000,
                    Temperature = 0.4,
                    Format = ResponseFormat.Text
                },
                IncludeSources = true
            };

            var response = await _semanticKernel.GetChatCompletionAsync(
                generationRequest.Query, 
                generationRequest.SystemPrompt);

            // Create source references for all analyzed documents - 为所有分析的文档创建源引用
            var sources = documentList.Select(doc => new RagRetrievedChunk
            {
                Chunk = new RagDocumentChunk
                {
                    Id = doc.Id,
                    DocumentId = doc.Id,
                    Content = doc.Content.Length > 500 ? doc.Content.Substring(0, 500) + "..." : doc.Content,
                    Metadata = new Dictionary<string, object>
                    {
                        ["title"] = doc.Title,
                        ["document_type"] = "analyzed_document"
                    }
                },
                Score = 1.0f // All documents are equally relevant for analysis
            }).ToList();

            return new RagResponse
            {
                Response = response,
                Sources = sources,
                GenerationTimeMs = 0,
                Metadata = new Dictionary<string, object>
                {
                    ["analysis_type"] = "multi_document",
                    ["document_count"] = documentList.Count,
                    ["document_ids"] = documentIds.ToList(),
                    ["total_content_length"] = documentList.Sum(d => d.Content.Length)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform multi-document analysis");
            throw;
        }
    }

    #endregion

    #region Collection Management - 集合管理

    /// <summary>
    /// Create knowledge base with RAG configuration
    /// 使用RAG配置创建知识库
    /// </summary>
    public async Task<string> CreateKnowledgeBaseAsync(string name, RagCollectionConfig config)
    {
        try
        {
            _logger.LogInformation("Creating knowledge base: {Name}", name);

            var vectorConfig = new VectorCollectionConfig
            {
                EmbeddingDimension = 1536, // Default for OpenAI embeddings
                DistanceMetric = DistanceMetric.Cosine,
                SupportedModalities = new HashSet<Modality> { Modality.Text },
                Metadata = new Dictionary<string, object>
                {
                    ["description"] = config.Description,
                    ["chunk_size"] = config.ChunkSize,
                    ["chunk_overlap"] = config.ChunkOverlap,
                    ["embedding_model"] = config.EmbeddingModel,
                    ["keyword_index"] = config.EnableKeywordIndex,
                    ["semantic_index"] = config.EnableSemanticIndex,
                    ["created_at"] = DateTime.UtcNow.ToString("O")
                }
            };

            // Add custom metadata - 添加自定义元数据
            foreach (var kvp in config.Metadata)
            {
                vectorConfig.Metadata[$"custom_{kvp.Key}"] = kvp.Value;
            }

            var collection = await _vectorDb.CreateCollectionAsync(name, vectorConfig);
            
            _logger.LogInformation("Successfully created knowledge base: {Name}", name);
            return collection.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create knowledge base: {Name}", name);
            throw;
        }
    }

    /// <summary>
    /// Delete knowledge base
    /// 删除知识库
    /// </summary>
    public async Task DeleteKnowledgeBaseAsync(string name)
    {
        try
        {
            _logger.LogInformation("Deleting knowledge base: {Name}", name);
            
            await _vectorDb.DeleteCollectionAsync(name);
            
            _logger.LogInformation("Successfully deleted knowledge base: {Name}", name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete knowledge base: {Name}", name);
            throw;
        }
    }

    /// <summary>
    /// List all knowledge bases
    /// 列出所有知识库
    /// </summary>
    public async Task<IEnumerable<string>> ListKnowledgeBasesAsync()
    {
        try
        {
            var collections = await _vectorDb.ListCollectionsAsync();
            return collections.Select(c => c.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list knowledge bases");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods - 私有辅助方法

    /// <summary>
    /// Split document into chunks for better retrieval
    /// 将文档分割成块以便更好地检索
    /// </summary>
    private async Task<List<RagDocumentChunk>> SplitDocumentIntoChunksAsync(RagDocument document, int chunkSize = 1000, int overlap = 200)
    {
        var chunks = new List<RagDocumentChunk>();
        var content = document.Content;
        
        // Simple sentence-aware chunking - 简单的句子感知分块
        var sentences = content.Split(new[] { '.', '!', '?', '。', '！', '？' }, StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();
        var chunkIndex = 0;

        foreach (var sentence in sentences)
        {
            var trimmedSentence = sentence.Trim();
            if (string.IsNullOrEmpty(trimmedSentence)) continue;

            // Check if adding this sentence would exceed chunk size - 检查添加此句子是否会超过块大小
            if (currentChunk.Length + trimmedSentence.Length > chunkSize && currentChunk.Length > 0)
            {
                // Create chunk - 创建块
                var chunk = new RagDocumentChunk
                {
                    Id = $"{document.Id}_chunk_{chunkIndex}",
                    DocumentId = document.Id,
                    Content = currentChunk.ToString().Trim(),
                    Position = chunkIndex,
                    Metadata = new Dictionary<string, object>
                    {
                        ["chunk_size"] = currentChunk.Length,
                        ["sentence_count"] = currentChunk.ToString().Split('.', '。').Length
                    }
                };
                
                chunks.Add(chunk);
                chunkIndex++;

                // Start new chunk with overlap - 使用重叠开始新块
                if (overlap > 0 && currentChunk.Length > overlap)
                {
                    var overlapText = currentChunk.ToString().Substring(currentChunk.Length - overlap);
                    currentChunk.Clear();
                    currentChunk.Append(overlapText);
                }
                else
                {
                    currentChunk.Clear();
                }
            }

            currentChunk.Append(trimmedSentence).Append("。");
        }

        // Add final chunk if there's remaining content - 如果有剩余内容，添加最后一个块
        if (currentChunk.Length > 0)
        {
            var finalChunk = new RagDocumentChunk
            {
                Id = $"{document.Id}_chunk_{chunkIndex}",
                DocumentId = document.Id,
                Content = currentChunk.ToString().Trim(),
                Position = chunkIndex,
                Metadata = new Dictionary<string, object>
                {
                    ["chunk_size"] = currentChunk.Length,
                    ["sentence_count"] = currentChunk.ToString().Split('.', '。').Length
                }
            };
            
            chunks.Add(finalChunk);
        }

        return chunks;
    }

    /// <summary>
    /// Merge results from different retrieval strategies
    /// 合并不同检索策略的结果
    /// </summary>
    private List<RagRetrievedChunk> MergeRetrievalResults(
        RagRetrievalResult vectorResults,
        RagRetrievalResult keywordResults,
        RagRetrievalResult semanticResults,
        HybridRetrievalWeights weights)
    {
        var mergedResults = new Dictionary<string, RagRetrievedChunk>();

        // Process vector results - 处理向量结果
        foreach (var chunk in vectorResults.Chunks)
        {
            if (!mergedResults.ContainsKey(chunk.Chunk.Id))
            {
                mergedResults[chunk.Chunk.Id] = new RagRetrievedChunk
                {
                    Chunk = chunk.Chunk,
                    VectorScore = chunk.VectorScore,
                    KeywordScore = 0f,
                    SemanticScore = 0f
                };
            }
            else
            {
                mergedResults[chunk.Chunk.Id].VectorScore = Math.Max(
                    mergedResults[chunk.Chunk.Id].VectorScore, chunk.VectorScore);
            }
        }

        // Process keyword results - 处理关键词结果
        foreach (var chunk in keywordResults.Chunks)
        {
            if (!mergedResults.ContainsKey(chunk.Chunk.Id))
            {
                mergedResults[chunk.Chunk.Id] = new RagRetrievedChunk
                {
                    Chunk = chunk.Chunk,
                    VectorScore = 0f,
                    KeywordScore = chunk.KeywordScore,
                    SemanticScore = 0f,
                    HighlightedContent = chunk.HighlightedContent
                };
            }
            else
            {
                mergedResults[chunk.Chunk.Id].KeywordScore = Math.Max(
                    mergedResults[chunk.Chunk.Id].KeywordScore, chunk.KeywordScore);
                if (!string.IsNullOrEmpty(chunk.HighlightedContent))
                {
                    mergedResults[chunk.Chunk.Id].HighlightedContent = chunk.HighlightedContent;
                }
            }
        }

        // Process semantic results - 处理语义结果
        foreach (var chunk in semanticResults.Chunks)
        {
            if (!mergedResults.ContainsKey(chunk.Chunk.Id))
            {
                mergedResults[chunk.Chunk.Id] = new RagRetrievedChunk
                {
                    Chunk = chunk.Chunk,
                    VectorScore = 0f,
                    KeywordScore = 0f,
                    SemanticScore = chunk.SemanticScore
                };
            }
            else
            {
                mergedResults[chunk.Chunk.Id].SemanticScore = Math.Max(
                    mergedResults[chunk.Chunk.Id].SemanticScore, chunk.SemanticScore);
            }
        }

        // Calculate hybrid scores - 计算混合分数
        foreach (var chunk in mergedResults.Values)
        {
            chunk.Score = (chunk.VectorScore * weights.VectorWeight) +
                         (chunk.KeywordScore * weights.KeywordWeight) +
                         (chunk.SemanticScore * weights.SemanticWeight);
        }

        return mergedResults.Values.OrderByDescending(c => c.Score).ToList();
    }

    /// <summary>
    /// Apply re-ranking to retrieved results
    /// 对检索结果应用重排序
    /// </summary>
    private async Task<List<RagRetrievedChunk>> ApplyReRankingAsync(
        string query, 
        List<RagRetrievedChunk> chunks, 
        ReRankingOptions options)
    {
        try
        {
            // Take top candidates for re-ranking - 取前几个候选进行重排序
            var candidates = chunks.Take(options.MaxResults).ToList();
            var rerankedChunks = new List<RagRetrievedChunk>();

            foreach (var chunk in candidates)
            {
                // Use semantic similarity for re-ranking - 使用语义相似度进行重排序
                var reRankScore = await CalculateSemanticSimilarityAsync(query, chunk.Chunk.Content);
                
                if (reRankScore >= options.Threshold)
                {
                    chunk.ReRankScore = reRankScore;
                    // Combine original score with re-rank score - 将原始分数与重排序分数结合
                    chunk.Score = (chunk.Score * 0.7f) + (reRankScore * 0.3f);
                    rerankedChunks.Add(chunk);
                }
            }

            // Add remaining chunks that weren't re-ranked - 添加未重排序的剩余块
            var remainingChunks = chunks.Skip(options.MaxResults).ToList();
            rerankedChunks.AddRange(remainingChunks);

            return rerankedChunks.OrderByDescending(c => c.Score).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Re-ranking failed, returning original results");
            return chunks;
        }
    }

    /// <summary>
    /// Apply filters and limit results
    /// 应用过滤器并限制结果
    /// </summary>
    private List<RagRetrievedChunk> ApplyFiltersAndLimit(List<RagRetrievedChunk> chunks, RagQuery query)
    {
        var filteredChunks = chunks.AsEnumerable();

        // Apply similarity threshold - 应用相似度阈值
        filteredChunks = filteredChunks.Where(c => c.Score >= query.MinSimilarity);

        // Apply metadata filters if specified - 如果指定，应用元数据过滤器
        if (query.Filters?.Any() == true)
        {
            foreach (var filter in query.Filters)
            {
                filteredChunks = filteredChunks.Where(c => 
                    c.Chunk.Metadata.ContainsKey(filter.Key) && 
                    c.Chunk.Metadata[filter.Key].Equals(filter.Value));
            }
        }

        // Limit to top K results - 限制为前K个结果
        return filteredChunks.Take(query.TopK).ToList();
    }

    /// <summary>
    /// Extract keywords from query text
    /// 从查询文本中提取关键词
    /// </summary>
    private List<string> ExtractKeywords(string text)
    {
        // Simple keyword extraction - remove stop words and short words
        // 简单的关键词提取 - 移除停用词和短词
        var stopWords = new HashSet<string> { "的", "是", "在", "有", "和", "与", "或", "但", "如果", "因为", "所以", "这", "那", "什么", "怎么", "为什么", "哪里", "谁", "when", "where", "what", "how", "why", "who", "the", "is", "are", "and", "or", "but", "if", "because", "so", "this", "that" };
        
        var words = Regex.Split(text.ToLower(), @"\W+")
                         .Where(w => w.Length > 2 && !stopWords.Contains(w))
                         .Distinct()
                         .ToList();
        
        return words;
    }

    /// <summary>
    /// Calculate BM25 score for keyword matching
    /// 计算关键词匹配的BM25分数
    /// </summary>
    private float CalculateBM25Score(string document, List<string> keywords)
    {
        if (string.IsNullOrEmpty(document) || !keywords.Any())
            return 0f;

        var docWords = Regex.Split(document.ToLower(), @"\W+").Where(w => w.Length > 0).ToList();
        var docLength = docWords.Count;
        var avgDocLength = 100f; // Assumed average document length
        
        const float k1 = 1.2f;
        const float b = 0.75f;
        
        float score = 0f;
        
        foreach (var keyword in keywords)
        {
            var termFreq = docWords.Count(w => w.Contains(keyword));
            if (termFreq > 0)
            {
                var idf = (float)Math.Log((1000f + 1) / (termFreq + 1)); // Simplified IDF
                var tf = (termFreq * (k1 + 1)) / (termFreq + k1 * (1 - b + b * (docLength / avgDocLength)));
                score += idf * tf;
            }
        }
        
        return score;
    }

    /// <summary>
    /// Highlight keywords in content
    /// 在内容中高亮关键词
    /// </summary>
    private string HighlightKeywords(string content, List<string> keywords)
    {
        var highlightedContent = content;
        
        foreach (var keyword in keywords)
        {
            var pattern = $@"\b{Regex.Escape(keyword)}\b";
            highlightedContent = Regex.Replace(highlightedContent, pattern, 
                $"**{keyword}**", RegexOptions.IgnoreCase);
        }
        
        return highlightedContent;
    }

    /// <summary>
    /// Calculate semantic similarity using prompt-based approach
    /// 使用基于提示的方法计算语义相似度
    /// </summary>
    private async Task<float> CalculateSemanticSimilarityAsync(string query, string content)
    {
        try
        {
            var prompt = $@"
请评估以下查询和内容的语义相似度，返回0到1之间的分数（1表示完全相关，0表示完全不相关）。

查询：{query}

内容：{content.Substring(0, Math.Min(content.Length, 500))}

请只返回数字分数，不要其他解释。
";

            var response = await _semanticKernel.GetChatCompletionAsync(prompt);
            
            if (float.TryParse(response.Trim(), out var score))
            {
                return Math.Max(0f, Math.Min(1f, score));
            }
            
            return 0.5f; // Default score if parsing fails
        }
        catch
        {
            return 0.5f; // Default score on error
        }
    }

    /// <summary>
    /// Build context from retrieved chunks
    /// 从检索到的块构建上下文
    /// </summary>
    private string BuildContextFromChunks(List<RagRetrievedChunk> chunks)
    {
        var context = new StringBuilder();
        
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            context.AppendLine($"[来源 {i + 1}] {chunk.Chunk.Content}");
            context.AppendLine();
        }
        
        return context.ToString();
    }

    /// <summary>
    /// Build system prompt with context
    /// 使用上下文构建系统提示
    /// </summary>
    private string BuildSystemPrompt(string? customPrompt, string context, bool includeSources)
    {
        var defaultPrompt = @"
你是一个专业的AI助手，专门基于提供的知识库内容回答用户问题。

回答要求：
1. 准确性：严格基于提供的上下文信息回答，不要编造或推测
2. 完整性：尽可能提供全面的答案
3. 相关性：确保答案直接回应用户的问题
4. 清晰性：使用清晰、易懂的语言
5. 引用：{(includeSources ? "在适当的地方引用来源编号 [来源 X]" : "无需标注来源")}

如果提供的上下文信息不足以回答问题，请明确说明并建议用户提供更多信息或咨询相关专家。

上下文信息：
{CONTEXT}
";

        var systemPrompt = customPrompt ?? defaultPrompt;
        return systemPrompt.Replace("{CONTEXT}", context);
    }

    /// <summary>
    /// Calculate confidence score for generated response
    /// 计算生成响应的置信度分数
    /// </summary>
    private float CalculateConfidenceScore(List<RagRetrievedChunk> sources, string response)
    {
        if (!sources.Any())
            return 0.1f;

        // Calculate confidence based on source quality and relevance
        // 基于源质量和相关性计算置信度
        var avgSourceScore = sources.Average(s => s.Score);
        var sourceCount = Math.Min(sources.Count, 5); // Cap at 5 for diminishing returns
        var sourceCountFactor = sourceCount / 5f;
        
        // Response length factor (longer responses might be more comprehensive)
        // 响应长度因子（较长的响应可能更全面）
        var responseLengthFactor = Math.Min(response.Length / 1000f, 1f);
        
        var confidence = (avgSourceScore * 0.6f) + (sourceCountFactor * 0.3f) + (responseLengthFactor * 0.1f);
        
        return Math.Max(0.1f, Math.Min(1f, confidence));
    }

    #endregion
}


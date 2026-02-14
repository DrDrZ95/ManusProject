namespace Agent.Core.Tests.Services
{
    using Agent.Application.Services.RAG;
    using Agent.Application.Services.VectorDatabase;
    using Agent.Application.Services.SemanticKernel;
    using Agent.Core.Cache;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// RagService 核心逻辑单元测试
    /// Unit tests for RagService core logic
    /// </summary>
    public class RagServiceTests : IClassFixture<RagTestFixture>
    {
        private readonly Mock<IVectorDatabaseService> _mockVectorDb;
        private readonly Mock<ISemanticKernelService> _mockSemanticKernel;
        private readonly Mock<ILogger<RagService>> _mockLogger;
        private readonly Mock<IAgentCacheService> _mockCacheService;
        private readonly Mock<IRagCacheWarmer> _mockCacheWarmer;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobs;
        private readonly RagService _ragService;
        private readonly RagTestFixture _fixture;

        public RagServiceTests(RagTestFixture fixture)
        {
            _fixture = fixture;
            _mockVectorDb = new Mock<IVectorDatabaseService>();
            _mockSemanticKernel = new Mock<ISemanticKernelService>();
            _mockLogger = new Mock<ILogger<RagService>>();
            _mockCacheService = new Mock<IAgentCacheService>();
            _mockCacheWarmer = new Mock<IRagCacheWarmer>();
            _mockBackgroundJobs = new Mock<IBackgroundJobClient>();

            _ragService = new RagService(
                _mockVectorDb.Object,
                _mockSemanticKernel.Object,
                _mockLogger.Object,
                _mockCacheService.Object,
                _mockCacheWarmer.Object,
                _mockBackgroundJobs.Object);
        }

        #region Document Processing Tests (Step 1 & 2)

        /// <summary>
        /// 测试文档处理流程 (DocumentProcessor)
        /// Test document processing flow
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_ShouldProcessAndStoreChunks()
        {
            // Arrange
            var doc = _fixture.CreateStandardDocument();
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f });

            // Act
            var resultId = await _ragService.AddDocumentAsync("test-collection", doc);

            // Assert
            Assert.Equal(doc.Id, resultId);
            _mockVectorDb.Verify(v => v.AddDocumentsAsync("test-collection", It.IsAny<IEnumerable<VectorDocument>>()), Times.AtLeastOnce);
            _mockSemanticKernel.Verify(s => s.GenerateEmbeddingAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// 测试向量生成与嵌入 (EmbeddingGenerator)
        /// Test vector generation and embedding
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_ShouldGenerateEmbeddingsForSummary()
        {
            // Arrange
            var doc = _fixture.CreateDocumentWithSummary();
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[] { 0.9f, 0.8f, 0.7f });

            // Act
            await _ragService.AddDocumentAsync("test-collection", doc);

            // Assert
            // 验证是否为摘要生成了嵌入 (Verify embedding generated for summary)
            _mockSemanticKernel.Verify(s => s.GenerateEmbeddingAsync(doc.Summary), Times.Once);
        }

        /// <summary>
        /// 测试向量维度正确性
        /// Test vector dimension correctness
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_ShouldGenerateCorrectDimensionEmbeddings()
        {
            // Arrange
            var doc = _fixture.CreateStandardDocument();
            var expectedDimension = 1536;
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[expectedDimension]);

            // Act
            await _ragService.AddDocumentAsync("test-collection", doc);

            // Assert
            _mockVectorDb.Verify(v => v.AddDocumentsAsync("test-collection",
                It.Is<IEnumerable<VectorDocument>>(docs => docs.All(d => d.Embedding.Length == expectedDimension))), Times.Once);
        }

        #endregion

        #region Boundary Condition Tests (Step 3)

        /// <summary>
        /// 测试边界条件：空文档
        /// Test edge case: empty document
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_EmptyContent_ShouldHandleGracefully()
        {
            // Arrange
            var doc = new RagDocument { Id = "empty-doc", Content = "", Title = "Empty" };

            // Act & Assert
            // Depending on implementation, it might throw or just return ID. 
            // Previous test assumed ArgumentException, let's stick to that if that's the expected behavior.
            // If the service doesn't throw, we might need to adjust.
            // Assuming the implementation handles it by either throwing or processing empty chunks.
            // Let's assume it should throw for empty content as it makes no sense to add empty doc.
            await Assert.ThrowsAsync<ArgumentException>(() => _ragService.AddDocumentAsync("test-collection", doc));
        }

        /// <summary>
        /// 测试边界条件：超大文档
        /// Test edge case: extremely large document
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_LargeDocument_ShouldChunkCorrectly()
        {
            // Arrange
            var largeContent = string.Join(" ", Enumerable.Repeat("word", 10000));
            var doc = new RagDocument { Id = "large-doc", Content = largeContent, Title = "Large" };
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[1536]);

            // Act
            await _ragService.AddDocumentAsync("test-collection", doc);

            // Assert
            // 验证是否进行了多次分块存储 (Verify multiple chunks stored)
            _mockVectorDb.Verify(v => v.AddDocumentsAsync("test-collection", It.Is<IEnumerable<VectorDocument>>(docs => docs.Count() > 1)), Times.AtLeastOnce);
        }

        /// <summary>
        /// 测试边界条件：特殊字符
        /// Test edge case: special characters
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_SpecialCharacters_ShouldPreserveContent()
        {
            // Arrange
            var specialContent = "Special chars: !@#$%^&*()_+ \n \t 中文测试 😊";
            var doc = new RagDocument { Id = "special-doc", Content = specialContent, Title = "Special" };
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[1536]);

            // Act
            await _ragService.AddDocumentAsync("test-collection", doc);

            // Assert
            _mockVectorDb.Verify(v => v.AddDocumentsAsync("test-collection",
                It.Is<IEnumerable<VectorDocument>>(docs => docs.Any(d => d.Content.Contains("😊")))), Times.Once);
        }

        /// <summary>
        /// 测试并发添加多个文档的线程安全性
        /// Test thread safety for concurrent document additions
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_ConcurrentCalls_ShouldBeThreadSafe()
        {
            // Arrange
            int concurrency = 10;
            var docs = Enumerable.Range(0, concurrency).Select(i => new RagDocument
            {
                Id = $"doc-{i}",
                Content = $"Content {i}",
                Title = $"Title {i}"
            }).ToList();

            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[1536]);

            // Act
            var tasks = docs.Select(d => _ragService.AddDocumentAsync("test-collection", d));
            await Task.WhenAll(tasks);

            // Assert
            _mockVectorDb.Verify(v => v.AddDocumentsAsync("test-collection", It.IsAny<IEnumerable<VectorDocument>>()), Times.Exactly(concurrency));
        }

        #endregion

        #region Search & Retrieval Tests (Step 4)

        /// <summary>
        /// 测试混合检索功能
        /// Test hybrid retrieval function
        /// </summary>
        [Fact]
        public async Task HybridRetrievalAsync_ShouldMergeResults()
        {
            // Arrange
            var collectionName = "test-collection";
            var query = new RagQuery { Text = "test query", TopK = 5 };

            // Mock Vector Search
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(query.Text))
                .ReturnsAsync(new float[1536]);

            var vectorMatches = new List<VectorSearchMatch>
            {
                new VectorSearchMatch { Id = "chunk1", Score = 0.9f, Content = "Content 1", Metadata = new Dictionary<string, object> { ["document_id"] = "doc1" } }
            };
            _mockVectorDb.Setup(v => v.SearchByEmbeddingAsync(collectionName, It.IsAny<float[]>(), It.IsAny<VectorSearchOptions>()))
                .ReturnsAsync(new VectorSearchResult { Matches = vectorMatches });

            // Mock Keyword Search (Simulated via GetDocumentsAsync and manual filtering in service, but service might use DB search)
            // RagService.KeywordRetrievalAsync calls GetDocumentsAsync and does in-memory BM25
            var allDocs = new List<VectorDocument>
            {
                new VectorDocument { Id = "chunk1", Content = "test query match", Metadata = new Dictionary<string, object> { ["document_id"] = "doc1" } }
            };
            _mockVectorDb.Setup(v => v.GetDocumentsAsync(collectionName, null, null))
                .ReturnsAsync(allDocs);

            // Act
            var result = await _ragService.HybridRetrievalAsync(collectionName, query);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Chunks);
            Assert.Equal(RetrievalStrategy.Hybrid, result.Strategy);
        }

        /// <summary>
        /// 测试缓存机制
        /// Test caching mechanism
        /// </summary>
        [Fact]
        public async Task HybridRetrievalAsync_ShouldReturnCachedResult_WhenAvailable()
        {
            // Arrange
            var collectionName = "test-collection";
            var query = new RagQuery { Text = "cached query" };
            var cachedResult = new RagRetrievalResult
            {
                Chunks = new List<RagRetrievedChunk> { new RagRetrievedChunk { Score = 1.0f } }
            };

            _mockCacheService.Setup(c => c.GetAsync<RagRetrievalResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedResult);

            // Act
            var result = await _ragService.HybridRetrievalAsync(collectionName, query);

            // Assert
            Assert.Same(cachedResult, result);
            _mockVectorDb.Verify(v => v.SearchByEmbeddingAsync(It.IsAny<string>(), It.IsAny<float[]>(), It.IsAny<VectorSearchOptions>()), Times.Never);
        }

        #endregion

        #region Collection Management (Step 5)

        /// <summary>
        /// 测试文档更新
        /// Test document update
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ShouldDeleteAndAdd()
        {
            // Arrange
            var doc = _fixture.CreateStandardDocument();
            _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[1536]);

            // Act
            await _ragService.UpdateDocumentAsync("test-collection", doc);

            // Assert
            _mockVectorDb.Verify(v => v.DeleteDocumentsAsync("test-collection", null, It.IsAny<VectorFilter>()), Times.Once);
            _mockVectorDb.Verify(v => v.AddDocumentsAsync("test-collection", It.IsAny<IEnumerable<VectorDocument>>()), Times.Once);
        }

        /// <summary>
        /// 测试文档删除
        /// Test document deletion
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ShouldCallVectorDbDelete()
        {
            // Arrange
            var docId = "doc-to-delete";

            // Act
            await _ragService.DeleteDocumentAsync("test-collection", docId);

            // Assert
            _mockVectorDb.Verify(v => v.DeleteDocumentsAsync("test-collection", null, It.Is<VectorFilter>(f => f.Equals["document_id"].Equals(docId))), Times.Once);
        }

        #endregion
    }

    /// <summary>
    /// RAG 测试固件 (Fixture Pattern)
    /// </summary>
    public class RagTestFixture
    {
        public RagDocument CreateStandardDocument()
        {
            return new RagDocument
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Standard Doc",
                Content = "This is a standard document content for testing RAG processing flow.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public RagDocument CreateDocumentWithSummary()
        {
            return new RagDocument
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Summary Doc",
                Content = "Detailed content about AI agents and their capabilities in modern software architecture.",
                Summary = "Brief summary of AI agent capabilities.",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}


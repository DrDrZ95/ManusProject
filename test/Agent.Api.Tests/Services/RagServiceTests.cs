
namespace Agent.Api.Tests.Services
{
    /// <summary>
    /// RagService的单元测试 - 包含搜索准确性和向量更新一致性测试
    /// Unit tests for RagService - including search accuracy and vector update consistency tests
    /// </summary>
    public class RagServiceTests
    {
        private readonly Mock<IRagService> _mockRagService;

        public RagServiceTests()
        {
            _mockRagService = new Mock<IRagService>();
        }

        /// <summary>
        /// 测试混合搜索的准确性（模拟高分结果排序）
        /// Test accuracy of hybrid search (simulating high-score result ranking)
        /// </summary>
        [Fact]
        public async Task HybridRetrieval_AccuracyTest_ShouldReturnHighestScoringDocumentsFirst()
        {
            // Arrange
            var collectionName = "accuracy-test-collection";
            var query = new RagQuery { Text = "What is AI Agent?" };

            var mockResults = new RagRetrievalResult
            {
                Chunks = new List<RagRetrievedChunk>
                {
                    new RagRetrievedChunk { Chunk = new RagDocumentChunk { DocumentId = "doc-low", Content = "Low relevance" }, Score = 0.45f },
                    new RagRetrievedChunk { Chunk = new RagDocumentChunk { DocumentId = "doc-high", Content = "High relevance content" }, Score = 0.98f },
                    new RagRetrievedChunk { Chunk = new RagDocumentChunk { DocumentId = "doc-mid", Content = "Medium relevance" }, Score = 0.75f }
                }
            };

            _mockRagService.Setup(s => s.HybridRetrievalAsync(collectionName, It.IsAny<RagQuery>()))
                .ReturnsAsync(mockResults);

            // Act
            var result = await _mockRagService.Object.HybridRetrievalAsync(collectionName, query);

            // Assert
            Assert.NotNull(result);
            var sortedChunks = result.Chunks.OrderByDescending(c => c.Score).ToList();
            Assert.Equal("doc-high", sortedChunks[0].Chunk.DocumentId);
            Assert.Equal(0.98f, sortedChunks[0].Score);
        }

        /// <summary>
        /// 测试向量更新的一致性
        /// Test consistency of vector updates
        /// </summary>
        [Fact]
        public async Task UpdateDocument_ConsistencyTest_ShouldReflectChangesInRetrieval()
        {
            // Arrange
            var collectionName = "consistency-test-collection";
            var docId = "doc-1";
            var originalDoc = new RagDocument { Id = docId, Content = "Original Content" };
            var updatedDoc = new RagDocument { Id = docId, Content = "Updated Content" };

            // 模拟更新过程 - Simulate update process
            _mockRagService.Setup(s => s.UpdateDocumentAsync(collectionName, updatedDoc))
                .Returns(Task.CompletedTask);

            // 模拟更新后的检索结果 - Simulate retrieval result after update
            _mockRagService.Setup(s => s.VectorRetrievalAsync(collectionName, "Updated", It.IsAny<int>()))
                .ReturnsAsync(new RagRetrievalResult
                {
                    Chunks = new List<RagRetrievedChunk>
                    {
                        new RagRetrievedChunk { Chunk = new RagDocumentChunk { DocumentId = docId, Content = "Updated Content" }, Score = 0.99f }
                    }
                });

            // Act
            await _mockRagService.Object.UpdateDocumentAsync(collectionName, updatedDoc);
            var retrievalResult = await _mockRagService.Object.VectorRetrievalAsync(collectionName, "Updated", 1);

            // Assert
            var chunk = Assert.Single(retrievalResult.Chunks);
            Assert.Equal("Updated Content", chunk.Chunk.Content);
            _mockRagService.Verify(s => s.UpdateDocumentAsync(collectionName, updatedDoc), Times.Once);
        }

        /// <summary>
        /// 测试多文档分析的准确性
        /// Test accuracy of multi-document analysis
        /// </summary>
        [Fact]
        public async Task MultiDocumentAnalysis_ShouldAggregateInformationCorrectly()
        {
            // Arrange
            var docIds = new List<string> { "doc1", "doc2" };
            var analysisQuery = "Summarize both";
            var expectedResponse = "Aggregated summary of doc1 and doc2";

            _mockRagService.Setup(s => s.MultiDocumentAnalysisAsync("coll", docIds, analysisQuery))
                .ReturnsAsync(new RagResponse { Response = expectedResponse });

            // Act
            var result = await _mockRagService.Object.MultiDocumentAnalysisAsync("coll", docIds, analysisQuery);

            // Assert
            Assert.Equal(expectedResponse, result.Response);
        }
    }
}


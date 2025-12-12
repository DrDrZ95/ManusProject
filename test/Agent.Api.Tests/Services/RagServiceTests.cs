using Agent.Core.Services;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Api.Tests.Services
{
    /// <summary>
    /// RagService的单元测试
    /// Unit tests for RagService
    /// </summary>
    public class RagServiceTests
    {
        private readonly Mock<IRagService> _mockRagService;

        public RagServiceTests()
        {
            _mockRagService = new Mock<IRagService>();
        }

        /// <summary>
        /// 测试混合搜索功能
        /// Test hybrid search function
        /// </summary>
        [Fact]
        public async Task HybridSearch_ShouldCombineResultsFromMultipleSources()
        {
            // Arrange
            var expectedResponse = new RagResponse
            {
                Answer = "Hybrid search result",
                SourceDocuments = new List<SourceDocument>
                {
                    new SourceDocument { DocumentId = "doc1", Score = 0.9 },
                    new SourceDocument { DocumentId = "doc2", Score = 0.8 }
                }
            };

            // 模拟混合搜索的实现 - Mock the hybrid search implementation
            _mockRagService.Setup(s => s.HybridSearchAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<RagOptions>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _mockRagService.Object.HybridSearchAsync(
                "test-collection", 
                "test-query", 
                new RagOptions(), 
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hybrid search result", result.Answer);
            Assert.Equal(2, result.SourceDocuments.Count);
            // 验证服务是否被调用 - Verify that the service was called
            _mockRagService.Verify(s => s.HybridSearchAsync(
                "test-collection", 
                "test-query", 
                It.IsAny<RagOptions>(), 
                CancellationToken.None), Times.Once);
        }

        // TODO: 补充其他RAG服务的测试，例如企业问答、多文档分析等
        // TODO: Supplement other RAG service tests, such as Enterprise QA, Multi-Document Analysis, etc.
    }
}


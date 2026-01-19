using Agent.Application.Services.RAG;
using Agent.Application.Services.SemanticKernel;
using Agent.Application.Services.VectorDatabase;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Core.Tests.Services
{
    /// <summary>
    /// RagService 核心逻辑单元测试
    /// Unit tests for RagService core logic
    /// </summary>
    public class RagServiceTests : IClassFixture<RagTestFixture>
    {
        private readonly Mock<IVectorDatabaseService> _mockVectorDb;
        private readonly Mock<ISemanticKernelService> _mockSemanticKernel;
        private readonly Mock<ILogger<RagService>> _mockLogger;
        private readonly RagService _ragService;
        private readonly RagTestFixture _fixture;

        public RagServiceTests(RagTestFixture fixture)
        {
            _fixture = fixture;
            _mockVectorDb = new Mock<IVectorDatabaseService>();
            _mockSemanticKernel = new Mock<ISemanticKernelService>();
            _mockLogger = new Mock<ILogger<RagService>>();
            _ragService = new RagService(_mockVectorDb.Object, _mockSemanticKernel.Object, _mockLogger.Object);
        }

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
        /// 测试边界条件：空文档
        /// Test edge case: empty document
        /// </summary>
        [Fact]
        public async Task AddDocumentAsync_EmptyContent_ShouldHandleGracefully()
        {
            // Arrange
            var doc = new RagDocument { Id = "empty-doc", Content = "", Title = "Empty" };

            // Act & Assert
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

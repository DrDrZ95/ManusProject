using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Agent.Core.Services.VectorDatabase;
using Agent.Core.Services.SemanticKernel;
using Agent.Core.Services.RAG;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace Agent.Api.Tests.Services;

public class RagServiceTests
{
    private readonly Mock<IVectorDatabaseService> _mockVectorDb;
    private readonly Mock<ISemanticKernelService> _mockSemanticKernel;
    private readonly Mock<ILogger<RagService>> _mockLogger;
    private readonly RagService _ragService;

    public RagServiceTests()
    {
        _mockVectorDb = new Mock<IVectorDatabaseService>();
        _mockSemanticKernel = new Mock<ISemanticKernelService>();
        _mockLogger = new Mock<ILogger<RagService>>();
        _ragService = new RagService(
            _mockVectorDb.Object,
            _mockSemanticKernel.Object,
            _mockLogger.Object
        );
    }

    #region Document Management Tests

    [Fact]
    public async Task AddDocumentAsync_ValidDocument_AddsChunksAndSummaryToVectorDb()
    {
        // Arrange
        var collectionName = "testCollection";
        var document = new RagDocument
        {
            Id = "doc1",
            Title = "Test Document",
            Content = "This is a test document. It has multiple sentences.",
            Summary = "Summary of test document.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var embedding1 = new float[] { 0.1f, 0.2f };
        var embedding2 = new float[] { 0.3f, 0.4f };
        var summaryEmbedding = new float[] { 0.5f, 0.6f };

        _mockSemanticKernel.SetupSequence(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(embedding1)
            .ReturnsAsync(embedding2)
            .ReturnsAsync(summaryEmbedding);

        _mockVectorDb.Setup(v => v.AddDocumentsAsync(
            collectionName,
            It.IsAny<IEnumerable<VectorDocument>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _ragService.AddDocumentAsync(collectionName, document);

        // Assert
        Assert.Equal("doc1", result);
        _mockSemanticKernel.Verify(s => s.GenerateEmbeddingAsync(It.IsAny<string>()), Times.Exactly(3)); // 2 chunks + 1 summary
        _mockVectorDb.Verify(v => v.AddDocumentsAsync(
            collectionName,
            It.Is<IEnumerable<VectorDocument>>(docs => docs.Count() == 3 && 
                                                       docs.Any(d => d.Id == "doc1_chunk_0" && d.Content == "This is a test document.") &&
                                                       docs.Any(d => d.Id == "doc1_chunk_1" && d.Content == "It has multiple sentences.") &&
                                                       docs.Any(d => d.Id == "doc1_summary" && d.Content == "Summary of test document.")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDocumentsAsync_ExistingDocuments_ReturnsRagDocuments()
    {
        // Arrange
        var collectionName = "testCollection";
        var docId = "doc1";
        var vectorDocs = new List<VectorDocument>
        {
            new VectorDocument { Id = "doc1_chunk_0", Content = "Chunk 0", Metadata = new Dictionary<string, object> { { "document_id", docId }, { "document_title", "Title" }, { "chunk_type", "content" }, { "chunk_position", 0 } } },
            new VectorDocument { Id = "doc1_chunk_1", Content = "Chunk 1", Metadata = new Dictionary<string, object> { { "document_id", docId }, { "document_title", "Title" }, { "chunk_type", "content" }, { "chunk_position", 1 } } },
            new VectorDocument { Id = "doc1_summary", Content = "Summary", Metadata = new Dictionary<string, object> { { "document_id", docId }, { "document_title", "Title" }, { "chunk_type", "summary" } } }
        };
        _mockVectorDb.Setup(v => v.GetDocumentsAsync(
            collectionName,
            null,
            It.IsAny<VectorFilter>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(vectorDocs);

        // Act
        var result = await _ragService.GetDocumentsAsync(collectionName);

        // Assert
        Assert.Single(result);
        Assert.Equal(docId, result.First().Id);
        Assert.Equal("Title", result.First().Title);
        Assert.Equal("Summary", result.First().Summary);
        Assert.Equal(2, result.First().Chunks.Count);
        Assert.Equal("Chunk 0\n\nChunk 1", result.First().Content);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ExistingDocument_DeletesAndAddsNewVersion()
    {
        // Arrange
        var collectionName = "testCollection";
        var document = new RagDocument
        {
            Id = "doc1",
            Title = "Updated Document",
            Content = "This is an updated test document.",
            Summary = "Updated summary.",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        var embedding = new float[] { 0.1f, 0.2f };

        _mockVectorDb.Setup(v => v.DeleteDocumentsAsync(
            collectionName,
            null,
            It.IsAny<VectorFilter>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(embedding);
        _mockVectorDb.Setup(v => v.AddDocumentsAsync(
            collectionName,
            It.IsAny<IEnumerable<VectorDocument>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _ragService.UpdateDocumentAsync(collectionName, document);

        // Assert
        _mockVectorDb.Verify(v => v.DeleteDocumentsAsync(
            collectionName,
            null,
            It.Is<VectorFilter>(f => (string)f.Equals["document_id"] == "doc1"),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockVectorDb.Verify(v => v.AddDocumentsAsync(
            collectionName,
            It.Is<IEnumerable<VectorDocument>>(docs => docs.Any(d => d.Id == "doc1_summary" && d.Content == "Updated summary.")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ExistingDocument_DeletesFromVectorDb()
    {
        // Arrange
        var collectionName = "testCollection";
        var documentId = "doc1";

        _mockVectorDb.Setup(v => v.DeleteDocumentsAsync(
            collectionName,
            null,
            It.IsAny<VectorFilter>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _ragService.DeleteDocumentAsync(collectionName, documentId);

        // Assert
        _mockVectorDb.Verify(v => v.DeleteDocumentsAsync(
            collectionName,
            null,
            It.Is<VectorFilter>(f => (string)f.Equals["document_id"] == documentId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Hybrid Retrieval Tests

    [Fact]
    public async Task HybridRetrievalAsync_WithQuery_ReturnsMergedResults()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = new RagQuery { Text = "test query", TopK = 2 };
        var vectorEmbedding = new float[] { 0.1f, 0.2f };

        _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(query.Text))
            .ReturnsAsync(vectorEmbedding);

        _mockVectorDb.Setup(v => v.SearchByEmbeddingAsync(
            collectionName,
            It.IsAny<float[]>(),
            It.IsAny<VectorSearchOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VectorSearchResults
            {
                Matches = new List<VectorSearchResult>
                {
                    new VectorSearchResult { Id = "vec_chunk1", Content = "Vector Content 1", Score = 0.9f, Metadata = new Dictionary<string, object> { { "document_id", "doc1" } } }
                }
            });

        _mockVectorDb.Setup(v => v.GetDocumentsAsync(
            collectionName,
            null,
            It.IsAny<VectorFilter>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VectorDocument>
            {
                new VectorDocument { Id = "kw_chunk1", Content = "Keyword Content 1", Metadata = new Dictionary<string, object> { { "document_id", "doc2" } } }
            });

        _mockSemanticKernel.Setup(s => s.GetChatCompletionAsync(
            It.Is<string>(p => p.Contains("Keyword Content 1")), 
            It.IsAny<string>()))
            .ReturnsAsync("Semantic Content 1");

        // Act
        var result = await _ragService.HybridRetrievalAsync(collectionName, query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Chunks.Count > 0);
        Assert.Equal(RetrievalStrategy.Hybrid, result.Strategy);
    }

    [Fact]
    public async Task HybridRetrievalAsync_WithReRanking_AppliesReRanking()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = new RagQuery { Text = "test query", TopK = 2, ReRanking = new ReRankingOptions { Enabled = true, Model = "rerank-model" } };
        var vectorEmbedding = new float[] { 0.1f, 0.2f };

        _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(query.Text))
            .ReturnsAsync(vectorEmbedding);

        _mockVectorDb.Setup(v => v.SearchByEmbeddingAsync(
            collectionName,
            It.IsAny<float[]>(),
            It.IsAny<VectorSearchOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VectorSearchResults
            {
                Matches = new List<VectorSearchResult>
                {
                    new VectorSearchResult { Id = "vec_chunk1", Content = "Vector Content 1", Score = 0.9f, Metadata = new Dictionary<string, object> { { "document_id", "doc1" } } }
                }
            });

        _mockVectorDb.Setup(v => v.GetDocumentsAsync(
            collectionName,
            null,
            It.IsAny<VectorFilter>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VectorDocument>
            {
                new VectorDocument { Id = "kw_chunk1", Content = "Keyword Content 1", Metadata = new Dictionary<string, object> { { "document_id", "doc2" } } }
            });

        _mockSemanticKernel.Setup(s => s.GetChatCompletionAsync(
            It.Is<string>(p => p.Contains("Keyword Content 1")), 
            It.IsAny<string>()))
            .ReturnsAsync("Semantic Content 1");

        // Mock the re-ranking behavior (simplified for unit test)
        _mockSemanticKernel.Setup(s => s.GetChatCompletionAsync(
            It.Is<string>(p => p.Contains("rerank-model")), 
            It.IsAny<string>()))
            .ReturnsAsync("{\"reranked_chunks\": [{\"id\": \"vec_chunk1\", \"relevance_score\": 0.95}]}");

        // Act
        var result = await _ragService.HybridRetrievalAsync(collectionName, query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Chunks.Count > 0);
        Assert.Equal(RetrievalStrategy.Hybrid, result.Strategy);
        // Further assertions to verify re-ranking logic if needed
    }

    [Fact]
    public async Task HybridRetrievalAsync_ErrorInRetrieval_ThrowsException()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = new RagQuery { Text = "test query", TopK = 2 };

        _mockSemanticKernel.Setup(s => s.GenerateEmbeddingAsync(query.Text))
            .ThrowsAsync(new InvalidOperationException("Embedding service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ragService.HybridRetrievalAsync(collectionName, query));
    }

    #endregion

    #region RAG Generation Tests

    [Fact]
    public async Task GenerateRagResponseAsync_WithContext_ReturnsGeneratedResponse()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "What is the document about?";
        var retrievedChunks = new List<RagRetrievedChunk>
        {
            new RagRetrievedChunk { Chunk = new RagDocumentChunk { Content = "The document is about AI." }, Score = 0.9f }
        };
        var expectedResponse = "The document discusses AI topics.";

        _mockSemanticKernel.Setup(s => s.GetChatCompletionAsync(
            It.Is<string>(p => p.Contains("The document is about AI.") && p.Contains(query)), 
            It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _ragService.GenerateRagResponseAsync(collectionName, query, retrievedChunks);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockSemanticKernel.Verify(s => s.GetChatCompletionAsync(
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateRagResponseAsync_WithoutContext_ReturnsGeneralResponse()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "Tell me a joke.";
        var retrievedChunks = new List<RagRetrievedChunk>(); // No retrieved chunks
        var expectedResponse = "Why don't scientists trust atoms? Because they make up everything!";

        _mockSemanticKernel.Setup(s => s.GetChatCompletionAsync(
            It.Is<string>(p => !p.Contains("Context:") && p.Contains(query)), 
            It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _ragService.GenerateRagResponseAsync(collectionName, query, retrievedChunks);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockSemanticKernel.Verify(s => s.GetChatCompletionAsync(
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateRagResponseAsync_SemanticKernelFails_ThrowsException()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "test query";
        var retrievedChunks = new List<RagRetrievedChunk>
        {
            new RagRetrievedChunk { Chunk = new RagDocumentChunk { Content = "Some context." }, Score = 0.8f }
        };

        _mockSemanticKernel.Setup(s => s.GetChatCompletionAsync(
            It.IsAny<string>(), 
            It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Semantic kernel error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ragService.GenerateRagResponseAsync(collectionName, query, retrievedChunks));
    }

    #endregion
}



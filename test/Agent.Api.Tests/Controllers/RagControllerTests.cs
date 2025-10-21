using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Agent.Core.Services.RAG;
using Agent.Api;
using Agent.Core.Services.Telemetry;
using System.Collections.Generic;

namespace Agent.Core.Tests.Controllers;

public class RagControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IRagService> _mockRagService;
    private readonly Mock<IAgentTelemetryProvider> _mockTelemetryProvider;

    public RagControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing IRagService and IAgentTelemetryProvider registrations
                services.RemoveAll<IRagService>();
                services.RemoveAll<IAgentTelemetryProvider>();

                // Add mocked services
                _mockRagService = new Mock<IRagService>();
                _mockTelemetryProvider = new Mock<IAgentTelemetryProvider>();
                
                services.AddSingleton(_mockRagService.Object);
                services.AddSingleton(_mockTelemetryProvider.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AddDocument_ValidDocument_ReturnsOk()
    {
        // Arrange
        var collectionName = "test-collection";
        var document = new RagDocument { Id = "doc1", Content = "Test content" };
        _mockRagService.Setup(s => s.AddDocumentAsync(collectionName, document, It.IsAny<CancellationToken>()))
            .ReturnsAsync("doc1");
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(document), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/documents", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        _mockRagService.Verify(s => s.AddDocumentAsync(collectionName, document, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDocuments_ExistingDocuments_ReturnsOkWithDocuments()
    {
        // Arrange
        var collectionName = "test-collection";
        var documents = new List<RagDocument>
        {
            new RagDocument { Id = "doc1", Content = "Content 1" },
            new RagDocument { Id = "doc2", Content = "Content 2" }
        };
        _mockRagService.Setup(s => s.GetDocumentsAsync(collectionName, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        // Act
        var response = await _client.GetAsync($"/api/Rag/collections/{collectionName}/documents");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        Assert.Contains("doc2", responseString);
        _mockRagService.Verify(s => s.GetDocumentsAsync(collectionName, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDocument_ExistingDocument_ReturnsOk()
    {
        // Arrange
        var collectionName = "test-collection";
        var documentId = "doc1";
        var updatedDocument = new RagDocument { Id = documentId, Content = "Updated content" };
        _mockRagService.Setup(s => s.UpdateDocumentAsync(collectionName, updatedDocument, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedDocument), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/Rag/collections/{collectionName}/documents/{documentId}", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockRagService.Verify(s => s.UpdateDocumentAsync(collectionName, updatedDocument, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDocument_ExistingDocument_ReturnsOk()
    {
        // Arrange
        var collectionName = "test-collection";
        var documentId = "doc1";
        _mockRagService.Setup(s => s.DeleteDocumentAsync(collectionName, documentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        // Act
        var response = await _client.DeleteAsync($"/api/Rag/collections/{collectionName}/documents/{documentId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockRagService.Verify(s => s.DeleteDocumentAsync(collectionName, documentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDocumentCount_ReturnsOkWithCount()
    {
        // Arrange
        var collectionName = "test-collection";
        _mockRagService.Setup(s => s.GetDocumentCountAsync(collectionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        // Act
        var response = await _client.GetAsync($"/api/Rag/collections/{collectionName}/count");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("5", responseString);
        _mockRagService.Verify(s => s.GetDocumentCountAsync(collectionName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HybridRetrieval_ValidQuery_ReturnsOkWithResult()
    {
        // Arrange
        var collectionName = "test-collection";
        var query = new RagQuery { QueryText = "test query" };
        var retrievalResult = new RagRetrievalResult
        {
            Query = query.QueryText,
            RetrievedDocuments = new List<RetrievedRagDocument>
            {
                new RetrievedRagDocument { Id = "retrieved_doc1", Content = "Relevant content" }
            }
        };
        _mockRagService.Setup(s => s.HybridRetrievalAsync(collectionName, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/hybrid", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Relevant content", responseString);
        _mockRagService.Verify(s => s.HybridRetrievalAsync(collectionName, query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VectorRetrieval_ValidQuery_ReturnsOkWithResult()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new VectorRetrievalRequest { Query = "vector query", TopK = 3 };
        var retrievalResult = new RagRetrievalResult
        {
            Query = request.Query,
            RetrievedDocuments = new List<RetrievedRagDocument>
            {
                new RetrievedRagDocument { Id = "vector_doc1", Content = "Vector relevant content" }
            }
        };
        _mockRagService.Setup(s => s.VectorRetrievalAsync(collectionName, request.Query, request.TopK, It.IsAny<CancellationToken>()))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/vector", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Vector relevant content", responseString);
        _mockRagService.Verify(s => s.VectorRetrievalAsync(collectionName, request.Query, request.TopK, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task KeywordRetrieval_ValidQuery_ReturnsOkWithResult()
    {
        // Arrange
        var collectionName = "test-collection";
        var query = new RagQuery { QueryText = "keyword query" };
        var retrievalResult = new RagRetrievalResult
        {
            Query = query.QueryText,
            RetrievedDocuments = new List<RetrievedRagDocument>
            {
                new RetrievedRagDocument { Id = "keyword_doc1", Content = "Keyword relevant content" }
            }
        };
        _mockRagService.Setup(s => s.KeywordRetrievalAsync(collectionName, query.QueryText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/keyword", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Keyword relevant content", responseString);
        _mockRagService.Verify(s => s.KeywordRetrievalAsync(collectionName, query.QueryText, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rerank_ValidRequest_ReturnsOkWithResult()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new RerankRequest
        {
            Query = "rerank query",
            Documents = new List<string> { "doc1", "doc2" }
        };
        var rerankResult = new RerankResult
        {
            Query = request.Query,
            RerankedDocuments = new List<RerankedDocument>
            {
                new RerankedDocument { Document = "doc1", Score = 0.9f },
                new RerankedDocument { Document = "doc2", Score = 0.8f }
            }
        };
        _mockRagService.Setup(s => s.RerankAsync(collectionName, request.Query, request.Documents, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rerankResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/rerank", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        Assert.Contains("0.9", responseString);
        _mockRagService.Verify(s => s.RerankAsync(collectionName, request.Query, request.Documents, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAnswer_ValidRequest_ReturnsOkWithAnswer()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new GenerateAnswerRequest
        {
            Query = "answer query",
            Context = new List<string> { "context1", "context2" }
        };
        var answerResult = new GenerateAnswerResult
        {
            Query = request.Query,
            Answer = "Generated answer based on context."
        };
        _mockRagService.Setup(s => s.GenerateAnswerAsync(collectionName, request.Query, request.Context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(answerResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/answer", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Generated answer based on context.", responseString);
        _mockRagService.Verify(s => s.GenerateAnswerAsync(collectionName, request.Query, request.Context, It.IsAny<CancellationToken>()), Times.Once);
    }
}

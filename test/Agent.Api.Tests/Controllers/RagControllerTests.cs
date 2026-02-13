using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Agent.Api.Tests.Controllers;

public class RagControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private Mock<IRagService> _mockRagService;
    private Mock<IAgentTelemetryProvider> _mockTelemetryProvider;

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
        _mockRagService.Setup(s => s.AddDocumentAsync(collectionName, document))
            .ReturnsAsync("doc1");
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(document), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/documents", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        _mockRagService.Verify(s => s.AddDocumentAsync(collectionName, document), Times.Once);
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
        _mockRagService.Setup(s => s.GetDocumentsAsync(collectionName, null))
            .ReturnsAsync(documents);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        // Act
        var response = await _client.GetAsync($"/api/Rag/collections/{collectionName}/documents");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        Assert.Contains("doc2", responseString);
        _mockRagService.Verify(s => s.GetDocumentsAsync(collectionName, null), Times.Once);
    }

    [Fact]
    public async Task UpdateDocument_ExistingDocument_ReturnsOk()
    { // Arrange
        var collectionName = "test-collection";
        var documentId = "doc1";
        var document = new RagDocument { Id = documentId, Content = "Updated content" };
        _mockRagService.Setup(s => s.UpdateDocumentAsync(collectionName, document))
            .Returns(Task.CompletedTask);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(document), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/Rag/collections/{collectionName}/documents/{documentId}", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockRagService.Verify(s => s.UpdateDocumentAsync(collectionName, document), Times.Once);
    }

    [Fact]
    public async Task DeleteDocument_ExistingDocument_ReturnsOk()
    {
        // Arrange
        var collectionName = "test-collection";
        var documentId = "doc1";
        _mockRagService.Setup(s => s.DeleteDocumentAsync(collectionName, documentId))
            .Returns(Task.CompletedTask);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        // Act
        var response = await _client.DeleteAsync($"/api/Rag/collections/{collectionName}/documents/{documentId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockRagService.Verify(s => s.DeleteDocumentAsync(collectionName, documentId), Times.Once);
    }

    [Fact]
    public async Task GetDocumentCount_ReturnsOkWithCount()
    {
        // Arrange
        var collectionName = "test-collection";
        _mockRagService.Setup(s => s.GetDocumentCountAsync(collectionName))
            .ReturnsAsync(5);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        // Act
        var response = await _client.GetAsync($"/api/Rag/collections/{collectionName}/count");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("5", responseString);
        _mockRagService.Verify(s => s.GetDocumentCountAsync(collectionName), Times.Once);
    }

    [Fact]
    public async Task HybridRetrieval_ValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var collectionName = "test-collection";
        var query = new RagQuery { QueryText = "hybrid query" };
        var retrievalResult = new RagRetrievalResult
        {
            Chunks = new List<RagRetrievedChunk>
            {
                new RagRetrievedChunk 
                { 
                    Chunk = new RagDocumentChunk { DocumentId = "doc1", Content = "content1" },
                    Score = 0.9f 
                }
            }
        };
        _mockRagService.Setup(s => s.HybridRetrievalAsync(collectionName, query))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/hybrid", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        _mockRagService.Verify(s => s.HybridRetrievalAsync(collectionName, query), Times.Once);
    }

    [Fact]
    public async Task VectorRetrieval_ValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new VectorRetrievalRequest { Query = "vector query", TopK = 5 };
        var retrievalResult = new RagRetrievalResult
        {
            Chunks = new List<RagRetrievedChunk>
            {
                new RagRetrievedChunk 
                { 
                    Chunk = new RagDocumentChunk { DocumentId = "doc1", Content = "content1" },
                    Score = 0.85f 
                }
            }
        };
        _mockRagService.Setup(s => s.VectorRetrievalAsync(collectionName, request.Query, request.TopK))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/vector", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        _mockRagService.Verify(s => s.VectorRetrievalAsync(collectionName, request.Query, request.TopK), Times.Once);
    }

    [Fact]
    public async Task KeywordRetrieval_ValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new KeywordRetrievalRequest { Query = "keyword query", TopK = 5 };
        var retrievalResult = new RagRetrievalResult
        {
            Chunks = new List<RagRetrievedChunk>
            {
                new RagRetrievedChunk 
                { 
                    Chunk = new RagDocumentChunk { DocumentId = "doc1", Content = "content1" },
                    Score = 0.75f 
                }
            }
        };
        _mockRagService.Setup(s => s.KeywordRetrievalAsync(collectionName, request.Query, request.TopK))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/keyword", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        _mockRagService.Verify(s => s.KeywordRetrievalAsync(collectionName, request.Query, request.TopK), Times.Once);
    }

    [Fact]
    public async Task SemanticRetrieval_ValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new SemanticRetrievalRequest { Query = "semantic query", TopK = 5 };
        var retrievalResult = new RagRetrievalResult
        {
            Chunks = new List<RagRetrievedChunk>
            {
                new RagRetrievedChunk 
                { 
                    Chunk = new RagDocumentChunk { DocumentId = "doc1", Content = "content1" },
                    Score = 0.95f 
                }
            }
        };
        _mockRagService.Setup(s => s.SemanticRetrievalAsync(collectionName, request.Query, request.TopK))
            .ReturnsAsync(retrievalResult);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/retrieve/semantic", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc1", responseString);
        _mockRagService.Verify(s => s.SemanticRetrievalAsync(collectionName, request.Query, request.TopK), Times.Once);
    }

    [Fact]
    public async Task GenerateResponse_ValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var collectionName = "test-collection";
        var request = new RagGenerationRequest
        {
            Prompt = "answer query"
        };
        var ragResponse = new RagResponse
        {
            Response = "Generated answer based on context."
        };
        _mockRagService.Setup(s => s.GenerateResponseAsync(collectionName, request))
            .ReturnsAsync(ragResponse);
        _mockTelemetryProvider.Setup(s => s.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
            .Returns(new Mock<IAgentSpan>().Object);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/Rag/collections/{collectionName}/generate", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Generated answer based on context.", responseString);
        _mockRagService.Verify(s => s.GenerateResponseAsync(collectionName, request), Times.Once);
    }
}

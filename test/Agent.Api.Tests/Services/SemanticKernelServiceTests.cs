using Xunit;
using Moq;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.Logging;
using Agent.Core.Services.VectorDatabase;
using Agent.Core.Services.SemanticKernel;
using Agent.Core.Services.SemanticKernel.Planner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Agent.Api.Tests.Services;

public class SemanticKernelServiceTests
{
    private readonly Mock<Kernel> _mockKernel;
    private readonly Mock<IChatCompletionService> _mockChatService;
    private readonly Mock<ITextEmbeddingGenerationService> _mockEmbeddingService;
    private readonly Mock<IVectorDatabaseService> _mockVectorDatabase;
    private readonly SemanticKernelOptions _options;
    private readonly Mock<ILogger<SemanticKernelService>> _mockLogger;
    private readonly Mock<IKubernetesPlanner> _mockKubernetesPlanner;
    private readonly Mock<IIstioPlanner> _mockIstioPlanner;
    private readonly Mock<IPostgreSQLPlanner> _mockPostgreSQLPlanner;
    private readonly Mock<IClickHousePlanner> _mockClickHousePlanner;
    private readonly SemanticKernelService _semanticKernelService;

    public SemanticKernelServiceTests()
    {
        _mockKernel = new Mock<Kernel>();
        _mockChatService = new Mock<IChatCompletionService>();
        _mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();
        _mockVectorDatabase = new Mock<IVectorDatabaseService>();
        _options = new SemanticKernelOptions { MaxTokens = 100, Temperature = 0.7 };
        _mockLogger = new Mock<ILogger<SemanticKernelService>>();
        _mockKubernetesPlanner = new Mock<IKubernetesPlanner>();
        _mockIstioPlanner = new Mock<IIstioPlanner>();
        _mockPostgreSQLPlanner = new Mock<IPostgreSQLPlanner>();
        _mockClickHousePlanner = new Mock<IClickHousePlanner>();

        _semanticKernelService = new SemanticKernelService(
            _mockKernel.Object,
            _mockChatService.Object,
            _mockEmbeddingService.Object,
            _mockVectorDatabase.Object,
            _options,
            _mockLogger.Object,
            _mockKubernetesPlanner.Object,
            _mockIstioPlanner.Object,
            _mockPostgreSQLPlanner.Object,
            _mockClickHousePlanner.Object
        );
    }

    #region GetChatCompletionAsync Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithBasicPrompt_ReturnsResponse()
    {
        // Arrange
        var prompt = "Hello, how are you?";
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "I am fine, thank you.");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionAsync(prompt);

        // Assert
        Assert.Equal("I am fine, thank you.", result);
        _mockChatService.Verify(s => s.GetChatMessageContentAsync(
            It.Is<ChatHistory>(h => h.Last().Content == prompt && h.Count == 1), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithSystemMessage_ReturnsResponse()
    {
        // Arrange
        var prompt = "What is your purpose?";
        var systemMessage = "You are a helpful AI assistant.";
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "My purpose is to assist you.");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionAsync(prompt, systemMessage);

        // Assert
        Assert.Equal("My purpose is to assist you.", result);
        _mockChatService.Verify(s => s.GetChatMessageContentAsync(
            It.Is<ChatHistory>(h => h.First().Content == systemMessage && h.Last().Content == prompt && h.Count == 2), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatCompletionAsync_ChatServiceFails_ThrowsException()
    {
        // Arrange
        var prompt = "Test prompt";
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Chat service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.GetChatCompletionAsync(prompt));
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithEmptyPrompt_ReturnsEmptyString()
    {
        // Arrange
        var prompt = "";
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionAsync(prompt);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region GetStreamingChatCompletionAsync Tests

    [Fact]
    public async Task GetStreamingChatCompletionAsync_WithBasicPrompt_ReturnsStreamingResponse()
    {
        // Arrange
        var prompt = "Tell me a story.";
        var streamingContents = new List<string> { "Once ", "upon ", "a time." };
        _mockChatService.Setup(s => s.GetStreamingChatMessageContentsAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .Returns(streamingContents.Select(c => new StreamingChatMessageContent(AuthorRole.Assistant, c)).ToAsyncEnumerable());

        // Act
        var result = new List<string>();
        await foreach (var content in _semanticKernelService.GetStreamingChatCompletionAsync(prompt))
        {
            result.Add(content);
        }

        // Assert
        Assert.Equal(streamingContents, result);
        _mockChatService.Verify(s => s.GetStreamingChatMessageContentsAsync(
            It.Is<ChatHistory>(h => h.Last().Content == prompt && h.Count == 1), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStreamingChatCompletionAsync_WithSystemMessage_ReturnsStreamingResponse()
    {
        // Arrange
        var prompt = "Tell me a story.";
        var systemMessage = "You are a storyteller.";
        var streamingContents = new List<string> { "Once ", "upon ", "a time." };
        _mockChatService.Setup(s => s.GetStreamingChatMessageContentsAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .Returns(streamingContents.Select(c => new StreamingChatMessageContent(AuthorRole.Assistant, c)).ToAsyncEnumerable());

        // Act
        var result = new List<string>();
        await foreach (var content in _semanticKernelService.GetStreamingChatCompletionAsync(prompt, systemMessage))
        {
            result.Add(content);
        }

        // Assert
        Assert.Equal(streamingContents, result);
        _mockChatService.Verify(s => s.GetStreamingChatMessageContentsAsync(
            It.Is<ChatHistory>(h => h.First().Content == systemMessage && h.Last().Content == prompt && h.Count == 2), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStreamingChatCompletionAsync_ChatServiceFails_ThrowsException()
    {
        // Arrange
        var prompt = "Test prompt";
        _mockChatService.Setup(s => s.GetStreamingChatMessageContentsAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("Streaming chat service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var content in _semanticKernelService.GetStreamingChatCompletionAsync(prompt))
            {
                // Consume the enumerable to trigger the exception
            }
        });
    }

    #endregion

    #region GetChatCompletionWithHistoryAsync Tests

    [Fact]
    public async Task GetChatCompletionWithHistoryAsync_WithSimpleHistory_ReturnsResponse()
    {
        // Arrange
        var chatHistory = new List<ChatMessage>
        {
            new ChatMessage { Role = "user", Content = "Hi" },
            new ChatMessage { Role = "assistant", Content = "Hello!" },
            new ChatMessage { Role = "user", Content = "How are you?" }
        };
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "I am good.");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionWithHistoryAsync(chatHistory);

        // Assert
        Assert.Equal("I am good.", result);
        _mockChatService.Verify(s => s.GetChatMessageContentAsync(
            It.Is<ChatHistory>(h => h.Count == 3 && h[0].Content == "Hi" && h[1].Content == "Hello!" && h[2].Content == "How are you?"), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatCompletionWithHistoryAsync_WithSystemMessageInHistory_ReturnsResponse()
    {
        // Arrange
        var chatHistory = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = "You are a bot." },
            new ChatMessage { Role = "user", Content = "What is your name?" }
        };
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "I am a bot.");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionWithHistoryAsync(chatHistory);

        // Assert
        Assert.Equal("I am a bot.", result);
        _mockChatService.Verify(s => s.GetChatMessageContentAsync(
            It.Is<ChatHistory>(h => h.Count == 2 && h[0].Content == "You are a bot." && h[1].Content == "What is your name?"), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatCompletionWithHistoryAsync_WithEmptyHistory_ReturnsEmptyString()
    {
        // Arrange
        var chatHistory = new List<ChatMessage>();
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionWithHistoryAsync(chatHistory);

        // Assert
        Assert.Equal(string.Empty, result);
        _mockChatService.Verify(s => s.GetChatMessageContentAsync(
            It.Is<ChatHistory>(h => !h.Any()), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatCompletionWithHistoryAsync_WithUnknownMessageRole_LogsWarningAndIgnores()
    {
        // Arrange
        var chatHistory = new List<ChatMessage>
        {
            new ChatMessage { Role = "unknown", Content = "This should be ignored" },
            new ChatMessage { Role = "user", Content = "Hello" }
        };
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "Hi there.");
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatMessageContent);

        // Act
        var result = await _semanticKernelService.GetChatCompletionWithHistoryAsync(chatHistory);

        // Assert
        Assert.Equal("Hi there.", result);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unknown message role: unknown")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        _mockChatService.Verify(s => s.GetChatMessageContentAsync(
            It.Is<ChatHistory>(h => h.Count == 1 && h.First().Content == "Hello"), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChatCompletionWithHistoryAsync_ChatServiceFails_ThrowsException()
    {
        // Arrange
        var chatHistory = new List<ChatMessage> { new ChatMessage { Role = "user", Content = "Hi" } };
        _mockChatService.Setup(s => s.GetChatMessageContentAsync(
            It.IsAny<ChatHistory>(), 
            It.IsAny<OpenAIPromptExecutionSettings>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Chat service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.GetChatCompletionWithHistoryAsync(chatHistory));
    }

    #endregion

    #region GenerateEmbeddingAsync Tests

    [Fact]
    public async Task GenerateEmbeddingAsync_WithValidText_ReturnsEmbedding()
    {
        // Arrange
        var text = "Test text";
        var expectedEmbedding = new float[] { 0.1f, 0.2f, 0.3f };
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(text, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(expectedEmbedding));

        // Act
        var result = await _semanticKernelService.GenerateEmbeddingAsync(text);

        // Assert
        Assert.Equal(expectedEmbedding, result);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithEmptyText_ReturnsEmbedding()
    {
        // Arrange
        var text = "";
        var expectedEmbedding = new float[] { 0.0f, 0.0f, 0.0f }; // Assuming empty text returns a zero embedding or similar
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(text, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(expectedEmbedding));

        // Act
        var result = await _semanticKernelService.GenerateEmbeddingAsync(text);

        // Assert
        Assert.Equal(expectedEmbedding, result);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_EmbeddingServiceFails_ThrowsException()
    {
        // Arrange
        var text = "Test text";
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(text, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.GenerateEmbeddingAsync(text));
    }

    #endregion

    #region GenerateEmbeddingsAsync Tests

    [Fact]
    public async Task GenerateEmbeddingsAsync_WithValidTexts_ReturnsEmbeddings()
    {
        // Arrange
        var texts = new List<string> { "Text 1", "Text 2" };
        var expectedEmbeddings = new List<float[]> { new float[] { 0.1f, 0.2f }, new float[] { 0.3f, 0.4f } };
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingsAsync(texts, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmbeddings.Select(e => new ReadOnlyMemory<float>(e)));

        // Act
        var result = await _semanticKernelService.GenerateEmbeddingsAsync(texts);

        // Assert
        Assert.Equal(expectedEmbeddings.Count, result.Count());
        Assert.True(expectedEmbeddings.Zip(result, (e1, e2) => e1.SequenceEqual(e2)).All(x => x));
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_WithEmptyTexts_ReturnsEmptyList()
    {
        // Arrange
        var texts = new List<string>();
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingsAsync(texts, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReadOnlyMemory<float>>());

        // Act
        var result = await _semanticKernelService.GenerateEmbeddingsAsync(texts);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_EmbeddingServiceFails_ThrowsException()
    {
        // Arrange
        var texts = new List<string> { "Text 1" };
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingsAsync(texts, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.GenerateEmbeddingsAsync(texts));
    }

    #endregion

    #region InvokeFunctionAsync (non-generic) Tests

    [Fact]
    public async Task InvokeFunctionAsync_ValidFunctionNoArguments_ReturnsResult()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var functionName = "TestFunction";
        var expectedResult = "Function executed.";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FunctionResult(functionName, expectedResult));

        // Act
        var result = await _semanticKernelService.InvokeFunctionAsync(pluginName, functionName);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task InvokeFunctionAsync_ValidFunctionWithArguments_ReturnsResult()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var functionName = "TestFunctionWithArgs";
        var arguments = new Dictionary<string, object> { { "input", "value" } };
        var expectedResult = "Function with args executed.";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.Is<KernelArguments>(ka => ka["input"].ToString() == "value"), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FunctionResult(functionName, expectedResult));

        // Act
        var result = await _semanticKernelService.InvokeFunctionAsync(pluginName, functionName, arguments);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task InvokeFunctionAsync_KubernetesPlannerFunction_LogsWarning()
    {
        // Arrange
        var pluginName = "KubernetesPlanner";
        var functionName = "KubernetesPlanner.Deploy";
        var expectedResult = "Deployment initiated.";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FunctionResult(functionName, expectedResult));

        // Act
        var result = await _semanticKernelService.InvokeFunctionAsync(pluginName, functionName);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Access control check for KubernetesPlanner.Deploy: Requires 'highest permission'. (Simulated)")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task InvokeFunctionAsync_InvalidFunction_ThrowsException()
    {
        // Arrange
        var pluginName = "NonExistentPlugin";
        var functionName = "NonExistentFunction";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KernelException("Function not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KernelException>(() => _semanticKernelService.InvokeFunctionAsync(pluginName, functionName));
    }

    #endregion

    #region InvokeFunctionAsync (generic) Tests

    [Fact]
    public async Task InvokeFunctionAsyncGeneric_ValidFunction_ReturnsTypedResult()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var functionName = "GetString";
        var expectedResult = "Typed string result.";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FunctionResult(functionName, expectedResult));

        // Act
        var result = await _semanticKernelService.InvokeFunctionAsync<string>(pluginName, functionName);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task InvokeFunctionAsyncGeneric_KubernetesPlannerFunction_LogsWarning()
    {
        // Arrange
        var pluginName = "KubernetesPlanner";
        var functionName = "KubernetesPlanner.GetStatus";
        var expectedResult = "Status: Running.";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FunctionResult(functionName, expectedResult));

        // Act
        var result = await _semanticKernelService.InvokeFunctionAsync<string>(pluginName, functionName);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Access control check for KubernetesPlanner.GetStatus: Requires 'highest permission'. (Simulated)")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task InvokeFunctionAsyncGeneric_InvalidCast_ThrowsException()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var functionName = "GetInt";
        var kernelResult = new FunctionResult(functionName, "not an int");
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(kernelResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCastException>(() => _semanticKernelService.InvokeFunctionAsync<int>(pluginName, functionName));
    }

    [Fact]
    public async Task InvokeFunctionAsyncGeneric_KernelFails_ThrowsException()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var functionName = "FailingFunction";
        _mockKernel.Setup(k => k.InvokeAsync(
            pluginName, 
            functionName, 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KernelException("Kernel execution error"));

        // Act & Assert
        await Assert.ThrowsAsync<KernelException>(() => _semanticKernelService.InvokeFunctionAsync<string>(pluginName, functionName));
    }

    #endregion

    #region AddPlugin Tests

    [Fact]
    public void AddPlugin_WithPluginName_AddsPlugin()
    {
        // Arrange
        var plugin = new object();
        var pluginName = "MyPlugin";

        // Act
        _semanticKernelService.AddPlugin(plugin, pluginName);

        // Assert
        _mockKernel.Verify(k => k.Plugins.AddFromObject(plugin, pluginName), Times.Once);
    }

    [Fact]
    public void AddPlugin_WithoutPluginName_AddsPluginWithTypeName()
    {
        // Arrange
        var plugin = new TestPluginClass();
        var expectedPluginName = nameof(TestPluginClass);

        // Act
        _semanticKernelService.AddPlugin(plugin);

        // Assert
        _mockKernel.Verify(k => k.Plugins.AddFromObject(plugin, expectedPluginName), Times.Once);
    }

    [Fact]
    public void AddPlugin_NullPlugin_ThrowsException()
    {
        // Arrange
        object plugin = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _semanticKernelService.AddPlugin(plugin));
    }

    public class TestPluginClass { }

    #endregion

    #region AddPluginFromType Tests

    [Fact]
    public void AddPluginFromType_WithPluginName_AddsPlugin()
    {
        // Arrange
        var pluginName = "MyTypePlugin";

        // Act
        _semanticKernelService.AddPluginFromType<TestPluginClass>(pluginName);

        // Assert
        _mockKernel.Verify(k => k.Plugins.AddFromObject(It.IsAny<TestPluginClass>(), pluginName), Times.Once);
    }

    [Fact]
    public void AddPluginFromType_WithoutPluginName_AddsPluginWithTypeName()
    {
        // Arrange
        // Act
        _semanticKernelService.AddPluginFromType<TestPluginClass>();

        // Assert
        _mockKernel.Verify(k => k.Plugins.AddFromObject(It.IsAny<TestPluginClass>(), nameof(TestPluginClass)), Times.Once);
    }

    [Fact]
    public void AddPluginFromType_TypeCannotBeInstantiated_ThrowsException()
    {
        // Arrange
        // Act & Assert
        Assert.Throws<MissingMethodException>(() => _semanticKernelService.AddPluginFromType<ClassWithoutParameterlessConstructor>());
    }

    public class ClassWithoutParameterlessConstructor
    {
        public ClassWithoutParameterlessConstructor(string arg) { }
    }

    #endregion

    #region GetAvailableFunctions Tests

    [Fact]
    public void GetAvailableFunctions_WithRegisteredPlugins_ReturnsFunctionNames()
    {
        // Arrange
        var mockPlugin1 = new Mock<KernelPlugin>("Plugin1");
        mockPlugin1.Setup(p => p.GetEnumerator()).Returns(new List<KernelFunction> { new Mock<KernelFunction>().Object }.GetEnumerator());
        mockPlugin1.Object.AddFunction(KernelFunctionFactory.CreateFromMethod(() => "", "Function1"));

        var mockPlugin2 = new Mock<KernelPlugin>("Plugin2");
        mockPlugin2.Object.AddFunction(KernelFunctionFactory.CreateFromMethod(() => "", "Function2"));

        _mockKernel.Setup(k => k.Plugins.GetEnumerator()).Returns(new List<KernelPlugin> { mockPlugin1.Object, mockPlugin2.Object }.GetEnumerator());

        // Act
        var result = _semanticKernelService.GetAvailableFunctions().ToList();

        // Assert
        Assert.Contains("Plugin1.Function1", result);
        Assert.Contains("Plugin2.Function2", result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetAvailableFunctions_NoRegisteredPlugins_ReturnsEmptyList()
    {
        // Arrange
        _mockKernel.Setup(k => k.Plugins.GetEnumerator()).Returns(new List<KernelPlugin>().GetEnumerator());

        // Act
        var result = _semanticKernelService.GetAvailableFunctions().ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Memory Operations Tests

    [Fact]
    public async Task SaveMemoryAsync_ValidInput_CallsVectorDatabaseService()
    {
        // Arrange
        var collectionName = "testCollection";
        var text = "test text";
        var id = "testId";
        var embedding = new float[] { 0.1f, 0.2f };
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(text, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(embedding));

        // Act
        await _semanticKernelService.SaveMemoryAsync(collectionName, text, id);

        // Assert
        _mockVectorDatabase.Verify(v => v.SaveVectorAsync(
            collectionName,
            It.Is<VectorDocument>(doc => doc.Id == id && doc.Content == text && doc.Embedding.SequenceEqual(embedding)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveMemoryAsync_WithMetadata_CallsVectorDatabaseServiceWithMetadata()
    {
        // Arrange
        var collectionName = "testCollection";
        var text = "test text";
        var id = "testId";
        var metadata = new Dictionary<string, object> { { "source", "document" } };
        var embedding = new float[] { 0.1f, 0.2f };
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(text, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(embedding));

        // Act
        await _semanticKernelService.SaveMemoryAsync(collectionName, text, id, metadata);

        // Assert
        _mockVectorDatabase.Verify(v => v.SaveVectorAsync(
            collectionName,
            It.Is<VectorDocument>(doc => doc.Id == id && doc.Content == text && doc.Metadata["source"].ToString() == "document"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveMemoryAsync_EmbeddingServiceFails_ThrowsException()
    {
        // Arrange
        var collectionName = "testCollection";
        var text = "test text";
        var id = "testId";
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(text, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.SaveMemoryAsync(collectionName, text, id));
    }

    [Fact]
    public async Task SearchMemoryAsync_ValidQuery_ReturnsRelevantMemories()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "test query";
        var queryEmbedding = new float[] { 0.5f, 0.6f };
        var searchResult = new List<VectorSearchResult>
        {
            new VectorSearchResult { Id = "doc1", Content = "content1", Score = 0.9f }
        };

        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(query, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(queryEmbedding));
        _mockVectorDatabase.Setup(v => v.SearchVectorAsync(
            collectionName,
            It.Is<float[]>(e => e.SequenceEqual(queryEmbedding)),
            It.IsAny<int>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _semanticKernelService.SearchMemoryAsync(collectionName, query);

        // Assert
        Assert.Single(result);
        Assert.Equal("doc1", result.First().Id);
    }

    [Fact]
    public async Task SearchMemoryAsync_WithMinRelevanceScore_FiltersResults()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "test query";
        var queryEmbedding = new float[] { 0.5f, 0.6f };
        var searchResult = new List<VectorSearchResult>
        {
            new VectorSearchResult { Id = "doc1", Content = "content1", Score = 0.9f },
            new VectorSearchResult { Id = "doc2", Content = "content2", Score = 0.6f }
        };

        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(query, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(queryEmbedding));
        _mockVectorDatabase.Setup(v => v.SearchVectorAsync(
            collectionName,
            It.Is<float[]>(e => e.SequenceEqual(queryEmbedding)),
            It.IsAny<int>(),
            0.7,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult.Where(r => r.Score >= 0.7));

        // Act
        var result = await _semanticKernelService.SearchMemoryAsync(collectionName, query, minRelevanceScore: 0.7);

        // Assert
        Assert.Single(result);
        Assert.Equal("doc1", result.First().Id);
    }

    [Fact]
    public async Task SearchMemoryAsync_EmptyQuery_ThrowsArgumentException()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _semanticKernelService.SearchMemoryAsync(collectionName, query));
    }

    [Fact]
    public async Task SearchMemoryAsync_EmbeddingServiceFails_ThrowsException()
    {
        // Arrange
        var collectionName = "testCollection";
        var query = "test query";
        _mockEmbeddingService.Setup(s => s.GenerateEmbeddingAsync(query, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.SearchMemoryAsync(collectionName, query));
    }

    [Fact]
    public async Task DeleteMemoryAsync_ValidInput_CallsVectorDatabaseService()
    {
        // Arrange
        var collectionName = "testCollection";
        var id = "testId";

        // Act
        await _semanticKernelService.DeleteMemoryAsync(collectionName, id);

        // Assert
        _mockVectorDatabase.Verify(v => v.DeleteVectorAsync(collectionName, id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMemoryAsync_VectorDatabaseFails_ThrowsException()
    {
        // Arrange
        var collectionName = "testCollection";
        var id = "testId";
        _mockVectorDatabase.Setup(v => v.DeleteVectorAsync(collectionName, id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Vector database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _semanticKernelService.DeleteMemoryAsync(collectionName, id));
    }

    #endregion

}



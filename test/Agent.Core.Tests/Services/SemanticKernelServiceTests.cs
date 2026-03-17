namespace Agent.Core.Tests.Services;
public class SemanticKernelServiceTests
{
    private readonly Mock<ILlmChatService> _mockChatService;
    private readonly Mock<IEmbeddingAndMemoryService> _mockMemoryService;
    private readonly Mock<IPluginOrchestrationService> _mockPluginService;
    private readonly SemanticKernelService _service;

    public SemanticKernelServiceTests()
    {
        _mockChatService = new Mock<ILlmChatService>();
        _mockMemoryService = new Mock<IEmbeddingAndMemoryService>();
        _mockPluginService = new Mock<IPluginOrchestrationService>();
        _service = new SemanticKernelService(_mockChatService.Object, _mockMemoryService.Object, _mockPluginService.Object);
    }

    [Fact]
    public async Task GetChatCompletionAsync_ShouldDelegateToChatService()
    {
        var prompt = "Hello";
        var expected = "OK";
        _mockChatService.Setup(s => s.GetChatCompletionAsync(prompt, null)).ReturnsAsync(expected);

        var result = await _service.GetChatCompletionAsync(prompt);

        Assert.Equal(expected, result);
        _mockChatService.Verify(s => s.GetChatCompletionAsync(prompt, null), Times.Once);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ShouldDelegateToMemoryService()
    {
        var embedding = new[] { 0.1f, 0.2f };
        _mockMemoryService.Setup(s => s.GenerateEmbeddingAsync("x")).ReturnsAsync(embedding);

        var result = await _service.GenerateEmbeddingAsync("x");

        Assert.Equal(embedding, result);
        _mockMemoryService.Verify(s => s.GenerateEmbeddingAsync("x"), Times.Once);
    }

    [Fact]
    public async Task InvokeFunctionAsync_ShouldDelegateToPluginService()
    {
        _mockPluginService.Setup(s => s.InvokeFunctionAsync("P", "F", null, null, null)).ReturnsAsync("R");

        var result = await _service.InvokeFunctionAsync("P", "F");

        Assert.Equal("R", result);
        _mockPluginService.Verify(s => s.InvokeFunctionAsync("P", "F", null, null, null), Times.Once);
    }
}


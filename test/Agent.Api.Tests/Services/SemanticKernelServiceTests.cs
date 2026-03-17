namespace Agent.Api.Tests.Services;

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
    public async Task GetChatCompletionAsync_DelegatesToChatService()
    {
        var prompt = "Hello";
        _mockChatService.Setup(s => s.GetChatCompletionAsync(prompt, null)).ReturnsAsync("OK");

        var result = await _service.GetChatCompletionAsync(prompt);

        Assert.Equal("OK", result);
        _mockChatService.Verify(s => s.GetChatCompletionAsync(prompt, null), Times.Once);
    }
}

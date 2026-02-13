namespace Agent.Api.Tests.Controllers;

/// <summary>
/// SemanticKernelController的单元测试
/// Unit tests for SemanticKernelController
/// </summary>
public class SemanticKernelControllerTests
{
    private readonly Mock<ISemanticKernelService> _mockService;
    private readonly Mock<ILogger<SemanticKernelController>> _mockLogger;
    private readonly SemanticKernelController _controller;

    public SemanticKernelControllerTests()
    {
        _mockService = new Mock<ISemanticKernelService>();
        var mockVectorDatabase = new Mock<IVectorDatabaseService>();
        _mockLogger = new Mock<ILogger<SemanticKernelController>>();
        _controller = new SemanticKernelController(_mockService.Object, mockVectorDatabase.Object, _mockLogger.Object);
    }

    /// <summary>
    /// 测试异常处理
    /// Test exception handling
    /// </summary>
    [Fact]
    public async Task GetChatCompletion_ThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockService.Setup(s => s.GetChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("Service failure"));

        // Act
        var result = await _controller.GetChatCompletion(new ChatCompletionRequest { Prompt = "test" });

        // Assert
        var actionResult = Assert.IsType<ActionResult<ApiResponse<ChatCompletionResponse>>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        // 验证日志是否记录了错误 - Verify that the error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to process chat completion request")),
                It.IsAny<System.Exception>(),
                It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }

    // TODO: 补充授权验证和输入验证的测试
    // TODO: Supplement tests for authorization verification and input validation
}


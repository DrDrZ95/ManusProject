using Agent.Api.Controllers;
using Agent.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Api.Tests.Controllers
{
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
            _mockLogger = new Mock<ILogger<SemanticKernelController>>();
            _controller = new SemanticKernelController(_mockService.Object, _mockLogger.Object);
        }

        /// <summary>
        /// 测试异常处理
        /// Test exception handling
        /// </summary>
        [Fact]
        public async Task ExecutePlanAsync_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockService.Setup(s => s.ExecutePlanAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("Service failure"));

            // Act
            var result = await _controller.ExecutePlanAsync("test-plan", "test-prompt", CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            // 验证日志是否记录了错误 - Verify that the error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Service failure")),
                    It.IsAny<System.Exception>(),
                    It.Is<Func<It.IsAnyType, System.Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // TODO: 补充授权验证和输入验证的测试
        // TODO: Supplement tests for authorization verification and input validation
    }
}


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
    /// SandboxTerminalController的单元测试
    /// Unit tests for SandboxTerminalController
    /// </summary>
    public class SandboxTerminalControllerTests
    {
        private readonly Mock<ISandboxService> _mockService;
        private readonly Mock<ILogger<SandboxTerminalController>> _mockLogger;
        private readonly SandboxTerminalController _controller;

        public SandboxTerminalControllerTests()
        {
            _mockService = new Mock<ISandboxService>();
            _mockLogger = new Mock<ILogger<SandboxTerminalController>>();
            _controller = new SandboxTerminalController(_mockService.Object, _mockLogger.Object);
        }

        /// <summary>
        /// 测试异常处理
        /// Test exception handling
        /// </summary>
        [Fact]
        public async Task ExecuteCommand_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockService.Setup(s => s.ExecuteCommandAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("Sandbox service error"));

            // Act
            var result = await _controller.ExecuteCommand("ls -l", CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            // 验证日志是否记录了错误 - Verify that the error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sandbox service error")),
                    It.IsAny<System.Exception>(),
                    It.Is<Func<It.IsAnyType, System.Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // TODO: 补充授权验证和输入验证的测试
        // TODO: Supplement tests for authorization verification and input validation
    }
}


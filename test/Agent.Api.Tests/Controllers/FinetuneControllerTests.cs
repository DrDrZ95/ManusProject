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
    /// FinetuneController的单元测试
    /// Unit tests for FinetuneController
    /// </summary>
    public class FinetuneControllerTests
    {
        private readonly Mock<IFinetuneService> _mockService;
        private readonly Mock<ILogger<FinetuneController>> _mockLogger;
        private readonly FinetuneController _controller;

        public FinetuneControllerTests()
        {
            _mockService = new Mock<IFinetuneService>();
            _mockLogger = new Mock<ILogger<FinetuneController>>();
            _controller = new FinetuneController(_mockService.Object, _mockLogger.Object);
        }

        /// <summary>
        /// 测试输入验证（例如，模型名称为空）
        /// Test input validation (e.g., empty model name)
        /// </summary>
        [Fact]
        public async Task StartFinetuneJob_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var request = new FinetuneController.StartFinetuneRequest { JobName = "test-job", ModelName = "" };

            // Act
            var result = await _controller.StartFinetuneJob(request, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("ModelName cannot be empty", badRequestResult.Value.ToString());
        }

        /// <summary>
        /// 测试异常处理
        /// Test exception handling
        /// </summary>
        [Fact]
        public async Task StartFinetuneJob_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new FinetuneController.StartFinetuneRequest { JobName = "test-job", ModelName = "gpt-3.5-turbo" };
            _mockService.Setup(s => s.StartFinetuneJobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("Finetune service failed"));

            // Act
            var result = await _controller.StartFinetuneJob(request, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            // 验证日志是否记录了错误 - Verify that the error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finetune service failed")),
                    It.IsAny<System.Exception>(),
                    It.Is<Func<It.IsAnyType, System.Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // TODO: 补充授权验证的测试
        // TODO: Supplement tests for authorization verification
    }
}


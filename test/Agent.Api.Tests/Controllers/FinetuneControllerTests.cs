
namespace Agent.Api.Tests.Controllers
{
    /// <summary>
    /// FinetuneController的单元测试 - 包含边界条件、并发和恢复测试
    /// Unit tests for FinetuneController - including boundary conditions, concurrency, and recovery tests
    /// </summary>
    public class FinetuneControllerTests
    {
        private readonly Mock<IPythonFinetuneService> _mockService;
        private readonly Mock<ILogger<FinetuneController>> _mockLogger;
        private readonly Mock<IAgentTelemetryProvider> _mockTelemetry;
        private readonly FinetuneController _controller;

        public FinetuneControllerTests()
        {
            _mockService = new Mock<IPythonFinetuneService>();
            _mockLogger = new Mock<ILogger<FinetuneController>>();
            _mockTelemetry = new Mock<IAgentTelemetryProvider>();

            // Mock telemetry span
            var mockSpan = new Mock<IAgentSpan>();
            _mockTelemetry.Setup(t => t.StartSpan(It.IsAny<string>(), It.IsAny<SpanKind>()))
                .Returns(mockSpan.Object);

            _controller = new FinetuneController(_mockService.Object, _mockLogger.Object, _mockTelemetry.Object);
        }

        /// <summary>
        /// 测试输入验证：任务名称为空
        /// Test input validation: empty job name
        /// </summary>
        [Fact]
        public async Task StartFinetune_EmptyJobName_ReturnsBadRequest()
        {
            // Arrange
            var request = new FinetuneRequest { JobName = "", DatasetPath = "data.json", OutputDir = "out" };

            // Act
            var result = await _controller.StartFinetune(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StartFinetuneResponse>>(badRequestResult.Value);
            Assert.Contains("Job name is required", apiResponse.Message);
        }

        /// <summary>
        /// 测试大文件上传/处理边界条件
        /// Test boundary conditions for large file processing
        /// </summary>
        [Fact]
        public async Task StartFinetune_LargeDatasetPath_ReturnsOk()
        {
            // Arrange
            var request = new FinetuneRequest
            {
                JobName = "large-file-job",
                DatasetPath = new string('a', 1024 * 10), // 模拟超长路径 - Simulate very long path
                OutputDir = "out"
            };
            _mockService.Setup(s => s.StartFinetuningAsync(It.IsAny<FinetuneRequest>()))
                .ReturnsAsync("job-123");

            // Act
            var result = await _controller.StartFinetune(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<StartFinetuneResponse>>(okResult.Value);
            Assert.Equal("job-123", apiResponse.Data.JobId);
        }

        /// <summary>
        /// 测试并发训练请求
        /// Test concurrent training requests
        /// </summary>
        [Fact]
        public async Task StartFinetune_ConcurrentRequests_HandledCorrectly()
        {
            // Arrange
            var request = new FinetuneRequest { JobName = "concurrent-job", DatasetPath = "data.json", OutputDir = "out" };
            _mockService.Setup(s => s.StartFinetuningAsync(It.IsAny<FinetuneRequest>()))
                .ReturnsAsync((FinetuneRequest r) => Guid.NewGuid().ToString());

            // Act
            var tasks = new List<Task<ActionResult<ApiResponse<StartFinetuneResponse>>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_controller.StartFinetune(request));
            }
            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result.Result);
            }
            _mockService.Verify(s => s.StartFinetuningAsync(It.IsAny<FinetuneRequest>()), Times.Exactly(10));
        }

        /// <summary>
        /// 测试训练中断与恢复：获取已存在的任务状态
        /// Test training interruption and recovery: getting status of an existing job
        /// </summary>
        [Fact]
        public async Task GetJobStatus_ExistingJob_ReturnsStatusForRecovery()
        {
            // Arrange
            var jobId = "job-recovery-123";
            var expectedRecord = new FinetuneRecordEntity
            {
                Id = jobId,
                JobName = "recovery-test",
                Status = FinetuneStatus.Running,
                Progress = 45,
                CurrentEpoch = 5,
                TotalEpochs = 10
            };
            _mockService.Setup(s => s.GetJobStatusAsync(jobId))
                .ReturnsAsync(expectedRecord);

            // Act
            var result = await _controller.GetJobStatus(jobId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<FinetuneJobStatusResponse>>(okResult.Value);
            var response = apiResponse.Data;
            Assert.Equal(jobId, response.JobId);
            Assert.Equal(45, response.Progress);
            Assert.Equal("Running", response.Status);
        }

        /// <summary>
        /// 测试异常处理：服务抛出异常
        /// Test exception handling: service throws exception
        /// </summary>
        [Fact]
        public async Task StartFinetune_ServiceThrows_ReturnsInternalServerError()
        {
            // Arrange
            var request = new FinetuneRequest { JobName = "fail-job", DatasetPath = "data.json", OutputDir = "out" };
            _mockService.Setup(s => s.StartFinetuningAsync(It.IsAny<FinetuneRequest>()))
                .ThrowsAsync(new Exception("GPU out of memory"));

            // Act
            var result = await _controller.StartFinetune(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse<StartFinetuneResponse>>(statusCodeResult.Value);
            Assert.Contains("GPU out of memory", apiResponse.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GPU out of memory")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}


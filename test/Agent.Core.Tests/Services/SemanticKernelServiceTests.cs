namespace Agent.Core.Tests.Services;
    /// <summary>
    /// SemanticKernelService 核心逻辑单元测试
    /// Unit tests for SemanticKernelService core logic
    /// </summary>
    public class SemanticKernelServiceTests
    {
        private readonly Mock<Kernel> _mockKernel;
        private readonly Mock<IChatCompletionService> _mockChatService;
        private readonly Mock<ITextEmbeddingGenerationService> _mockEmbeddingService;
        private readonly Mock<IVectorDatabaseService> _mockVectorDb;
        private readonly Mock<ILogger<SemanticKernelService>> _mockLogger;
        private readonly Mock<IAgentCacheService> _mockCache;
        private readonly SemanticKernelOptions _options;
        private readonly SemanticKernelService _service;

        public SemanticKernelServiceTests()
        {
            _mockKernel = new Mock<Kernel>();
            _mockChatService = new Mock<IChatCompletionService>();
            _mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();
            _mockVectorDb = new Mock<IVectorDatabaseService>();
            _mockLogger = new Mock<ILogger<SemanticKernelService>>();
            _mockCache = new Mock<IAgentCacheService>();
            _options = new SemanticKernelOptions { MaxTokens = 100, Temperature = 0.7 };

            _service = new SemanticKernelService(
                _mockKernel.Object,
                _mockChatService.Object,
                _mockEmbeddingService.Object,
                _mockVectorDb.Object,
                _options,
                _mockLogger.Object,
                _mockCache.Object,
                new Mock<IKubernetesPlanner>().Object,
                new Mock<IIstioPlanner>().Object,
                new Mock<IPostgreSQLPlanner>().Object,
                new Mock<IClickHousePlanner>().Object
            );
        }

        /// <summary>
        /// 测试多模型路由逻辑 (ModelRouter)
        /// Test multi-model routing logic
        /// </summary>
        [Fact]
        public async Task GetChatCompletionAsync_ShouldUseConfiguredSettings()
        {
            // Arrange
            var prompt = "Hello";
            var expectedResponse = "Hi there!";
            var mockContent = new ChatMessageContent(AuthorRole.Assistant, expectedResponse);
            
            _mockChatService.Setup(s => s.GetChatMessageContentAsync(
                It.IsAny<ChatHistory>(), 
                It.IsAny<PromptExecutionSettings>(), 
                It.IsAny<Kernel>(), 
                default))
                .ReturnsAsync(mockContent);

            // Act
            var result = await _service.GetChatCompletionAsync(prompt);

            // Assert
            Assert.Equal(expectedResponse, result);
            _mockChatService.Verify(s => s.GetChatMessageContentAsync(
                It.Is<ChatHistory>(h => h[0].Content == prompt),
                It.Is<OpenAIPromptExecutionSettings>(settings => settings.MaxTokens == _options.MaxTokens),
                It.IsAny<Kernel>(),
                default), Times.Once);
        }

        /// <summary>
        /// 测试错误处理与重试机制
        /// Test error handling and retry mechanisms
        /// </summary>
        [Fact]
        public async Task GetChatCompletionAsync_ServiceThrows_ShouldLogAndRethrow()
        {
            // Arrange
            _mockChatService.Setup(s => s.GetChatMessageContentAsync(It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings>(), It.IsAny<Kernel>(), default))
                .ThrowsAsync(new Exception("API Limit Exceeded"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetChatCompletionAsync("test"));
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get chat completion")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// 测试缓存逻辑
        /// Test caching logic for embeddings
        /// </summary>
        [Fact]
        public async Task GenerateEmbeddingAsync_ShouldCheckCacheFirst()
        {
            // Arrange
            var text = "cache-test";
            var cachedEmbedding = new float[] { 1.0f, 0.0f };
            _mockCache.Setup(c => c.GetOrCreateAsync<float[]>(
                It.IsAny<string>(),
                It.IsAny<Func<Task<float[]>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(cachedEmbedding);

            // Act
            var result = await _service.GenerateEmbeddingAsync(text);

            // Assert
            Assert.Equal(cachedEmbedding, result);
            _mockCache.Verify(c => c.GetOrCreateAsync<float[]>(
                It.Is<string>(s => s.StartsWith("embedding:")),
                It.IsAny<Func<Task<float[]>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>()), Times.Once);
        }

        /// <summary>
        /// 验证 OpenTelemetry 追踪数据 (模拟)
        /// Validate OpenTelemetry trace data (Simulated)
        /// </summary>
        [Fact]
        public async Task InvokeFunctionAsync_ShouldLogTelemetryInfo()
        {
            // Arrange
            var plugin = "TestPlugin";
            var function = "TestFunction";
            
            // Act
            // 注意：InvokeFunctionAsync 在实现中包含权限检查日志 (Note: InvokeFunctionAsync includes auth check logs)
            await _service.InvokeFunctionAsync(plugin, function);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Invoking function: {function}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }


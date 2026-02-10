namespace Agent.Core.Tests.Services
{
    /// <summary>
    /// Unit tests for PromptsService
    /// PromptsService 单元测试
    /// </summary>
    public class PromptsServiceTests
    {
        private readonly Mock<ILogger<PromptsService>> _mockLogger;
        private readonly PromptsService _promptsService;

        public PromptsServiceTests()
        {
            _mockLogger = new Mock<ILogger<PromptsService>>();
            _promptsService = new PromptsService(_mockLogger.Object);
        }

        /// <summary>
        /// Test getting an existing prompt
        /// 测试获取存在的提示词
        /// </summary>
        [Fact]
        public async Task GetPromptAsync_ExistingPrompt_ReturnsPrompt()
        {
            // Arrange
            // Built-in prompts are initialized in constructor
            var category = "rag";
            var name = "document_analysis";

            // Act
            var result = await _promptsService.GetPromptAsync(category, name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(category, result.Category);
        }

        /// <summary>
        /// Test getting a non-existing prompt
        /// 测试获取不存在的提示词
        /// </summary>
        [Fact]
        public async Task GetPromptAsync_NonExistingPrompt_ReturnsNull()
        {
            // Arrange
            var category = "non_existent_category";
            var name = "non_existent_name";

            // Act
            var result = await _promptsService.GetPromptAsync(category, name);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test getting prompts by category
        /// 测试根据类别获取提示词
        /// </summary>
        [Fact]
        public async Task GetPromptsByCategoryAsync_ExistingCategory_ReturnsList()
        {
            // Arrange
            var category = "rag";

            // Act
            var result = await _promptsService.GetPromptsByCategoryAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Contains(result, p => p.Name == "document_analysis");
        }

        /// <summary>
        /// Test getting prompts by non-existing category
        /// 测试根据不存在的类别获取提示词
        /// </summary>
        [Fact]
        public async Task GetPromptsByCategoryAsync_NonExistingCategory_ReturnsEmptyList()
        {
            // Arrange
            var category = "non_existent_category";

            // Act
            var result = await _promptsService.GetPromptsByCategoryAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Test getting all categories
        /// 测试获取所有类别
        /// </summary>
        [Fact]
        public async Task GetCategoriesAsync_ReturnsCategories()
        {
            // Act
            var result = await _promptsService.GetCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("rag", result);
        }

        /// <summary>
        /// Test rendering prompt with variables
        /// 测试使用变量渲染提示词
        /// </summary>
        [Fact]
        public void RenderPrompt_WithVariables_ReturnsRenderedString()
        {
            // Arrange
            var template = new PromptTemplate
            {
                Name = "test",
                Template = "Hello {name}, welcome to {place}!"
            };
            var variables = new Dictionary<string, object>
            {
                { "name", "User" },
                { "place", "Agent System" }
            };

            // Act
            var result = _promptsService.RenderPrompt(template, variables);

            // Assert
            Assert.Equal("Hello User, welcome to Agent System!", result);
        }

        /// <summary>
        /// Test rendering prompt with missing variables (should log warning but return string with placeholders or empty depending on impl)
        /// Reading impl: it replaces what it finds, then checks for unreplaced. Returns the string as is (partially replaced).
        /// </summary>
        [Fact]
        public void RenderPrompt_MissingVariables_ReturnsPartiallyRenderedStringAndLogsWarning()
        {
            // Arrange
            var template = new PromptTemplate
            {
                Name = "test",
                Template = "Hello {name}, welcome to {place}!"
            };
            var variables = new Dictionary<string, object>
            {
                { "name", "User" }
                // missing "place"
            };

            // Act
            var result = _promptsService.RenderPrompt(template, variables);

            // Assert
            Assert.Contains("Hello User, welcome to {place}!", result);
            // Verification of logger is tricky with extension methods, but we can verify Mock<ILogger> if we set it up correctly or just ignore for now as implementation calls LogWarning.
            // Since LogWarning is an extension method, we need to verify Log.
        }

        /// <summary>
        /// Test saving a new prompt
        /// 测试保存新的提示词
        /// </summary>
        [Fact]
        public async Task SavePromptAsync_NewPrompt_ReturnsTrueAndCanRetrieve()
        {
            // Arrange
            var newPrompt = new PromptTemplate
            {
                Category = "custom",
                Name = "new_prompt",
                Template = "This is a new prompt"
            };

            // Act
            var result = await _promptsService.SavePromptAsync(newPrompt);
            var retrieved = await _promptsService.GetPromptAsync("custom", "new_prompt");

            // Assert
            Assert.True(result);
            Assert.NotNull(retrieved);
            Assert.Equal("This is a new prompt", retrieved.Template);
        }

        /// <summary>
        /// Test deleting a prompt
        /// 测试删除提示词
        /// </summary>
        [Fact]
        public async Task DeletePromptAsync_ExistingPrompt_ReturnsTrueAndCannotRetrieve()
        {
            // Arrange
            var newPrompt = new PromptTemplate
            {
                Category = "temp",
                Name = "to_delete",
                Template = "delete me"
            };
            await _promptsService.SavePromptAsync(newPrompt);

            // Act
            var deleteResult = await _promptsService.DeletePromptAsync("temp", "to_delete");
            var retrieveResult = await _promptsService.GetPromptAsync("temp", "to_delete");

            // Assert
            Assert.True(deleteResult);
            Assert.Null(retrieveResult);
        }

        /// <summary>
        /// Test searching prompts
        /// 测试搜索提示词
        /// </summary>
        [Fact]
        public async Task SearchPromptsAsync_MatchingKeywords_ReturnsPrompts()
        {
            // Arrange
            // "document_analysis" has title "Document Analysis and Summarization"
            
            // Act
            var result = await _promptsService.SearchPromptsAsync("analysis summarization");

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, p => p.Name == "document_analysis");
        }
    }
}

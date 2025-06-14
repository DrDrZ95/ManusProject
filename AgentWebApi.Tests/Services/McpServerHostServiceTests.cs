using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;
using AgentWebApi.Services;

namespace AgentWebApi.Tests.Services
{
    public class McpServerHostServiceTests
    {
        private readonly Mock<ILogger<McpServerHostService>> _loggerMock;
        private readonly McpServerHostService _service;

        public McpServerHostServiceTests()
        {
            _loggerMock = new Mock<ILogger<McpServerHostService>>();
            _service = new McpServerHostService(_loggerMock.Object);
        }

        [Fact]
        public void CreateMcpServerHost_ShouldReturnHostBuilder()
        {
            // Arrange
            string[] args = [];

            // Act
            var result = _service.CreateMcpServerHost(args);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IHostBuilder>(result);
        }

        [Fact(Skip = "Integration test that requires actual server startup")]
        public async Task RunMcpServerHostAsync_ShouldNotThrow()
        {
            // Arrange
            var mockHostBuilder = new Mock<IHostBuilder>();
            var mockHost = new Mock<IHost>();
            
            mockHostBuilder
                .Setup(b => b.Build())
                .Returns(mockHost.Object);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => 
                _service.RunMcpServerHostAsync(mockHostBuilder.Object));
            
            Assert.Null(exception);
        }
    }
}

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using Moq;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Agent.Core.McpTools;

namespace Agent.Api.Tests.McpTools;

public class McpServerExamplesTests
{
    private readonly Mock<ILogger<McpServerExamples>> _loggerMock;
    private readonly McpServerExamples _serverExamples;

    public McpServerExamplesTests()
    {
        _loggerMock = new Mock<ILogger<McpServerExamples>>();
        _serverExamples = new McpServerExamples(_loggerMock.Object);
    }

    [Fact]
    public void Echo_ShouldReturnFormattedMessage()
    {
        // Arrange
        string message = "Test Message";
        string expected = $"Echo: {message}";

        // Act
        string result = _serverExamples.Echo(message);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact(Skip = "Requires actual HTTP client and MCP server")]
    public async Task SummarizeDownloadedContent_ShouldReturnSummary()
    {
        // Arrange
        var mockServer = new Mock<IMcpServer>();
        var mockHttpClient = new Mock<HttpClient>();
        string url = "https://example.com";
        string content = "This is a test content that needs to be summarized.";
        string summary = "Test content summary.";
        
        // This test is skipped because it requires actual HTTP client and MCP server
        // In a real test environment, you would mock these dependencies properly

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            _serverExamples.SummarizeDownloadedContent(
                mockServer.Object, 
                mockHttpClient.Object, 
                url, 
                CancellationToken.None));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Summarize_ShouldReturnChatMessage()
    {
        // Arrange
        string content = "Test content to summarize";
        string expectedPrompt = $"Please summarize this content into a single sentence: {content}";

        // Act
        var result = McpServerPrompts.Summarize(content);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ChatRole.User, result.Role);
        Assert.Equal(expectedPrompt, result.Content);
    }
}

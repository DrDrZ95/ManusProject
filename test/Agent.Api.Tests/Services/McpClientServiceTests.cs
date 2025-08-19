using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Agent.Core.Services;

namespace Agent.Api.Tests.Services;

public class McpClientServiceTests
{
    private readonly Mock<ILogger<McpClientService>> _loggerMock;
    private readonly McpClientService _service;

    public McpClientServiceTests()
    {
        _loggerMock = new Mock<ILogger<McpClientService>>();
        _service = new McpClientService(_loggerMock.Object);
    }

    [Fact(Skip = "Requires actual MCP server process")]
    public async Task CreateStdioClientAsync_ShouldCreateClient()
    {
        // Arrange
        string name = "TestClient";
        string command = "echo";
        string[] arguments = ["Hello MCP!"];

        // Act & Assert
        // This test is skipped because it requires an actual MCP server process
        // In a real environment, you would use a test server or mock the transport
        var exception = await Record.ExceptionAsync(() => 
            _service.CreateStdioClientAsync(name, command, arguments));
        
        Assert.Null(exception);
    }

    [Fact]
    public async Task ListToolsAsync_ShouldReturnTools()
    {
        // Arrange
        var mockClient = new Mock<IMcpClient>();
        var expectedTools = new List<McpClientTool>
        {
            new McpClientTool
            {
                Name = "echo",
                Description = "Echoes the input back to the client"
            },
            new McpClientTool
            {
                Name = "summarize",
                Description = "Summarizes the provided content"
            }
        };

        mockClient.Setup(c => c.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTools);

        // Act
        var result = await _service.ListToolsAsync(mockClient.Object);

        // Assert
        Assert.Equal(expectedTools.Count, result.Count);
        Assert.Equal(expectedTools[0].Name, result[0].Name);
        Assert.Equal(expectedTools[1].Name, result[1].Name);
        mockClient.Verify(c => c.ListToolsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CallToolAsync_ShouldReturnResponse()
    {
        // Arrange
        var mockClient = new Mock<IMcpClient>();
        string toolName = "echo";
        var parameters = new Dictionary<string, object?>
        {
            ["message"] = "Hello MCP!"
        };

        var expectedResponse = new CallToolResponse
        {
            Content = new List<Content>
            {
                new Content { Type = "text", Text = "Echo: Hello MCP!" }
            }
        };

        mockClient.Setup(c => c.CallToolAsync(
                It.Is<string>(s => s == toolName),
                It.Is<Dictionary<string, object?>>(d => d.ContainsKey("message")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.CallToolAsync(mockClient.Object, toolName, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Content);
        Assert.Equal("text", result.Content[0].Type);
        Assert.Equal("Echo: Hello MCP!", result.Content[0].Text);
        mockClient.Verify(c => c.CallToolAsync(
            It.Is<string>(s => s == toolName),
            It.Is<Dictionary<string, object?>>(d => d.ContainsKey("message")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

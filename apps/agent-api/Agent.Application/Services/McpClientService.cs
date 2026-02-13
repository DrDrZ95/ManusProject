namespace Agent.Application.Services;

/// <summary>
/// Service for interacting with MCP clients.
/// </summary>
public class McpClientService : IMcpClientService
{
    private readonly ILogger<McpClientService> _logger;

    public McpClientService(ILogger<McpClientService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates an MCP client using stdio transport.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="arguments">The arguments for the command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An MCP client.</returns>
    public async Task<IMcpClient> CreateStdioClientAsync(
        string name,
        string command,
        string[] arguments,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating MCP client with name: {Name}, command: {Command}", name, command);

        var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = name,
            Command = command,
            Arguments = arguments,
        });

        var client = await McpClientFactory.CreateAsync(clientTransport, cancellationToken: cancellationToken);

        _logger.LogInformation("MCP client created successfully");

        return client;
    }

    /// <summary>
    /// Lists all tools available from an MCP client.
    /// </summary>
    /// <param name="client">The MCP client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of MCP client tools.</returns>
    public async Task<IList<McpClientTool>> ListToolsAsync(
        IMcpClient client,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing tools from MCP client");

        var tools = await client.ListToolsAsync();

        _logger.LogInformation("Found {Count} tools from MCP client", tools.Count);

        return tools;
    }

    /// <summary>
    /// Calls a tool on an MCP client.
    /// </summary>
    /// <param name="client">The MCP client.</param>
    /// <param name="toolName">The name of the tool to call.</param>
    /// <param name="parameters">The parameters for the tool.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the tool call.</returns>
    public async ValueTask<CallToolResult> CallToolAsync(
        IMcpClient client,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling tool {ToolName} on MCP client", toolName);

        var result = await client.CallToolAsync(
            toolName,
            parameters,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Tool {ToolName} called successfully", toolName);

        return result;
    }
}

/// <summary>
/// Interface for MCP client service.
/// </summary>
public interface IMcpClientService
{
    Task<IMcpClient> CreateStdioClientAsync(
        string name,
        string command,
        string[] arguments,
        CancellationToken cancellationToken = default);

    Task<IList<McpClientTool>> ListToolsAsync(
        IMcpClient client,
        CancellationToken cancellationToken = default);

    ValueTask<CallToolResult> CallToolAsync(
        IMcpClient client,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}

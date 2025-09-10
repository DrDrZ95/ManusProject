using System.Threading.Tasks;

namespace Agent.McpGateway
{
    public interface IMcpClient
    {
        // Generic method for MCP interaction
        Task<string> InteractAsync(string request);
    }
}


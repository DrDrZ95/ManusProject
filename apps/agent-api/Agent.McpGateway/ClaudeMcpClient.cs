using System.Threading.Tasks;

namespace Agent.McpGateway
{
    public class ClaudeMcpClient : IMcpClient
    {
        public async Task<string> InteractAsync(string request)
        {
            // Placeholder for Claude MCP interaction logic
            return await Task.FromResult($"Claude MCP response for: {request}");
        }
    }
}


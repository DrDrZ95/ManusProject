using System.Threading.Tasks;

namespace Agent.McpGateway
{
    public class ChromeMcpClient : IMcpClient
    {
        public async Task<string> InteractAsync(string request)
        {
            // Placeholder for Chrome MCP interaction logic
            return await Task.FromResult($"Chrome MCP response for: {request}");
        }
    }
}


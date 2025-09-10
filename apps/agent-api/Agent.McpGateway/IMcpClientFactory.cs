using System.Threading.Tasks;

namespace Agent.McpGateway
{
    public interface IMcpClientFactory
    {
        IMcpClient CreateClaudeClient();
        IMcpClient CreateChromeClient();
        IMcpClient CreateGitHubClient();
        IMcpClient CreatePostgreSqlClient();
    }
}


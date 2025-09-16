using System;

namespace Agent.McpGateway
{
    public class McpClientFactory : IMcpClientFactory
    {
        public IMcpClient CreateClient(string clientType)
        {
            switch (clientType.ToLower())
            {
                case "claude":
                    return new ClaudeMcpClient();
                case "chrome":
                    return new ChromeMcpClient();
                case "github":
                    return new GitHubMcpClient();
                case "postgresql":
                    return new PostgreSqlClient();
                default:
                    throw new ArgumentException($"Unknown client type: {clientType}");
            }
        }
    }
}


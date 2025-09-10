namespace Agent.McpGateway
{
    public class McpClientFactory : IMcpClientFactory
    {
        public IMcpClient CreateClaudeClient()
        {
            return new ClaudeMcpClient();
        }

        public IMcpClient CreateChromeClient()
        {
            return new ChromeMcpClient();
        }

        public IMcpClient CreateGitHubClient()
        {
            return new GitHubMcpClient();
        }

        public IMcpClient CreatePostgreSqlClient()
        {
            return new PostgreSqlClient();
        }
    }
}


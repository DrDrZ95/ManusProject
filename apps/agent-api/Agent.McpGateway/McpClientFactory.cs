
namespace Agent.McpGateway;
/// <summary>
/// MCP 客户端工厂实现
/// MCP Client Factory Implementation
/// </summary>
public class McpClientFactory : IMcpClientFactory
{
    /// <summary>
    /// 创建指定类型的 MCP 客户端
    /// Creates an MCP client of the specified type
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="clientType">客户端类型 (e.g., "Chrome", "GitHub", "Claude", "PostgreSQL")</param>
    /// <returns>MCP 客户端实例</returns>
    public IMcpClient<TEntity> CreateClient<TEntity>(string clientType) where TEntity : IMcpEntity
    {
        switch (clientType.ToLower())
        {
            case "claude":
                // 确保 ClaudeMcpClient 继承自 McpBaseClient<ClaudeEntity>
                return (IMcpClient<TEntity>)(object)new ClaudeMcpClient();
            case "chrome":
                // 确保 ChromeMcpClient 继承自 McpBaseClient<ChromeEntity>
                return (IMcpClient<TEntity>)(object)new ChromeMcpClient();
            case "github":
                // 确保 GitHubMcpClient 继承自 McpBaseClient<GitHubEntity>
                return (IMcpClient<TEntity>)(object)new GitHubMcpClient();
            case "postgresql":
                // 确保 PostgreSqlClient 继承自 McpBaseClient<PostgreSqlEntity>
                return (IMcpClient<TEntity>)(object)new PostgreSqlClient();
            default:
                throw new ArgumentException($"Unknown client type: {clientType}");
        }
    }
}





namespace Agent.McpGateway
{
    /// <summary>
    /// MCP 客户端工厂接口
    /// MCP Client Factory Interface
    /// </summary>
    public interface IMcpClientFactory
    {
        /// <summary>
        /// 创建指定类型的 MCP 客户端
        /// Creates an MCP client of the specified type
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="clientType">客户端类型 (e.g., "Chrome", "GitHub", "Claude", "PostgreSQL")</param>
        /// <returns>MCP 客户端实例</returns>
        IMcpClient<TEntity> CreateClient<TEntity>(string clientType) where TEntity : IMcpEntity;
    }
}


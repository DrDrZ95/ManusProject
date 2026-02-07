
namespace Agent.McpGateway;
    /// <summary>
    /// Claude MCP 客户端接口
    /// Claude MCP Client Interface
    /// </summary>
    public interface IClaudeMcpClient : IMcpClient<ClaudeEntity>
    {
        /// <summary>
        /// 启动一个新的 Claude 线程
        /// Starts a new Claude thread
        /// </summary>
        /// <param name="initialMessage">初始消息</param>
        /// <returns>线程 ID</returns>
        Task<string> StartThread(string initialMessage);

        /// <summary>
        /// 继续一个 Claude 线程
        /// Continues a Claude thread
        /// </summary>
        /// <param name="threadId">线程 ID</param>
        /// <param name="newMessage">新消息</param>
        /// <returns>响应消息</returns>
        Task<string> ContinueThread(string threadId, string newMessage);

        /// <summary>
        /// 获取线程消息
        /// Gets thread messages
        /// </summary>
        /// <param name="threadId">线程 ID</param>
        /// <returns>消息列表</returns>
        Task<string> GetThreadMessages(string threadId);
    }



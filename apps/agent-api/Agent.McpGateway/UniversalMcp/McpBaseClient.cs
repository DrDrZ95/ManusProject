
using System.Threading.Tasks;

namespace Agent.McpGateway
{
    /// <summary>
    /// 通用 MCP 客户端基类
    /// Universal MCP Client Base Class
    /// </summary>
    /// <typeparam name="TEntity">MCP 实体的类型</typeparam>
    public abstract class McpBaseClient<TEntity> : IMcpClient<TEntity>
        where TEntity : IMcpEntity
    {
        public string ClientName { get; protected set; }

        protected McpBaseClient(string clientName)
        {
            ClientName = clientName;
        }

        /// <summary>
        /// 执行 JSON-RPC 请求
        /// Executes a JSON-RPC request
        /// </summary>
        /// <param name="method">JSON-RPC 方法名</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>JSON-RPC 响应</returns>
        public abstract Task<string> ExecuteJsonRpc(string method, object parameters);

        /// <summary>
        /// 获取所有实体
        /// Gets all entities
        /// </summary>
        /// <returns>实体列表</returns>
        public abstract Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// 根据 ID 获取实体
        /// Gets an entity by ID
        /// </summary>
        /// <param name="id">实体 ID</param>
        /// <returns>实体</returns>
        public abstract Task<TEntity> GetByIdAsync(string id);

        /// <summary>
        /// 创建实体
        /// Creates an entity
        /// </summary>
        /// <param name="entity">要创建的实体</param>
        /// <returns>创建后的实体</returns>
        public abstract Task<TEntity> CreateAsync(TEntity entity);

        /// <summary>
        /// 更新实体
        /// Updates an entity
        /// </summary>
        /// <param name="entity">要更新的实体</param>
        /// <returns>更新后的实体</returns>
        public abstract Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// 删除实体
        /// Deletes an entity
        /// </summary>
        /// <param name="id">要删除的实体 ID</param>
        /// <returns>操作结果</returns>
        public abstract Task<bool> DeleteAsync(string id);
    }
}


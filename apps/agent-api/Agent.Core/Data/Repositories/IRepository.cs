using System.Linq.Expressions;

namespace Agent.Core.Data.Repositories;

/// <summary>
/// Generic repository interface for data access
/// 通用仓储接口，用于数据访问
/// 
/// 提供标准的CRUD操作和查询功能
/// Provides standard CRUD operations and query functionality
/// </summary>
/// <typeparam name="TEntity">Entity type - 实体类型</typeparam>
/// <typeparam name="TKey">Primary key type - 主键类型</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>
    /// Get entity by ID - 根据ID获取实体
    /// </summary>
    /// <param name="id">Entity ID - 实体ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Entity or null - 实体或null</returns>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entity by ID with includes - 根据ID获取实体（包含关联数据）
    /// </summary>
    /// <param name="id">Entity ID - 实体ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <param name="includes">Navigation properties to include - 要包含的导航属性</param>
    /// <returns>Entity or null - 实体或null</returns>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get all entities - 获取所有实体
    /// </summary>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>List of entities - 实体列表</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities by predicate - 根据条件查找实体
    /// </summary>
    /// <param name="predicate">Search predicate - 搜索条件</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>List of matching entities - 匹配的实体列表</returns>
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities by predicate with includes - 根据条件查找实体（包含关联数据）
    /// </summary>
    /// <param name="predicate">Search predicate - 搜索条件</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <param name="includes">Navigation properties to include - 要包含的导航属性</param>
    /// <returns>List of matching entities - 匹配的实体列表</returns>
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get first entity matching predicate - 获取第一个匹配条件的实体
    /// </summary>
    /// <param name="predicate">Search predicate - 搜索条件</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>First matching entity or null - 第一个匹配的实体或null</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new entity - 添加新实体
    /// </summary>
    /// <param name="entity">Entity to add - 要添加的实体</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Added entity - 添加的实体</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities - 添加多个实体
    /// </summary>
    /// <param name="entities">Entities to add - 要添加的实体</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Task - 任务</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity - 更新实体
    /// </summary>
    /// <param name="entity">Entity to update - 要更新的实体</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Updated entity - 更新的实体</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity by ID - 根据ID删除实体
    /// </summary>
    /// <param name="id">Entity ID - 实体ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity - 删除实体
    /// </summary>
    /// <param name="entity">Entity to delete - 要删除的实体</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if entity exists - 检查实体是否存在
    /// </summary>
    /// <param name="predicate">Search predicate - 搜索条件</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Existence status - 存在状态</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities - 计算实体数量
    /// </summary>
    /// <param name="predicate">Optional filter predicate - 可选的过滤条件</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Entity count - 实体数量</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paged results - 获取分页结果
    /// </summary>
    /// <param name="pageNumber">Page number (1-based) - 页码（从1开始）</param>
    /// <param name="pageSize">Page size - 页面大小</param>
    /// <param name="predicate">Optional filter predicate - 可选的过滤条件</param>
    /// <param name="orderBy">Optional ordering - 可选的排序</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Paged result - 分页结果</returns>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paged results with includes - 获取分页结果（包含关联数据）
    /// </summary>
    /// <param name="pageNumber">Page number (1-based) - 页码（从1开始）</param>
    /// <param name="pageSize">Page size - 页面大小</param>
    /// <param name="predicate">Optional filter predicate - 可选的过滤条件</param>
    /// <param name="orderBy">Optional ordering - 可选的排序</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <param name="includes">Navigation properties to include - 要包含的导航属性</param>
    /// <returns>Paged result - 分页结果</returns>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes);
}

/// <summary>
/// Paged result container
/// 分页结果容器
/// </summary>
/// <typeparam name="T">Item type - 项目类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Items in current page - 当前页的项目
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total item count - 总项目数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number - 当前页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size - 页面大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total page count - 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Has previous page - 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Has next page - 是否有下一页
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}


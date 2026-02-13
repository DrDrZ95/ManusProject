namespace Agent.Core.Data.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework Core
/// 使用Entity Framework Core的通用仓储实现
/// 
/// 提供标准的数据访问操作，支持PostgreSQL数据库
/// Provides standard data access operations with PostgreSQL database support
/// </summary>
/// <typeparam name="TEntity">Entity type - 实体类型</typeparam>
/// <typeparam name="TKey">Primary key type - 主键类型</typeparam>
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    protected readonly AgentDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<Repository<TEntity, TKey>> _logger;

    public Repository(AgentDbContext context, ILogger<Repository<TEntity, TKey>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get entity by ID - 根据ID获取实体
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting entity by ID: {Id}", id);
            return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by ID: {Id}", id);
            throw;
        }
    }

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all entities - 获取所有实体
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all entities of type {EntityType}", typeof(TEntity).Name);
            await Task.CompletedTask;
            return _dbSet.AsQueryable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Find entities by predicate - 根据条件查找实体
    /// </summary>
    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Finding entities with predicate for type {EntityType}", typeof(TEntity).Name);
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities with predicate for type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get first entity matching predicate - 获取第一个匹配条件的实体
    /// </summary>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting first entity with predicate for type {EntityType}", typeof(TEntity).Name);
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first entity with predicate for type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Add new entity - 添加新实体
    /// </summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding new entity of type {EntityType}", typeof(TEntity).Name);

            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added entity of type {EntityType}", typeof(TEntity).Name);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Add multiple entities - 添加多个实体
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            var entityList = entities.ToList();
            _logger.LogDebug("Adding {Count} entities of type {EntityType}", entityList.Count, typeof(TEntity).Name);

            await _dbSet.AddRangeAsync(entityList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added {Count} entities of type {EntityType}", entityList.Count, typeof(TEntity).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding multiple entities of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Update entity - 更新实体
    /// </summary>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating entity of type {EntityType}", typeof(TEntity).Name);

            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated entity of type {EntityType}", typeof(TEntity).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Delete entity by ID - 根据ID删除实体
    /// </summary>
    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting entity by ID: {Id}", id);

            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Entity with ID {Id} not found for deletion", id);
                return false;
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted entity with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity by ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete entity - 删除实体
    /// </summary>
    public virtual async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting entity of type {EntityType}", typeof(TEntity).Name);

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted entity of type {EntityType}", typeof(TEntity).Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Check if entity exists - 检查实体是否存在
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking entity existence for type {EntityType}", typeof(TEntity).Name);
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking entity existence for type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Count entities - 计算实体数量
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Counting entities of type {EntityType}", typeof(TEntity).Name);

            if (predicate == null)
            {
                return await _dbSet.CountAsync(cancellationToken);
            }

            return await _dbSet.CountAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Get paged results - 获取分页结果
    /// </summary>
    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting paged results for type {EntityType}, Page: {PageNumber}, Size: {PageSize}",
                typeof(TEntity).Name, pageNumber, pageSize);

            // 验证分页参数 - Validate paging parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 1000) pageSize = 1000; // 限制最大页面大小 - Limit maximum page size

            var query = _dbSet.AsQueryable();

            // 应用过滤条件 - Apply filter predicate
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // 获取总数 - Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // 应用排序 - Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // 应用分页 - Apply paging
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<TEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogDebug("Retrieved {ItemCount} items out of {TotalCount} for page {PageNumber}",
                items.Count, totalCount, pageNumber);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged results for type {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public Task<PagedResult<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// Specialized repository implementation for workflow plans
/// 工作流计划的专用仓储实现
/// </summary>
public class WorkflowPlanRepository : Repository<WorkflowPlanEntity, string>, IWorkflowPlanRepository
{
    public WorkflowPlanRepository(AgentDbContext context, ILogger<WorkflowPlanRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Get plans with their steps - 获取包含步骤的计划
    /// </summary>
    public async Task<List<WorkflowPlanEntity>> GetPlansWithStepsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all workflow plans with steps");

            // 注意：这里需要手动加载步骤，因为我们没有在实体中定义导航属性
            // Note: We need to manually load steps here since we don't have navigation properties in entities
            var plans = await _dbSet.ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} workflow plans", plans.Count);
            return plans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow plans with steps");
            throw;
        }
    }

    /// <summary>
    /// Get plan with steps by ID - 根据ID获取包含步骤的计划
    /// </summary>
    public async Task<WorkflowPlanEntity?> GetPlanWithStepsAsync(string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting workflow plan with steps by ID: {PlanId}", planId);

            var plan = await GetByIdAsync(planId, cancellationToken);

            if (plan != null)
            {
                _logger.LogInformation("Retrieved workflow plan with ID: {PlanId}", planId);
            }
            else
            {
                _logger.LogWarning("Workflow plan not found with ID: {PlanId}", planId);
            }

            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow plan with steps by ID: {PlanId}", planId);
            throw;
        }
    }

    /// <summary>
    /// Get entity by ID with includes - 根据ID获取实体（包含关联数据）
    /// </summary>
    public virtual async Task<WorkflowPlanEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default,
        params Expression<Func<WorkflowPlanEntity, object>>[] includes)
    {
        try
        {
            _logger.LogDebug("Getting entity by ID: {Id} with includes", id);
            var query = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // EF Core 6+ can use FindAsync with includes, but it's safer to use Where(id) for consistency
            // and to ensure includes are applied. We assume the entity has a single primary key.
            var parameter = Expression.Parameter(typeof(WorkflowPlanEntity), "e");
            var property = Expression.Property(parameter, "Id"); // Assuming the primary key property is named "Id"
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<WorkflowPlanEntity, bool>>(equal, parameter);

            return await query.FirstOrDefaultAsync(lambda, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by ID: {Id} with includes", id);
            throw;
        }
    }

    /// <summary>
    /// Find entities by predicate with includes - 根据条件查找实体（包含关联数据）
    /// </summary>
    public virtual async Task<List<WorkflowPlanEntity>> FindAsync(Expression<Func<WorkflowPlanEntity, bool>> predicate,
        CancellationToken cancellationToken = default, params Expression<Func<WorkflowPlanEntity, object>>[] includes)
    {
        try
        {
            _logger.LogDebug("Finding entities with predicate and includes for type {EntityType}",
                typeof(WorkflowPlanEntity).Name);
            var query = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.Where(predicate).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities with predicate and includes for type {EntityType}",
                typeof(WorkflowPlanEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Get paged results with includes - 获取分页结果（包含关联数据）
    /// </summary>
    public virtual async Task<PagedResult<WorkflowPlanEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<WorkflowPlanEntity, bool>>? predicate = null,
        Func<IQueryable<WorkflowPlanEntity>, IOrderedQueryable<WorkflowPlanEntity>>? orderBy = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<WorkflowPlanEntity, object>>[] includes)
    {
        try
        {
            _logger.LogDebug(
                "Getting paged results with includes for type {EntityType}, Page: {PageNumber}, Size: {PageSize}",
                typeof(WorkflowPlanEntity).Name, pageNumber, pageSize);

            // 验证分页参数 - Validate paging parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 1000) pageSize = 1000; // 限制最大页面大小 - Limit maximum page size

            var query = _dbSet.AsQueryable();

            // 应用关联数据 - Apply includes
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // 应用过滤条件 - Apply filter predicate
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // 获取总数 - Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // 应用排序 - Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // 应用分页 - Apply paging
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WorkflowPlanEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogDebug("Retrieved {ItemCount} items out of {TotalCount} for page {PageNumber}",
                items.Count, totalCount, pageNumber);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged results with includes for type {EntityType}",
                typeof(WorkflowPlanEntity).Name);
            throw;
        }
    }
}

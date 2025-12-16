namespace Agent.Api.Extensions;

/// <summary>
/// PostgreSQL database extensions for dependency injection
/// PostgreSQL数据库的依赖注入扩展
/// 
/// 提供可选的PostgreSQL数据库集成，支持EF Core和仓储模式
/// Provides optional PostgreSQL database integration with EF Core and Repository pattern
/// </summary>
public static class PostgreSqlExtensions
{
    /// <summary>
    /// Add PostgreSQL database services to the service collection
    /// 将PostgreSQL数据库服务添加到服务集合
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddPostgreSqlDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 获取连接字符串 - Get connection string
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL connection string not found. Please configure 'ConnectionStrings:PostgreSQL' in appsettings.json");
        }

        // 添加DbContext - Add DbContext
        services.AddDbContext<AgentDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // 启用重试机制 - Enable retry mechanism
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                
                // 设置命令超时 - Set command timeout
                npgsqlOptions.CommandTimeout(30);
            });

            // 开发环境启用敏感数据日志 - Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // 注册仓储服务 - Register repository services
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<IWorkflowPlanRepository, WorkflowPlanRepository>();

        // 添加健康检查 - Add health checks
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "database", "postgresql" });

        return services;
    }

    /// <summary>
    /// Add PostgreSQL database services with custom options
    /// 使用自定义选项添加PostgreSQL数据库服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="connectionString">Database connection string - 数据库连接字符串</param>
    /// <param name="configureOptions">Options configuration action - 选项配置操作</param>
    /// <returns>Service collection for chaining - 用于链式调用的服务集合</returns>
    public static IServiceCollection AddPostgreSqlDatabase(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
        }

        // 添加DbContext - Add DbContext
        services.AddDbContext<AgentDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // 启用重试机制 - Enable retry mechanism
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                
                // 设置命令超时 - Set command timeout
                npgsqlOptions.CommandTimeout(30);
            });

            // 应用自定义配置 - Apply custom configuration
            configureOptions?.Invoke(options);
        });

        // 注册仓储服务 - Register repository services
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<IWorkflowPlanRepository, WorkflowPlanRepository>();

        // 添加健康检查 - Add health checks
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "database", "postgresql" });

        return services;
    }

    /// <summary>
    /// Configure database for the application
    /// 为应用程序配置数据库
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <param name="isDevelopment">Is development environment - 是否为开发环境</param>
    /// <returns>Application builder for chaining - 用于链式调用的应用程序构建器</returns>
    public static IApplicationBuilder UsePostgreSqlDatabase(
        this IApplicationBuilder app,
        bool isDevelopment = false)
    {
        // 在开发环境中自动应用迁移 - Auto-apply migrations in development
        if (isDevelopment)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
            
            try
            {
                // 确保数据库已创建 - Ensure database is created
                context.Database.EnsureCreated();
                
                // 应用待处理的迁移 - Apply pending migrations
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AgentDbContext>>();
                logger.LogError(ex, "An error occurred while migrating the database");
                
                // 在开发环境中，我们可以选择继续运行而不是崩溃
                // In development, we can choose to continue running instead of crashing
                if (!isDevelopment)
                {
                    throw;
                }
            }
        }

        return app;
    }

    /// <summary>
    /// Seed initial data to the database
    /// 向数据库播种初始数据
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <returns>Application builder for chaining - 用于链式调用的应用程序构建器</returns>
    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AgentDbContext>>();

        try
        {
            // 检查是否需要播种数据 - Check if seeding is needed
            if (!context.WorkflowPlans.Any())
            {
                logger.LogInformation("Seeding initial workflow plan data");
                
                // 创建示例工作流计划 - Create sample workflow plan
                var samplePlan = new WorkflowPlanEntity
                {
                    Id = new Guid(),
                    Title = "AI-Agent 示例工作流",
                    Description = "这是一个示例工作流计划，展示AI-Agent系统的基本功能",
                    Metadata = """{"priority": "medium", "category": "sample"}""",
                    ExecutorKeys = """["system", "user"]""",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.WorkflowPlans.Add(samplePlan);

                // 创建示例步骤 - Create sample steps
                var sampleSteps = new[]
                {
                    new WorkflowStepEntity
                    {
                        PlanId = samplePlan.Id,
                        Text = "初始化AI-Agent系统",
                        Type = "setup",
                        Status = PlanStepStatus.Completed, // Completed
                        Result = "系统初始化完成",
                        StartedAt = DateTime.UtcNow.AddMinutes(-30),
                        CompletedAt = DateTime.UtcNow.AddMinutes(-25),
                        Metadata = """{"duration": "5 minutes"}"""
                    },
                    new WorkflowStepEntity
                    {
                        PlanId = samplePlan.Id,
                        Text = "配置数据库连接",
                        Type = "config",
                        Status = PlanStepStatus.InProgress, // InProgress
                        StartedAt = DateTime.UtcNow.AddMinutes(-25),
                        Metadata = """{"database": "postgresql"}"""
                    },
                    new WorkflowStepEntity
                    {
                        PlanId = samplePlan.Id,
                        Text = "测试API端点",
                        Type = "test",
                        Status = PlanStepStatus.NotStarted, // NotStarted
                        Metadata = """{"endpoints": ["workflow", "rag", "semantic-kernel"]}""",
                    }
                };

                context.WorkflowSteps.AddRange(sampleSteps);
                context.SaveChanges();

                logger.LogInformation("Successfully seeded initial data");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }

        return app;
    }
}

/// <summary>
/// PostgreSQL configuration options
/// PostgreSQL配置选项
/// </summary>
public class PostgreSqlOptions
{
    /// <summary>
    /// Connection string - 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Enable automatic migrations - 启用自动迁移
    /// </summary>
    public bool EnableAutoMigration { get; set; } = false;

    /// <summary>
    /// Enable sensitive data logging - 启用敏感数据日志
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Command timeout in seconds - 命令超时（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Maximum retry count - 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Maximum retry delay - 最大重试延迟
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}


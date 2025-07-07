using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgentWebApi.Identity;

namespace AgentWebApi.Extensions;

/// <summary>
/// IdentityServer4 extensions for dependency injection
/// IdentityServer4依赖注入扩展
/// 
/// 为AI-Agent系统提供身份验证和授权服务的注册
/// Provides service registration for authentication and authorization in AI-Agent system
/// </summary>
public static class IdentityServerExtensions
{
    /// <summary>
    /// Add IdentityServer4 services with RSA key generation
    /// 添加带RSA密钥生成的IdentityServer4服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddIdentityServerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加ASP.NET Core Identity - Add ASP.NET Core Identity
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // 密码策略配置 - Password policy configuration
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            
            // 用户策略配置 - User policy configuration
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // 开发环境设置 - Development setting
        })
        .AddEntityFrameworkStores<AgentDbContext>()
        .AddDefaultTokenProviders();

        // 获取RSA密钥路径 - Get RSA key path
        var keyPath = configuration.GetValue<string>("IdentityServer:RsaKeyPath") ?? 
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ai-agent", "keys", "rsa-key.pem");

        // 生成或加载RSA密钥 - Generate or load RSA key
        var signingCredentials = IdentityConfig.CreateOrLoadRsaKey(keyPath);

        // 配置IdentityServer4 - Configure IdentityServer4
        var identityServerBuilder = services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
            
            // 发行者URI配置 - Issuer URI configuration
            options.IssuerUri = configuration.GetValue<string>("IdentityServer:IssuerUri") ?? "https://localhost:5001";
            
            // 用户交互配置 - User interaction configuration
            options.UserInteraction.LoginUrl = "/Account/Login";
            options.UserInteraction.LogoutUrl = "/Account/Logout";
            options.UserInteraction.ErrorUrl = "/Home/Error";
        })
        .AddSigningCredential(signingCredentials) // 使用生成的RSA密钥 - Use generated RSA key
        .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
        .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
        .AddInMemoryApiResources(IdentityConfig.ApiResources)
        .AddInMemoryClients(IdentityConfig.Clients)
        .AddTestUsers(IdentityConfig.TestUsers) // 开发环境使用测试用户 - Use test users in development
        .AddAspNetIdentity<IdentityUser>();

        // 在生产环境中，可以使用EntityFramework存储 - In production, can use EntityFramework stores
        if (configuration.GetValue<bool>("IdentityServer:UseEntityFramework"))
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            identityServerBuilder
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
                    
                    // 自动清理令牌 - Automatic token cleanup
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 3600; // 1小时 - 1 hour
                });
        }

        // 添加JWT Bearer认证 - Add JWT Bearer authentication
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration.GetValue<string>("IdentityServer:Authority") ?? "https://localhost:5001";
                options.RequireHttpsMetadata = configuration.GetValue<bool>("IdentityServer:RequireHttpsMetadata", true);
                options.Audience = "ai-agent-api";
                
                // SignalR支持 - SignalR support
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // 从查询字符串获取访问令牌（SignalR需要） - Get access token from query string (required for SignalR)
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });

        // 添加授权策略 - Add authorization policies
        services.AddAuthorization(options =>
        {
            AuthorizationPolicies.ConfigurePolicies(options);
        });

        return services;
    }

    /// <summary>
    /// Configure IdentityServer4 middleware
    /// 配置IdentityServer4中间件
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <param name="env">Web host environment - Web主机环境</param>
    /// <returns>Application builder - 应用程序构建器</returns>
    public static IApplicationBuilder UseIdentityServerServices(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 在开发环境中初始化数据库 - Initialize database in development environment
        if (env.IsDevelopment())
        {
            InitializeDatabase(app);
        }

        // 使用IdentityServer4中间件 - Use IdentityServer4 middleware
        app.UseIdentityServer();
        
        // 使用认证和授权 - Use authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Initialize IdentityServer4 database
    /// 初始化IdentityServer4数据库
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        if (serviceScope == null) return;

        try
        {
            // 迁移配置数据库 - Migrate configuration database
            var configContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            configContext.Database.Migrate();

            // 检查是否需要种子数据 - Check if seed data is needed
            if (!configContext.Clients.Any())
            {
                // 添加客户端配置 - Add client configuration
                foreach (var client in IdentityConfig.Clients)
                {
                    configContext.Clients.Add(client.ToEntity());
                }

                // 添加身份资源 - Add identity resources
                foreach (var resource in IdentityConfig.IdentityResources)
                {
                    configContext.IdentityResources.Add(resource.ToEntity());
                }

                // 添加API范围 - Add API scopes
                foreach (var scope in IdentityConfig.ApiScopes)
                {
                    configContext.ApiScopes.Add(scope.ToEntity());
                }

                // 添加API资源 - Add API resources
                foreach (var resource in IdentityConfig.ApiResources)
                {
                    configContext.ApiResources.Add(resource.ToEntity());
                }

                configContext.SaveChanges();
            }

            // 迁移操作数据库 - Migrate operational database
            var persistedGrantContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            persistedGrantContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the IdentityServer4 database - 初始化IdentityServer4数据库时发生错误");
        }
    }

    /// <summary>
    /// Add health checks for IdentityServer4
    /// 为IdentityServer4添加健康检查
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddIdentityServerHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // 添加IdentityServer4健康检查 - Add IdentityServer4 health check
        healthChecksBuilder.AddCheck<IdentityServerHealthCheck>("identity_server", tags: new[] { "identity", "auth" });

        // 如果使用EntityFramework，添加数据库健康检查 - If using EntityFramework, add database health check
        if (configuration.GetValue<bool>("IdentityServer:UseEntityFramework"))
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                healthChecksBuilder.AddNpgSql(connectionString, name: "identity_database", tags: new[] { "database", "identity" });
            }
        }

        return services;
    }
}

/// <summary>
/// IdentityServer4 health check
/// IdentityServer4健康检查
/// </summary>
public class IdentityServerHealthCheck : IHealthCheck
{
    private readonly ILogger<IdentityServerHealthCheck> _logger;

    public IdentityServerHealthCheck(ILogger<IdentityServerHealthCheck> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check health status
    /// 检查健康状态
    /// </summary>
    /// <param name="context">Health check context - 健康检查上下文</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Health check result - 健康检查结果</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing IdentityServer4 health check - 执行IdentityServer4健康检查");

            // 检查RSA密钥是否存在 - Check if RSA key exists
            var keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ai-agent", "keys", "rsa-key.pem");
            var keyExists = File.Exists(keyPath);

            var data = new Dictionary<string, object>
            {
                ["rsa_key_exists"] = keyExists,
                ["key_path"] = keyPath,
                ["check_time"] = DateTime.UtcNow
            };

            if (keyExists)
            {
                _logger.LogInformation("IdentityServer4 health check passed - IdentityServer4健康检查通过");
                return HealthCheckResult.Healthy("IdentityServer4 is healthy", data);
            }
            else
            {
                _logger.LogWarning("IdentityServer4 health check warning: RSA key not found - IdentityServer4健康检查警告：未找到RSA密钥");
                return HealthCheckResult.Degraded("IdentityServer4 RSA key not found", data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IdentityServer4 health check error - IdentityServer4健康检查错误");
            
            return HealthCheckResult.Unhealthy(
                "IdentityServer4 health check failed with exception", 
                ex, 
                new Dictionary<string, object> { ["exception"] = ex.Message });
        }
    }
}


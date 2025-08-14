using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Agent.Core.Data;
using Agent.Core.Models.Identity;
using Agent.Core.Services.Authorization;
using Agent.Core.Authorization;

namespace Agent.Core.Extensions;

/// <summary>
/// Identity service extensions for dependency injection
/// Identity服务扩展，用于依赖注入
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// Add Identity services with PostgreSQL and JWT Bearer authentication
    /// 添加带有PostgreSQL和JWT Bearer认证的Identity服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add PostgreSQL DbContext
        // 添加PostgreSQL数据库上下文
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=Agent.ApiDb;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("Agent.Api");
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
            
            // Enable sensitive data logging in development
            // 在开发环境中启用敏感数据日志记录
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Add Identity services
        // 添加Identity服务
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            // 密码设置
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            // 锁定设置
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            // 用户设置
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Sign in settings
            // 登录设置
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Add JWT Bearer authentication
        // 添加JWT Bearer认证
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = jwtSettings["Issuer"] ?? "Agent.Api";
        var audience = jwtSettings["Audience"] ?? "Agent.ApiUsers";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            // Handle JWT events
            // 处理JWT事件
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError("Authentication failed: {Exception}", context.Exception);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("Token validated for user: {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Challenge: {Error} - {ErrorDescription}", context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });

        // Add permission-based authorization
        // 添加基于权限的授权
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, MultiplePermissionsAuthorizationHandler>();

        // Add permission service
        // 添加权限服务
        services.AddScoped<IPermissionService, PermissionService>();

        // Add CORS for frontend integration
        // 为前端集成添加CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Use Identity middleware
    /// 使用Identity中间件
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <returns>Application builder - 应用程序构建器</returns>
    public static IApplicationBuilder UseIdentityServices(this IApplicationBuilder app)
    {
        // Use CORS
        // 使用CORS
        app.UseCors("AllowFrontend");

        // Use authentication and authorization
        // 使用认证和授权
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Initialize Identity database and seed data
    /// 初始化Identity数据库并播种数据
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <returns>Application builder - 应用程序构建器</returns>
    public static async Task<IApplicationBuilder> InitializeIdentityDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            // Ensure database is created
            // 确保数据库已创建
            await context.Database.EnsureCreatedAsync();

            // Apply pending migrations
            // 应用待处理的迁移
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending database migrations...");
                await context.Database.MigrateAsync();
            }

            // Seed default admin user if not exists
            // 如果不存在，则播种默认管理员用户
            await SeedDefaultAdminUserAsync(userManager, roleManager, logger);

            logger.LogInformation("Identity database initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the Identity database");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Seed default admin user
    /// 播种默认管理员用户
    /// </summary>
    private static async Task SeedDefaultAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        const string adminEmail = "admin@agentwebapi.com";
        const string adminPassword = "Admin123!";
        const string adminRole = "Administrator";

        // Check if admin user already exists
        // 检查管理员用户是否已存在
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null)
        {
            logger.LogInformation("Default admin user already exists");
            return;
        }

        // Ensure admin role exists
        // 确保管理员角色存在
        var role = await roleManager.FindByNameAsync(adminRole);
        if (role == null)
        {
            logger.LogWarning("Administrator role not found in database");
            return;
        }

        // Create admin user
        // 创建管理员用户
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            Department = "IT",
            JobTitle = "System Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
            logger.LogInformation("Default admin user created successfully: {Email}", adminEmail);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to create default admin user: {Errors}", errors);
        }
    }
}


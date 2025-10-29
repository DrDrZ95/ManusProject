namespace Agent.Api.Extensions;

/// <summary>
/// Hangfire 扩展方法，用于配置和添加 Hangfire 服务
/// Hangfire extension methods for configuring and adding Hangfire services
/// </summary>
public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加 Hangfire 服务
        // Add Hangfire services
        services.AddHangfire(hangfireConfig => hangfireConfig
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            // 使用内存存储，生产环境应替换为持久化存储，如 SQL Server, PostgreSQL, Redis 等
            // Use in-memory storage; in production, replace with persistent storage like SQL Server, PostgreSQL, Redis, etc.
            .UseRedisStorage(configuration.GetSection("Redis:ConnectionString").Value));

        // 添加 Hangfire 服务器，用于处理后台任务
        // Add Hangfire server for processing background jobs
        services.AddHangfireServer();

        return services;
    }

    /// <summary>
    /// 配置 Hangfire Dashboard 中间件
    /// Configure Hangfire Dashboard middleware
    /// </summary>
    /// <param name="app"></param>
    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app)
    {
        // 启用 Hangfire Dashboard
        // Enable Hangfire Dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            // 可以在此处配置认证，例如只允许管理员访问
            // Authentication can be configured here, e.g., restrict access to administrators
            // Authorization = new [] { new HangfireAuthorizationFilter() }
        });

        // 示例：添加一个简单的后台任务
        // Example: Add a simple background job
        RecurringJob.AddOrUpdate(
            "EasyJob",
            () => System.Console.WriteLine("Hello from Hangfire!"),
            Cron.Minutely); // 每分钟执行一次 (Executes every minute)

        return app;
    }
}

// 示例：Hangfire Dashboard 授权过滤器
// Example: Hangfire Dashboard Authorization Filter
// public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
// {
//     public bool Authorize([NotNull] DashboardContext context)
//     // {
//         // 在此处添加您的认证逻辑
//         // Add your authentication logic here
//         // var httpContext = context.Get//HttpContext();
//         // return httpContext.User.IsInRole("Admin");
//         // return true;
//     }
// }


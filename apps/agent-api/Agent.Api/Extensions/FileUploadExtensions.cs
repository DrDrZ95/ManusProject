
namespace Agent.Api.Extensions;

/// <summary>
/// File upload service extensions for dependency injection
/// 文件上传服务扩展，用于依赖注入
/// </summary>
public static class FileUploadExtensions
{
    /// <summary>
    /// Add file upload services with OWASP security measures
    /// 添加具有OWASP安全措施的文件上传服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddFileUploadServices(this IServiceCollection services)
    {
        // Register file upload service
        // 注册文件上传服务
        services.AddScoped<IFileUploadService, FileUploadService>();

        // Configure request size limits (OWASP security measure)
        // 配置请求大小限制（OWASP安全措施）
        services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
        });

        // Configure form options for file uploads
        // 配置文件上传的表单选项
        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });

        return services;
    }

    /// <summary>
    /// Add file upload middleware configuration
    /// 添加文件上传中间件配置
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <returns>Application builder - 应用程序构建器</returns>
    public static IApplicationBuilder UseFileUploadSecurity(this IApplicationBuilder app)
    {
        // Add security headers for file uploads
        // 为文件上传添加安全头
        app.Use(async (context, next) =>
        {
            // Prevent MIME type sniffing (OWASP security measure)
            // 防止MIME类型嗅探（OWASP安全措施）
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            
            // Prevent clickjacking attacks
            // 防止点击劫持攻击
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            
            // Enable XSS protection
            // 启用XSS保护
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            
            await next();
        });

        return app;
    }
}


namespace Agent.Api.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI and Scalar documentation.
/// 配置 OpenAPI 和 Scalar 文档的扩展方法。
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds OpenAPI and Scalar services to the service collection.
    /// 向服务集合添加 OpenAPI 和 Scalar 服务。
    /// </summary>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        // In .NET 8, we continue to use Swashbuckle for metadata generation
        // as AddOpenApi is a .NET 9 feature. 
        // Swashbuckle is already configured in SwaggerExtensions.cs
        return services;
    }

    /// <summary>
    /// Maps the Scalar API documentation UI.
    /// 映射 Scalar API 文档 UI。
    /// </summary>
    public static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        // Map Scalar UI
        if (app is WebApplication webApp)
        {
            // 配置 Scalar 显式引用项目根目录生成的 openapi.json 文件
            // Configure Scalar to explicitly reference the openapi.json file generated in the project root
            webApp.MapScalarApiReference(options =>
            {
                options.WithTitle("AgentProject AI API Reference")
                       .WithTheme(ScalarTheme.Moon)
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                       // 显式指向由下方 MapGet("/openapi.json") 提供的路径
                       // Explicitly point to the path provided by MapGet("/openapi.json") below
                       .WithOpenApiRoutePattern("/openapi.json");
            });

            // 公开 openapi.json 路由，使其能够通过 http://localhost:5069/openapi.json 访问
            // Expose the openapi.json route so it can be accessed via http://localhost:5069/openapi.json
            webApp.MapGet("/openapi.json", async context =>
            {
                var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
                var filePath = Path.Combine(env.ContentRootPath, "openapi.json");
                
                if (File.Exists(filePath))
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.SendFileAsync(filePath);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("openapi.json not found. Please wait for the application to generate it on first run.");
                }
            });
        }

        return app;
    }

    /// <summary>
    /// Exports the OpenAPI document to a physical file.
    /// 将 OpenAPI 文档导出为物理文件。
    /// </summary>
    public static void ExportOpenApiDocument(this IHost app)
    {
        var environment = app.Services.GetRequiredService<IHostEnvironment>();
        // 仅在开发环境下自动导出，避免生产环境权限问题
        // Only export automatically in Development environment to avoid permission issues in production
        if (!environment.IsDevelopment()) return;

        // 使用 Task.Run 并在内部等待，或者直接在后台线程执行，避免阻塞主线程启动
        // Use Task.Run and wait inside, or execute on a background thread to avoid blocking main thread startup
        Task.Run(async () =>
        {
            try
            {
                // 等待应用完全启动，确保 Swashbuckle 能够获取到所有 Controller 的元数据
                // Wait for the app to be fully ready to ensure Swashbuckle can retrieve metadata for all Controllers
                await Task.Delay(5000);

                using var scope = app.Services.CreateScope();
                
                // 获取 Swagger 生成器和 API 版本描述提供者
                // Get Swagger generator and API version description provider
                var generator = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
                var provider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                
                // 获取最新的 API 版本（例如 v1）
                // Get the latest API version (e.g., v1)
                var latestVersion = provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion).FirstOrDefault();

                if (latestVersion != null)
                {
                    // 生成 Swagger 对象并序列化为 JSON 字符串
                    // Generate Swagger object and serialize to JSON string
                    var swagger = generator.GetSwagger(latestVersion.GroupName, null, "/");
                    var json = swagger.SerializeAsJson(Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
                    
                    // 将 JSON 写入项目根目录，以便于 Scalar 引用和前端消费
                    // Write JSON to the project root for Scalar reference and frontend consumption
                    var outputPath = Path.Combine(environment.ContentRootPath, "openapi.json");
                    await File.WriteAllTextAsync(outputPath, json);
                    
                    Console.WriteLine($"[OpenAPI] Successfully exported openapi.json to: {outputPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpenAPI] Error exporting OpenAPI document: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[OpenAPI] Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"[OpenAPI] Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }
            }
        });
    }

    /// <summary>
    /// Prints a beautified message to the console with the Scalar API reference URL.
    /// 向控制台打印带有 Scalar API 参考 URL 的美化消息。
    /// </summary>
    public static void PrintApiReferenceWelcome(this WebApplication app)
    {
        var originalColor = Console.ForegroundColor;
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================================");
        Console.WriteLine("🚀 AgentProject AI API is now running!");
        Console.WriteLine("--------------------------------------------------------------------------------");
        
        // Try to get the actual listening addresses
        var addresses = app.Urls;
        var primaryAddress = addresses.FirstOrDefault() ?? "http://localhost:5069";
        
        Console.WriteLine($"📝 Scalar API Reference: {primaryAddress.TrimEnd('/')}/scalar/v1");
        Console.WriteLine($"📂 OpenAPI Specification: {primaryAddress.TrimEnd('/')}/openapi.json");
        Console.WriteLine($"📜 Project Root Document: openapi.json (Exported automatically)");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine("Enjoy building with the AgentProject AI system!");
        Console.WriteLine("================================================================================");
        Console.WriteLine();
        
        Console.ForegroundColor = originalColor;
    }
}

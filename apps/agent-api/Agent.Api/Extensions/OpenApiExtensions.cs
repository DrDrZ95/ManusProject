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
            webApp.MapScalarApiReference();
        }

        return app;
    }

    /// <summary>
    /// Exports the OpenAPI document to a physical file.
    /// 将 OpenAPI 文档导出为物理文件。
    /// </summary>
    public static async Task ExportOpenApiDocumentAsync(this IHost app)
    {
        var environment = app.Services.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment()) return;

        try
        {
            // Wait for the app to be fully ready
            await Task.Delay(2000);

            using var scope = app.Services.CreateScope();
            // Since we're using Swashbuckle for versioned swagger.json and Microsoft.AspNetCore.OpenApi for /openapi/v1.json
            // We'll export the one from Swashbuckle as it's more complete for now with our versioning setup.
            var generator = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
            var provider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
            var latestVersion = provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion).FirstOrDefault();

            if (latestVersion != null)
            {
                var swagger = generator.GetSwagger(latestVersion.GroupName, null, "/");
                var json = swagger.SerializeAsJson(Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
                
                // Save to project root
                var outputPath = Path.Combine(environment.ContentRootPath, "openapi.json");
                await File.WriteAllTextAsync(outputPath, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting OpenAPI document: {ex.Message}");
        }
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

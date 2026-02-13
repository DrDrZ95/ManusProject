namespace Agent.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring API versioning.
/// 提供用于配置API版本控制的扩展方法。
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning services to the service collection.
    /// 将API版本控制服务添加到服务集合中。
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    /// <returns>The IServiceCollection instance for chaining.</returns>
    public static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        }).AddMvc().AddApiExplorer(options =>
        {
            // Add the versioned API explorer, which also adds IApiVersionDescriptionProvider service
            // 添加版本化的API资源管理器，它也添加了IApiVersionDescriptionProvider服务
            options.GroupNameFormat = "'v'VVV";

            // Note: this option is only necessary when versioning by URL segment.
            // When versioning by query string, header, or media type, then this option is not needed.
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }



    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = $"Agent.Api {description.ApiVersion}",
            Version = description.ApiVersion.ToString(),
            Description = "API for the AI Agent application. 应用程序的AI Agent API。",
            Contact = new OpenApiContact { Name = "Agent Team", Email = "help@agent.im" },
            License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
        };

        if (description.IsDeprecated)
        {
            info.Description += " (This API version has been deprecated. 该API版本已被弃用。)";
        }

        return info;
    }
}


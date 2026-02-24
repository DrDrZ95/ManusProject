namespace Agent.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        return services;
    }


    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        // ReDoc uses Swagger JSON generation, but we can keep the SwaggerUI registration 
        // code here for reference or future fallback.
        // ReDoc 使用 Swagger JSON 生成，但我们可以保留 SwaggerUI 注册代码在此作为参考或未来回退。
        app.UseSwagger(c =>
        {
            // Set the route template to use openapi.json instead of the default swagger.json
            // 设置路由模板，使用 openapi.json 替代默认的 swagger.json
            c.RouteTemplate = "swagger/{documentName}/openapi.json";
        });

        /*
        // SwaggerUI code is preserved but not called by default in favor of ReDoc.
        // SwaggerUI 代码已保留，但默认不调用，改为使用 ReDoc。
        app.UseSwaggerUI(c =>
        {
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                c.SwaggerEndpoint($"/swagger/{description.GroupName}/openapi.json", description.GroupName.ToUpperInvariant());
            }
            c.RoutePrefix = "swagger";
        });
        */

        return app;
    }


    /// <summary>
    /// Maps the OpenAPI/Swagger JSON endpoint to a fixed path at the root.
    /// 将 OpenAPI/Swagger JSON 端点映射到根目录下的固定路径。
    /// </summary>
    /// <param name="endpoints">The IEndpointRouteBuilder instance. IEndpointRouteBuilder 实例。</param>
    /// <returns>The IEndpointConventionBuilder instance for chaining. 用于链式调用的 IEndpointConventionBuilder 实例。</returns>
    public static IEndpointConventionBuilder MapOpenApi(this IEndpointRouteBuilder endpoints)
    {
        // This maps /openapi.json to serve the latest version of the swagger JSON
        // 这将映射 /openapi.json 以提供最新版本的 swagger JSON
        var conventionBuilder = endpoints.MapGet("/openapi.json", async context =>
        {
            var provider = context.RequestServices.GetRequiredService<IApiVersionDescriptionProvider>();
            var latestVersion = provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion).FirstOrDefault();
            
            if (latestVersion != null)
            {
                // Redirect to the actual versioned JSON file
                // 重定向到实际的版本化 JSON 文件
                context.Response.Redirect($"/swagger/{latestVersion.GroupName}/openapi.json");
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        });

        // Optional: Export the swagger JSON to a physical file in Development environment
        // 可选：在开发环境中将 swagger JSON 导出为物理文件
        var hostEnvironment = endpoints.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (hostEnvironment.IsDevelopment())
        {
            Task.Run(async () =>
            {
                try
                {
                    // Wait a bit for the server to start
                    await Task.Delay(5000);
                    
                    using var scope = endpoints.ServiceProvider.CreateScope();
                    var generator = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
                    var provider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                    var latestVersion = provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion).FirstOrDefault();
                    
                    if (latestVersion != null)
                    {
                        var swagger = generator.GetSwagger(latestVersion.GroupName, null, "/");
                        var json = swagger.SerializeAsJson(Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
                        var outputPath = Path.Combine(hostEnvironment.ContentRootPath, "openapi.json");
                        await File.WriteAllTextAsync(outputPath, json);
                    }
                }
                catch (Exception)
                {
                    // Ignore errors during background file export
                }
            });
        }

        return conventionBuilder;
    }


    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;
        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each discovered API version
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }

            static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
            {
                var info = new OpenApiInfo()
                {
                    Title = "AgentProject API",
                    Version = description.ApiVersion.ToString(),
                    Description = "The core API for the AgentProject AI Agent system. This API provides all necessary endpoints for agent management, RAG operations, fine-tuning, and monitoring.",
                    Contact = new OpenApiContact() { Name = "Agent Team", Email = "support@agent.im" },
                    License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                };

                if (description.IsDeprecated)
                {
                    info.Description += " **(This API version has been deprecated.)**";
                }

                return info;
            }

            // Add JWT Bearer authentication definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });

            // Include XML comments if file exists
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Enable Swagger Annotations
            options.EnableAnnotations();

            // Add a custom document filter to fix the issue with the default Swagger generation
            options.OperationFilter<SwaggerDefaultValues>();
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null)
                {
                    parameter.Required = description.IsRequired;
                }
            }
        }
    }
}


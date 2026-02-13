namespace Agent.Api.Extensions;

/// <summary>
/// Extension methods for configuring Semantic Kernel services
/// 配置语义内核服务的扩展方法
/// </summary>
public static class SemanticKernelExtensions
{
    /// <summary>
    /// Add Semantic Kernel services to the service collection
    /// 向服务集合添加语义内核服务
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        // Get Semantic Kernel configuration - 获取语义内核配置
        var options = new SemanticKernelOptions();
        configuration.GetSection("SemanticKernel").Bind(options);

        // Register options - 注册选项
        services.AddSingleton(options);

        // Register planners
        services.AddTransient<IKubernetesPlanner, KubernetesPlanner>();
        services.AddTransient<IIstioPlanner, IstioPlanner>();
        services.AddTransient<IPostgreSQLPlanner, PostgreSQLPlanner>();
        services.AddTransient<IClickHousePlanner, ClickHousePlanner>();

        // Register Kernel - 注册内核
        services.AddSingleton<Kernel>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<Kernel>>();
            logger.LogInformation("Initializing Semantic Kernel");

            var kernelBuilder = Kernel.CreateBuilder();

            // Configure AI services based on options - 根据选项配置AI服务
            if (!string.IsNullOrEmpty(options.AzureOpenAIEndpoint) && !string.IsNullOrEmpty(options.AzureOpenAIApiKey))
            {
                // Use Azure OpenAI - 使用Azure OpenAI
                logger.LogInformation("Configuring Azure OpenAI services");

                kernelBuilder.AddAzureOpenAIChatCompletion(
                    deploymentName: options.AzureChatDeploymentName ?? options.ChatModel,
                    endpoint: options.AzureOpenAIEndpoint,
                    apiKey: options.AzureOpenAIApiKey);

                kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: options.AzureEmbeddingDeploymentName ?? options.EmbeddingModel,
                    endpoint: options.AzureOpenAIEndpoint,
                    apiKey: options.AzureOpenAIApiKey);
            }
            else if (!string.IsNullOrEmpty(options.OpenAIApiKey))
            {
                // Use OpenAI - 使用OpenAI
                logger.LogInformation("Configuring OpenAI services");

                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: options.ChatModel,
                    apiKey: options.OpenAIApiKey);

                kernelBuilder.AddOpenAITextEmbeddingGeneration(
                    modelId: options.EmbeddingModel,
                    apiKey: options.OpenAIApiKey);
            }
            else
            {
                throw new InvalidOperationException("Either OpenAI or Azure OpenAI configuration must be provided");
            }

            var kernel = kernelBuilder.Build();
            logger.LogInformation("Semantic Kernel initialized successfully");

            return kernel;
        });

        // Register individual services - 注册各个服务
        services.AddSingleton<IChatCompletionService>(provider =>
        {
            var kernel = provider.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<IChatCompletionService>();
        });

        services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
        {
            var kernel = provider.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        });

        // Register Semantic Kernel service - 注册语义内核服务
        services.AddScoped<ISemanticKernelService, SemanticKernelService>();

        return services;
    }

    /// <summary>
    /// Add Semantic Kernel services with custom configuration
    /// 使用自定义配置添加语义内核服务
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services,
        Action<SemanticKernelOptions> configureOptions)
    {
        var options = new SemanticKernelOptions();
        configureOptions(options);

        // Register options - 注册选项
        services.AddSingleton(options);

        // Register planners
        services.AddTransient<IKubernetesPlanner, KubernetesPlanner>();
        services.AddTransient<IIstioPlanner, IstioPlanner>();
        services.AddTransient<IPostgreSQLPlanner, PostgreSQLPlanner>();
        services.AddTransient<IClickHousePlanner, ClickHousePlanner>();

        // Register Kernel - 注册内核
        services.AddSingleton<Kernel>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<Kernel>>();
            logger.LogInformation("Initializing Semantic Kernel with custom configuration");

            var kernelBuilder = Kernel.CreateBuilder();

            // Configure AI services - 配置AI服务
            if (!string.IsNullOrEmpty(options.AzureOpenAIEndpoint) && !string.IsNullOrEmpty(options.AzureOpenAIApiKey))
            {
                logger.LogInformation("Configuring Azure OpenAI services");

                kernelBuilder.AddAzureOpenAIChatCompletion(
                    deploymentName: options.AzureChatDeploymentName ?? options.ChatModel,
                    endpoint: options.AzureOpenAIEndpoint,
                    apiKey: options.AzureOpenAIApiKey);

                kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: options.AzureEmbeddingDeploymentName ?? options.EmbeddingModel,
                    endpoint: options.AzureOpenAIEndpoint,
                    apiKey: options.AzureOpenAIApiKey);
            }
            else if (!string.IsNullOrEmpty(options.OpenAIApiKey))
            {
                logger.LogInformation("Configuring OpenAI services");

                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: options.ChatModel,
                    apiKey: options.OpenAIApiKey);

                kernelBuilder.AddOpenAITextEmbeddingGeneration(
                    modelId: options.EmbeddingModel,
                    apiKey: options.OpenAIApiKey);
            }
            else
            {
                throw new InvalidOperationException("Either OpenAI or Azure OpenAI configuration must be provided");
            }

            var kernel = kernelBuilder.Build();
            logger.LogInformation("Semantic Kernel initialized successfully");

            return kernel;
        });

        // Register individual services - 注册各个服务
        services.AddSingleton<IChatCompletionService>(provider =>
        {
            var kernel = provider.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<IChatCompletionService>();
        });

        services.AddSingleton<ITextEmbeddingGenerationService>(provider =>
        {
            var kernel = provider.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        });

        // Register Semantic Kernel service - 注册语义内核服务
        services.AddScoped<ISemanticKernelService, SemanticKernelService>();

        return services;
    }

    /// <summary>
    /// Add vector database services for Semantic Kernel
    /// 为语义内核添加向量数据库服务
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddVectorDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Register universal vector database service - 注册通用向量数据库服务
        services.AddScoped<IVectorDatabaseService, ChromaVectorDatabaseService>();

        return services;
    }
}



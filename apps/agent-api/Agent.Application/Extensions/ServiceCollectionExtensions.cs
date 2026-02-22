namespace Agent.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IPostgreSqlService, PostgreSqlService>();

        // Memory Services
        services.AddScoped<IShortTermMemory, ShortTermMemoryService>();
        services.AddScoped<ILongTermMemory, LongTermMemoryService>();
        services.AddScoped<ITaskMemory, TaskMemoryService>();
        services.AddScoped<IAdvancedAgentMemory, ComprehensiveAgentMemory>();

        // Tool Registry Services
        services.AddScoped<IToolRegistryService, ToolRegistryService>();
        services.AddScoped<ISmartToolSelector, SmartToolSelector>();

            services.AddScoped<IAgentTraceService, AgentTraceService>();

        return services;
    }
}




namespace Agent.Application.Extensions;

using Agent.Core.Memory.Interfaces;
using Agent.Application.Services.Memory;
using Agent.Application.Services.PostgreSQL;
using Agent.Core.Tools.Interfaces;
using Agent.Application.Services.Tools;
using Microsoft.Extensions.DependencyInjection;

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
        
        return services;
    }
}
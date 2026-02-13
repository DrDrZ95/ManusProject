namespace Agent.Api.Extensions;

/// <summary>
/// eBPF 侦探模块扩展方法 - eBPF Detective Module Extension Methods
/// </summary>
public static class eBPFExtensions
{
    /// <summary>
    /// 添加 eBPF 侦探服务 - Add eBPF Detective Services
    /// </summary>
    /// <param name="services">服务集合 - Service collection</param>
    /// <returns>服务集合 - Service collection</returns>
    public static IServiceCollection AddeBPFDetectiveServices(this IServiceCollection services)
    {
        services.AddSingleton<IeBPFDetectiveService, eBPFDetectiveService>();
        return services;
    }
}


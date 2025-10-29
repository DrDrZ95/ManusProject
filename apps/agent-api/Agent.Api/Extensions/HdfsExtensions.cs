namespace Agent.Api.Extensions;

/// <summary>
/// HDFS 服务扩展方法 - HDFS Service Extension Methods
/// </summary>
public static class HdfsExtensions
{
    /// <summary>
    /// 添加 HDFS 服务 - Add HDFS Services
    /// </summary>
    /// <param name="services">服务集合 - Service collection</param>
    /// <param name="configuration">配置 - Configuration</param>
    /// <returns>服务集合 - Service collection</returns>
    public static IServiceCollection AddHdfsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHdfsService, HdfsService>();
        return services;
    }
}



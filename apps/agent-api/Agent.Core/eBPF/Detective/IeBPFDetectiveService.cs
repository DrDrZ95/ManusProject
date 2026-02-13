namespace Agent.Core.eBPF.Detective;

/// <summary>
/// eBPF 侦探服务接口 - eBPF Detective Service Interface
/// 定义了与eBPF相关的系统监控和分析操作 - Defines eBPF-related system monitoring and analysis operations
/// </summary>
public interface IeBPFDetectiveService
{
    /// <summary>
    /// 运行eBPF脚本并获取输出 - Run eBPF script and get output
    /// </summary>
    /// <param name="scriptName">脚本名称 - Script name</param>
    /// <param name="args">脚本参数 - Script arguments</param>
    /// <returns>eBPF脚本的输出 - Output of the eBPF script</returns>
    Task<string> RunBpftraceScriptAsync(string scriptName, string args = "");

    /// <summary>
    /// 获取系统CPU使用率 - Get system CPU usage
    /// </summary>
    /// <returns>CPU使用率百分比 - CPU usage percentage</returns>
    Task<double> GetCpuUsageAsync();

    /// <summary>
    /// 获取系统内存使用率 - Get system memory usage
    /// </summary>
    /// <returns>内存使用率百分比 - Memory usage percentage</returns>
    Task<double> GetMemoryUsageAsync();

    /// <summary>
    /// 监控网络活动 - Monitor network activity
    /// </summary>
    /// <param name="durationSeconds">监控持续时间（秒） - Monitoring duration (seconds)</param>
    /// <returns>网络活动数据 - Network activity data</returns>
    IAsyncEnumerable<string> MonitorNetworkActivityAsync(int durationSeconds);

    /// <summary>
    /// 监控进程活动 - Monitor process activity
    /// </summary>
    /// <param name="processName">进程名称 - Process name</param>
    /// <param name="durationSeconds">监控持续时间（秒） - Monitoring duration (seconds)</param>
    /// <returns>进程活动数据 - Process activity data</returns>
    IAsyncEnumerable<string> MonitorProcessActivityAsync(string processName, int durationSeconds);
}


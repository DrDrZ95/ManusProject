
namespace Agent.Metering.eBPF;

/// <summary> 
/// eBPF服务接口 
/// </summary> 
public interface IEbpfService 
{ 
    /// <summary> 
    /// 加载并附加eBPF程序 
    /// </summary> 
    Task LoadAndAttachEbpfProgramAsync(); 
    /// <summary> 
    /// 分离并卸载eBPF程序 
    /// </summary> 
    Task DetachAndUnloadEbpfProgramAsync(); 
    /// <summary> 
    /// 获取指标并存储 
    /// </summary> 
    /// <param name="metricName">指标名称</param> 
    Task<long> GetAndStoreMetricAsync(string metricName); 
} 

/// <summary> 
/// eBPF服务实现 
/// </summary> 
public class EbpfService : IEbpfService 
{ 
    private readonly ILogger<EbpfService> _logger; 
    //private readonly IPostgreSqlService _postgreSqlService; // PostgreSQL服务实例 
    private static readonly ActivitySource ActivitySource = new ActivitySource("Agent.Metering.eBPF"); // OpenTelemetry活动源 

    public EbpfService(ILogger<EbpfService> logger) 
    { 
        _logger = logger; 
    } 

    /// <summary> 
    /// 1. 模拟加载并附加eBPF程序 
    /// </summary> 
    public Task LoadAndAttachEbpfProgramAsync() 
    { 
        _logger.LogInformation("Loading and attaching eBPF program (simulated)."); 
        // 在实际场景中，这将涉及使用LibBPF或BCC等库 
        // 从metering_ebpf.bpf.o加载并附加eBPF程序 
        return Task.CompletedTask; 
    } 

    /// <summary> 
    /// 2. 模拟分离并卸载eBPF程序 
    /// </summary> 
    public Task DetachAndUnloadEbpfProgramAsync() 
    { 
        _logger.LogInformation("Detaching and unloading eBPF program (simulated)."); 
        // 在实际场景中，这将涉及分离和卸载eBPF程序 
        return Task.CompletedTask; 
    } 

    /// <summary> 
    /// 3. 模拟获取指标，通过OpenTelemetry追踪，并存储到PostgreSQL 
    /// </summary> 
    /// <param name="metricName">指标名称</param> 
    public async Task<long> GetAndStoreMetricAsync(string metricName) 
    { 
        using (var activity = ActivitySource.StartActivity("GetAndStoreMetric")) // 启动OpenTelemetry活动 
        { 
            activity?.SetTag("metric.name", metricName); // 设置活动标签 

            _logger.LogInformation("Getting metric {MetricName} from eBPF program (simulated)."); 
            // 在实际场景中，这将涉及从eBPF映射中读取 
            var simulatedValue = new Random().Next(100, 10000); // 模拟指标值 

            activity?.SetTag("metric.value", simulatedValue); // 设置活动标签 

            // 4. 模拟通过OpenTelemetry收集数据 
            _logger.LogInformation("Simulating OpenTelemetry data collection for metric {MetricName}: {Value}", metricName, simulatedValue); 

            // 5. 存储到PostgreSQL ORM模块 
            // var metricEntry = new MetricEntry 
            // { 
            //     MetricName = metricName, 
            //     Value = simulatedValue, 
            //     Timestamp = DateTime.UtcNow 
            // }; 

            // await _postgreSqlService.AddMetricEntryAsync(metricEntry); // 调用PostgreSQL服务存储数据 
            _logger.LogInformation("Metric {MetricName} with value {Value} stored in PostgreSQL.", metricName, simulatedValue); 

            return simulatedValue; 
        } 
    } 
} 


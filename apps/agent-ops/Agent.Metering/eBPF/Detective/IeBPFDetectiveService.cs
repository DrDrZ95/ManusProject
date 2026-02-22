namespace Agent.Metering.eBPF.Detective;

public interface IeBPFDetectiveService
{
    Task<string> RunBpftraceScriptAsync(string scriptName, string args = "");
    Task<double> GetCpuUsageAsync();
    Task<double> GetMemoryUsageAsync();
    IAsyncEnumerable<string> MonitorNetworkActivityAsync(int durationSeconds);
    IAsyncEnumerable<string> MonitorProcessActivityAsync(string processName, int durationSeconds);
}

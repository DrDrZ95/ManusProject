namespace Agent.Core.eBPF.Detective;

/// <summary>
/// eBPF 侦探服务实现 - eBPF Detective Service Implementation
/// 负责执行eBPF脚本和收集系统指标 - Responsible for executing eBPF scripts and collecting system metrics
/// </summary>
public class eBPFDetectiveService : IeBPFDetectiveService
{
    private readonly ILogger<eBPFDetectiveService> _logger;

    public eBPFDetectiveService(ILogger<eBPFDetectiveService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 运行bpftrace脚本 - Run bpftrace script
    /// </summary>
    /// <param name="scriptName">脚本名称 (位于Scripts目录下) - Script name (located in Scripts directory)</param>
    /// <param name="args">传递给bpftrace的参数 - Arguments passed to bpftrace</param>
    /// <returns>脚本输出 - Script output</returns>
    public async Task<string> RunBpftraceScriptAsync(string scriptName, string args = "")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _logger.LogWarning("bpftrace is only supported on Linux. Current OS: {OS}", RuntimeInformation.OSDescription);
            return "";
        }

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "eBPF", "Scripts", scriptName);
        if (!File.Exists(scriptPath))
        {
            _logger.LogError("eBPF script not found: {ScriptPath}", scriptPath);
            throw new FileNotFoundException($"eBPF script not found: {scriptPath}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "sudo", // bpftrace通常需要root权限 - bpftrace usually requires root privileges
            Arguments = $"bpftrace {scriptPath} {args}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("bpftrace script \'{ScriptName}\' failed with exit code {ExitCode}. Error: {Error}", scriptName, process.ExitCode, error);
            throw new InvalidOperationException($"bpftrace script failed: {error}");
        }

        _logger.LogInformation("Successfully ran bpftrace script \'{ScriptName}\'. Output: {Output}", scriptName, output);
        return output;
    }

    /// <summary>
    /// 获取系统CPU使用率 (示例实现，可能需要更复杂的eBPF脚本) - Get system CPU usage (example, may need more complex eBPF script)
    /// </summary>
    public async Task<double> GetCpuUsageAsync()
    {
        // 这是一个简化示例，实际生产环境可能需要更精确的eBPF或/proc解析
        // This is a simplified example, real production might need more precise eBPF or /proc parsing
        try
        {
            var output = await RunBpftraceScriptAsync("cpu_usage.bt", "");
            // 假设脚本输出是百分比数字 - Assuming script output is a percentage number
            if (double.TryParse(output.Trim(), out var cpuUsage))
            {
                return cpuUsage;
            }
            _logger.LogWarning("Could not parse CPU usage from bpftrace output: {Output}", output);
            return 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage.");
            return 0.0;
        }
    }

    /// <summary>
    /// 获取系统内存使用率 (示例实现) - Get system memory usage (example)
    /// </summary>
    public async Task<double> GetMemoryUsageAsync()
    {
        try
        {
            var output = await RunBpftraceScriptAsync("memory_usage.bt", "");
            if (double.TryParse(output.Trim(), out var memoryUsage))
            {
                return memoryUsage;
            }
            _logger.LogWarning("Could not parse memory usage from bpftrace output: {Output}", output);
            return 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory usage.");
            return 0.0;
        }
    }

    /// <summary>
    /// 监控网络活动 (示例实现，流式输出) - Monitor network activity (example, streaming output)
    /// </summary>
    public async IAsyncEnumerable<string> MonitorNetworkActivityAsync(int durationSeconds)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _logger.LogWarning("bpftrace is only supported on Linux. Current OS: {OS}", RuntimeInformation.OSDescription);
            yield break;
        }

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "eBPF", "Scripts", "network_monitor.bt");
        if (!File.Exists(scriptPath))
        {
            _logger.LogError("eBPF script not found: {ScriptPath}", scriptPath);
            throw new FileNotFoundException($"eBPF script not found: {scriptPath}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "sudo",
            Arguments = $"bpftrace {scriptPath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var startTime = DateTime.UtcNow;
        while (!process.HasExited && (DateTime.UtcNow - startTime).TotalSeconds < durationSeconds)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            if (line != null)
            {
                yield return line;
            }
            else
            {
                // 等待一小段时间以避免CPU空转 - Wait a short period to avoid CPU spin
                await Task.Delay(100);
            }
        }

        if (!process.HasExited)
        {
            process.Kill(); // 结束进程 - Terminate process
        }

        var error = await process.StandardError.ReadToEndAsync();
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("bpftrace network monitor script reported errors: {Error}", error);
        }
    }

    /// <summary>
    /// 监控进程活动 (示例实现，流式输出) - Monitor process activity (example, streaming output)
    /// </summary>
    public async IAsyncEnumerable<string> MonitorProcessActivityAsync(string processName, int durationSeconds)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _logger.LogWarning("bpftrace is only supported on Linux. Current OS: {OS}", RuntimeInformation.OSDescription);
            yield break;
        }

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "eBPF", "Scripts", "process_monitor.bt");
        if (!File.Exists(scriptPath))
        {
            _logger.LogError("eBPF script not found: {ScriptPath}", scriptPath);
            throw new FileNotFoundException($"eBPF script not found: {scriptPath}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "sudo",
            Arguments = $"bpftrace {scriptPath} {processName}", // 传递进程名称作为参数 - Pass process name as argument
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var startTime = DateTime.UtcNow;
        while (!process.HasExited && (DateTime.UtcNow - startTime).TotalSeconds < durationSeconds)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            if (line != null)
            {
                yield return line;
            }
            else
            {
                await Task.Delay(100);
            }
        }

        if (!process.HasExited)
        {
            process.Kill();
        }

        var error = await process.StandardError.ReadToEndAsync();
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("bpftrace process monitor script reported errors: {Error}", error);
        }
    }
}



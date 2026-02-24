namespace Agent.Metering.eBPF.Detective;

public class eBPFDetectiveService : IeBPFDetectiveService
{
    private readonly ILogger<eBPFDetectiveService> _logger;

    public eBPFDetectiveService(ILogger<eBPFDetectiveService> logger)
    {
        _logger = logger;
    }

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
            FileName = "sudo",
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
            _logger.LogError("bpftrace script '{ScriptName}' failed with exit code {ExitCode}. Error: {Error}", scriptName, process.ExitCode, error);
            throw new InvalidOperationException($"bpftrace script failed: {error}");
        }

        _logger.LogInformation("Successfully ran bpftrace script '{ScriptName}'. Output: {Output}", scriptName, output);
        return output;
    }

    public async Task<double> GetCpuUsageAsync()
    {
        try
        {
            var output = await RunBpftraceScriptAsync("cpu_usage.bt", "");
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
            _logger.LogError("bpftrace network monitor script reported errors: {Error}", error);
        }
    }

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
            Arguments = $"bpftrace {scriptPath} {processName}",
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

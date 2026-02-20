namespace Agent.Metering.Pipelines;

/// <summary>
/// OTLP 风格日志导出选项
/// 实际导出依赖宿主环境中配置的 OpenTelemetry 日志管道（本导出器负责统一日志格式）
/// </summary>
public sealed class OtlpLogExporterOptions
{
    /// <summary>
    /// 导出器名称（与 pipeline 配置中的名称一致）
    /// </summary>
    public string Name { get; set; } = "otlp-logs";
}

/// <summary>
/// OTLP 风格日志导出器
/// 设计思路：
/// - 使用标准 ILogger 记录结构化日志，由宿主中配置的 OTLP 日志导出器负责发送到远端。
/// - 本导出器不直接依赖任何 profiler/attach 能力，完全遵循 Strict Mode 约束。
/// </summary>
public sealed class OtlpLogExporter : IMeteringExporter
{
    private readonly string _name;
    private readonly ILogger<OtlpLogExporter> _logger;

    public string Name => _name;

    public OtlpLogExporter(string name, OtlpLogExporterOptions options, ILogger<OtlpLogExporter> logger)
    {
        _name = name;
        _logger = logger;
    }

    /// <summary>
    /// 将批量指标转换为结构化日志写入 ILogger
    /// </summary>
    public Task ExportAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken)
    {
        foreach (var record in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var payload = new
            {
                timestamp = record.Timestamp,
                source = record.Source,
                name = record.Name,
                value = record.Value,
                tags = record.Tags
            };

            _logger.LogInformation("metering-log {@Payload}", payload);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// 本地调试落盘导出选项
/// </summary>
public sealed class DebugFileLogExporterOptions
{
    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string FilePath { get; set; } = "logs/metering-debug.log";

    /// <summary>
    /// 导出器名称（与 pipeline 配置中的名称一致）
    /// </summary>
    public string Name { get; set; } = "file-debug";
}

/// <summary>
/// 本地 JSON 行日志导出器，主要用于开发与调试
/// </summary>
public sealed class DebugFileLogExporter : IMeteringExporter
{
    private readonly string _name;
    private readonly DebugFileLogExporterOptions _options;
    private readonly ILogger<DebugFileLogExporter> _logger;

    public string Name => _name;

    public DebugFileLogExporter(string name, DebugFileLogExporterOptions options, ILogger<DebugFileLogExporter> logger)
    {
        _name = name;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 将批量指标序列化为 JSON 行写入本地文件
    /// </summary>
    public async Task ExportAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken)
    {
        try
        {
            var directory = Path.GetDirectoryName(_options.FilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = new FileStream(_options.FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await using var writer = new StreamWriter(stream);

            foreach (var record in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var payload = new
                {
                    timestamp = record.Timestamp,
                    source = record.Source,
                    name = record.Name,
                    value = record.Value,
                    tags = record.Tags
                };

                var json = JsonSerializer.Serialize(payload);
                await writer.WriteLineAsync(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DebugFileLogExporter {Name} failed to write logs to {Path}.", _name,
                _options.FilePath);
        }
    }
}

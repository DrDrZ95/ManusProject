namespace Agent.Metering.Pipelines;

/// <summary>
/// 文件 tail 采集器配置选项（参考 Datadog / Fluent Bit 的 file tail 能力）
/// </summary>
public sealed class FileTailReceiverOptions
{
    /// <summary>
    /// 要采集的日志文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 断点续读的偏移量文件路径（存储最后一次读取的字节偏移）
    /// </summary>
    public string? PositionFilePath { get; set; }

    /// <summary>
    /// 轮询间隔（未有新数据时的休眠时间）
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// 采集速率限制（字节/秒），0 表示不限制
    /// </summary>
    public int MaxBytesPerSecond { get; set; } = 1024 * 1024;

    /// <summary>
    /// 是否跟随文件轮转（当文件被截断或替换时从头开始读取）
    /// </summary>
    public bool FollowRotation { get; set; } = true;
}

/// <summary>
/// 文件 tail 采集器：严格模式下只读日志文件，不注入业务进程
/// - 支持断点续读（position file）
/// - 支持文件轮转检测（长度回退视为轮转）
/// - 支持简单的速率限制（按秒限流）
/// </summary>
public sealed class FileTailReceiver : IMeteringReceiver
{
    private readonly string _name;
    private readonly IOptionsMonitor<FileTailReceiverOptions> _optionsMonitor;
    private readonly ILogger<FileTailReceiver> _logger;

    public string Name => _name;

    public FileTailReceiver(
        string name,
        IOptionsMonitor<FileTailReceiverOptions> optionsMonitor,
        ILogger<FileTailReceiver> logger)
    {
        _name = name;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    /// <summary>
    /// 启动 tail 循环，从文件中读取增量日志并写入 Channel
    /// </summary>
    public async Task StartAsync(ChannelWriter<MeterRecord> writer, CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.Get(_name);

        if (string.IsNullOrWhiteSpace(options.FilePath))
        {
            _logger.LogWarning("FileTailReceiver {Name} is not configured with a valid FilePath.", _name);
            return;
        }

        var positionFilePath = options.PositionFilePath ??
                               Path.Combine(Path.GetDirectoryName(options.FilePath) ?? ".", ".position");

        long position = LoadPosition(positionFilePath);
        var buffer = new byte[8192];
        var decoder = Encoding.UTF8.GetDecoder();
        var charBuffer = new char[8192];
        var lineBuilder = new StringBuilder();

        var windowStart = DateTime.UtcNow;
        var bytesThisWindow = 0;

        _logger.LogInformation("FileTailReceiver {Name} starting. File={File}, Position={Position}", _name,
            options.FilePath, position);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!File.Exists(options.FilePath))
                {
                    await Task.Delay(options.PollInterval, cancellationToken);
                    continue;
                }

                var fileInfo = new FileInfo(options.FilePath);

                // 轮转检测：文件长度小于当前位置，视为新文件，从头开始
                if (options.FollowRotation && fileInfo.Length < position)
                {
                    _logger.LogInformation("FileTailReceiver {Name} detected rotation. Resetting position.", _name);
                    position = 0;
                }

                if (fileInfo.Length == position)
                {
                    await Task.Delay(options.PollInterval, cancellationToken);
                    continue;
                }

                // 速率限制：简单地按秒限流，超过阈值则休眠到下一秒
                if (options.MaxBytesPerSecond > 0)
                {
                    var now = DateTime.UtcNow;
                    if ((now - windowStart).TotalSeconds >= 1)
                    {
                        windowStart = now;
                        bytesThisWindow = 0;
                    }
                    else if (bytesThisWindow >= options.MaxBytesPerSecond)
                    {
                        var sleep = 1000 - (int)(now - windowStart).TotalMilliseconds;
                        if (sleep > 0)
                        {
                            await Task.Delay(sleep, cancellationToken);
                        }

                        windowStart = DateTime.UtcNow;
                        bytesThisWindow = 0;
                    }
                }

                using (var stream = new FileStream(
                           options.FilePath,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.ReadWrite | FileShare.Delete))
                {
                    stream.Seek(position, SeekOrigin.Begin);

                    while (!cancellationToken.IsCancellationRequested && stream.Position < stream.Length)
                    {
                        var bytesToRead = Math.Min(buffer.Length, (int)(stream.Length - stream.Position));
                        var read = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);

                        if (read <= 0)
                        {
                            break;
                        }

                        bytesThisWindow += read;
                        position += read;

                        // 将字节流解码为字符，并按行拆分
                        var chars = decoder.GetChars(buffer, 0, read, charBuffer, 0, false);
                        for (var i = 0; i < chars; i++)
                        {
                            var ch = charBuffer[i];
                            if (ch == '\n')
                            {
                                var line = lineBuilder.ToString().TrimEnd('\r');
                                lineBuilder.Clear();

                                if (line.Length > 0)
                                {
                                    var record = new MeterRecord
                                    {
                                        Timestamp = DateTime.UtcNow,
                                        Source = options.FilePath,
                                        Name = "log",
                                        Value = 0,
                                        Tags = new Dictionary<string, object?>
                                        {
                                            ["message"] = line
                                        }
                                    };

                                    if (!await writer.WaitToWriteAsync(cancellationToken))
                                    {
                                        return;
                                    }

                                    if (!writer.TryWrite(record))
                                    {
                                        // Channel 满时，根据 Channel 配置决定丢弃策略
                                        _logger.LogDebug(
                                            "FileTailReceiver {Name} dropped a log record due to full channel.",
                                            _name);
                                    }
                                }
                            }
                            else
                            {
                                lineBuilder.Append(ch);
                            }
                        }

                        SavePosition(positionFilePath, position);

                        if (options.MaxBytesPerSecond > 0 && bytesThisWindow >= options.MaxBytesPerSecond)
                        {
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FileTailReceiver {Name} encountered an error while reading file.", _name);
                await Task.Delay(options.PollInterval, cancellationToken);
            }
        }

        _logger.LogInformation("FileTailReceiver {Name} stopped.", _name);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // FileTailReceiver 由上层 pipeline 的取消令牌驱动，这里无需额外逻辑
        return Task.CompletedTask;
    }

    /// <summary>
    /// 从 position 文件读取上次的偏移量
    /// </summary>
    private static long LoadPosition(string positionFilePath)
    {
        try
        {
            if (!File.Exists(positionFilePath))
            {
                return 0;
            }

            var text = File.ReadAllText(positionFilePath);
            return long.TryParse(text, out var value) ? value : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 将最新偏移量写入 position 文件
    /// </summary>
    private static void SavePosition(string positionFilePath, long position)
    {
        try
        {
            File.WriteAllText(positionFilePath, position.ToString());
        }
        catch
        {
            // position 写入失败不应影响采集主流程
        }
    }
}

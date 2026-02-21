namespace Agent.Metering.Pipelines;

/// <summary>
/// Dapr Prometheus metrics 采集配置选项
/// </summary>
public sealed class DaprPrometheusOptions
{
    /// <summary>
    /// 是否启用 Dapr metrics 抓取
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 默认 metrics 端点（Dapr 官方默认 9090/metrics）
    /// </summary>
    public string Endpoint { get; set; } = "http://127.0.0.1:9090/metrics";

    /// <summary>
    /// 轮询抓取间隔
    /// </summary>
    public TimeSpan ScrapeInterval { get; set; } = TimeSpan.FromSeconds(15);
}

/// <summary>
/// Dapr Prometheus metrics 抓取 Receiver
/// - 自动发现端点：优先读取环境变量中的端口，其次使用默认 9090/metrics
/// - 解析 Prometheus 文本格式，将样本映射为 MeterRecord，方便后续转换为 OTLP metrics 或 Prom remote-write
/// </summary>
public sealed class DaprPrometheusReceiver : IMeteringReceiver
{
    private readonly string _name;
    private readonly DaprPrometheusOptions _options;
    private readonly ILogger<DaprPrometheusReceiver> _logger;

    public string Name => _name;

    public DaprPrometheusReceiver(string name, DaprPrometheusOptions options, ILogger<DaprPrometheusReceiver> logger)
    {
        _name = name;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 周期性从 Dapr sidecar 的 Prometheus metrics 端点拉取指标
    /// </summary>
    public async Task StartAsync(ChannelWriter<MeterRecord> writer, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var endpoint = ResolveEndpoint();

        using var httpClient = new HttpClient();

        _logger.LogInformation("DaprPrometheusReceiver {Name} started. Endpoint={Endpoint}", _name, endpoint);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await httpClient.GetAsync(endpoint, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DaprPrometheusReceiver {Name} scrape failed: {StatusCode}", _name, response.StatusCode);
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    foreach (var record in ParsePrometheus(body))
                    {
                        if (!await writer.WaitToWriteAsync(cancellationToken))
                        {
                            return;
                        }

                        if (!writer.TryWrite(record))
                        {
                            _logger.LogDebug("DaprPrometheusReceiver {Name} dropped a metric due to full channel.", _name);
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
                _logger.LogWarning(ex, "DaprPrometheusReceiver {Name} encountered an error during scrape.", _name);
            }

            try
            {
                await Task.Delay(_options.ScrapeInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("DaprPrometheusReceiver {Name} stopped.", _name);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据环境变量和默认配置解析最终的 metrics 端点
    /// - 优先使用 DAPR_METRICS_PORT / DAPR_METRICS_HTTP_PORT
    /// - 否则使用配置中的 Endpoint
    /// </summary>
    private string ResolveEndpoint()
    {
        var port = Environment.GetEnvironmentVariable("DAPR_METRICS_HTTP_PORT")
                   ?? Environment.GetEnvironmentVariable("DAPR_METRICS_PORT");

        if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out var parsed) && parsed > 0)
        {
            return $"http://127.0.0.1:{parsed}/metrics";
        }

        return _options.Endpoint;
    }

    /// <summary>
    /// 解析简单 Prometheus 文本格式，将样本映射为 MeterRecord
    /// 仅处理类似 "name{label=\"value\"} 1.23" 的单行样本，忽略注释与 HELP/TYPE 元信息
    /// </summary>
    private static IEnumerable<MeterRecord> ParsePrometheus(string body)
    {
        using var reader = new StringReader(body);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();

            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var spaceIndex = line.LastIndexOf(' ');
            if (spaceIndex <= 0 || spaceIndex == line.Length - 1)
            {
                continue;
            }

            var metricAndLabels = line.Substring(0, spaceIndex);
            var valuePart = line.Substring(spaceIndex + 1);

            if (!double.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                continue;
            }

            string metricName;
            var tags = new Dictionary<string, object?>();

            var braceIndex = metricAndLabels.IndexOf('{');
            if (braceIndex < 0)
            {
                metricName = metricAndLabels;
            }
            else
            {
                metricName = metricAndLabels.Substring(0, braceIndex);
                var labelsPart = metricAndLabels.Substring(braceIndex + 1);
                var endBrace = labelsPart.LastIndexOf('}');
                if (endBrace >= 0)
                {
                    labelsPart = labelsPart.Substring(0, endBrace);
                }

                foreach (var label in labelsPart.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var eqIndex = label.IndexOf('=');
                    if (eqIndex <= 0 || eqIndex == label.Length - 1)
                    {
                        continue;
                    }

                    var key = label.Substring(0, eqIndex).Trim();
                    var rawValue = label.Substring(eqIndex + 1).Trim();

                    if (rawValue.Length >= 2 && rawValue[0] == '"' && rawValue[^1] == '"')
                    {
                        rawValue = rawValue.Substring(1, rawValue.Length - 2);
                    }

                    if (key.Length > 0 && !tags.ContainsKey(key))
                    {
                        tags[key] = rawValue;
                    }
                }
            }

            tags["metric.source"] = "dapr";

            yield return new MeterRecord
            {
                Timestamp = DateTime.UtcNow,
                Source = "dapr-prometheus",
                Name = metricName,
                Value = value,
                Tags = tags
            };
        }
    }
}


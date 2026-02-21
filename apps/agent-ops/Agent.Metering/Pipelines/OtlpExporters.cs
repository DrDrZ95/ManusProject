namespace Agent.Metering.Pipelines;

/// <summary>
/// OTLP 导出协议类型（当前实现侧重 OTLP/HTTP，OTLP/gRPC 通过网关或 Collector 适配）
/// </summary>
public enum OtlpExporterProtocol
{
    Http,
    Grpc
}

/// <summary>
/// OTLP 导出器配置选项
/// </summary>
public sealed class OtlpExporterOptions
{
    /// <summary>
    /// 使用的 OTLP 协议类型（Http 或 Grpc）
    /// </summary>
    public OtlpExporterProtocol Protocol { get; set; } = OtlpExporterProtocol.Http;

    /// <summary>
    /// OTLP 端点地址，默认指向本机 OTLP/HTTP metrics 入口
    /// 例如：http://localhost:4318/v1/metrics 或 http://localhost:4318/v1/logs
    /// </summary>
    public string Endpoint { get; set; } = "http://127.0.0.1:4318/v1/metrics";

    /// <summary>
    /// 是否期望使用 TLS（HTTPS），实际行为由 Endpoint 的 scheme 决定
    /// </summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// 自定义请求头，用于注入认证信息或路由标记
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 导出请求超时时间
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// 基于 OTLP/HTTP 风格的通用导出器
/// - 将 MeterRecord 批量序列化为 JSON 并 POST 到配置的 OTLP 端点
/// - 可用于对接原生 OTLP/HTTP Collector，或通过网关/Agent 转换为正式 OTLP/gRPC
/// </summary>
public sealed class OtlpHttpExporter : IMeteringExporter
{
    private readonly string _name;
    private readonly OtlpExporterOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OtlpHttpExporter> _logger;

    public string Name => _name;

    public OtlpHttpExporter(string name, OtlpExporterOptions options, ILogger<OtlpHttpExporter> logger)
    {
        _name = name;
        _options = options;
        _logger = logger;

        var handler = new HttpClientHandler();
        _httpClient = new HttpClient(handler)
        {
            Timeout = _options.Timeout
        };
    }

    /// <summary>
    /// 将批量 MeterRecord 作为一个 OTLP 风格的 payload 发送到远端
    /// </summary>
    public async Task ExportAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        try
        {
            var payload = BuildPayload(batch);
            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            foreach (var header in _options.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    if (!request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        _logger.LogDebug("OtlpHttpExporter {Name} failed to add header {Header}.", _name, header.Key);
                    }
                }
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OtlpHttpExporter {Name} export failed. StatusCode={StatusCode}",
                    _name,
                    response.StatusCode);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OtlpHttpExporter {Name} encountered an error while exporting batch.", _name);
        }
    }

    /// <summary>
    /// 将内部 MeterRecord 批量映射为 OTLP 风格的 JSON payload
    /// </summary>
    private object BuildPayload(IReadOnlyList<MeterRecord> batch)
    {
        var now = DateTimeOffset.UtcNow;

        var records = new List<object>(batch.Count);
        foreach (var record in batch)
        {
            records.Add(new
            {
                timestamp = record.Timestamp,
                source = record.Source,
                name = record.Name,
                value = record.Value,
                tags = record.Tags
            });
        }

        var payload = new
        {
            protocol = _options.Protocol.ToString(),
            exported_at = now,
            records
        };

        return payload;
    }
}


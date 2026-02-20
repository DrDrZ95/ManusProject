namespace Agent.Metering.Pipelines;

/// <summary>
/// 多行日志聚合配置（类似 Fluent Bit 的 multiline 语义）
/// </summary>
public sealed class LogMultilineOptions
{
    /// <summary>
    /// 标识该配置对应的处理器名称（与 pipeline 中引用的名称一致）
    /// </summary>
    public string Name { get; set; } = "log-multiline";

    /// <summary>
    /// 匹配多行日志起始行的正则表达式（例如异常栈第一行）
    /// </summary>
    public string StartPattern { get; set; } = @"^\S+.*";

    /// <summary>
    /// 多行聚合超时时间，超过该时长未收到新行则强制flush
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// 多行日志聚合处理器：按配置的起始正则和超时，将多行日志合并为一条记录
/// </summary>
public sealed class LogMultilineProcessor : IMeteringProcessor
{
    private readonly string _name;
    private readonly Regex _startRegex;
    private readonly TimeSpan _timeout;

    private readonly object _syncRoot = new();
    private StringBuilder? _buffer;
    private MeterRecord? _bufferBaseRecord;
    private DateTime _bufferStartTime;

    public string Name => _name;

    public LogMultilineProcessor(string name, LogMultilineOptions options)
    {
        _name = name;
        _startRegex = new Regex(options.StartPattern, RegexOptions.Compiled);
        _timeout = options.Timeout;
    }

    /// <summary>
    /// 将多行日志合并为单条记录，保留原始标签信息
    /// </summary>
    public Task<IReadOnlyList<MeterRecord>> ProcessAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken)
    {
        var output = new List<MeterRecord>();

        foreach (var record in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!record.Tags.TryGetValue("message", out var messageObj) || messageObj is not string message)
            {
                output.Add(record);
                continue;
            }

            lock (_syncRoot)
            {
                var isStart = _startRegex.IsMatch(message);
                var now = record.Timestamp;

                // 如果当前缓冲已存在且超时，则先flush
                if (_buffer != null && (now - _bufferStartTime) > _timeout)
                {
                    output.Add(CreateAggregatedRecord());
                    _buffer = null;
                    _bufferBaseRecord = null;
                }

                if (_buffer == null || isStart)
                {
                    if (_buffer != null && _bufferBaseRecord != null)
                    {
                        output.Add(CreateAggregatedRecord());
                    }

                    _buffer = new StringBuilder();
                    _buffer.AppendLine(message);
                    _bufferBaseRecord = record;
                    _bufferStartTime = now;
                }
                else
                {
                    _buffer.AppendLine(message);
                }
            }
        }

        return Task.FromResult((IReadOnlyList<MeterRecord>)output);
    }

    private MeterRecord CreateAggregatedRecord()
    {
        if (_bufferBaseRecord == null || _buffer == null)
        {
            throw new InvalidOperationException("No buffered record to aggregate.");
        }

        var tags = new Dictionary<string, object?>(_bufferBaseRecord.Tags)
        {
            ["message"] = _buffer.ToString().TrimEnd('\r', '\n')
        };

        return new MeterRecord
        {
            Timestamp = _bufferBaseRecord.Timestamp,
            Source = _bufferBaseRecord.Source,
            Name = _bufferBaseRecord.Name,
            Value = _bufferBaseRecord.Value,
            Tags = tags
        };
    }
}

/// <summary>
/// 单条脱敏规则配置（参考 Datadog / Fluent Bit 的日志过滤与脱敏能力）
/// </summary>
public sealed class LogMaskRuleOptions
{
    /// <summary>
    /// 需要脱敏的正则表达式（用于匹配密码、令牌等敏感信息）
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// 替换内容，默认使用 "***" 掩盖敏感信息
    /// </summary>
    public string Replacement { get; set; } = "***";

    /// <summary>
    /// 目标字段名称列表，空表示对 message 及所有字符串字段生效
    /// </summary>
    public List<string> TargetFields { get; set; } = new();
}

/// <summary>
/// 日志字段提取与脱敏配置
/// </summary>
public sealed class LogTransformOptions
{
    /// <summary>
    /// 处理器名称（与 pipeline 配置中的名称一致）
    /// </summary>
    public string Name { get; set; } = "log-transform";

    /// <summary>
    /// 用于字段提取的正则表达式（支持命名捕获组）
    /// 例如: ^(?&lt;level&gt;INFO|WARN|ERROR)\s+(?&lt;message&gt;.+)$
    /// </summary>
    public string? FieldPattern { get; set; }

    /// <summary>
    /// 脱敏规则列表
    /// </summary>
    public List<LogMaskRuleOptions> MaskRules { get; set; } = new();
}

/// <summary>
/// 日志字段提取与敏感信息脱敏处理器
/// - 可选字段提取：根据命名分组将日志内容拆成结构化字段
/// - 脱敏：按配置的正则和目标字段对敏感信息进行替换
/// </summary>
public sealed class LogTransformProcessor : IMeteringProcessor
{
    private readonly string _name;
    private readonly Regex? _fieldRegex;
    private readonly List<(Regex Regex, LogMaskRuleOptions Rule)> _maskRules;

    public string Name => _name;

    public LogTransformProcessor(string name, LogTransformOptions options)
    {
        _name = name;

        if (!string.IsNullOrWhiteSpace(options.FieldPattern))
        {
            _fieldRegex = new Regex(options.FieldPattern, RegexOptions.Compiled);
        }

        _maskRules = new List<(Regex, LogMaskRuleOptions)>();
        foreach (var rule in options.MaskRules)
        {
            if (string.IsNullOrWhiteSpace(rule.Pattern))
            {
                continue;
            }

            var regex = new Regex(rule.Pattern, RegexOptions.Compiled);
            _maskRules.Add((regex, rule));
        }
    }

    /// <summary>
    /// 对批量日志记录执行字段提取和脱敏处理
    /// </summary>
    public Task<IReadOnlyList<MeterRecord>> ProcessAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken)
    {
        var output = new List<MeterRecord>(batch.Count);

        foreach (var record in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tags = new Dictionary<string, object?>(record.Tags);

            // 1. 字段提取：从 message 中按正则提取结构化字段
            if (_fieldRegex != null && tags.TryGetValue("message", out var messageObj) && messageObj is string message)
            {
                var match = _fieldRegex.Match(message);
                if (match.Success)
                {
                    foreach (var groupName in _fieldRegex.GetGroupNames())
                    {
                        if (int.TryParse(groupName, out _))
                        {
                            continue;
                        }

                        var value = match.Groups[groupName].Value;
                        if (!string.IsNullOrEmpty(value))
                        {
                            tags[groupName] = value;
                        }
                    }
                }
            }

            // 2. 脱敏：对 message 和配置的字段应用正则替换
            if (_maskRules.Count > 0)
            {
                ApplyMaskRules(tags);
            }

            var transformed = new MeterRecord
            {
                Timestamp = record.Timestamp,
                Source = record.Source,
                Name = record.Name,
                Value = record.Value,
                Tags = tags
            };

            output.Add(transformed);
        }

        return Task.FromResult((IReadOnlyList<MeterRecord>)output);
    }

    private void ApplyMaskRules(IDictionary<string, object?> tags)
    {
        foreach (var (regex, rule) in _maskRules)
        {
            // 目标字段为空时，对 message 以及所有字符串字段生效
            if (rule.TargetFields == null || rule.TargetFields.Count == 0)
            {
                var keys = tags.Keys.ToList();
                foreach (var key in keys)
                {
                    if (tags[key] is string str && str.Length > 0)
                    {
                        tags[key] = regex.Replace(str, rule.Replacement);
                    }
                }
            }
            else
            {
                foreach (var field in rule.TargetFields)
                {
                    if (tags.TryGetValue(field, out var value) && value is string str && str.Length > 0)
                    {
                        tags[field] = regex.Replace(str, rule.Replacement);
                    }
                }
            }
        }
    }
}

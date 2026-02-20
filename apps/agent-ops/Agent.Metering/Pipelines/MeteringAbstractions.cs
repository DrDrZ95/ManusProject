namespace Agent.Metering.Pipelines;

public sealed class MeterRecord
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Source { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Value { get; init; }
    public IReadOnlyDictionary<string, object?> Tags { get; init; } = new Dictionary<string, object?>();
}

public enum DropPolicy
{
    DropNewest,
    DropOldest,
    Block
}

public sealed class BackpressureOptions
{
    public int ChannelCapacity { get; set; } = 10000;
    public int BatchSize { get; set; } = 256;
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
    public DropPolicy DropPolicy { get; set; } = DropPolicy.DropNewest;
}

public sealed class MeteringPipelineOptions
{
    public string Name { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public List<string> Processors { get; set; } = new();
    public List<string> Exporters { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public BackpressureOptions Backpressure { get; set; } = new();
}

public sealed class MeteringOptions
{
    public List<MeteringPipelineOptions> Pipelines { get; set; } = new();
}

public interface IMeteringReceiver
{
    string Name { get; }
    Task StartAsync(ChannelWriter<MeterRecord> writer, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public interface IMeteringProcessor
{
    string Name { get; }
    Task<IReadOnlyList<MeterRecord>> ProcessAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken);
}

public interface IMeteringExporter
{
    string Name { get; }
    Task ExportAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken);
}

public interface IMeteringComponentRegistry
{
    IMeteringReceiver? GetReceiver(string name);
    IReadOnlyList<IMeteringProcessor> GetProcessors(IEnumerable<string> names);
    IReadOnlyList<IMeteringExporter> GetExporters(IEnumerable<string> names);
}

public sealed class MeteringComponentRegistry : IMeteringComponentRegistry
{
    private readonly IReadOnlyDictionary<string, IMeteringReceiver> _receivers;
    private readonly IReadOnlyDictionary<string, IMeteringProcessor> _processors;
    private readonly IReadOnlyDictionary<string, IMeteringExporter> _exporters;

    public MeteringComponentRegistry(
        IEnumerable<IMeteringReceiver> receivers,
        IEnumerable<IMeteringProcessor> processors,
        IEnumerable<IMeteringExporter> exporters)
    {
        _receivers = receivers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _processors = processors.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _exporters = exporters.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IMeteringReceiver? GetReceiver(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _receivers.TryGetValue(name, out var receiver) ? receiver : null;
    }

    public IReadOnlyList<IMeteringProcessor> GetProcessors(IEnumerable<string> names)
    {
        var list = new List<IMeteringProcessor>();
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (_processors.TryGetValue(name, out var processor))
            {
                list.Add(processor);
            }
        }

        return list;
    }

    public IReadOnlyList<IMeteringExporter> GetExporters(IEnumerable<string> names)
    {
        var list = new List<IMeteringExporter>();
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (_exporters.TryGetValue(name, out var exporter))
            {
                list.Add(exporter);
            }
        }

        return list;
    }
}


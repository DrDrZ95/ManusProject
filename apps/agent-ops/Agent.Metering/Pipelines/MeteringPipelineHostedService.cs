namespace Agent.Metering.Pipelines;

public sealed class MeteringPipelineRunner
{
    private readonly IMeteringReceiver _receiver;
    private readonly IReadOnlyList<IMeteringProcessor> _processors;
    private readonly IReadOnlyList<IMeteringExporter> _exporters;
    private readonly BackpressureOptions _options;
    private readonly ILogger _logger;

    private Channel<MeterRecord>? _channel;
    private CancellationTokenSource? _cts;
    private Task? _receiverTask;
    private Task? _consumerTask;

    public string Name { get; }

    public MeteringPipelineRunner(
        string name,
        IMeteringReceiver receiver,
        IReadOnlyList<IMeteringProcessor> processors,
        IReadOnlyList<IMeteringExporter> exporters,
        BackpressureOptions options,
        ILogger logger)
    {
        Name = name;
        _receiver = receiver;
        _processors = processors;
        _exporters = exporters;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken applicationToken)
    {
        if (_cts != null)
        {
            return;
        }

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(applicationToken);
        _cts = linkedCts;

        var channelOptions = new BoundedChannelOptions(_options.ChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = _options.DropPolicy switch
            {
                DropPolicy.Block => BoundedChannelFullMode.Wait,
                DropPolicy.DropOldest => BoundedChannelFullMode.DropOldest,
                _ => BoundedChannelFullMode.DropWrite
            }
        };

        var channel = Channel.CreateBounded<MeterRecord>(channelOptions);
        _channel = channel;

        _receiverTask = Task.Run(() => _receiver.StartAsync(channel.Writer, linkedCts.Token), linkedCts.Token);
        _consumerTask = Task.Run(() => RunConsumerAsync(channel.Reader, linkedCts.Token), linkedCts.Token);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts == null)
        {
            return;
        }

        try
        {
            _cts.Cancel();
            _channel?.Writer.TryComplete();

            var tasks = new List<Task>();
            if (_receiverTask != null)
            {
                tasks.Add(_receiverTask);
            }

            if (_consumerTask != null)
            {
                tasks.Add(_consumerTask);
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _channel = null;
            _receiverTask = null;
            _consumerTask = null;
        }

        await Task.CompletedTask;
    }

    private async Task RunConsumerAsync(ChannelReader<MeterRecord> reader, CancellationToken cancellationToken)
    {
        var batch = new List<MeterRecord>(_options.BatchSize);

        while (!cancellationToken.IsCancellationRequested)
        {
            batch.Clear();
            var delayTask = Task.Delay(_options.FlushInterval, cancellationToken);

            while (batch.Count < _options.BatchSize && !cancellationToken.IsCancellationRequested)
            {
                var readTask = reader.ReadAsync(cancellationToken).AsTask();
                var completed = await Task.WhenAny(readTask, delayTask);

                if (completed == delayTask)
                {
                    break;
                }

                var record = await readTask;
                batch.Add(record);
            }

            if (batch.Count == 0)
            {
                continue;
            }

            IReadOnlyList<MeterRecord> current = batch;

            foreach (var processor in _processors)
            {
                current = await processor.ProcessAsync(current, cancellationToken);
            }

            foreach (var exporter in _exporters)
            {
                await exporter.ExportAsync(current, cancellationToken);
            }
        }
    }
}

public sealed class MeteringHealthSnapshot
{
    public DateTime LastReloadTime { get; init; }
    public int PipelineCount { get; init; }
    public IReadOnlyList<string> ActiveScenarios { get; init; } = Array.Empty<string>();
}

public interface IMeteringHealthSnapshotProvider
{
    MeteringHealthSnapshot GetSnapshot();
}

public interface IScenarioManager
{
    Task StartScenarioAsync(string name, CancellationToken cancellationToken);
    Task StopScenarioAsync(string name, CancellationToken cancellationToken);
    IReadOnlyCollection<string> GetActiveScenarios();
}

public sealed class MeteringPipelineHostedService : BackgroundService, IMeteringHealthSnapshotProvider, IScenarioManager
{
    private readonly ILogger<MeteringPipelineHostedService> _logger;
    private readonly IMeteringComponentRegistry _registry;
    private readonly IOptionsMonitor<MeteringOptions> _optionsMonitor;

    private readonly object _syncRoot = new();
    private List<MeteringPipelineRunner> _pipelines = new();
    private IDisposable? _reloadSubscription;
    private readonly ConcurrentDictionary<string, byte> _activeScenarios = new();

    private DateTime _lastReloadTime = DateTime.MinValue;

    public MeteringPipelineHostedService(
        ILogger<MeteringPipelineHostedService> logger,
        IMeteringComponentRegistry registry,
        IOptionsMonitor<MeteringOptions> optionsMonitor)
    {
        _logger = logger;
        _registry = registry;
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        BuildPipelines(_optionsMonitor.CurrentValue, stoppingToken);

        _reloadSubscription = _optionsMonitor.OnChange(options =>
        {
            BuildPipelines(options, stoppingToken);
        });

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _reloadSubscription?.Dispose();

        List<MeteringPipelineRunner> pipelines;
        lock (_syncRoot)
        {
            pipelines = _pipelines.ToList();
            _pipelines.Clear();
        }

        foreach (var pipeline in pipelines)
        {
            await pipeline.StopAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    public MeteringHealthSnapshot GetSnapshot()
    {
        List<MeteringPipelineRunner> pipelines;
        lock (_syncRoot)
        {
            pipelines = _pipelines.ToList();
        }

        var scenarios = _activeScenarios.Keys.ToList();

        return new MeteringHealthSnapshot
        {
            LastReloadTime = _lastReloadTime,
            PipelineCount = pipelines.Count,
            ActiveScenarios = scenarios
        };
    }

    public Task StartScenarioAsync(string name, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _activeScenarios[name] = 1;
        }

        return Task.CompletedTask;
    }

    public Task StopScenarioAsync(string name, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _activeScenarios.TryRemove(name, out _);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyCollection<string> GetActiveScenarios()
    {
        return _activeScenarios.Keys.ToList();
    }

    private void BuildPipelines(MeteringOptions options, CancellationToken applicationToken)
    {
        var newPipelines = new List<MeteringPipelineRunner>();

        foreach (var pipeline in options.Pipelines)
        {
            if (!pipeline.Enabled)
            {
                continue;
            }

            var receiver = _registry.GetReceiver(pipeline.Receiver);
            if (receiver == null)
            {
                _logger.LogWarning("Receiver {Receiver} not found for pipeline {Pipeline}", pipeline.Receiver, pipeline.Name);
                continue;
            }

            var processors = _registry.GetProcessors(pipeline.Processors);
            var exporters = _registry.GetExporters(pipeline.Exporters);

            if (exporters.Count == 0)
            {
                _logger.LogWarning("No exporters configured for pipeline {Pipeline}", pipeline.Name);
                continue;
            }

            var runnerLogger = _logger;

            var runner = new MeteringPipelineRunner(
                pipeline.Name,
                receiver,
                processors,
                exporters,
                pipeline.Backpressure,
                runnerLogger);

            newPipelines.Add(runner);
        }

        List<MeteringPipelineRunner> oldPipelines;
        lock (_syncRoot)
        {
            oldPipelines = _pipelines.ToList();
            _pipelines = newPipelines;
            _lastReloadTime = DateTime.UtcNow;
        }

        foreach (var pipeline in newPipelines)
        {
            _ = pipeline.StartAsync(applicationToken);
        }

        foreach (var pipeline in oldPipelines)
        {
            _ = pipeline.StopAsync(CancellationToken.None);
        }
    }
}


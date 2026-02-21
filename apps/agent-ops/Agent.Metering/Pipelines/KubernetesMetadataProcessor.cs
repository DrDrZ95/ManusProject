namespace Agent.Metering.Pipelines;

public sealed class KubernetesMetadataOptions
{
    public bool Enabled { get; set; } = true;
    public string PodNameEnv { get; set; } = "POD_NAME";
    public string NamespaceEnv { get; set; } = "POD_NAMESPACE";
    public string NodeNameEnv { get; set; } = "NODE_NAME";
    public string? PodLabelsPath { get; set; }
    public string? PodAnnotationsPath { get; set; }
    public bool UseClusterAgent { get; set; }
    public string? ClusterAgentBaseUrl { get; set; }
}

public sealed class KubernetesMetadataProcessor : IMeteringProcessor
{
    private readonly string _name;
    private readonly KubernetesMetadataOptions _options;
    private readonly ILogger<KubernetesMetadataProcessor> _logger;
    private readonly object _initLock = new();
    private bool _initialized;
    private Dictionary<string, object?> _staticTags = new();

    public string Name => _name;

    public KubernetesMetadataProcessor(string name, KubernetesMetadataOptions options, ILogger<KubernetesMetadataProcessor> logger)
    {
        _name = name;
        _options = options;
        _logger = logger;
    }

    public Task<IReadOnlyList<MeterRecord>> ProcessAsync(IReadOnlyList<MeterRecord> batch, CancellationToken cancellationToken)
    {
        if (!_options.Enabled || batch.Count == 0)
        {
            return Task.FromResult(batch);
        }

        EnsureInitialized();

        if (_staticTags.Count == 0)
        {
            return Task.FromResult(batch);
        }

        var output = new List<MeterRecord>(batch.Count);

        foreach (var record in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tags = new Dictionary<string, object?>(record.Tags);

            foreach (var kvp in _staticTags)
            {
                if (!tags.ContainsKey(kvp.Key))
                {
                    tags[kvp.Key] = kvp.Value;
                }
            }

            var enriched = new MeterRecord
            {
                Timestamp = record.Timestamp,
                Source = record.Source,
                Name = record.Name,
                Value = record.Value,
                Tags = tags
            };

            output.Add(enriched);
        }

        return Task.FromResult((IReadOnlyList<MeterRecord>)output);
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (_initLock)
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                var tags = new Dictionary<string, object?>();

                var podName = Environment.GetEnvironmentVariable(_options.PodNameEnv);
                var ns = Environment.GetEnvironmentVariable(_options.NamespaceEnv);
                var nodeName = Environment.GetEnvironmentVariable(_options.NodeNameEnv);

                if (!string.IsNullOrWhiteSpace(podName))
                {
                    tags["k8s.pod.name"] = podName;
                }

                if (!string.IsNullOrWhiteSpace(ns))
                {
                    tags["k8s.namespace.name"] = ns;
                }

                if (!string.IsNullOrWhiteSpace(nodeName))
                {
                    tags["k8s.node.name"] = nodeName;
                }

                if (!string.IsNullOrWhiteSpace(_options.PodLabelsPath) && File.Exists(_options.PodLabelsPath))
                {
                    var labelsText = File.ReadAllText(_options.PodLabelsPath);
                    var labels = ParseKeyValueLines(labelsText);
                    foreach (var kvp in labels)
                    {
                        tags[$"k8s.pod.label.{kvp.Key}"] = kvp.Value;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_options.PodAnnotationsPath) && File.Exists(_options.PodAnnotationsPath))
                {
                    var annotationsText = File.ReadAllText(_options.PodAnnotationsPath);
                    var annotations = ParseKeyValueLines(annotationsText);
                    foreach (var kvp in annotations)
                    {
                        tags[$"k8s.pod.annotation.{kvp.Key}"] = kvp.Value;
                    }
                }

                _staticTags = tags;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "KubernetesMetadataProcessor initialization failed, metadata enrichment will be disabled.");
                _staticTags = new Dictionary<string, object?>();
            }
            finally
            {
                _initialized = true;
            }
        }
    }

    private static Dictionary<string, string> ParseKeyValueLines(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StringReader(text);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var idx = line.IndexOf('=');
            if (idx <= 0 || idx == line.Length - 1)
            {
                continue;
            }

            var key = line.Substring(0, idx).Trim();
            var value = line.Substring(idx + 1).Trim();

            if (key.Length == 0)
            {
                continue;
            }

            if (!result.ContainsKey(key))
            {
                result[key] = value;
            }
        }

        return result;
    }
}


namespace Agent.Application.Services.Telemetry;

public class AgentTelemetryProvider : IAgentTelemetryProvider
{
    private readonly ActivitySource _activitySource;

    public AgentTelemetryProvider(string activitySourceName)
    {
        _activitySource = new ActivitySource(activitySourceName);
    }

    public IAgentSpan StartSpan(string operationName, SpanKind kind = SpanKind.Internal)
    {
        var activity = _activitySource.StartActivity(operationName, GetActivityKind(kind));
        return new AgentSpan(activity);
    }

    public void RecordMetric<T>(string name, T value, params KeyValuePair<string, object?>[] tags)
    {
        // In a real scenario, you would use OpenTelemetry Metrics API here.
        // For simplicity, we'll just log it or add as event for now.
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            currentActivity.AddEvent(new ActivityEvent($"Metric: {name}", tags: new ActivityTagsCollection(tags.Append(new KeyValuePair<string, object?>("value", value)))));
        }
    }

    public void RecordEvent(string name, object? data = null)
    {
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            var tags = new ActivityTagsCollection();
            if (data != null)
            {
                foreach (var prop in data.GetType().GetProperties())
                {
                    tags.Add(prop.Name, prop.GetValue(data));
                }
            }
            currentActivity.AddEvent(new ActivityEvent(name, tags: tags));
        }
    }

    private ActivityKind GetActivityKind(SpanKind kind)
    {
        return kind switch
        {
            SpanKind.Client => ActivityKind.Client,
            SpanKind.Consumer => ActivityKind.Consumer,
            SpanKind.Internal => ActivityKind.Internal,
            SpanKind.Producer => ActivityKind.Producer,
            SpanKind.Server => ActivityKind.Server,
            _ => ActivityKind.Internal,
        };
    }
}

public class AgentSpan : IAgentSpan
{
    private readonly Activity? _activity;

    public AgentSpan(Activity? activity)
    {
        _activity = activity;
    }

    public void SetAttribute(string key, object? value)
    {
        _activity?.SetTag(key, value);
    }

    public void RecordException(Exception exception)
    {
        if (_activity == null)
        {
            return;
        }

        var tags = new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName ?? string.Empty },
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.StackTrace ?? string.Empty }
        };

        _activity.AddEvent(new ActivityEvent("exception", tags: tags));
    }

    public void SetStatus(ActivityStatusCode statusCode, string? description = null)
    {
        _activity?.SetStatus(statusCode, description);
    }

    public void AddEvent(string name, object? data = null)
    {
        var tags = new ActivityTagsCollection();
        if (data != null)
        {
            foreach (var prop in data.GetType().GetProperties())
            {
                tags.Add(prop.Name, prop.GetValue(data));
            }
        }
        _activity?.AddEvent(new ActivityEvent(name, tags: tags));
    }

    public void Dispose()
    {
        _activity?.Dispose();
    }
}

public enum SpanKind
{
    Internal,
    Server,
    Client,
    Producer,
    Consumer
}


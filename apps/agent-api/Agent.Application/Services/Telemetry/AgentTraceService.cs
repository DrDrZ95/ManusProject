namespace Agent.Application.Services.Telemetry;

public enum TraceType
{
    Thinking,
    ToolCall,
    Result
}

public class AgentTrace
{
    public string TraceId { get; set; } = Guid.NewGuid().ToString();

    public string SessionId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public TraceType Type { get; set; }

    public object? Data { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public double CostUsd { get; set; }

    public int TokenCount { get; set; }
}

public interface IAgentTraceService
{
    Task<AgentTrace> RecordTraceAsync(AgentTrace trace, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentTrace>> GetTraceHistoryAsync(string sessionId, CancellationToken cancellationToken = default);
}

public class AgentTraceService : IAgentTraceService
{
    private readonly IRepository<AgentTraceEntity, Guid> _repository;
    private readonly IHubContext<AIAgentHub> _hubContext;
    private readonly IAgentTelemetryProvider _telemetryProvider;
    private readonly ILogger<AgentTraceService> _logger;

    public AgentTraceService(
        IRepository<AgentTraceEntity, Guid> repository,
        IHubContext<AIAgentHub> hubContext,
        IAgentTelemetryProvider telemetryProvider,
        ILogger<AgentTraceService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _telemetryProvider = telemetryProvider ?? throw new ArgumentNullException(nameof(telemetryProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AgentTrace> RecordTraceAsync(AgentTrace trace, CancellationToken cancellationToken = default)
    {
        using var span = _telemetryProvider.StartSpan("AgentTrace.Record", SpanKind.Producer);
        span.SetAttribute("agent.trace.type", trace.Type.ToString());
        span.SetAttribute("agent.trace.session_id", trace.SessionId);

        try
        {
            var entity = new AgentTraceEntity
            {
                Id = Guid.NewGuid(),
                TraceId = string.IsNullOrWhiteSpace(trace.TraceId) ? Guid.NewGuid().ToString() : trace.TraceId,
                SessionId = string.IsNullOrWhiteSpace(trace.SessionId) ? "unknown" : trace.SessionId,
                Timestamp = trace.Timestamp == default ? DateTime.UtcNow : trace.Timestamp,
                Type = trace.Type.ToString(),
                Data = trace.Data != null ? JsonSerializer.Serialize(trace.Data) : null,
                Metadata = trace.Metadata != null ? JsonSerializer.Serialize(trace.Metadata) : null,
                CostUsd = trace.CostUsd > 0 ? (decimal?)trace.CostUsd : null,
                TokenCount = trace.TokenCount > 0 ? trace.TokenCount : null
            };

            await _repository.AddAsync(entity, cancellationToken);

            var normalizedTrace = new AgentTrace
            {
                TraceId = entity.TraceId,
                SessionId = entity.SessionId,
                Timestamp = entity.Timestamp,
                Type = Enum.TryParse<TraceType>(entity.Type, out var parsedType) ? parsedType : trace.Type,
                Data = trace.Data,
                Metadata = trace.Metadata,
                CostUsd = trace.CostUsd,
                TokenCount = trace.TokenCount
            };

            await _hubContext.Clients.All.SendAsync("AgentTraceUpdated", normalizedTrace, cancellationToken);

            return normalizedTrace;
        }
        catch (Exception ex)
        {
            span.RecordException(ex);
            span.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to record agent trace");
            throw;
        }
    }

    public async Task<IReadOnlyList<AgentTrace>> GetTraceHistoryAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        using var span = _telemetryProvider.StartSpan("AgentTrace.History", SpanKind.Internal);
        span.SetAttribute("agent.trace.session_id", sessionId);

        try
        {
            var entities = await _repository.FindAsync(e => e.SessionId == sessionId, cancellationToken);
            var ordered = entities.OrderBy(e => e.Timestamp).ToList();

            var traces = new List<AgentTrace>(ordered.Count);

            foreach (var entity in ordered)
            {
                Dictionary<string, object>? metadata = null;
                if (!string.IsNullOrEmpty(entity.Metadata))
                {
                    metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata);
                }

                object? data = null;
                if (!string.IsNullOrEmpty(entity.Data))
                {
                    data = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Data);
                }

                var trace = new AgentTrace
                {
                    TraceId = entity.TraceId,
                    SessionId = entity.SessionId,
                    Timestamp = entity.Timestamp,
                    Type = Enum.TryParse<TraceType>(entity.Type, out var parsedType) ? parsedType : TraceType.Result,
                    Data = data,
                    Metadata = metadata,
                    CostUsd = entity.CostUsd.HasValue ? (double)entity.CostUsd.Value : 0,
                    TokenCount = entity.TokenCount ?? 0
                };

                traces.Add(trace);
            }

            return traces;
        }
        catch (Exception ex)
        {
            span.RecordException(ex);
            span.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to get trace history for session {SessionId}", sessionId);
            throw;
        }
    }
}


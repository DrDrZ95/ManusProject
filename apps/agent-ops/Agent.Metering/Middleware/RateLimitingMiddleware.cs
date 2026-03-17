namespace Agent.Metering.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly RateLimitOptions _options;

    private const string Script = @"
local key = KEYS[1]
local capacity = tonumber(ARGV[1])
local rate = tonumber(ARGV[2])
local now = tonumber(ARGV[3])
local ttl = tonumber(ARGV[4])

local tokens = tonumber(redis.call('HGET', key, 'tokens')) or capacity
local ts = tonumber(redis.call('HGET', key, 'ts')) or now

local delta = math.max(0, now - ts)
tokens = math.min(capacity, tokens + delta * rate)

local allowed = 0
if tokens >= 1 then
  tokens = tokens - 1
  allowed = 1
end

redis.call('HSET', key, 'tokens', tokens, 'ts', now)
redis.call('EXPIRE', key, ttl)

local remaining = math.floor(tokens)
local reset = 0
if tokens < 1 then
  reset = math.ceil((1 - tokens) / rate)
end

return {allowed, remaining, reset}
";

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IConnectionMultiplexer redis,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _logger = logger;
        _redis = redis;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = context.User?.IsInRole("admin") == true || context.User?.IsInRole("Administrator") == true;
        var identity = !string.IsNullOrEmpty(userId) ? userId : (context.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        var rpm = !string.IsNullOrEmpty(userId)
            ? (isAdmin ? _options.AdminRpm : _options.AuthenticatedRpm)
            : _options.AnonymousRpm;

        var capacity = rpm;
        var ratePerSecond = rpm / 60.0;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var ttlSeconds = 60;

        var key = $"ratelimit:{identity}";

        var db = _redis.GetDatabase();
        var result = (RedisResult[])(await db.ScriptEvaluateAsync(
            Script,
            new RedisKey[] { key },
            new RedisValue[] { capacity, ratePerSecond, now, ttlSeconds }));

        var allowed = (int)result[0] == 1;
        var remaining = (int)result[1];
        var reset = (int)result[2];

        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = reset.ToString();

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests");
            return;
        }

        await _next(context);
    }
}

public sealed class RateLimitOptions
{
    public int AnonymousRpm { get; set; } = 30;
    public int AuthenticatedRpm { get; set; } = 200;
    public int AdminRpm { get; set; } = 1000;
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}


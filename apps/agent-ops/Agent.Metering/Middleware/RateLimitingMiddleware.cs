
namespace Agent.Metering.Middleware
{
    /// <summary>
    /// API请求限流中间件
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, (DateTime FirstRequest, int RequestCount)> _clientRequestCounts = new ConcurrentDictionary<string, (DateTime, int)>();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _maxRequests = 5; // 示例：每5秒最多5个请求
            _timeWindow = TimeSpan.FromSeconds(5);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(clientIp))
            {
                await _next(context);
                return;
            }

            // 1. 获取或创建客户端请求计数
            var clientEntry = _clientRequestCounts.GetOrAdd(clientIp, _ => (DateTime.UtcNow, 0));

            // 2. 检查时间窗口
            if ((DateTime.UtcNow - clientEntry.FirstRequest) > _timeWindow)
            {
                // 超过时间窗口，重置计数
                _clientRequestCounts.TryUpdate(clientIp, (DateTime.UtcNow, 1), clientEntry);
            }
            else
            {
                // 仍在时间窗口内，增加计数
                _clientRequestCounts.TryUpdate(clientIp, (clientEntry.FirstRequest, clientEntry.RequestCount + 1), clientEntry);
            }

            // 3. 检查请求是否超过限制
            if (_clientRequestCounts[clientIp].RequestCount > _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests; // 429 Too Many Requests
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            await _next(context);
        }
    }

    /// <summary>
    /// RateLimitingMiddleware的扩展方法
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}


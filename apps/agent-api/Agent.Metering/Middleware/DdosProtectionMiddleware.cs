using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace Agent.Metering.Middleware
{
    /// <summary>
    /// 简单的DDoS保护中间件
    /// </summary>
    public class DdosProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DdosProtectionMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, (DateTime LastRequest, int RequestCount)> _ipRequestData = new ConcurrentDictionary<string, (DateTime, int)>();
        private readonly int _burstLimit;
        private readonly TimeSpan _burstWindow;
        private readonly TimeSpan _blockDuration;
        private static readonly ConcurrentDictionary<string, DateTime> _blockedIps = new ConcurrentDictionary<string, DateTime>();

        public DdosProtectionMiddleware(RequestDelegate next, ILogger<DdosProtectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _burstLimit = 10; // 示例：在短时间内允许的最大请求数
            _burstWindow = TimeSpan.FromSeconds(1); // 示例：短时间窗口
            _blockDuration = TimeSpan.FromMinutes(5); // 示例：IP被阻止的持续时间
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(clientIp))
            {
                await _next(context);
                return;
            }

            // 1. 检查IP是否被阻止
            if (_blockedIps.TryGetValue(clientIp, out var unblockTime) && DateTime.UtcNow < unblockTime)
            {
                _logger.LogWarning("Blocked IP {ClientIp} attempted to access resource.", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden; // 403 Forbidden
                await context.Response.WriteAsync("Access denied due to suspicious activity.");
                return;
            }
            else if (DateTime.UtcNow >= unblockTime) // 如果阻止时间已过，则移除
            {
                _blockedIps.TryRemove(clientIp, out _);
            }

            // 2. 更新请求数据
            _ipRequestData.AddOrUpdate(clientIp,
                (DateTime.UtcNow, 1), // 如果是新IP，则添加
                (key, existingVal) =>
                {
                    if ((DateTime.UtcNow - existingVal.LastRequest) > _burstWindow)
                    {
                        return (DateTime.UtcNow, 1); // 如果超出时间窗口，重置计数
                    }
                    else
                    {
                        return (DateTime.UtcNow, existingVal.RequestCount + 1); // 否则增加计数
                    }
                });

            // 3. 检查是否达到DDoS阈值
            if (_ipRequestData.TryGetValue(clientIp, out var currentRequestData) && currentRequestData.RequestCount > _burstLimit)
            {
                _logger.LogError("DDoS attack detected from IP: {ClientIp}. Blocking for {BlockDuration} minutes.", clientIp, _blockDuration.TotalMinutes);
                _blockedIps.TryAdd(clientIp, DateTime.UtcNow.Add(_blockDuration)); // 阻止IP
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden; // 403 Forbidden
                await context.Response.WriteAsync("Access denied due to suspicious activity.");
                return;
            }

            await _next(context);
        }
    }

    /// <summary>
    /// DdosProtectionMiddleware的扩展方法
    /// </summary>
    public static class DdosProtectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseDdosProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DdosProtectionMiddleware>();
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using AgentWebApi.Hubs;

namespace AgentWebApi.Extensions;

/// <summary>
/// SignalR extensions for dependency injection
/// SignalR依赖注入扩展
/// 
/// 为AI-Agent系统提供实时通信服务的注册
/// Provides service registration for real-time communication in AI-Agent system
/// </summary>
public static class SignalRExtensions
{
    /// <summary>
    /// Add SignalR services for AI-Agent system
    /// 为AI-Agent系统添加SignalR服务
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddSignalRServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加SignalR服务 - Add SignalR services
        var signalRBuilder = services.AddSignalR(options =>
        {
            // 配置SignalR选项 - Configure SignalR options
            options.EnableDetailedErrors = configuration.GetValue<bool>("SignalR:EnableDetailedErrors", false);
            options.KeepAliveInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("SignalR:KeepAliveIntervalSeconds", 15));
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("SignalR:ClientTimeoutIntervalSeconds", 30));
            options.HandshakeTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("SignalR:HandshakeTimeoutSeconds", 15));
            
            // 配置最大消息大小 - Configure maximum message size
            options.MaximumReceiveMessageSize = configuration.GetValue<long>("SignalR:MaximumReceiveMessageSize", 1024 * 1024); // 1MB
            
            // 启用详细错误（仅开发环境） - Enable detailed errors (development only)
            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                options.EnableDetailedErrors = true;
            }
        });

        // 配置JSON序列化选项 - Configure JSON serialization options
        signalRBuilder.AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = false;
            options.PayloadSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        // 在生产环境中可以添加Redis背板 - Can add Redis backplane in production
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = "ai-agent-signalr";
            });
        }

        // 添加SignalR相关服务 - Add SignalR related services
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
        services.AddScoped<ISignalRConnectionManager, SignalRConnectionManager>();

        return services;
    }

    /// <summary>
    /// Configure SignalR middleware and hubs
    /// 配置SignalR中间件和集线器
    /// </summary>
    /// <param name="app">Application builder - 应用程序构建器</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Application builder - 应用程序构建器</returns>
    public static IApplicationBuilder UseSignalRServices(this IApplicationBuilder app, IConfiguration configuration)
    {
        // 配置CORS以支持SignalR - Configure CORS to support SignalR
        app.UseCors(policy =>
        {
            var allowedOrigins = configuration.GetSection("SignalR:AllowedOrigins").Get<string[]>() ?? 
                new[] { "https://localhost:5173", "https://localhost:3000", "http://localhost:5173", "http://localhost:3000" };

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // SignalR需要凭据 - SignalR requires credentials
        });

        // 映射SignalR集线器 - Map SignalR hubs
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            // AI-Agent主集线器 - AI-Agent main hub
            endpoints.MapHub<AIAgentHub>("/hubs/ai-agent", options =>
            {
                // 配置集线器选项 - Configure hub options
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                                    Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                
                // 配置WebSocket选项 - Configure WebSocket options
                options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
                options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
            });

            // 可以添加更多专用集线器 - Can add more specialized hubs
            // endpoints.MapHub<FinetuneHub>("/hubs/finetune");
            // endpoints.MapHub<RAGHub>("/hubs/rag");
        });

        return app;
    }

    /// <summary>
    /// Add health checks for SignalR
    /// 为SignalR添加健康检查
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddSignalRHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // 添加SignalR健康检查 - Add SignalR health check
        healthChecksBuilder.AddCheck<SignalRHealthCheck>("signalr", tags: new[] { "signalr", "realtime" });

        // 如果使用Redis背板，添加Redis健康检查 - If using Redis backplane, add Redis health check
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(redisConnectionString, name: "signalr_redis", tags: new[] { "redis", "signalr" });
        }

        return services;
    }
}

/// <summary>
/// SignalR notification service interface
/// SignalR通知服务接口
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>
    /// Send notification to specific user
    /// 向特定用户发送通知
    /// </summary>
    /// <param name="userId">User ID - 用户ID</param>
    /// <param name="message">Notification message - 通知消息</param>
    /// <param name="data">Additional data - 附加数据</param>
    /// <returns>Task</returns>
    Task SendNotificationToUserAsync(string userId, string message, object? data = null);

    /// <summary>
    /// Send notification to users with specific role
    /// 向具有特定角色的用户发送通知
    /// </summary>
    /// <param name="role">User role - 用户角色</param>
    /// <param name="message">Notification message - 通知消息</param>
    /// <param name="data">Additional data - 附加数据</param>
    /// <returns>Task</returns>
    Task SendNotificationToRoleAsync(string role, string message, object? data = null);

    /// <summary>
    /// Send fine-tuning job update
    /// 发送微调任务更新
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <param name="status">Job status - 任务状态</param>
    /// <param name="progress">Progress percentage - 进度百分比</param>
    /// <returns>Task</returns>
    Task SendFinetuneJobUpdateAsync(string jobId, string status, int progress);

    /// <summary>
    /// Send RAG query result
    /// 发送RAG查询结果
    /// </summary>
    /// <param name="userId">User ID - 用户ID</param>
    /// <param name="queryId">Query ID - 查询ID</param>
    /// <param name="result">Query result - 查询结果</param>
    /// <returns>Task</returns>
    Task SendRAGQueryResultAsync(string userId, string queryId, object result);
}

/// <summary>
/// SignalR notification service implementation
/// SignalR通知服务实现
/// </summary>
public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<AIAgentHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IHubContext<AIAgentHub> hubContext, ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Send notification to specific user
    /// 向特定用户发送通知
    /// </summary>
    public async Task SendNotificationToUserAsync(string userId, string message, object? data = null)
    {
        try
        {
            await _hubContext.Clients.User(userId).SendAsync("Notification", new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Notification sent to user - 通知已发送给用户: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user - 向用户发送通知失败: {UserId}", userId);
        }
    }

    /// <summary>
    /// Send notification to users with specific role
    /// 向具有特定角色的用户发送通知
    /// </summary>
    public async Task SendNotificationToRoleAsync(string role, string message, object? data = null)
    {
        try
        {
            await _hubContext.Clients.Group($"Role_{role}").SendAsync("Notification", new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Notification sent to role - 通知已发送给角色: {Role}", role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to role - 向角色发送通知失败: {Role}", role);
        }
    }

    /// <summary>
    /// Send fine-tuning job update
    /// 发送微调任务更新
    /// </summary>
    public async Task SendFinetuneJobUpdateAsync(string jobId, string status, int progress)
    {
        try
        {
            await _hubContext.Clients.Group($"FinetuneJob_{jobId}").SendAsync("FinetuneJobUpdate", new
            {
                JobId = jobId,
                Status = status,
                Progress = progress,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Fine-tuning job update sent - 微调任务更新已发送: {JobId} - {Status} ({Progress}%)", jobId, status, progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send fine-tuning job update - 发送微调任务更新失败: {JobId}", jobId);
        }
    }

    /// <summary>
    /// Send RAG query result
    /// 发送RAG查询结果
    /// </summary>
    public async Task SendRAGQueryResultAsync(string userId, string queryId, object result)
    {
        try
        {
            await _hubContext.Clients.User(userId).SendAsync("RAGQueryResult", new
            {
                QueryId = queryId,
                Result = result,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("RAG query result sent to user - RAG查询结果已发送给用户: {UserId} - {QueryId}", userId, queryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send RAG query result - 发送RAG查询结果失败: {UserId} - {QueryId}", userId, queryId);
        }
    }
}

/// <summary>
/// SignalR connection manager interface
/// SignalR连接管理器接口
/// </summary>
public interface ISignalRConnectionManager
{
    /// <summary>
    /// Get active connections count
    /// 获取活跃连接数
    /// </summary>
    /// <returns>Active connections count - 活跃连接数</returns>
    Task<int> GetActiveConnectionsCountAsync();

    /// <summary>
    /// Get connections for specific user
    /// 获取特定用户的连接
    /// </summary>
    /// <param name="userId">User ID - 用户ID</param>
    /// <returns>Connection IDs - 连接ID列表</returns>
    Task<IEnumerable<string>> GetUserConnectionsAsync(string userId);

    /// <summary>
    /// Check if user is online
    /// 检查用户是否在线
    /// </summary>
    /// <param name="userId">User ID - 用户ID</param>
    /// <returns>True if online - 如果在线则返回true</returns>
    Task<bool> IsUserOnlineAsync(string userId);
}

/// <summary>
/// SignalR connection manager implementation
/// SignalR连接管理器实现
/// </summary>
public class SignalRConnectionManager : ISignalRConnectionManager
{
    private readonly IHubContext<AIAgentHub> _hubContext;
    private readonly ILogger<SignalRConnectionManager> _logger;
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    public SignalRConnectionManager(IHubContext<AIAgentHub> hubContext, ILogger<SignalRConnectionManager> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get active connections count
    /// 获取活跃连接数
    /// </summary>
    public async Task<int> GetActiveConnectionsCountAsync()
    {
        await Task.CompletedTask; // 占位符，实际实现可能需要更复杂的逻辑 - Placeholder, actual implementation may need more complex logic
        return _userConnections.Values.Sum(connections => connections.Count);
    }

    /// <summary>
    /// Get connections for specific user
    /// 获取特定用户的连接
    /// </summary>
    public async Task<IEnumerable<string>> GetUserConnectionsAsync(string userId)
    {
        await Task.CompletedTask; // 占位符 - Placeholder
        return _userConnections.TryGetValue(userId, out var connections) ? connections.ToList() : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Check if user is online
    /// 检查用户是否在线
    /// </summary>
    public async Task<bool> IsUserOnlineAsync(string userId)
    {
        await Task.CompletedTask; // 占位符 - Placeholder
        return _userConnections.ContainsKey(userId) && _userConnections[userId].Any();
    }
}

/// <summary>
/// SignalR health check
/// SignalR健康检查
/// </summary>
public class SignalRHealthCheck : IHealthCheck
{
    private readonly ISignalRConnectionManager _connectionManager;
    private readonly ILogger<SignalRHealthCheck> _logger;

    public SignalRHealthCheck(ISignalRConnectionManager connectionManager, ILogger<SignalRHealthCheck> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check health status
    /// 检查健康状态
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing SignalR health check - 执行SignalR健康检查");

            var activeConnections = await _connectionManager.GetActiveConnectionsCountAsync();

            var data = new Dictionary<string, object>
            {
                ["active_connections"] = activeConnections,
                ["check_time"] = DateTime.UtcNow
            };

            _logger.LogInformation("SignalR health check passed - SignalR健康检查通过. Active connections: {ActiveConnections}", activeConnections);
            return HealthCheckResult.Healthy("SignalR is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR health check error - SignalR健康检查错误");
            
            return HealthCheckResult.Unhealthy(
                "SignalR health check failed with exception", 
                ex, 
                new Dictionary<string, object> { ["exception"] = ex.Message });
        }
    }
}


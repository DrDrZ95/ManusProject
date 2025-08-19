using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Agent.Api.Extensions;

/// <summary>
/// SignalR configuration extensions with automatic reconnection and JWT authentication
/// SignalR配置扩展，包含自动重连和JWT认证
/// </summary>
public static class SignalRExtensions
{
    /// <summary>
    /// Add SignalR services with automatic reconnection and JWT authentication
    /// 添加SignalR服务，包含自动重连和JWT认证
    /// </summary>
    /// <param name="services">Service collection - 服务集合</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Service collection - 服务集合</returns>
    public static IServiceCollection AddSignalRServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add JWT authentication for SignalR
        // 为SignalR添加JWT认证
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
                };

                // Configure JWT for SignalR
                // 为SignalR配置JWT
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        // 如果请求是针对我们的hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && 
                            (path.StartsWithSegments("/aiagentHub") || path.StartsWithSegments("/chathub")))
                        {
                            // Read the token out of the query string
                            // 从查询字符串中读取token
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Add authorization policies for SignalR
        // 为SignalR添加授权策略
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.SignalRAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "signalr.access");
            });

            options.AddPolicy(AuthorizationPolicies.RagAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "rag.access");
            });

            options.AddPolicy(AuthorizationPolicies.FinetuneAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "finetune.access");
            });
        });

        // Add SignalR with automatic reconnection configuration
        // 添加SignalR并配置自动重连
        services.AddSignalR(options =>
        {
            // Configure automatic reconnection settings
            // 配置自动重连设置
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            
            // Maximum message size (1MB)
            // 最大消息大小 (1MB)
            options.MaximumReceiveMessageSize = 1024 * 1024;
            
            // Stream buffer capacity
            // 流缓冲区容量
            options.StreamBufferCapacity = 10;
        })
        .AddJsonProtocol(options =>
        {
            // Configure JSON serialization for SignalR
            // 为SignalR配置JSON序列化
            options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = false;
        });

        return services;
    }

    /// <summary>
    /// Configure SignalR middleware with automatic reconnection
    /// 配置SignalR中间件并启用自动重连
    /// </summary>
    /// <param name="app">Web application - Web应用程序</param>
    /// <param name="configuration">Configuration - 配置</param>
    /// <returns>Web application - Web应用程序</returns>
    public static WebApplication UseSignalRServices(this WebApplication app, IConfiguration configuration)
    {
        // Use authentication and authorization
        // 使用认证和授权
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure CORS for SignalR
        // 为SignalR配置CORS
        app.UseCors(policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://localhost:3000",
                "https://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });

        // Map SignalR hubs with automatic reconnection
        // 映射SignalR hubs并启用自动重连
        app.MapHub<AIAgentHub>("/aiagentHub", options =>
        {
            // Configure transport options for automatic reconnection
            // 为自动重连配置传输选项
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            
            // Configure automatic reconnection intervals
            // 配置自动重连间隔
            options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
            options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
        });

        // Alternative hub mapping for compatibility
        // 兼容性的替代hub映射
        app.MapHub<AIAgentHub>("/chathub", options =>
        {
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
            options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
        });

        return app;
    }

    /// <summary>
    /// Configure SignalR client with automatic reconnection
    /// 配置SignalR客户端并启用自动重连
    /// </summary>
    /// <param name="hubUrl">Hub URL - Hub地址</param>
    /// <param name="accessToken">JWT access token - JWT访问令牌</param>
    /// <returns>Hub connection - Hub连接</returns>
    public static HubConnection CreateSignalRConnection(string hubUrl, string? accessToken = null)
    {
        var connectionBuilder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Configure JWT authentication
                // 配置JWT认证
                if (!string.IsNullOrEmpty(accessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                }

                // Configure transport options
                // 配置传输选项
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                   Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            })
            .WithAutomaticReconnect(new[] {
                TimeSpan.Zero,          // First retry immediately - 第一次立即重试
                TimeSpan.FromSeconds(2), // Second retry after 2 seconds - 第二次2秒后重试
                TimeSpan.FromSeconds(10), // Third retry after 10 seconds - 第三次10秒后重试
                TimeSpan.FromSeconds(30), // Fourth retry after 30 seconds - 第四次30秒后重试
                TimeSpan.FromSeconds(60)  // Subsequent retries after 60 seconds - 后续每60秒重试
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });

        return connectionBuilder.Build();
    }
}

/// <summary>
/// SignalR automatic reconnection policy
/// SignalR自动重连策略
/// </summary>
public class CustomRetryPolicy : IRetryPolicy
{
    private readonly TimeSpan[] _retryDelays = new[]
    {
        TimeSpan.Zero,
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(60),
        TimeSpan.FromSeconds(120)
    };

    /// <summary>
    /// Get next retry delay
    /// 获取下次重试延迟
    /// </summary>
    /// <param name="retryContext">Retry context - 重试上下文</param>
    /// <returns>Next retry delay or null to stop retrying - 下次重试延迟或null停止重试</returns>
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        // Stop retrying after 10 attempts
        // 10次尝试后停止重试
        if (retryContext.PreviousRetryCount >= 10)
        {
            return null;
        }

        // Use predefined delays, then use the last delay for subsequent retries
        // 使用预定义延迟，然后对后续重试使用最后一个延迟
        var delayIndex = Math.Min(retryContext.PreviousRetryCount, _retryDelays.Length - 1);
        return _retryDelays[delayIndex];
    }
}


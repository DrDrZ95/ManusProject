using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AgentWebApi.Identity;

namespace AgentWebApi.Hubs;

/// <summary>
/// AI-Agent SignalR hub for real-time communication with JWT authentication
/// AI-Agent SignalR集线器，用于实时通信，包含JWT认证
/// 
/// 提供LLM、RAG和其他AI服务的实时交互功能，所有方法都需要JWT认证
/// Provides real-time interaction capabilities for LLM, RAG, and other AI services, all methods require JWT authentication
/// </summary>
[Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
public class AIAgentHub : Hub
{
    private readonly ILogger<AIAgentHub> _logger;
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly IRagService _ragService;
    private readonly IPythonFinetuneService _finetuneService;

    public AIAgentHub(
        ILogger<AIAgentHub> logger,
        ISemanticKernelService semanticKernelService,
        IRagService ragService,
        IPythonFinetuneService finetuneService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _semanticKernelService = semanticKernelService ?? throw new ArgumentNullException(nameof(semanticKernelService));
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _finetuneService = finetuneService ?? throw new ArgumentNullException(nameof(finetuneService));
    }

    /// <summary>
    /// Handle client connection with JWT validation
    /// 处理客户端连接并验证JWT
    /// </summary>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value; // JWT ID claim

        _logger.LogInformation("User connected to AI-Agent hub with JWT - 用户通过JWT连接到AI-Agent集线器: {UserId} ({UserName}) with role {UserRole}, JWT ID: {JwtId}", 
            userId, userName, userRole, jwtId);

        // Validate JWT claims
        // 验证JWT声明
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Connection rejected: Missing user ID in JWT - 连接被拒绝：JWT中缺少用户ID");
            Context.Abort();
            return;
        }

        // 将用户添加到基于角色的组 - Add user to role-based groups
        if (!string.IsNullOrEmpty(userRole))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{userRole}");
        }

        // 发送欢迎消息 - Send welcome message
        await Clients.Caller.SendAsync("Welcome", new
        {
            Message = $"Welcome to AI-Agent, {userName}! - 欢迎使用AI-Agent，{userName}！",
            UserId = userId,
            Role = userRole,
            JwtId = jwtId,
            ConnectedAt = DateTime.UtcNow,
            AuthenticationMethod = "JWT"
        });

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle client disconnection
    /// 处理客户端断开连接
    /// </summary>
    /// <param name="exception">Exception if any - 异常（如果有）</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        if (exception != null)
        {
            _logger.LogWarning(exception, "User disconnected from AI-Agent hub with error - 用户从AI-Agent集线器断开连接时出错: {UserId} ({UserName}), JWT ID: {JwtId}", 
                userId, userName, jwtId);
        }
        else
        {
            _logger.LogInformation("User disconnected from AI-Agent hub - 用户从AI-Agent集线器断开连接: {UserId} ({UserName}), JWT ID: {JwtId}", 
                userId, userName, jwtId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send chat message to LLM with streaming response (JWT required)
    /// 向LLM发送聊天消息并获取流式响应（需要JWT）
    /// </summary>
    /// <param name="message">User message - 用户消息</param>
    /// <param name="conversationId">Conversation ID - 对话ID</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public async Task SendChatMessage(string message, string? conversationId = null)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT and user authorization
        // 验证JWT和用户授权
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token - 未授权：无效的JWT令牌",
                ErrorCode = "INVALID_JWT",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        try
        {
            _logger.LogInformation("Processing chat message from authenticated user - 处理已认证用户聊天消息: {UserId} (JWT: {JwtId}) - {Message}", userId, jwtId, message);

            // 发送消息接收确认 - Send message received confirmation
            await Clients.Caller.SendAsync("MessageReceived", new
            {
                ConversationId = conversationId ?? Guid.NewGuid().ToString(),
                Message = message,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            // 使用语义内核处理消息 - Process message using Semantic Kernel
            var response = await _semanticKernelService.GetChatCompletionAsync(message);

            // 发送完整响应 - Send complete response
            await Clients.Caller.SendAsync("ChatResponse", new
            {
                ConversationId = conversationId,
                Response = response,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow,
                IsComplete = true
            });

            _logger.LogInformation("Chat response sent to authenticated user - 聊天响应已发送给已认证用户: {UserId} (JWT: {JwtId})", userId, jwtId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message from authenticated user - 处理已认证用户聊天消息时出错: {UserId} (JWT: {JwtId}) - {Message}", userId, jwtId, message);

            await Clients.Caller.SendAsync("Error", new
            {
                Message = "Failed to process your message. Please try again. - 处理您的消息失败，请重试。",
                ErrorCode = "CHAT_ERROR",
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Send streaming chat message to LLM (JWT required)
    /// 向LLM发送流式聊天消息（需要JWT）
    /// </summary>
    /// <param name="message">User message - 用户消息</param>
    /// <param name="conversationId">Conversation ID - 对话ID</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public async Task SendStreamingChatMessage(string message, string? conversationId = null)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT and user authorization
        // 验证JWT和用户授权
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token - 未授权：无效的JWT令牌",
                ErrorCode = "INVALID_JWT",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        try
        {
            _logger.LogInformation("Processing streaming chat message from authenticated user - 处理已认证用户流式聊天消息: {UserId} (JWT: {JwtId}) - {Message}", userId, jwtId, message);

            var currentConversationId = conversationId ?? Guid.NewGuid().ToString();

            // 发送流开始信号 - Send stream start signal
            await Clients.Caller.SendAsync("StreamStart", new
            {
                ConversationId = currentConversationId,
                Message = message,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            // 使用语义内核的流式响应 - Use Semantic Kernel streaming response
            await foreach (var chunk in _semanticKernelService.GetStreamingChatCompletionAsync(message))
            {
                // 发送流式数据块 - Send streaming data chunk
                await Clients.Caller.SendAsync("StreamChunk", new
                {
                    ConversationId = currentConversationId,
                    Chunk = chunk,
                    UserId = userId,
                    JwtId = jwtId,
                    Timestamp = DateTime.UtcNow
                });

                // 添加小延迟以避免过快发送 - Add small delay to avoid sending too fast
                await Task.Delay(50);
            }

            // 发送流结束信号 - Send stream end signal
            await Clients.Caller.SendAsync("StreamEnd", new
            {
                ConversationId = currentConversationId,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Streaming chat response completed for authenticated user - 已认证用户流式聊天响应完成: {UserId} (JWT: {JwtId})", userId, jwtId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing streaming chat message from authenticated user - 处理已认证用户流式聊天消息时出错: {UserId} (JWT: {JwtId}) - {Message}", userId, jwtId, message);

            await Clients.Caller.SendAsync("StreamError", new
            {
                ConversationId = conversationId,
                Message = "Failed to process your streaming message. Please try again. - 处理您的流式消息失败，请重试。",
                ErrorCode = "STREAM_ERROR",
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Query RAG system with real-time results (JWT required)
    /// 查询RAG系统并获取实时结果（需要JWT）
    /// </summary>
    /// <param name="query">Search query - 搜索查询</param>
    /// <param name="collectionName">Collection name - 集合名称</param>
    /// <param name="options">RAG options - RAG选项</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.RagAccess)]
    public async Task QueryRAG(string query, string collectionName, object? options = null)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT and RAG access
        // 验证JWT和RAG访问权限
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token for RAG access - 未授权：RAG访问的JWT令牌无效",
                ErrorCode = "INVALID_JWT_RAG",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        try
        {
            _logger.LogInformation("Processing RAG query from authenticated user - 处理已认证用户RAG查询: {UserId} (JWT: {JwtId}) - {Query}", userId, jwtId, query);

            // 发送查询开始信号 - Send query start signal
            await Clients.Caller.SendAsync("RAGQueryStart", new
            {
                Query = query,
                CollectionName = collectionName,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            // 执行RAG查询 - Execute RAG query
            var ragOptions = new RagOptions
            {
                EnableCitation = true,
                MaxResults = 5,
                Language = "zh-CN"
            };

            var result = await _ragService.QueryAsync(collectionName, query, ragOptions);

            // 发送RAG结果 - Send RAG results
            await Clients.Caller.SendAsync("RAGQueryResult", new
            {
                Query = query,
                CollectionName = collectionName,
                Result = result,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("RAG query completed for authenticated user - 已认证用户RAG查询完成: {UserId} (JWT: {JwtId})", userId, jwtId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAG query from authenticated user - 处理已认证用户RAG查询时出错: {UserId} (JWT: {JwtId}) - {Query}", userId, jwtId, query);

            await Clients.Caller.SendAsync("RAGQueryError", new
            {
                Query = query,
                CollectionName = collectionName,
                Message = "Failed to process your RAG query. Please try again. - 处理您的RAG查询失败，请重试。",
                ErrorCode = "RAG_ERROR",
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Monitor fine-tuning job progress (JWT required)
    /// 监控微调任务进度（需要JWT）
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.FinetuneAccess)]
    public async Task MonitorFinetuneJob(string jobId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT and finetune access
        // 验证JWT和微调访问权限
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token for finetune access - 未授权：微调访问的JWT令牌无效",
                ErrorCode = "INVALID_JWT_FINETUNE",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        try
        {
            _logger.LogInformation("Starting finetune job monitoring for authenticated user - 开始为已认证用户监控微调任务: {UserId} (JWT: {JwtId}) - {JobId}", userId, jwtId, jobId);

            // 将用户添加到任务监控组 - Add user to job monitoring group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"FinetuneJob_{jobId}");

            // 获取当前任务状态 - Get current job status
            var jobStatus = await _finetuneService.GetJobStatusAsync(jobId);

            // 发送当前状态 - Send current status
            await Clients.Caller.SendAsync("FinetuneJobStatus", new
            {
                JobId = jobId,
                Status = jobStatus,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Finetune job monitoring started for authenticated user - 已认证用户微调任务监控已开始: {UserId} (JWT: {JwtId}) - {JobId}", userId, jwtId, jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting finetune job monitoring for authenticated user - 开始已认证用户微调任务监控时出错: {UserId} (JWT: {JwtId}) - {JobId}", userId, jwtId, jobId);

            await Clients.Caller.SendAsync("FinetuneJobError", new
            {
                JobId = jobId,
                Message = "Failed to start monitoring fine-tuning job. - 开始监控微调任务失败。",
                ErrorCode = "MONITOR_ERROR",
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Stop monitoring fine-tuning job (JWT required)
    /// 停止监控微调任务（需要JWT）
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.FinetuneAccess)]
    public async Task StopMonitoringFinetuneJob(string jobId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT
        // 验证JWT
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token - 未授权：无效的JWT令牌",
                ErrorCode = "INVALID_JWT",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        try
        {
            _logger.LogInformation("Stopping finetune job monitoring for authenticated user - 停止为已认证用户监控微调任务: {UserId} (JWT: {JwtId}) - {JobId}", userId, jwtId, jobId);

            // 从任务监控组移除用户 - Remove user from job monitoring group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"FinetuneJob_{jobId}");

            await Clients.Caller.SendAsync("FinetuneJobMonitoringStopped", new
            {
                JobId = jobId,
                UserId = userId,
                JwtId = jwtId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Finetune job monitoring stopped for authenticated user - 已认证用户微调任务监控已停止: {UserId} (JWT: {JwtId}) - {JobId}", userId, jwtId, jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping finetune job monitoring for authenticated user - 停止已认证用户微调任务监控时出错: {UserId} (JWT: {JwtId}) - {JobId}", userId, jwtId, jobId);
        }
    }

    /// <summary>
    /// Join a specific room for group communication (JWT required)
    /// 加入特定房间进行群组通信（需要JWT）
    /// </summary>
    /// <param name="roomName">Room name - 房间名称</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public async Task JoinRoom(string roomName)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT
        // 验证JWT
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token for room access - 未授权：房间访问的JWT令牌无效",
                ErrorCode = "INVALID_JWT_ROOM",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

        _logger.LogInformation("Authenticated user joined room - 已认证用户加入房间: {UserId} ({UserName}) (JWT: {JwtId}) joined {RoomName}", userId, userName, jwtId, roomName);

        // 通知房间内其他用户 - Notify other users in the room
        await Clients.Group(roomName).SendAsync("UserJoinedRoom", new
        {
            UserId = userId,
            UserName = userName,
            JwtId = jwtId,
            RoomName = roomName,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Leave a specific room (JWT required)
    /// 离开特定房间（需要JWT）
    /// </summary>
    /// <param name="roomName">Room name - 房间名称</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public async Task LeaveRoom(string roomName)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT
        // 验证JWT
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token - 未授权：无效的JWT令牌",
                ErrorCode = "INVALID_JWT",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

        _logger.LogInformation("Authenticated user left room - 已认证用户离开房间: {UserId} ({UserName}) (JWT: {JwtId}) left {RoomName}", userId, userName, jwtId, roomName);

        // 通知房间内其他用户 - Notify other users in the room
        await Clients.Group(roomName).SendAsync("UserLeftRoom", new
        {
            UserId = userId,
            UserName = userName,
            JwtId = jwtId,
            RoomName = roomName,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send message to a specific room (JWT required)
    /// 向特定房间发送消息（需要JWT）
    /// </summary>
    /// <param name="roomName">Room name - 房间名称</param>
    /// <param name="message">Message - 消息</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public async Task SendMessageToRoom(string roomName, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT
        // 验证JWT
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token for room messaging - 未授权：房间消息的JWT令牌无效",
                ErrorCode = "INVALID_JWT_ROOM_MESSAGE",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        _logger.LogInformation("Authenticated user sending message to room - 已认证用户向房间发送消息: {UserId} ({UserName}) (JWT: {JwtId}) to {RoomName}", userId, userName, jwtId, roomName);

        // 发送消息到房间 - Send message to room
        await Clients.Group(roomName).SendAsync("RoomMessage", new
        {
            UserId = userId,
            UserName = userName,
            JwtId = jwtId,
            RoomName = roomName,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send simple message (for compatibility with frontend simulation)
    /// 发送简单消息（与前端模拟兼容）
    /// </summary>
    /// <param name="user">User name - 用户名</param>
    /// <param name="message">Message - 消息</param>
    /// <returns>Task</returns>
    [Authorize(Policy = AuthorizationPolicies.SignalRAccess)]
    public async Task SendMessage(string user, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var jwtId = Context.User?.FindFirst("jti")?.Value;

        // Validate JWT
        // 验证JWT
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AuthorizationError", new
            {
                Message = "Unauthorized: Invalid JWT token - 未授权：无效的JWT令牌",
                ErrorCode = "INVALID_JWT",
                Timestamp = DateTime.UtcNow
            });
            return;
        }

        _logger.LogInformation("Authenticated user sending simple message - 已认证用户发送简单消息: {UserId} (JWT: {JwtId}) as {User}: {Message}", userId, jwtId, user, message);

        // 发送消息给所有连接的客户端 - Send message to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", user, message, new
        {
            UserId = userId,
            JwtId = jwtId,
            Timestamp = DateTime.UtcNow
        });
    }
}


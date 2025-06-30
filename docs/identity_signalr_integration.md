# IdentityServer4 and SignalR Integration Documentation

## Overview - 概述

This document describes the integration of IdentityServer4 for authentication/authorization and SignalR for real-time communication in the AI-Agent system.

本文档描述了在AI-Agent系统中集成IdentityServer4进行身份验证/授权和SignalR进行实时通信的方案。

## IdentityServer4 Integration - IdentityServer4集成

### Features - 功能特性

1. **RSA Key Generation** - RSA密钥生成
   - Automatic RSA key pair generation and storage
   - 自动RSA密钥对生成和存储
   - Secure key management with file-based persistence
   - 基于文件持久化的安全密钥管理

2. **Role-Based Access Control** - 基于角色的访问控制
   - Admin, User, Developer, Viewer roles
   - 管理员、用户、开发者、查看者角色
   - Fine-grained authorization policies
   - 细粒度授权策略

3. **JWT Token Support** - JWT令牌支持
   - Bearer token authentication
   - Bearer令牌认证
   - SignalR integration with query string tokens
   - SignalR与查询字符串令牌集成

### Configuration - 配置

```json
{
  "IdentityServer": {
    "IssuerUri": "https://localhost:5001",
    "Authority": "https://localhost:5001",
    "RequireHttpsMetadata": true,
    "RsaKeyPath": "~/.ai-agent/keys/rsa-key.pem",
    "UseEntityFramework": false
  }
}
```

### API Clients - API客户端

#### AI-Agent Web Client
- **Client ID**: `ai-agent-web`
- **Grant Types**: Authorization Code + PKCE
- **Scopes**: `openid`, `profile`, `ai-agent-api`
- **Redirect URIs**: Development and production URLs

#### AI-Agent API Client
- **Client ID**: `ai-agent-api-client`
- **Grant Types**: Client Credentials
- **Scopes**: `ai-agent-api`
- **For server-to-server communication**

### Authorization Policies - 授权策略

```csharp
// Basic access to AI-Agent system - AI-Agent系统基本访问权限
RequireAuthenticatedUser + "ai-agent-api" scope

// SignalR access - SignalR访问权限
RequireAuthenticatedUser + ("User" OR "Admin" OR "Developer" role)

// RAG system access - RAG系统访问权限
RequireAuthenticatedUser + ("User" OR "Admin" OR "Developer" role)

// Fine-tuning access - 微调访问权限
RequireAuthenticatedUser + ("Admin" OR "Developer" role)

// Admin operations - 管理员操作
RequireAuthenticatedUser + "Admin" role
```

## SignalR Integration - SignalR集成

### Features - 功能特性

1. **Real-time LLM Communication** - 实时LLM通信
   - Streaming chat responses
   - 流式聊天响应
   - Non-blocking message processing
   - 非阻塞消息处理

2. **RAG Integration** - RAG集成
   - Real-time document retrieval
   - 实时文档检索
   - Hybrid search results streaming
   - 混合搜索结果流式传输

3. **Fine-tuning Monitoring** - 微调监控
   - Real-time progress updates
   - 实时进度更新
   - Job status notifications
   - 任务状态通知

4. **Role-based Groups** - 基于角色的分组
   - Automatic group assignment
   - 自动分组分配
   - Targeted notifications
   - 定向通知

### Hub Endpoints - 集线器端点

#### Main Hub: `/hubs/ai-agent`

**Connection Events - 连接事件**
- `OnConnectedAsync`: User joins with role-based grouping
- `OnDisconnectedAsync`: Clean disconnection handling

**Chat Methods - 聊天方法**
- `SendChatMessage(message, conversationId)`: Standard chat
- `SendStreamingChatMessage(message, conversationId)`: Streaming chat

**RAG Methods - RAG方法**
- `QueryRAG(query, collectionName, options)`: Real-time RAG queries

**Fine-tuning Methods - 微调方法**
- `MonitorFinetuneJob(jobId)`: Start monitoring
- `StopMonitoringFinetuneJob(jobId)`: Stop monitoring

**Room Methods - 房间方法**
- `JoinRoom(roomName)`: Join group chat
- `LeaveRoom(roomName)`: Leave group chat
- `SendMessageToRoom(roomName, message)`: Group messaging

### Client Events - 客户端事件

#### Connection Events - 连接事件
```javascript
// Welcome message - 欢迎消息
connection.on("Welcome", (data) => {
    console.log(`Welcome ${data.UserName}! Role: ${data.Role}`);
});
```

#### Chat Events - 聊天事件
```javascript
// Message received confirmation - 消息接收确认
connection.on("MessageReceived", (data) => {
    console.log(`Message received: ${data.ConversationId}`);
});

// Complete chat response - 完整聊天响应
connection.on("ChatResponse", (data) => {
    console.log(`Response: ${data.Response}`);
});

// Streaming events - 流式事件
connection.on("StreamStart", (data) => {
    console.log(`Stream started for: ${data.ConversationId}`);
});

connection.on("StreamChunk", (data) => {
    console.log(`Chunk: ${data.Chunk}`);
});

connection.on("StreamEnd", (data) => {
    console.log(`Stream ended for: ${data.ConversationId}`);
});
```

#### RAG Events - RAG事件
```javascript
// RAG query results - RAG查询结果
connection.on("RAGQueryResult", (data) => {
    console.log(`RAG Result:`, data.Result);
});
```

#### Fine-tuning Events - 微调事件
```javascript
// Job status updates - 任务状态更新
connection.on("FinetuneJobStatus", (data) => {
    console.log(`Job ${data.JobId}: ${data.Status.Status} (${data.Status.Progress}%)`);
});

// Real-time updates - 实时更新
connection.on("FinetuneJobUpdate", (data) => {
    console.log(`Job ${data.JobId}: ${data.Status} (${data.Progress}%)`);
});
```

### Authentication with SignalR - SignalR身份验证

#### JavaScript Client Example - JavaScript客户端示例
```javascript
// Get JWT token from your authentication system
// 从身份验证系统获取JWT令牌
const token = await getJwtToken();

// Create connection with token - 使用令牌创建连接
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/ai-agent", {
        accessTokenFactory: () => token
    })
    .build();

// Alternative: Query string method (for some scenarios)
// 替代方案：查询字符串方法（适用于某些场景）
const connectionWithQuery = new signalR.HubConnectionBuilder()
    .withUrl(`/hubs/ai-agent?access_token=${token}`)
    .build();
```

#### C# Client Example - C#客户端示例
```csharp
// Create connection with authentication - 创建带身份验证的连接
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/hubs/ai-agent", options =>
    {
        options.AccessTokenProvider = async () =>
        {
            // Get token from your authentication system
            // 从身份验证系统获取令牌
            return await GetJwtTokenAsync();
        };
    })
    .Build();
```

## Usage Examples - 使用示例

### 1. Streaming Chat with LLM - 与LLM的流式聊天

```javascript
// Start streaming chat - 开始流式聊天
await connection.invoke("SendStreamingChatMessage", 
    "Explain quantum computing in simple terms", 
    "conv-123");

// Handle streaming response - 处理流式响应
let fullResponse = "";
connection.on("StreamChunk", (data) => {
    fullResponse += data.Chunk;
    updateChatUI(data.Chunk); // Update UI incrementally - 增量更新UI
});

connection.on("StreamEnd", (data) => {
    console.log("Complete response:", fullResponse);
});
```

### 2. Real-time RAG Queries - 实时RAG查询

```javascript
// Query RAG system - 查询RAG系统
await connection.invoke("QueryRAG", 
    "What is the company policy on remote work?", 
    "hr_policies", 
    { enableCitation: true, maxResults: 5 });

// Handle RAG results - 处理RAG结果
connection.on("RAGQueryResult", (data) => {
    displayRAGResults(data.Result);
    showCitations(data.Result.Citations);
});
```

### 3. Fine-tuning Job Monitoring - 微调任务监控

```javascript
// Start monitoring a fine-tuning job - 开始监控微调任务
await connection.invoke("MonitorFinetuneJob", "job-456");

// Handle real-time updates - 处理实时更新
connection.on("FinetuneJobUpdate", (data) => {
    updateProgressBar(data.Progress);
    updateStatusText(data.Status);
    
    if (data.Status === "Completed") {
        showCompletionNotification();
    }
});
```

### 4. Group Communication - 群组通信

```javascript
// Join a project room - 加入项目房间
await connection.invoke("JoinRoom", "project-alpha");

// Send message to room - 向房间发送消息
await connection.invoke("SendMessageToRoom", 
    "project-alpha", 
    "The new model training is complete!");

// Handle room messages - 处理房间消息
connection.on("RoomMessage", (data) => {
    displayRoomMessage(data.UserName, data.Message);
});
```

## Performance Considerations - 性能考虑

### Streaming Limitations - 流式限制

1. **Message Size Limits** - 消息大小限制
   - Maximum 1MB per message
   - 每条消息最大1MB
   - Use chunking for larger responses
   - 对于较大响应使用分块

2. **Connection Limits** - 连接限制
   - Monitor active connections
   - 监控活跃连接
   - Implement connection pooling
   - 实现连接池

3. **Rate Limiting** - 速率限制
   - Implement per-user rate limits
   - 实现每用户速率限制
   - Prevent abuse of streaming endpoints
   - 防止流式端点滥用

### Scalability - 可扩展性

1. **Redis Backplane** - Redis背板
   - Enable for multi-server deployments
   - 为多服务器部署启用
   - Configure Redis connection string
   - 配置Redis连接字符串

2. **Load Balancing** - 负载均衡
   - Use sticky sessions
   - 使用粘性会话
   - Configure proper WebSocket support
   - 配置适当的WebSocket支持

## Security Best Practices - 安全最佳实践

### 1. Token Management - 令牌管理
- Use short-lived access tokens (15-30 minutes)
- 使用短期访问令牌（15-30分钟）
- Implement refresh token rotation
- 实现刷新令牌轮换
- Secure token storage in clients
- 在客户端安全存储令牌

### 2. Connection Security - 连接安全
- Always use HTTPS in production
- 生产环境始终使用HTTPS
- Validate all incoming messages
- 验证所有传入消息
- Implement connection rate limiting
- 实现连接速率限制

### 3. Authorization Checks - 授权检查
- Verify permissions on every hub method
- 在每个集线器方法上验证权限
- Use role-based access control
- 使用基于角色的访问控制
- Log all security events
- 记录所有安全事件

## Troubleshooting - 故障排除

### Common Issues - 常见问题

1. **Connection Failures** - 连接失败
   - Check CORS configuration
   - 检查CORS配置
   - Verify token validity
   - 验证令牌有效性
   - Ensure WebSocket support
   - 确保WebSocket支持

2. **Authentication Errors** - 身份验证错误
   - Verify JWT token format
   - 验证JWT令牌格式
   - Check token expiration
   - 检查令牌过期
   - Validate issuer and audience
   - 验证发行者和受众

3. **Performance Issues** - 性能问题
   - Monitor connection count
   - 监控连接数
   - Check message size limits
   - 检查消息大小限制
   - Optimize streaming frequency
   - 优化流式频率

### Health Checks - 健康检查

The system includes comprehensive health checks:
系统包含全面的健康检查：

- **IdentityServer4**: RSA key validation, service availability
- **SignalR**: Active connections, Redis backplane (if configured)
- **Integration**: End-to-end authentication flow

Access health checks at: `/health`
访问健康检查：`/health`

## Deployment Notes - 部署说明

### Development Environment - 开发环境
- Services are commented out in `Program.cs` by default
- 服务在`Program.cs`中默认被注释
- Uncomment to enable IdentityServer4 and SignalR
- 取消注释以启用IdentityServer4和SignalR
- Use test users for development
- 开发环境使用测试用户

### Production Environment - 生产环境
- Configure proper HTTPS certificates
- 配置适当的HTTPS证书
- Use EntityFramework stores for IdentityServer4
- 为IdentityServer4使用EntityFramework存储
- Enable Redis backplane for SignalR
- 为SignalR启用Redis背板
- Implement proper logging and monitoring
- 实现适当的日志记录和监控

This integration provides a robust foundation for real-time AI-powered applications with enterprise-grade security and scalability.

此集成为具有企业级安全性和可扩展性的实时AI应用程序提供了强大的基础。


# ClickHouse 集成指南 (.NET 解决方案)

本文档提供了将 ClickHouse 与 ai-agent .NET 解决方案集成的全面指南，包括 SDK 安装、配置和简单使用示例。

## ClickHouse 简介

ClickHouse 是一个开源的列式数据库管理系统，允许对大型数据集进行实时分析处理。它特别适合 OLAP 工作负载，可以高效处理大量数据的分析查询。

## 安装 ClickHouse .NET SDK

要将 ClickHouse 与您的 .NET 解决方案集成，您需要安装 ClickHouse.Client NuGet 包：

```bash
# 使用 .NET CLI
dotnet add package ClickHouse.Client

# 使用包管理器控制台
Install-Package ClickHouse.Client
```

## 配置

### 1. 添加 ClickHouse 连接字符串

在您的 `appsettings.json` 文件中添加 ClickHouse 连接字符串：

```json
{
  "ConnectionStrings": {
    "ClickHouse": "Host=localhost;Port=9000;Database=default;Username=default;Password=;",
    "SqlServer": "Server=localhost;Database=AgentDb;Trusted_Connection=True;"
  }
}
```

### 2. 创建 ClickHouse 连接工厂

在 `Services` 目录中创建一个新文件 `ClickHouseConnectionFactory.cs`：

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentWebApi.Services
{
    /// <summary>
    /// 用于创建 ClickHouse 数据库连接的工厂。
    /// </summary>
    public class ClickHouseConnectionFactory : IClickHouseConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClickHouseConnectionFactory> _logger;

        public ClickHouseConnectionFactory(IConfiguration configuration, ILogger<ClickHouseConnectionFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 使用指定的连接名称创建 ClickHouse 连接。
        /// </summary>
        /// <param name="connectionName">配置中的连接名称。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>一个打开的 ClickHouse 连接。</returns>
        public async Task<ClickHouseConnection> CreateConnectionAsync(string connectionName = "ClickHouse", CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString(connectionName);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"在配置中未找到连接字符串 '{connectionName}'。", nameof(connectionName));
            }

            _logger.LogInformation("为 {ConnectionName} 创建 ClickHouse 连接", connectionName);
            
            var connection = new ClickHouseConnection(connectionString);
            
            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法为 {ConnectionName} 打开 ClickHouse 连接", connectionName);
                connection.Dispose();
                throw;
            }
        }
    }

    /// <summary>
    /// ClickHouse 连接工厂接口。
    /// </summary>
    public interface IClickHouseConnectionFactory
    {
        Task<ClickHouseConnection> CreateConnectionAsync(string connectionName = "ClickHouse", CancellationToken cancellationToken = default);
    }
}
```

### 3. 注册 ClickHouse 服务

更新 `Program.cs` 文件以注册 ClickHouse 连接工厂：

```csharp
// 添加 ClickHouse 服务
builder.Services.AddSingleton<IClickHouseConnectionFactory, ClickHouseConnectionFactory>();
```

## 简单使用案例

### 1. 创建 ClickHouse 仓储

在 `Services` 目录中创建一个新文件 `ClickHouseRepository.cs`：

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.Logging;

namespace AgentWebApi.Services
{
    /// <summary>
    /// 用于与 ClickHouse 数据库交互的仓储。
    /// </summary>
    public class ClickHouseRepository : IClickHouseRepository
    {
        private readonly IClickHouseConnectionFactory _connectionFactory;
        private readonly ILogger<ClickHouseRepository> _logger;

        public ClickHouseRepository(IClickHouseConnectionFactory connectionFactory, ILogger<ClickHouseRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// 执行查询并将结果作为字典列表返回。
        /// </summary>
        /// <param name="query">要执行的 SQL 查询。</param>
        /// <param name="parameters">可选的查询参数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示查询结果的字典列表。</returns>
        public async Task<List<Dictionary<string, object>>> QueryAsync(
            string query, 
            Dictionary<string, object>? parameters = null, 
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
            using var command = connection.CreateCommand();
            
            command.CommandText = query;
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = param.Key;
                    parameter.Value = param.Value;
                    command.Parameters.Add(parameter);
                }
            }

            _logger.LogInformation("执行 ClickHouse 查询: {Query}", query);
            
            var result = new List<Dictionary<string, object>>();
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object>();
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                
                result.Add(row);
            }
            
            return result;
        }

        /// <summary>
        /// 执行非查询命令。
        /// </summary>
        /// <param name="command">要执行的 SQL 命令。</param>
        /// <param name="parameters">可选的命令参数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>受影响的行数。</returns>
        public async Task<int> ExecuteAsync(
            string command, 
            Dictionary<string, object>? parameters = null, 
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
            using var cmd = connection.CreateCommand();
            
            cmd.CommandText = command;
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = param.Key;
                    parameter.Value = param.Value;
                    cmd.Parameters.Add(parameter);
                }
            }

            _logger.LogInformation("执行 ClickHouse 命令: {Command}", command);
            
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    /// ClickHouse 仓储接口。
    /// </summary>
    public interface IClickHouseRepository
    {
        Task<List<Dictionary<string, object>>> QueryAsync(
            string query, 
            Dictionary<string, object>? parameters = null, 
            CancellationToken cancellationToken = default);
            
        Task<int> ExecuteAsync(
            string command, 
            Dictionary<string, object>? parameters = null, 
            CancellationToken cancellationToken = default);
    }
}
```

### 2. 注册仓储

更新 `Program.cs` 文件以注册 ClickHouse 仓储：

```csharp
// 注册 ClickHouse 仓储
builder.Services.AddScoped<IClickHouseRepository, ClickHouseRepository>();
```

### 3. 创建代理日志服务

在 `Services` 目录中创建一个新文件 `AgentLoggingService.cs`：

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AgentWebApi.Services
{
    /// <summary>
    /// 用于将代理活动记录到 ClickHouse 的服务。
    /// </summary>
    public class AgentLoggingService : IAgentLoggingService
    {
        private readonly IClickHouseRepository _clickHouseRepository;
        private readonly ILogger<AgentLoggingService> _logger;

        public AgentLoggingService(IClickHouseRepository clickHouseRepository, ILogger<AgentLoggingService> logger)
        {
            _clickHouseRepository = clickHouseRepository;
            _logger = logger;
        }

        /// <summary>
        /// 将代理交互记录到 ClickHouse。
        /// </summary>
        /// <param name="sessionId">会话 ID。</param>
        /// <param name="userInput">用户输入。</param>
        /// <param name="agentResponse">代理响应。</param>
        /// <param name="metadata">可选的元数据。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示异步操作的任务。</returns>
        public async Task LogInteractionAsync(
            string sessionId,
            string userInput,
            string agentResponse,
            Dictionary<string, object>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "timestamp", DateTime.UtcNow },
                    { "userInput", userInput },
                    { "agentResponse", agentResponse },
                    { "metadata", metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : "{}" }
                };

                await _clickHouseRepository.ExecuteAsync(@"
                    INSERT INTO agent_interactions (
                        session_id,
                        timestamp,
                        user_input,
                        agent_response,
                        metadata
                    ) VALUES (
                        @sessionId,
                        @timestamp,
                        @userInput,
                        @agentResponse,
                        @metadata
                    )", parameters, cancellationToken);

                _logger.LogInformation("已记录会话 {SessionId} 的代理交互", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法记录会话 {SessionId} 的代理交互", sessionId);
                throw;
            }
        }

        /// <summary>
        /// 检索特定会话的代理交互。
        /// </summary>
        /// <param name="sessionId">会话 ID。</param>
        /// <param name="limit">要检索的最大交互数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>代理交互列表。</returns>
        public async Task<List<Dictionary<string, object>>> GetSessionInteractionsAsync(
            string sessionId,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "limit", limit }
                };

                return await _clickHouseRepository.QueryAsync(@"
                    SELECT
                        session_id,
                        timestamp,
                        user_input,
                        agent_response,
                        metadata
                    FROM agent_interactions
                    WHERE session_id = @sessionId
                    ORDER BY timestamp DESC
                    LIMIT @limit", parameters, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法检索会话 {SessionId} 的代理交互", sessionId);
                throw;
            }
        }
    }

    /// <summary>
    /// 代理日志服务接口。
    /// </summary>
    public interface IAgentLoggingService
    {
        Task LogInteractionAsync(
            string sessionId,
            string userInput,
            string agentResponse,
            Dictionary<string, object>? metadata = null,
            CancellationToken cancellationToken = default);
            
        Task<List<Dictionary<string, object>>> GetSessionInteractionsAsync(
            string sessionId,
            int limit = 100,
            CancellationToken cancellationToken = default);
    }
}
```

### 4. 注册代理日志服务

更新 `Program.cs` 文件以注册代理日志服务：

```csharp
// 注册代理日志服务
builder.Services.AddScoped<IAgentLoggingService, AgentLoggingService>();
```

### 5. 创建 ClickHouse 表

在使用服务之前，您需要在 ClickHouse 中创建必要的表。以下是创建 `agent_interactions` 表的 SQL 脚本：

```sql
CREATE TABLE IF NOT EXISTS agent_interactions (
    session_id String,
    timestamp DateTime,
    user_input String,
    agent_response String,
    metadata String,
    -- 根据需要添加其他字段
    INDEX idx_session_id session_id TYPE bloom_filter GRANULARITY 1
) ENGINE = MergeTree()
ORDER BY (session_id, timestamp);
```

## 使用示例

### 1. 在控制器中使用代理日志服务

```csharp
using System;
using System.Threading.Tasks;
using AgentWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentLoggingService _loggingService;

        public AgentController(IAgentLoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpPost("interact")]
        public async Task<IActionResult> Interact([FromBody] InteractionRequest request)
        {
            // 处理用户输入并生成响应
            // 这是您实际代理逻辑的占位符
            var response = $"响应：{request.UserInput}";
            
            // 将交互记录到 ClickHouse
            await _loggingService.LogInteractionAsync(
                request.SessionId,
                request.UserInput,
                response,
                new Dictionary<string, object>
                {
                    { "clientIp", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown" },
                    { "userAgent", Request.Headers["User-Agent"].ToString() }
                });
            
            return Ok(new { response });
        }

        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetHistory(string sessionId, [FromQuery] int limit = 100)
        {
            var interactions = await _loggingService.GetSessionInteractionsAsync(sessionId, limit);
            return Ok(interactions);
        }
    }

    public class InteractionRequest
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string UserInput { get; set; } = "";
    }
}
```

### 2. 直接 ClickHouse 查询示例

```csharp
using System;
using System.Threading.Tasks;
using AgentWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IClickHouseRepository _clickHouseRepository;

        public AnalyticsController(IClickHouseRepository clickHouseRepository)
        {
            _clickHouseRepository = clickHouseRepository;
        }

        [HttpGet("daily-interactions")]
        public async Task<IActionResult> GetDailyInteractions([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            var parameters = new Dictionary<string, object>
            {
                { "startDate", start },
                { "endDate", end }
            };
            
            var result = await _clickHouseRepository.QueryAsync(@"
                SELECT
                    toDate(timestamp) AS date,
                    count() AS interaction_count
                FROM agent_interactions
                WHERE timestamp BETWEEN @startDate AND @endDate
                GROUP BY date
                ORDER BY date", parameters);
            
            return Ok(result);
        }

        [HttpGet("popular-queries")]
        public async Task<IActionResult> GetPopularQueries([FromQuery] int limit = 10)
        {
            var parameters = new Dictionary<string, object>
            {
                { "limit", limit }
            };
            
            var result = await _clickHouseRepository.QueryAsync(@"
                SELECT
                    user_input,
                    count() AS query_count
                FROM agent_interactions
                GROUP BY user_input
                ORDER BY query_count DESC
                LIMIT @limit", parameters);
            
            return Ok(result);
        }
    }
}
```

## 性能考虑

ClickHouse 针对大型数据集的分析查询进行了优化。以下是一些性能考虑因素：

1. **批量插入**：对于高容量日志记录，考虑批量插入而不是一条一条地插入记录。

2. **分区**：对于大型表，考虑按日期或其他适当的列进行分区：

```sql
CREATE TABLE IF NOT EXISTS agent_interactions (
    session_id String,
    timestamp DateTime,
    user_input String,
    agent_response String,
    metadata String
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (session_id, timestamp);
```

3. **压缩**：ClickHouse 自动压缩数据，但您可以为具有特定数据模式的列指定压缩方法。

4. **物化视图**：对于频繁使用的聚合，考虑创建物化视图：

```sql
CREATE MATERIALIZED VIEW agent_interactions_daily_mv
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(date)
ORDER BY (date)
AS SELECT
    toDate(timestamp) AS date,
    count() AS interaction_count
FROM agent_interactions
GROUP BY date;
```

## 结论

本指南为将 ClickHouse 与您的 .NET 解决方案集成提供了基础。实现包括：

1. 使用 `ClickHouseConnectionFactory` 进行连接管理
2. 使用 `ClickHouseRepository` 进行数据访问
3. 使用 `AgentLoggingService` 进行领域特定服务
4. 示例控制器演示使用模式

通过遵循本指南，您可以利用 ClickHouse 的分析能力来大规模记录、监控和分析代理交互。

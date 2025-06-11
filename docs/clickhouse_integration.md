# ClickHouse Integration Guide for .NET Solution

This document provides comprehensive instructions for integrating ClickHouse with the ai-agent .NET solution, including SDK installation, configuration, and simple usage examples.

## Introduction to ClickHouse

ClickHouse is an open-source, column-oriented database management system that allows for real-time analytical processing of large datasets. It's particularly well-suited for OLAP workloads and can efficiently process analytical queries on large volumes of data.

## Installing the ClickHouse .NET SDK

To integrate ClickHouse with your .NET solution, you'll need to install the ClickHouse.Client NuGet package:

```bash
# Using .NET CLI
dotnet add package ClickHouse.Client

# Using Package Manager Console
Install-Package ClickHouse.Client
```

## Configuration

### 1. Add ClickHouse Connection String

Add the ClickHouse connection string to your `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "ClickHouse": "Host=localhost;Port=9000;Database=default;Username=default;Password=;",
    "SqlServer": "Server=localhost;Database=AgentDb;Trusted_Connection=True;"
  }
}
```

### 2. Create ClickHouse Connection Factory

Create a new file `ClickHouseConnectionFactory.cs` in the `Services` directory:

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
    /// Factory for creating ClickHouse database connections.
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
        /// Creates a ClickHouse connection using the specified connection name.
        /// </summary>
        /// <param name="connectionName">The name of the connection in configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An open ClickHouse connection.</returns>
        public async Task<ClickHouseConnection> CreateConnectionAsync(string connectionName = "ClickHouse", CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString(connectionName);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string '{connectionName}' not found in configuration.", nameof(connectionName));
            }

            _logger.LogInformation("Creating ClickHouse connection for {ConnectionName}", connectionName);
            
            var connection = new ClickHouseConnection(connectionString);
            
            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open ClickHouse connection for {ConnectionName}", connectionName);
                connection.Dispose();
                throw;
            }
        }
    }

    /// <summary>
    /// Interface for ClickHouse connection factory.
    /// </summary>
    public interface IClickHouseConnectionFactory
    {
        Task<ClickHouseConnection> CreateConnectionAsync(string connectionName = "ClickHouse", CancellationToken cancellationToken = default);
    }
}
```

### 3. Register ClickHouse Services

Update the `Program.cs` file to register the ClickHouse connection factory:

```csharp
// Add ClickHouse services
builder.Services.AddSingleton<IClickHouseConnectionFactory, ClickHouseConnectionFactory>();
```

## Simple Usage Cases

### 1. Create a ClickHouse Repository

Create a new file `ClickHouseRepository.cs` in the `Services` directory:

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
    /// Repository for interacting with ClickHouse database.
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
        /// Executes a query and returns the results as a list of dictionaries.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of dictionaries representing the query results.</returns>
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

            _logger.LogInformation("Executing ClickHouse query: {Query}", query);
            
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
        /// Executes a non-query command.
        /// </summary>
        /// <param name="command">The SQL command to execute.</param>
        /// <param name="parameters">Optional command parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of rows affected.</returns>
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

            _logger.LogInformation("Executing ClickHouse command: {Command}", command);
            
            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Interface for ClickHouse repository.
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

### 2. Register the Repository

Update the `Program.cs` file to register the ClickHouse repository:

```csharp
// Register ClickHouse repository
builder.Services.AddScoped<IClickHouseRepository, ClickHouseRepository>();
```

### 3. Create a Service for Agent Logging

Create a new file `AgentLoggingService.cs` in the `Services` directory:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AgentWebApi.Services
{
    /// <summary>
    /// Service for logging agent activities to ClickHouse.
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
        /// Logs an agent interaction to ClickHouse.
        /// </summary>
        /// <param name="sessionId">The session ID.</param>
        /// <param name="userInput">The user input.</param>
        /// <param name="agentResponse">The agent response.</param>
        /// <param name="metadata">Optional metadata.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

                _logger.LogInformation("Logged agent interaction for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log agent interaction for session {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves agent interactions for a specific session.
        /// </summary>
        /// <param name="sessionId">The session ID.</param>
        /// <param name="limit">Maximum number of interactions to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of agent interactions.</returns>
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
                _logger.LogError(ex, "Failed to retrieve agent interactions for session {SessionId}", sessionId);
                throw;
            }
        }
    }

    /// <summary>
    /// Interface for agent logging service.
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

### 4. Register the Agent Logging Service

Update the `Program.cs` file to register the agent logging service:

```csharp
// Register agent logging service
builder.Services.AddScoped<IAgentLoggingService, AgentLoggingService>();
```

### 5. Create ClickHouse Tables

Before using the services, you need to create the necessary tables in ClickHouse. Here's a SQL script to create the `agent_interactions` table:

```sql
CREATE TABLE IF NOT EXISTS agent_interactions (
    session_id String,
    timestamp DateTime,
    user_input String,
    agent_response String,
    metadata String,
    -- Add additional fields as needed
    INDEX idx_session_id session_id TYPE bloom_filter GRANULARITY 1
) ENGINE = MergeTree()
ORDER BY (session_id, timestamp);
```

## Usage Examples

### 1. Using the Agent Logging Service in a Controller

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
            // Process the user input and generate a response
            // This is a placeholder for your actual agent logic
            var response = $"Response to: {request.UserInput}";
            
            // Log the interaction to ClickHouse
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

### 2. Direct ClickHouse Query Example

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

## Performance Considerations

ClickHouse is optimized for analytical queries on large datasets. Here are some performance considerations:

1. **Batch Inserts**: For high-volume logging, consider batching inserts rather than inserting records one by one.

2. **Partitioning**: For large tables, consider partitioning by date or another appropriate column:

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

3. **Compression**: ClickHouse automatically compresses data, but you can specify compression methods for columns with specific data patterns.

4. **Materialized Views**: For frequently used aggregations, consider creating materialized views:

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

## Conclusion

This guide provides a foundation for integrating ClickHouse with your .NET solution. The implementation includes:

1. Connection management with `ClickHouseConnectionFactory`
2. Data access with `ClickHouseRepository`
3. Domain-specific service with `AgentLoggingService`
4. Example controllers demonstrating usage patterns

By following this guide, you can leverage ClickHouse's analytical capabilities for logging, monitoring, and analyzing agent interactions at scale.

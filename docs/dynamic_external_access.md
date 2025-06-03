# Dynamic External Access for MCP Integration

This document provides comprehensive guidance on using the `DynamicExternalAccessTool` class for real-time, dynamic access to external systems during conversation/inference time in the MCP (Model Context Protocol) framework.

## Overview

The `DynamicExternalAccessTool` provides a unified interface for AI models to interact with external systems in real-time during inference, enabling dynamic data retrieval and manipulation. This tool supports:

- **API Calls**: Make HTTP requests to external APIs
- **Database Operations**: Execute SQL queries against configured databases
- **File Operations**: Read, write, append, and delete files

## Configuration

### 1. Register Services

Add the following services to your `Program.cs` file:

```csharp
// Add HTTP client factory
builder.Services.AddHttpClient();

// Register the database connection factory
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Register the file operation service
builder.Services.AddScoped<IFileOperationService, FileOperationService>();

// Register the DynamicExternalAccessTool with MCP
builder.Services.AddMcpServer()
    .WithToolsFromAssembly(typeof(Program).Assembly);
```

### 2. Configure Security Settings

Add the following to your `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultDatabase": "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"
  },
  "DynamicExternalAccess": {
    "AllowedApiDomains": [
      "api.example.com",
      "data.example.org"
    ],
    "AllowedFilePaths": [
      "/app/data",
      "/app/logs"
    ]
  }
}
```

## Usage Examples

### API Operations

#### Making a GET Request

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("api") },
        { "url", new McpString("https://api.example.com/data") },
        { "method", new McpString("GET") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

#### Making a POST Request with JSON Body

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("api") },
        { "url", new McpString("https://api.example.com/users") },
        { "method", new McpString("POST") },
        { "body", new McpString("{\"name\":\"John Doe\",\"email\":\"john@example.com\"}") },
        { "headers", new McpString("{\"Content-Type\":\"application/json\"}") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

### Database Operations

#### Executing a SELECT Query

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("database") },
        { "connectionName", new McpString("DefaultDatabase") },
        { "query", new McpString("SELECT * FROM Users WHERE LastLoginDate > @lastLogin") },
        { "parameters", new McpString("{\"lastLogin\":\"2025-01-01\"}") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

#### Executing an INSERT Query

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("database") },
        { "connectionName", new McpString("DefaultDatabase") },
        { "query", new McpString("INSERT INTO Logs (Message, Timestamp) VALUES (@message, @timestamp)") },
        { "parameters", new McpString("{\"message\":\"User action logged\",\"timestamp\":\"2025-06-03T07:00:00Z\"}") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

### File Operations

#### Reading a File

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("file") },
        { "action", new McpString("read") },
        { "path", new McpString("/app/data/config.json") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

#### Writing to a File

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("file") },
        { "action", new McpString("write") },
        { "path", new McpString("/app/data/output.txt") },
        { "content", new McpString("This is the content to write to the file.") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

#### Appending to a File

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("file") },
        { "action", new McpString("append") },
        { "path", new McpString("/app/logs/application.log") },
        { "content", new McpString("New log entry: " + DateTime.Now.ToString() + "\n") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

#### Deleting a File

```csharp
var toolInput = new ToolInput
{
    Parameters = new Dictionary<string, McpPrimitive>
    {
        { "operation", new McpString("file") },
        { "action", new McpString("delete") },
        { "path", new McpString("/app/data/temporary.tmp") }
    }
};

var result = await dynamicExternalAccessTool.ExecuteAsync(toolInput, CancellationToken.None);
```

## Real-World Integration Example

Here's a complete example of integrating the `DynamicExternalAccessTool` with an AI conversation flow:

```csharp
// In a controller or service that handles AI conversations
public async Task<string> ProcessUserQuery(string userQuery, CancellationToken cancellationToken)
{
    // First, get a response from the AI model
    var aiResponse = await _qwenServiceClient.GenerateTextAsync(userQuery, cancellationToken);
    
    // Check if the AI response indicates a need for external data
    if (aiResponse.Contains("[NEED_WEATHER_DATA]"))
    {
        // Extract location from user query using simple pattern matching
        var locationMatch = Regex.Match(userQuery, @"weather in ([a-zA-Z\s]+)");
        if (locationMatch.Success)
        {
            var location = locationMatch.Groups[1].Value;
            
            // Create tool input for API call
            var toolInput = new ToolInput
            {
                Parameters = new Dictionary<string, McpPrimitive>
                {
                    { "operation", new McpString("api") },
                    { "url", new McpString($"https://api.weatherapi.com/v1/current.json?key=YOUR_API_KEY&q={Uri.EscapeDataString(location)}") },
                    { "method", new McpString("GET") }
                }
            };
            
            // Execute the tool
            var result = await _dynamicExternalAccessTool.ExecuteAsync(toolInput, cancellationToken);
            
            if (result.IsSuccessful && result.Results.TryGetValue("content", out var contentValue) && 
                contentValue?.Value is string weatherData)
            {
                // Log this interaction to the database
                await LogInteractionToDatabase(userQuery, "weather_api", cancellationToken);
                
                // Return enhanced response with real weather data
                return $"Based on real-time data: {weatherData}";
            }
        }
    }
    
    return aiResponse;
}

private async Task LogInteractionToDatabase(string query, string dataSource, CancellationToken cancellationToken)
{
    var toolInput = new ToolInput
    {
        Parameters = new Dictionary<string, McpPrimitive>
        {
            { "operation", new McpString("database") },
            { "connectionName", new McpString("DefaultDatabase") },
            { "query", new McpString("INSERT INTO UserInteractions (Query, DataSource, Timestamp) VALUES (@query, @dataSource, @timestamp)") },
            { "parameters", new McpString(JsonSerializer.Serialize(new 
            {
                query = query,
                dataSource = dataSource,
                timestamp = DateTime.UtcNow
            })) }
        }
    };
    
    await _dynamicExternalAccessTool.ExecuteAsync(toolInput, cancellationToken);
}
```

## Security Considerations

The `DynamicExternalAccessTool` implements several security measures:

1. **API Domain Allowlist**: Only configured domains can be accessed
2. **SQL Injection Prevention**: Parameterized queries and disallowed keywords
3. **File Path Restrictions**: Only allowed directories can be accessed
4. **Development vs. Production**: Stricter rules in production environments

### Extending Security

To enhance security further:

1. Add authentication for API calls:

```csharp
// In DbConnectionFactory.cs
private void AddAuthenticationHeaders(HttpRequestMessage request, string url)
{
    var uri = new Uri(url);
    if (uri.Host.EndsWith("example.com"))
    {
        var apiKey = _configuration["ApiKeys:ExampleApi"];
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
    }
}
```

2. Implement more granular database permissions:

```csharp
// In DbConnectionFactory.cs
private bool HasDatabasePermission(string connectionName, string query)
{
    // Check if the current user/context has permission to execute this query
    // Implementation depends on your authentication/authorization system
    return _authorizationService.CanExecuteQuery(connectionName, query);
}
```

## Extending the Tool

### Adding Support for New Database Types

The current implementation supports SQL Server. To add support for other database types:

```csharp
// In DbConnectionFactory.cs
public async Task<DbConnection> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
{
    var connectionString = _configuration.GetConnectionString(connectionName);
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ArgumentException($"Connection string '{connectionName}' not found in configuration.", nameof(connectionName));
    }

    // Determine database type from connection name or string
    DbConnection connection;
    if (connectionName.StartsWith("Postgres"))
    {
        connection = new NpgsqlConnection(connectionString);
    }
    else if (connectionName.StartsWith("MySql"))
    {
        connection = new MySqlConnection(connectionString);
    }
    else
    {
        // Default to SQL Server
        connection = new SqlConnection(connectionString);
    }
    
    await connection.OpenAsync(cancellationToken);
    return connection;
}
```

### Adding New Operation Types

To add support for new operation types (e.g., message queues, cache systems):

1. Add a new method to handle the operation:

```csharp
private async Task<ToolOutput> ExecuteCacheOperationAsync(ToolInput toolInput, CancellationToken cancellationToken)
{
    // Extract parameters
    if (!toolInput.Parameters.TryGetValue("cacheKey", out var keyValue) || 
        keyValue?.Value is not string key)
    {
        return CreateErrorOutput("Cache key parameter is required for cache operations.");
    }

    if (!toolInput.Parameters.TryGetValue("action", out var actionValue) || 
        actionValue?.Value is not string action)
    {
        return CreateErrorOutput("Action parameter is required for cache operations (get, set, delete).");
    }

    // Implement cache operations
    switch (action.ToLowerInvariant())
    {
        case "get":
            var value = await _cacheService.GetAsync(key, cancellationToken);
            return new ToolOutput
            {
                IsSuccessful = true,
                Results = new Dictionary<string, McpPrimitive>
                {
                    { "value", new McpString(value) }
                }
            };
        
        case "set":
            // Implementation for set operation
            // ...
            
        default:
            return CreateErrorOutput($"Unsupported cache operation: {action}");
    }
}
```

2. Update the `ExecuteAsync` method to handle the new operation type:

```csharp
public async Task<ToolOutput> ExecuteAsync(ToolInput toolInput, CancellationToken cancellationToken = default)
{
    // ...
    return operation.ToLowerInvariant() switch
    {
        "api" => await ExecuteApiOperationAsync(toolInput, cancellationToken),
        "database" => await ExecuteDatabaseOperationAsync(toolInput, cancellationToken),
        "file" => await ExecuteFileOperationAsync(toolInput, cancellationToken),
        "cache" => await ExecuteCacheOperationAsync(toolInput, cancellationToken), // New operation type
        _ => CreateErrorOutput($"Unsupported operation type: {operation}")
    };
    // ...
}
```

3. Update the tool definition to include the new parameters:

```csharp
public ToolDefinition GetDefinition()
{
    return new ToolDefinition
    {
        // ...
        InputSchema = new McpSchema
        (
            // ...
            Properties = new Dictionary<string, McpSchema>
            {
                // ...
                
                // Cache operation parameters
                { "cacheKey", new McpSchema(McpSchemaType.String, "Key for cache operations.") },
                { "cacheValue", new McpSchema(McpSchemaType.String, "Value to store in cache.") },
                { "cacheTtl", new McpSchema(McpSchemaType.Number, "Time-to-live in seconds for cached items.") }
            }
            // ...
        )
        // ...
    };
}
```

## Troubleshooting

### Common Issues and Solutions

1. **API Call Blocked by Security Policy**
   - Check the `AllowedApiDomains` configuration
   - Ensure the domain is correctly formatted
   - For development, set `"IsDevelopment": true` in appsettings.json

2. **Database Connection Failed**
   - Verify connection string in configuration
   - Check database server availability
   - Ensure proper credentials and permissions

3. **File Operation Failed**
   - Verify the path is within allowed directories
   - Check file system permissions
   - Ensure directories exist before writing files

### Logging and Debugging

The tool uses structured logging for easier debugging:

```csharp
// In Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

To enable detailed logging for the tool:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AgentWebApi.McpTools.DynamicExternalAccessTool": "Debug"
    }
  }
}
```

## Performance Considerations

For optimal performance:

1. **Connection Pooling**: Database connections are pooled by default
2. **HTTP Client Reuse**: The `HttpClientFactory` manages client lifecycles
3. **Async Operations**: All operations are fully asynchronous
4. **Cancellation Support**: All methods accept and respect cancellation tokens

For high-throughput scenarios, consider:

1. Adding caching for frequently accessed data
2. Implementing circuit breakers for external API calls
3. Using connection pooling for database operations
4. Implementing rate limiting for API calls

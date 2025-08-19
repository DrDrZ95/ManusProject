using System.Data;
using System.Data.Common;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Agent.Core.McpTools;

/// <summary>
/// Provides dynamic, real-time access to external systems during conversation/inference time.
/// Supports database queries, API calls, and file operations.
/// </summary>
public class DynamicExternalAccessTool : ITool
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DynamicExternalAccessTool> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IFileOperationService _fileOperationService;

    public DynamicExternalAccessTool(
        IHttpClientFactory httpClientFactory,
        ILogger<DynamicExternalAccessTool> logger,
        IConfiguration configuration,
        IDbConnectionFactory dbConnectionFactory,
        IFileOperationService fileOperationService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _dbConnectionFactory = dbConnectionFactory;
        _fileOperationService = fileOperationService;
    }

    public string Name => "DynamicExternalAccess";

    public string Description => "Provides live, dynamic access to external systems including databases, APIs, and file operations during conversation/inference time.";

    public async Task<ToolOutput> ExecuteAsync(ToolInput toolInput, CancellationToken cancellationToken = default)
    {
        if (toolInput.Parameters == null)
        {
            return CreateErrorOutput("No parameters provided.");
        }

        // Extract operation type
        if (!toolInput.Parameters.TryGetValue("operation", out var operationValue) || 
            operationValue?.ToString() is not string operation)
        {
            return CreateErrorOutput("Operation parameter is required (api, database, or file).");
        }

        try
        {
            return operation.ToLowerInvariant() switch
            {
                "api" => await ExecuteApiOperationAsync(toolInput, cancellationToken),
                "database" => await ExecuteDatabaseOperationAsync(toolInput, cancellationToken),
                "file" => await ExecuteFileOperationAsync(toolInput, cancellationToken),
                _ => CreateErrorOutput($"Unsupported operation type: {operation}. Supported types are: api, database, file.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing {Operation} operation", operation);
            return CreateErrorOutput($"Error executing {operation} operation: {ex.Message}");
        }
    }

    private async Task<ToolOutput> ExecuteApiOperationAsync(ToolInput toolInput, CancellationToken cancellationToken)
    {
        // Extract required parameters
        if (!toolInput.Parameters.TryGetValue("url", out var urlValue) || 
            urlValue?.ToString() is not string url)
        {
            return CreateErrorOutput("URL parameter is required for API operations.");
        }

        if (!toolInput.Parameters.TryGetValue("method", out var methodValue) || 
            methodValue?.ToString() is not string method)
        {
            return CreateErrorOutput("Method parameter is required for API operations (GET, POST, etc.).");
        }

        // Extract optional parameters
        string? requestBody = null;
        if (toolInput.Parameters.TryGetValue("body", out var bodyValue) && 
            bodyValue?.ToString() is string body)
        {
            requestBody = body;
        }

        Dictionary<string, string>? headers = null;
        if (toolInput.Parameters.TryGetValue("headers", out var headersValue) && 
            headersValue?.ToString() is string headersJson)
        {
            try
            {
                headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson);
            }
            catch (JsonException)
            {
                return CreateErrorOutput("Headers parameter must be a valid JSON object.");
            }
        }

        // Create HTTP client and request
        var client = _httpClientFactory.CreateClient("DynamicExternalAccess");
        
        // Apply security checks and rate limiting based on configuration
        if (!IsApiCallAllowed(url))
        {
            return CreateErrorOutput($"API call to {url} is not allowed by security policy.");
        }

        // Execute the API call
        HttpResponseMessage response;
        
        try
        {
            switch (method.ToUpperInvariant())
            {
                case "GET":
                    response = await client.GetAsync(url, cancellationToken);
                    break;
                case "POST":
                    response = await client.PostAsync(url, 
                        new StringContent(requestBody ?? "", System.Text.Encoding.UTF8, "application/json"), 
                        cancellationToken);
                    break;
                case "PUT":
                    response = await client.PutAsync(url, 
                        new StringContent(requestBody ?? "", System.Text.Encoding.UTF8, "application/json"), 
                        cancellationToken);
                    break;
                case "DELETE":
                    response = await client.DeleteAsync(url, cancellationToken);
                    break;
                default:
                    return CreateErrorOutput($"Unsupported HTTP method: {method}");
            }
        }
        catch (HttpRequestException ex)
        {
            return CreateErrorOutput($"HTTP request failed: {ex.Message}");
        }

        // Process the response
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return CreateErrorOutput($"API call failed with status code {(int)response.StatusCode}: {responseContent}");
        }

        return new ToolOutput
        {
            IsSuccessful = true,
            Results = new Dictionary<string, McpPrimitive>
            {
                { "statusCode", new McpNumber((int)response.StatusCode) },
                { "content", new McpString(responseContent) }
            }
        };
    }

    private async Task<ToolOutput> ExecuteDatabaseOperationAsync(ToolInput toolInput, CancellationToken cancellationToken)
    {
        // Extract required parameters
        if (!toolInput.Parameters.TryGetValue("connectionName", out var connectionNameValue) || 
            connectionNameValue?.ToString() is not string connectionName)
        {
            return CreateErrorOutput("ConnectionName parameter is required for database operations.");
        }

        if (!toolInput.Parameters.TryGetValue("query", out var queryValue) || 
            queryValue?.ToString() is not string query)
        {
            return CreateErrorOutput("Query parameter is required for database operations.");
        }

        // Extract optional parameters
        Dictionary<string, object>? parameters = null;
        if (toolInput.Parameters.TryGetValue("parameters", out var parametersValue) && 
            parametersValue?.ToString() is string parametersJson)
        {
            try
            {
                parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);
            }
            catch (JsonException)
            {
                return CreateErrorOutput("Parameters must be a valid JSON object.");
            }
        }

        // Security check for the query
        if (!IsDatabaseQueryAllowed(query))
        {
            return CreateErrorOutput("The provided query is not allowed by security policy.");
        }

        // Execute the database query
        try
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(connectionName, cancellationToken);
            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = param.Key;
                    dbParam.Value = param.Value ?? DBNull.Value;
                    command.Parameters.Add(dbParam);
                }
            }

            // Execute the query based on the expected result type
            if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                // Query returns data
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var result = new List<Dictionary<string, object>>();
                
                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    result.Add(row);
                }

                return new ToolOutput
                {
                    IsSuccessful = true,
                    Results = new Dictionary<string, McpPrimitive>
                    {
                        { "data", new McpString(JsonSerializer.Serialize(result)) },
                        { "rowCount", new McpNumber(result.Count) }
                    }
                };
            }
            else
            {
                // Query doesn't return data (INSERT, UPDATE, DELETE)
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                
                return new ToolOutput
                {
                    IsSuccessful = true,
                    Results = new Dictionary<string, McpPrimitive>
                    {
                        { "rowsAffected", new McpNumber(rowsAffected) }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            return CreateErrorOutput($"Database operation failed: {ex.Message}");
        }
    }

    private async Task<ToolOutput> ExecuteFileOperationAsync(ToolInput toolInput, CancellationToken cancellationToken)
    {
        // Extract required parameters
        if (!toolInput.Parameters.TryGetValue("action", out var actionValue) || 
            actionValue?.ToString() is not string action)
        {
            return CreateErrorOutput("Action parameter is required for file operations (read, write, append, delete).");
        }

        if (!toolInput.Parameters.TryGetValue("path", out var pathValue) || 
            pathValue?.ToString() is not string path)
        {
            return CreateErrorOutput("Path parameter is required for file operations.");
        }

        // Security check for the file path
        if (!IsFileOperationAllowed(path, action))
        {
            return CreateErrorOutput($"File operation {action} on path {path} is not allowed by security policy.");
        }

        try
        {
            switch (action.ToLowerInvariant())
            {
                case "read":
                    var content = await _fileOperationService.ReadFileAsync(path, cancellationToken);
                    return new ToolOutput
                    {
                        IsSuccessful = true,
                        Results = new Dictionary<string, McpPrimitive>
                        {
                            { "content", new McpString(content) }
                        }
                    };

                case "write":
                    if (!toolInput.Parameters.TryGetValue("content", out var contentValue) || 
                        contentValue?.ToString() is not string writeContent)
                    {
                        return CreateErrorOutput("Content parameter is required for write file operations.");
                    }
                    
                    await _fileOperationService.WriteFileAsync(path, writeContent, false, cancellationToken);
                    return new ToolOutput
                    {
                        IsSuccessful = true,
                        Results = new Dictionary<string, McpPrimitive>
                        {
                            { "message", new McpString($"File {path} written successfully.") }
                        }
                    };

                case "append":
                    if (!toolInput.Parameters.TryGetValue("content", out var appendContentValue) || 
                        appendContentValue?.ToString() is not string appendContent)
                    {
                        return CreateErrorOutput("Content parameter is required for append file operations.");
                    }
                    
                    await _fileOperationService.WriteFileAsync(path, appendContent, true, cancellationToken);
                    return new ToolOutput
                    {
                        IsSuccessful = true,
                        Results = new Dictionary<string, McpPrimitive>
                        {
                            { "message", new McpString($"Content appended to file {path} successfully.") }
                        }
                    };

                case "delete":
                    await _fileOperationService.DeleteFileAsync(path, cancellationToken);
                    return new ToolOutput
                    {
                        IsSuccessful = true,
                        Results = new Dictionary<string, McpPrimitive>
                        {
                            { "message", new McpString($"File {path} deleted successfully.") }
                        }
                    };

                default:
                    return CreateErrorOutput($"Unsupported file operation: {action}. Supported operations are: read, write, append, delete.");
            }
        }
        catch (Exception ex)
        {
            return CreateErrorOutput($"File operation failed: {ex.Message}");
        }
    }

    public ToolDefinition GetDefinition()
    {
        return new ToolDefinition
        {
            Name = Name,
            Description = Description,
            InputSchema = new McpSchema
            (
                //Type = McpSchemaType.Object,
                //Properties = new Dictionary<string, McpSchema>
                //{
                //    { "operation", new McpSchema(McpSchemaType.String, "Type of operation to perform: api, database, or file.") },
                //    
                //    // API operation parameters
                //    { "url", new McpSchema(McpSchemaType.String, "URL for API operations.") },
                //    { "method", new McpSchema(McpSchemaType.String, "HTTP method for API operations (GET, POST, PUT, DELETE).") },
                //    { "body", new McpSchema(McpSchemaType.String, "Request body for API operations (optional).") },
                //    { "headers", new McpSchema(McpSchemaType.String, "JSON string of headers for API operations (optional).") },
                //    
                //    // Database operation parameters
                //    { "connectionName", new McpSchema(McpSchemaType.String, "Name of the database connection to use.") },
                //    { "query", new McpSchema(McpSchemaType.String, "SQL query to execute.") },
                //    { "parameters", new McpSchema(McpSchemaType.String, "JSON string of parameters for the SQL query (optional).") },
                //    
                //    // File operation parameters
                //    { "action", new McpSchema(McpSchemaType.String, "File action to perform: read, write, append, or delete.") },
                //    { "path", new McpSchema(McpSchemaType.String, "Path to the file.") },
                //    { "content", new McpSchema(McpSchemaType.String, "Content to write or append to the file.") }
                //},
                //Required = new List<string> { "operation" }
            ),
            OutputSchema = new McpSchema
            (
                //Type = McpSchemaType.Object,
                //Properties = new Dictionary<string, McpSchema>
                //{
                //    { "statusCode", new McpSchema(McpSchemaType.Number, "HTTP status code for API operations.") },
                //    { "content", new McpSchema(McpSchemaType.String, "Response content for API operations or file read operations.") },
                //    { "data", new McpSchema(McpSchemaType.String, "JSON string of data returned from database queries.") },
                //    { "rowCount", new McpSchema(McpSchemaType.Number, "Number of rows returned from database queries.") },
                //    { "rowsAffected", new McpSchema(McpSchemaType.Number, "Number of rows affected by database operations.") },
                //    { "message", new McpSchema(McpSchemaType.String, "Success or error message.") }
                //}
            )
        };
    }

    private ToolOutput CreateErrorOutput(string errorMessage)
    {
        _logger.LogWarning("DynamicExternalAccessTool error: {ErrorMessage}", errorMessage);
        return new ToolOutput
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            Results = new Dictionary<string, McpPrimitive>
            {
                { "message", new McpString($"Error: {errorMessage}") }
            }
        };
    }

    private bool IsApiCallAllowed(string url)
    {
        // Get allowed domains from configuration
        var allowedDomains = _configuration.GetSection("DynamicExternalAccess:AllowedApiDomains")
            .Get<string[]>() ?? Array.Empty<string>();

        if (allowedDomains.Length == 0)
        {
            // If no domains are explicitly allowed, check if we're in development mode
            var isDevelopment = _configuration.GetValue<bool>("IsDevelopment", false);
            return isDevelopment; // Allow all in development mode, deny all in production
        }

        // Check if the URL's domain is in the allowed list
        var uri = new Uri(url);
        return allowedDomains.Any(domain => 
            uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase) || 
            uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsDatabaseQueryAllowed(string query)
    {
        // Check for potentially dangerous operations
        var normalizedQuery = query.ToUpperInvariant();
        
        // Disallow schema modifications, system procedures, etc.
        var disallowedKeywords = new[]
        {
            "DROP ", "ALTER ", "CREATE ", "TRUNCATE ", "EXEC ", "EXECUTE ", "SYSTEM_USER", 
            "INFORMATION_SCHEMA", "SYSOBJECTS", "SYS.", "XP_", "SP_"
        };

        return !disallowedKeywords.Any(keyword => normalizedQuery.Contains(keyword));
    }

    private bool IsFileOperationAllowed(string path, string operation)
    {
        // Get allowed paths from configuration
        var allowedPaths = _configuration.GetSection("DynamicExternalAccess:AllowedFilePaths")
            .Get<string[]>() ?? Array.Empty<string>();

        if (allowedPaths.Length == 0)
        {
            // If no paths are explicitly allowed, check if we're in development mode
            var isDevelopment = _configuration.GetValue<bool>("IsDevelopment", false);
            return isDevelopment; // Allow all in development mode, deny all in production
        }

        // Normalize the path
        var normalizedPath = Path.GetFullPath(path);
        
        // Check if the path is under any of the allowed paths
        return allowedPaths.Any(allowedPath => 
            normalizedPath.StartsWith(Path.GetFullPath(allowedPath), StringComparison.OrdinalIgnoreCase));
    }
}

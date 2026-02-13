namespace Agent.McpGateway;

/// <summary>
/// GitHub MCP 客户端实现
/// GitHub MCP Client Implementation
/// </summary>
public class GitHubMcpClient : McpBaseClient<GitHubEntity>
{
    public GitHubMcpClient() : base("GitHub")
    {
        // Constructor logic
    }

    /// <summary>
    /// 执行 JSON-RPC 请求
    /// Executes a JSON-RPC request
    /// </summary>
    /// <param name="method">JSON-RPC 方法名</param>
    /// <param name="parameters">请求参数</param>
    /// <returns>JSON-RPC 响应</returns>
    public override async Task<string> ExecuteJsonRpc(string method, object parameters)
    {
        string parametersJson = JsonSerializer.Serialize(parameters);
        // This part still uses the old InteractAsync logic for demonstration
        // In a real scenario, you would parse the method and parametersJson to call specific internal methods
        try
        {
            JObject jsonRpcRequest = JObject.Parse($"{{\"method\":\"{method}\", \"params\":{parametersJson}}}");
            string? rpcMethod = jsonRpcRequest["method"]?.ToString();
            JToken? rpcParams = jsonRpcRequest["params"];

            if (string.IsNullOrEmpty(rpcMethod))
            {
                return CreateErrorResponse($"{{\"method\":\"{method}\", \"params\":{parametersJson}}}", -32600, "Invalid Request: Method not found");
            }

            switch (rpcMethod)
            {
                case "GitHub.Everything":
                    return await EverythingAsync(rpcParams?["query"]?.ToString() ?? string.Empty);
                case "GitHub.Fetch":
                    return await FetchAsync(rpcParams?["repo"]?.ToString() ?? string.Empty, rpcParams?["path"]?.ToString() ?? string.Empty);
                case "GitHub.Filesystem":
                    return await FilesystemAsync(rpcParams?["repo"]?.ToString() ?? string.Empty, rpcParams?["path"]?.ToString() ?? string.Empty, rpcParams?["action"]?.ToString() ?? string.Empty);
                default:
                    return CreateErrorResponse($"{{\"method\":\"{method}\", \"params\":{parametersJson}}}", -32601, "Method not found");
            }
        }
        catch (JsonException)
        {
            return CreateErrorResponse($"{{\"method\":\"{method}\", \"params\":{parametersJson}}}", -32700, "Parse error");
        }
        catch (System.Exception ex)
        {
            return CreateErrorResponse($"{{\"method\":\"{method}\", \"params\":{parametersJson}}}", -32000, $"Internal error: {ex.Message}");
        }
    }

    // Everything
    public async Task<string> EverythingAsync(string query)
    {
        // Simulate searching everything on GitHub
        return await Task.FromResult(CreateJsonResponse("GitHub.Everything", new { success = true, query = query, results = new[] { "repo1", "user2" } }));
    }

    // Fetch
    public async Task<string> FetchAsync(string repo, string path)
    {
        // Simulate fetching content from a GitHub repository
        return await Task.FromResult(CreateJsonResponse("GitHub.Fetch", new { success = true, repo = repo, path = path, content = "Simulated file content" }));
    }

    // Filesystem
    public async Task<string> FilesystemAsync(string repo, string path, string action)
    {
        // Simulate filesystem operations on a GitHub repository
        return await Task.FromResult(CreateJsonResponse("GitHub.Filesystem", new { success = true, repo = repo, path = path, action = action, status = "Simulated success" }));
    }

    private string CreateJsonResponse(string method, object result)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = method,
            result = result
        });
    }

    private string CreateErrorResponse(string request, int code, string message)
    {
        JObject? jsonRpcRequest = null;
        try
        {
            jsonRpcRequest = JObject.Parse(request);
        }
        catch { /* ignore parse error for error response */ }

        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = code,
                message = message
            },
            id = jsonRpcRequest?["id"] ?? JValue.CreateNull()
        });
    }

    // --- Implement abstract methods from McpBaseClient --- //

    public override Task<IEnumerable<GitHubEntity>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<GitHubEntity>>(new List<GitHubEntity>());
    }

    public override Task<GitHubEntity> GetByIdAsync(string id)
    {
        return Task.FromResult(new GitHubEntity { Id = id, Name = $"GitHubEntity-{id}" });
    }

    public override Task<GitHubEntity> CreateAsync(GitHubEntity entity)
    {
        return Task.FromResult(entity);
    }

    public override Task<GitHubEntity> UpdateAsync(GitHubEntity entity)
    {
        return Task.FromResult(entity);
    }

    public override Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(true);
    }
}


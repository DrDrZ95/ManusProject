namespace Agent.McpGateway.UniversalMcp;
/// <summary>
/// GitHub 实体
/// GitHub Entity
/// </summary>
public class GitHubEntity : IMcpEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // Add GitHub-specific properties here
    public string RepositoryName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}



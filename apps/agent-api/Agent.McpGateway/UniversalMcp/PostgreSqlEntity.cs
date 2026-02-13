namespace Agent.McpGateway.UniversalMcp;
/// <summary>
/// PostgreSQL 实体
/// PostgreSQL Entity
/// </summary>
public class PostgreSqlEntity : IMcpEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // Add PostgreSQL-specific properties here
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}



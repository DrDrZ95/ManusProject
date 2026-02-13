namespace Agent.McpGateway.UniversalMcp;

/// <summary>
/// 通用 MCP 实体接口
/// Universal MCP Entity Interface
/// </summary>
public interface IMcpEntity
{
    /// <summary>
    /// 获取或设置实体的唯一标识符
    /// Gets or sets the unique identifier for the entity
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// 获取或设置实体的名称
    /// Gets or sets the name of the entity
    /// </summary>
    string Name { get; set; }
}


namespace Agent.Core.Tools.Interfaces;

/// <summary>
/// 工具注册中心接口 (Tool Registry Interface)
/// 负责工具元数据的 CRUD、版本管理、以及动态加载支持。
/// </summary>
public interface IToolRegistryService
{
    /// <summary>
    /// 注册新工具 (Register a new tool)
    /// </summary>
    Task<ToolMetadataEntity> RegisterToolAsync(ToolMetadataEntity metadata);

    /// <summary>
    /// 更新工具信息 (Update existing tool)
    /// </summary>
    Task<ToolMetadataEntity> UpdateToolAsync(ToolMetadataEntity metadata);

    /// <summary>
    /// 禁用/启用工具 (Enable or disable a tool)
    /// </summary>
    Task SetToolStatusAsync(Guid toolId, bool isEnabled);

    /// <summary>
    /// 获取所有活跃工具 (Get all active tools)
    /// </summary>
    Task<IEnumerable<ToolMetadataEntity>> GetActiveToolsAsync();

    /// <summary>
    /// 获取特定工具 (Get tool by name and version)
    /// </summary>
    Task<ToolMetadataEntity?> GetToolAsync(string name, string? version = null);

    /// <summary>
    /// 获取工具的所有版本 (Get all versions of a tool)
    /// </summary>
    Task<IEnumerable<ToolMetadataEntity>> GetToolVersionsAsync(string name);

    /// <summary>
    /// 热加载工具插件 (Hot-load tool plugins into the kernel)
    /// </summary>
    Task HotLoadToolsAsync();
}


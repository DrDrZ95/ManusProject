namespace Agent.Core.Workflow;

/// <summary>
/// Workflow service configuration options
/// 工作流服务配置选项
/// </summary>
public class WorkflowOptions
{
    /// <summary>
    /// Default todo list file directory
    /// 默认待办事项列表文件目录
    /// </summary>
    public string DefaultToDoDirectory { get; set; } = "todo_lists";

    /// <summary>
    /// Enable file auto-save
    /// 启用文件自动保存
    /// </summary>
    public bool EnableAutoSave { get; set; } = true;

    /// <summary>
    /// Auto-save interval in minutes
    /// 自动保存间隔（分钟）
    /// </summary>
    public int AutoSaveIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Maximum number of plans to keep in memory
    /// 内存中保留的最大计划数
    /// </summary>
    public int MaxPlansInMemory { get; set; } = 100;

    /// <summary>
    /// Enable detailed logging
    /// 启用详细日志记录
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
}

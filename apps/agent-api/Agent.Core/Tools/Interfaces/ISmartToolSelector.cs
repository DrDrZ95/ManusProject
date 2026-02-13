namespace Agent.Core.Tools.Interfaces;


/// <summary>
/// 智能工具选择引擎接口 (Smart Tool Selector Interface)
/// 负责基于任务需求和元数据指标选择最优工具。
/// </summary>
public interface ISmartToolSelector
{
    /// <summary>
    /// 基于任务描述推荐工具 (Recommend tools based on task description)
    /// </summary>
    /// <param name="taskDescription">任务需求描述</param>
    /// <param name="limit">返回数量限制</param>
    /// <returns>推荐的工具列表，按优先级排序</returns>
    Task<IEnumerable<ToolMetadataEntity>> RecommendToolsAsync(string taskDescription, int limit = 3);

    /// <summary>
    /// 为特定步骤选择最佳工具 (Select the best tool for a specific plan step)
    /// </summary>
    Task<ToolMetadataEntity?> SelectBestToolAsync(string goal, IEnumerable<ToolMetadataEntity> availableTools);

    /// <summary>
    /// 评估工具性能并更新元数据 (Evaluate tool performance and update metrics)
    /// </summary>
    Task EvaluateToolPerformanceAsync(Guid toolId, bool success, TimeSpan latency);
}

namespace Agent.Core.Memory.Interfaces;

/// <summary>
/// 智能体记忆模块的通用接口 (General interface for the Agent Memory module)
/// </summary>
public interface IAgentMemory
{
    /// <summary>
    /// 记忆模式的名称 (Name of the memory mode)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 初始化记忆存储 (Initializes the memory storage)
    /// </summary>
    /// <param name="conversationId">当前会话的唯一标识符 (Unique identifier for the current conversation)</param>
    Task InitializeAsync(long conversationId);

    /// <summary>
    /// 从记忆中加载上下文信息 (Loads context information from memory)
    /// </summary>
    /// <returns>包含历史消息和状态的上下文对象 (Context object containing history and state)</returns>
    Task<MemoryContext> LoadContextAsync();

    /// <summary>
    /// 将新的消息或状态更新保存到记忆中 (Saves new messages or state updates to memory)
    /// </summary>
    /// <param name="update">要保存的记忆更新对象 (Memory update object to be saved)</param>
    Task SaveUpdateAsync(MemoryUpdate update);

    /// <summary>
    /// 清除当前会话的记忆 (Clears the memory for the current conversation)
    /// </summary>
    Task ClearAsync();
}

/// <summary>
/// 记忆上下文数据结构 (Memory context data structure)
/// </summary>
public class MemoryContext
{
    /// <summary>
    /// 历史消息列表 (List of historical messages)
    /// </summary>
    public List<string> HistoryMessages { get; set; } = new List<string>();

    /// <summary>
    /// 当前会话的摘要或长期记忆 (Summary or long-term memory of the current conversation)
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 相关的知识片段 (Relevant knowledge snippets)
    /// </summary>
    public List<string> KnowledgeSnippets { get; set; } = new List<string>();
}

/// <summary>
/// 记忆更新数据结构 (Memory update data structure)
/// </summary>
public class MemoryUpdate
{
    /// <summary>
    /// 新的用户或助手消息 (New user or assistant message)
    /// </summary>
    public string? NewMessage { get; set; }

    /// <summary>
    /// 是否需要更新摘要 (Whether the summary needs to be updated)
    /// </summary>
    public bool ShouldUpdateSummary { get; set; } = false;

    /// <summary>
    /// 触发的工具调用日志 (Triggered tool call log)
    /// </summary>
    public string? AbilityLog { get; set; }
}


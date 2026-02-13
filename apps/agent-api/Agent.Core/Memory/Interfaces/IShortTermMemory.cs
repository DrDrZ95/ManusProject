namespace Agent.Core.Memory.Interfaces;

/// <summary>
/// Short-term memory interface (Working Memory)
/// 短期记忆接口（工作记忆）
/// </summary>
public interface IShortTermMemory
{
    /// <summary>
    /// Get recent chat history with sliding window
    /// 获取带滑动窗口的最近聊天记录
    /// </summary>
    Task<List<ChatMessage>> GetRecentHistoryAsync(string sessionId, int tokenLimit = 4000);

    /// <summary>
    /// Add a message to the history
    /// 添加消息到历史记录
    /// </summary>
    Task AddMessageAsync(string sessionId, ChatMessage message);

    /// <summary>
    /// Compress history if it exceeds limits
    /// 如果超出限制则压缩历史记录
    /// </summary>
    Task CompressHistoryAsync(string sessionId);
}


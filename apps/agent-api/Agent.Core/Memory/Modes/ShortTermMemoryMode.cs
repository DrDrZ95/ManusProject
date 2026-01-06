namespace Agent.Core.Memory.Modes;

/// <summary>
/// 短期记忆模式 (Short-Term Memory Mode) - 滑动窗口设计模式 (Sliding Window Design Pattern)
/// 仅保留最近的 N 条消息作为上下文，模拟人类的短期记忆。
/// Only keeps the N most recent messages as context, simulating human short-term memory.
/// </summary>
public class ShortTermMemoryMode : BaseAgentMemory
{
    public override string Name => "ShortTermMemory";
    private const int MaxMessages = 10; // 最大保留的消息数量 - Maximum number of messages to keep

    // 实际应用中，这里会从数据库中加载最近的消息 (In a real application, this would load recent messages from the database)
    private readonly List<string> _recentMessages = new();

    /// <inheritdoc />
    public override Task<MemoryContext> LoadContextAsync()
    {
        // 返回最近的 N 条消息 (Return the N most recent messages)
        var context = new MemoryContext
        {
            HistoryMessages = _recentMessages.TakeLast(MaxMessages).ToList()
        };
        return Task.FromResult(context);
    }

    /// <inheritdoc />
    public override Task SaveUpdateAsync(MemoryUpdate update)
    {
        if (!string.IsNullOrEmpty(update.NewMessage))
        {
            // 将新消息添加到列表 (Add new message to the list)
            _recentMessages.Add(update.NewMessage);

            // 保持滑动窗口大小 (Maintain the sliding window size)
            if (_recentMessages.Count > MaxMessages)
            {
                // 移除最旧的消息 (Remove the oldest message)
                _recentMessages.RemoveAt(0);
            }
        }
        // 忽略 AbilityLog 和 Summary 更新，因为这是短期记忆模式 (Ignore AbilityLog and Summary updates as this is short-term memory mode)
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task ClearAsync()
    {
        _recentMessages.Clear();
        return Task.CompletedTask;
    }
}

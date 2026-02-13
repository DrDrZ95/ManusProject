namespace Agent.Core.Memory.Modes;

/// <summary>
/// 长期记忆模式 (Long-Term Memory Mode) - 摘要/快照设计模式 (Summary/Snapshot Design Pattern)
/// 通过定期生成会话摘要或快照来压缩历史信息，以减少上下文窗口的压力。
/// Compresses historical information by periodically generating conversation summaries or snapshots to reduce context window pressure.
/// </summary>
public class LongTermMemoryMode : BaseAgentMemory
{
    public override string Name => "LongTermMemory";

    // 模拟数据库中的长期记忆数据 (Simulate long-term memory data in the database)
    private string _currentSummary = "会话开始，用户和AI正在讨论项目需求。 (Conversation started, user and AI are discussing project requirements.)";
    private readonly List<string> _recentMessages = new();
    private const int SummaryUpdateThreshold = 5; // 达到此消息数后触发摘要更新 (Trigger summary update after this many messages)

    /// <inheritdoc />
    public override Task<MemoryContext> LoadContextAsync()
    {
        // 返回摘要和最近的消息 (Return the summary and recent messages)
        var context = new MemoryContext
        {
            Summary = _currentSummary,
            HistoryMessages = _recentMessages
        };
        return Task.FromResult(context);
    }

    /// <inheritdoc />
    public override async Task SaveUpdateAsync(MemoryUpdate update)
    {
        if (!string.IsNullOrEmpty(update.NewMessage))
        {
            _recentMessages.Add(update.NewMessage);
        }

        // 模拟工具调用日志的保存 (Simulate saving of ability log)
        if (!string.IsNullOrEmpty(update.AbilityLog))
        {
            // 实际应用中，这里会保存到 conversation_ability_log 表 (In a real app, this saves to conversation_ability_log table)
            Console.WriteLine($"[LongTermMemory] Saving Ability Log: {update.AbilityLog}");
        }

        // 达到阈值或明确要求时，触发摘要更新 (Trigger summary update when threshold is reached or explicitly requested)
        if (_recentMessages.Count >= SummaryUpdateThreshold || update.ShouldUpdateSummary)
        {
            // 模拟调用 LLM 服务生成新摘要 (Simulate calling LLM service to generate new summary)
            _currentSummary = await GenerateNewSummaryAsync(_currentSummary, _recentMessages);

            // 模拟保存快照到 conversation_snapshot 表 (Simulate saving snapshot to conversation_snapshot table)
            Console.WriteLine($"[LongTermMemory] Snapshot saved for Conversation {ConversationId}. New Summary: {_currentSummary}");

            // 清空最近消息，等待下一轮摘要 (Clear recent messages for the next summary cycle)
            _recentMessages.Clear();
        }
    }

    /// <summary>
    /// 模拟生成新摘要的逻辑 (Simulates the logic for generating a new summary)
    /// </summary>
    private Task<string> GenerateNewSummaryAsync(string oldSummary, List<string> newMessages)
    {
        // 实际应用中，这里会调用 LLM API (In a real application, this calls the LLM API)
        var newContent = string.Join(" ", newMessages.TakeLast(3));
        return Task.FromResult($"[Updated Summary] Based on: {oldSummary}. Latest discussion: {newContent.Substring(0, Math.Min(newContent.Length, 50))}...");
    }

    /// <inheritdoc />
    public override Task ClearAsync()
    {
        _currentSummary = string.Empty;
        _recentMessages.Clear();
        // 实际应用中，这里会删除数据库中的所有相关记录 (In a real app, this deletes all related records in the database)
        return Task.CompletedTask;
    }
}

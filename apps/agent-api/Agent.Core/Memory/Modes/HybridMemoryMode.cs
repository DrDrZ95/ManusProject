namespace Agent.Core.Memory.Modes;

/// <summary>
/// 混合记忆模式 (Hybrid Memory Mode) - 策略设计模式 (Strategy Design Pattern)
/// 结合短期记忆 (Sliding Window) 和长期记忆 (Summary/Vector) 的优点，根据需要动态切换或组合记忆策略。
/// Combines the advantages of Short-Term Memory (Sliding Window) and Long-Term Memory (Summary/Vector), dynamically switching or combining memory strategies as needed.
/// </summary>
public class HybridMemoryMode : BaseAgentMemory
{
    public override string Name => "HybridMemory";

    // 组合不同的记忆策略 (Composition of different memory strategies)
    private readonly ShortTermMemoryMode _shortTermMemory = new();
    private readonly LongTermMemoryMode _longTermMemory = new();
    private readonly VectorMemoryMode _vectorMemory = new();

    /// <inheritdoc />
    public override async Task InitializeAsync(long conversationId)
    {
        await base.InitializeAsync(conversationId);
        // 初始化所有子记忆模块 (Initialize all sub-memory modules)
        await _shortTermMemory.InitializeAsync(conversationId);
        await _longTermMemory.InitializeAsync(conversationId);
        await _vectorMemory.InitializeAsync(conversationId);
    }

    /// <inheritdoc />
    public override async Task<MemoryContext> LoadContextAsync()
    {
        // 从短期记忆加载最近消息 (Load recent messages from short-term memory)
        var shortTermContext = await _shortTermMemory.LoadContextAsync();

        // 从长期记忆加载摘要 (Load summary from long-term memory)
        var longTermContext = await _longTermMemory.LoadContextAsync();

        // 从向量记忆加载相关知识 (Load relevant knowledge from vector memory)
        var vectorContext = await _vectorMemory.LoadContextAsync();

        // 组合所有上下文 (Combine all contexts)
        var combinedContext = new MemoryContext
        {
            HistoryMessages = shortTermContext.HistoryMessages,
            Summary = longTermContext.Summary,
            KnowledgeSnippets = vectorContext.KnowledgeSnippets
        };

        // 额外的逻辑：如果短期记忆为空，则尝试从长期记忆中提取更多信息 (Extra logic: if short-term memory is empty, try to extract more from long-term memory)
        if (!combinedContext.HistoryMessages.Any())
        {
            // 模拟从快照中恢复部分历史 (Simulate restoring partial history from snapshot)
            combinedContext.HistoryMessages.Add("... (从长期记忆快照中恢复的部分历史消息) - Partial history restored from long-term memory snapshot");
        }

        return combinedContext;
    }

    /// <inheritdoc />
    public override async Task SaveUpdateAsync(MemoryUpdate update)
    {
        // 将更新同时保存到所有相关的子记忆模块 (Save the update to all relevant sub-memory modules simultaneously)
        await _shortTermMemory.SaveUpdateAsync(update);
        await _longTermMemory.SaveUpdateAsync(update);
        await _vectorMemory.SaveUpdateAsync(update);
    }

    /// <inheritdoc />
    public override async Task ClearAsync()
    {
        // 清除所有子记忆模块 (Clear all sub-memory modules)
        await _shortTermMemory.ClearAsync();
        await _longTermMemory.ClearAsync();
        await _vectorMemory.ClearAsync();
    }
}

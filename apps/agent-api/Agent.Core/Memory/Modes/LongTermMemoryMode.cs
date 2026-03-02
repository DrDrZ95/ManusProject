using Agent.Core.Memory.Interfaces;

namespace Agent.Core.Memory.Modes;

/// <summary>
/// 长期记忆模式 (Long-Term Memory Mode) - 摘要/快照设计模式 (Summary/Snapshot Design Pattern)
/// 通过定期生成会话摘要或快照来压缩历史信息，以减少上下文窗口的压力。
/// Compresses historical information by periodically generating conversation summaries or snapshots to reduce context window pressure.
/// </summary>
public class LongTermMemoryMode : BaseAgentMemory
{
    private readonly dynamic? _semanticKernel;
    public override string Name => "LongTermMemory";

    public LongTermMemoryMode(object? semanticKernel = null)
    {
        _semanticKernel = semanticKernel;
    }

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

        // 达到阈值或明确要求时，触发摘要更新 (Trigger summary update when threshold is reached or explicitly requested)
        if (_recentMessages.Count >= SummaryUpdateThreshold || update.ShouldUpdateSummary)
        {
            // 模拟调用 LLM 服务生成新摘要 (Simulate calling LLM service to generate new summary)
            _currentSummary = await GenerateNewSummaryAsync(_currentSummary, _recentMessages);

            // 清空最近消息，等待下一轮摘要 (Clear recent messages for the next summary cycle)
            _recentMessages.Clear();
        }
    }

    /// <summary>
    /// 模拟生成新摘要的逻辑 (Simulates the logic for generating a new summary)
    /// </summary>
    private async Task<string> GenerateNewSummaryAsync(string oldSummary, List<string> newMessages)
    {
        if (_semanticKernel == null)
        {
            var newContent = string.Join(" ", newMessages.TakeLast(3));
            return $"[Mock Summary] Based on: {oldSummary}. Latest discussion: {newContent.Substring(0, Math.Min(newContent.Length, 50))}...";
        }

        var messages = string.Join("\n", newMessages);
        var systemPrompt = "你是一个专业的对话摘要助手，请将以下对话历史压缩为简洁的摘要，保留关键信息和决策。";
        var userPrompt = $"旧摘要：{oldSummary}\n\n新增对话：\n{messages}";

        try
        {
            return await _semanticKernel.GetChatCompletionAsync(userPrompt, systemPrompt);
        }
        catch (Exception ex)
        {
            // Fallback if LLM fails
            return $"[Error generating summary: {ex.Message}] Previous summary: {oldSummary}";
        }
    }

    /// <inheritdoc />
    public override Task ClearAsync()
    {
        _recentMessages.Clear();
        _currentSummary = string.Empty;
        return Task.CompletedTask;
    }
}

namespace Agent.Core.Memory.Interfaces;

/// <summary>
/// Advanced Agent Memory interface combining Short-term, Long-term, and Task memory
/// 高级智能体记忆接口，结合了短期、长期和任务记忆
/// </summary>
public interface IAdvancedAgentMemory : IAgentMemory
{
    IShortTermMemory ShortTerm { get; }
    ILongTermMemory LongTerm { get; }
    ITaskMemory Task { get; }
}

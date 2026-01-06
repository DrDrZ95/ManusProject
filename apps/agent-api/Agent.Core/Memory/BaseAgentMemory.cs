namespace Agent.Core.Memory;

/// <summary>
/// 智能体记忆模式的抽象基类 (Abstract base class for Agent Memory modes)
/// 提供了通用的结构和方法，简化具体记忆模式的实现 (Provides common structure and methods to simplify concrete memory mode implementation)
/// </summary>
public abstract class BaseAgentMemory : IAgentMemory
{
    protected long ConversationId { get; private set; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public virtual Task InitializeAsync(long conversationId)
    {
        ConversationId = conversationId;
        // 可以在这里添加通用的初始化逻辑，例如检查数据库连接 (Add common initialization logic here, e.g., checking DB connection)
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public abstract Task<MemoryContext> LoadContextAsync();

    /// <inheritdoc />
    public abstract Task SaveUpdateAsync(MemoryUpdate update);

    /// <inheritdoc />
    public abstract Task ClearAsync();
}

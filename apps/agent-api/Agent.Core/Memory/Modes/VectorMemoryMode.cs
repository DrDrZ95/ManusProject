namespace Agent.Core.Memory.Modes;

/// <summary>
/// 向量记忆模式 (Vector Memory Mode) - 向量数据库设计模式 (Vector Database Design Pattern)
/// 将所有消息和知识片段转换为向量并存储在 ChromaDB 中，通过语义相似度检索相关上下文。
/// Converts all messages and knowledge snippets into vectors and stores them in ChromaDB, retrieving relevant context via semantic similarity.
/// </summary>
public class VectorMemoryMode : BaseAgentMemory
{
    public override string Name => "VectorMemory";

    // 模拟 ChromaDB 客户端 (Simulate ChromaDB client)
    // 实际应用中，这里会注入 IVectorDatabaseService (In a real app, IVectorDatabaseService would be injected here)
    private readonly List<(string text, float[] vector)> _vectorStore = new();

    /// <inheritdoc />
    public override Task<MemoryContext> LoadContextAsync()
    {
        // 模拟用户输入 (Simulate user input for retrieval)
        var userInput = "用户最近问了关于项目截止日期的问题 (User recently asked about the project deadline)";

        // 模拟向量检索 (Simulate vector retrieval)
        var relevantSnippets = _vectorStore
            .Where(item => item.text.Contains("截止日期") || item.text.Contains("deadline")) // 简单模拟语义搜索 (Simple simulation of semantic search)
            .Select(item => item.text)
            .ToList();

        var context = new MemoryContext
        {
            // 向量记忆主要提供知识片段，历史消息可能由短期记忆补充 (Vector memory mainly provides knowledge snippets, history may be supplemented by short-term memory)
            KnowledgeSnippets = relevantSnippets,
            Summary = $"检索到 {relevantSnippets.Count} 条相关知识片段 (Retrieved {relevantSnippets.Count} relevant knowledge snippets)."
        };
        return Task.FromResult(context);
    }

    /// <inheritdoc />
    public override Task SaveUpdateAsync(MemoryUpdate update)
    {
        if (!string.IsNullOrEmpty(update.NewMessage))
        {
            // 模拟将新消息转换为向量并存储 (Simulate converting new message to vector and storing)
            // 实际应用中，这里会调用 IEmbeddingService (In a real app, IEmbeddingService would be called here)
            var mockVector = new float[] { 0.1f, 0.2f, 0.3f }; // 模拟向量 (Mock vector)
            _vectorStore.Add((update.NewMessage, mockVector));
            Console.WriteLine($"[VectorMemory] Stored new message as vector for Conversation {ConversationId}.");
        }
        // 向量记忆模式通常不直接处理 Summary 和 AbilityLog，而是将它们也向量化存储 (Vector memory mode usually vectorizes Summary and AbilityLog for storage)
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task ClearAsync()
    {
        _vectorStore.Clear();
        return Task.CompletedTask;
    }
}


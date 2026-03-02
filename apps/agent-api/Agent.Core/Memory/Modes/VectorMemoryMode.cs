using Agent.Core.Memory.Interfaces;

namespace Agent.Core.Memory.Modes;

/// <summary>
/// 向量记忆模式 (Vector Memory Mode) - 向量数据库设计模式 (Vector Database Design Pattern)
/// 将所有消息和知识片段转换为向量并存储在 ChromaDB 中，通过语义相似度检索相关上下文。
/// Converts all messages and knowledge snippets into vectors and stores them in ChromaDB, retrieving relevant context via semantic similarity.
/// </summary>
public class VectorMemoryMode : BaseAgentMemory
{
    private readonly dynamic? _vectorDb;
    private readonly dynamic? _semanticKernel;
    private readonly bool _enabled;

    public override string Name => "VectorMemory";

    public VectorMemoryMode(
        object? vectorDb = null,
        object? semanticKernel = null,
        bool enabled = false)
    {
        _vectorDb = vectorDb;
        _semanticKernel = semanticKernel;
        _enabled = enabled;
    }

    /// <inheritdoc />
    public override async Task<MemoryContext> LoadContextAsync()
    {
        if (!_enabled || _vectorDb == null)
        {
            return new MemoryContext
            {
                Summary = "Vector Memory is disabled or service is missing."
            };
        }

        try
        {
            // 以当前会话 ID 作为默认检索线索 (Use conversationId as search context)
            var collectionName = $"agent_memory_{ConversationId}";
            
            // 这里简单模拟一个检索查询，实际应根据当前上下文构建 (Simulate a search query)
            // Use dynamic to avoid lambda dispatch error
            var searchOptions = new { TopK = 5 };
            var searchResult = await _vectorDb.SearchByTextAsync(collectionName, "recent history", null);
            var relevantSnippets = new List<string>();
            
            if (searchResult != null && searchResult.Matches != null)
            {
                foreach (dynamic match in searchResult.Matches)
                {
                    if (match.Content != null)
                    {
                        relevantSnippets.Add(match.Content);
                    }
                }
            }

            return new MemoryContext
            {
                KnowledgeSnippets = relevantSnippets,
                Summary = $"检索到 {relevantSnippets.Count} 条相关向量知识片段。"
            };
        }
        catch (Exception ex)
        {
            return new MemoryContext { Summary = $"Vector search error: {ex.Message}" };
        }
    }

    /// <inheritdoc />
    public override async Task SaveUpdateAsync(MemoryUpdate update)
    {
        if (!_enabled || _vectorDb == null || _semanticKernel == null || string.IsNullOrEmpty(update.NewMessage))
        {
            return;
        }

        try
        {
            var collectionName = $"agent_memory_{ConversationId}";
            var embedding = await _semanticKernel.GenerateEmbeddingAsync(update.NewMessage);

            // Use dynamic object to represent VectorDocument to bypass type check issues
            // but ensure we match the property names of VectorDocument
            var doc = new
            {
                Id = Guid.NewGuid().ToString(),
                Content = update.NewMessage,
                Embedding = (float[])embedding,
                Metadata = new Dictionary<string, object>
                {
                    { "ConversationId", ConversationId },
                    { "Timestamp", DateTime.UtcNow }
                },
                Modality = 0 // Modality.Text is usually 0
            };

            var documents = new List<object> { doc };
            await _vectorDb.AddDocumentsAsync(collectionName, documents);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VectorMemory] Error saving update: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task ClearAsync()
    {
        if (_enabled && _vectorDb != null)
        {
            var collectionName = $"agent_memory_{ConversationId}";
            try
            {
                await _vectorDb.DeleteCollectionAsync(collectionName);
            }
            catch { /* Ignore if collection doesn't exist */ }
        }
    }
}


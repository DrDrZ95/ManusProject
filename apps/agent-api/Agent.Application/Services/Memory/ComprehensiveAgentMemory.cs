namespace Agent.Application.Services.Memory;

/// <summary>
/// Comprehensive Agent Memory implementation
/// </summary>
public class ComprehensiveAgentMemory : IAdvancedAgentMemory
{
    public string Name => "ComprehensiveMemory";

    public IShortTermMemory ShortTerm { get; }
    public ILongTermMemory LongTerm { get; }
    public ITaskMemory Task { get; }

    private long _conversationId;

    public ComprehensiveAgentMemory(
        IShortTermMemory shortTerm,
        ILongTermMemory longTerm,
        ITaskMemory task)
    {
        ShortTerm = shortTerm;
        LongTerm = longTerm;
        Task = task;
    }

    public async System.Threading.Tasks.Task InitializeAsync(long conversationId)
    {
        _conversationId = conversationId;
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.Task<MemoryContext> LoadContextAsync()
    {
        var sessionId = _conversationId.ToString();

        // a. ShortTerm.GetRecentHistoryAsync(sessionId, tokenLimit: 2000)
        var history = await ShortTerm.GetRecentHistoryAsync(sessionId, tokenLimit: 2000);
        var historyMessages = history.Select(m => $"{m.Role}: {m.Content}").ToList();

        // b. LongTerm.RetrieveRelevantKnowledgeAsync (as summary/context)
        var summary = string.Empty;
        try
        {
            var relevantKnowledge = await LongTerm.RetrieveRelevantKnowledgeAsync(sessionId, limit: 1);
            summary = relevantKnowledge.FirstOrDefault() ?? string.Empty;
        }
        catch
        {
            // Fallback if LongTerm fails
            summary = "Summary not available.";
        }

        // c. Task.GetCurrentTaskContextAsync
        var knowledgeSnippets = new List<string>();
        try
        {
            var taskContext = await Task.GetContextAsync(sessionId);
            if (taskContext != null)
            {
                knowledgeSnippets.Add($"Workflow: {taskContext.WorkflowId}, Step: {taskContext.StepId ?? "Initial"}");
            }
        }
        catch
        {
            // Ignore task memory errors
        }

        return new MemoryContext
        {
            HistoryMessages = historyMessages,
            Summary = summary,
            KnowledgeSnippets = knowledgeSnippets
        };
    }

    public async System.Threading.Tasks.Task SaveUpdateAsync(MemoryUpdate update)
    {
        // Route updates to sub-memories
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.Task ClearAsync()
    {
        await System.Threading.Tasks.Task.CompletedTask;
    }
}


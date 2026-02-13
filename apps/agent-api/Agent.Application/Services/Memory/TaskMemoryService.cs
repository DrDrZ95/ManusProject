namespace Agent.Application.Services.Memory;

/// <summary>
/// Task memory implementation using Cache and DB
/// </summary>
public class TaskMemoryService : ITaskMemory
{
    private readonly IAgentCacheService _cacheService;
    private readonly IRepository<TaskExecutionContextEntity, Guid> _taskRepo;
    private readonly ILogger<TaskMemoryService> _logger;

    public TaskMemoryService(
        IAgentCacheService cacheService,
        IRepository<TaskExecutionContextEntity, Guid> taskRepo,
        ILogger<TaskMemoryService> logger)
    {
        _cacheService = cacheService;
        _taskRepo = taskRepo;
        _logger = logger;
    }

    public async Task SaveContextAsync(TaskExecutionContextEntity context)
    {
        // Save to DB
        // Check if exists
        var existing = await _taskRepo.FirstOrDefaultAsync(t => t.Id == context.Id);
        if (existing != null)
        {
            existing.ContextData = context.ContextData;
            existing.ToolCallHistory = context.ToolCallHistory;
            existing.DecisionPath = context.DecisionPath;
            existing.StepId = context.StepId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _taskRepo.UpdateAsync(existing);
        }
        else
        {
            await _taskRepo.AddAsync(context);
        }

        // Update Cache
        var cacheKey = $"task_context:{context.WorkflowId}";
        await _cacheService.SetAsync(cacheKey, context, TimeSpan.FromMinutes(30), TimeSpan.FromDays(1));
    }

    public async Task<TaskExecutionContextEntity?> GetContextAsync(string workflowId, string? stepId = null)
    {
        var cacheKey = $"task_context:{workflowId}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            // If stepId is provided, we might want that specific step, but usually context is per workflow.
            // Assuming workflowId is unique for the run.
            return await _taskRepo.FirstOrDefaultAsync(t => t.WorkflowId == workflowId);
        }, TimeSpan.FromMinutes(30), TimeSpan.FromDays(1));
    }

    public async Task RecordToolCallAsync(string workflowId, string toolName, string parameters, string result)
    {
        var context = await GetContextAsync(workflowId);
        if (context == null)
        {
            context = new TaskExecutionContextEntity
            {
                WorkflowId = workflowId,
                ToolCallHistory = "[]" // Init as empty JSON array
            };
        }

        // Parse existing history
        var historyList = new List<Dictionary<string, string>>();
        if (!string.IsNullOrWhiteSpace(context.ToolCallHistory))
        {
            try
            {
                historyList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(context.ToolCallHistory)
                              ?? new List<Dictionary<string, string>>();
            }
            catch (JsonException)
            {
                // Fallback for any legacy data
                historyList = new List<Dictionary<string, string>>();
            }
        }

        // Add new entry
        historyList.Add(new Dictionary<string, string>
        {
            { "Tool", toolName },
            { "Params", parameters },
            { "Result", result },
            { "Timestamp", DateTime.UtcNow.ToString("O") }
        });

        // Serialize back
        context.ToolCallHistory = JsonSerializer.Serialize(historyList);

        await SaveContextAsync(context);
    }
}


using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agent.Core.Services.Workflow;

/// <summary>
/// Workflow service implementation
/// 工作流服务实现
/// 
/// 基于AI-Agent项目的planning.py转换而来，实现了核心的工作流管理和todo文件交互功能
/// Converted from AI-Agent project's planning.py, implementing core workflow management and todo file interaction
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly ILogger<WorkflowService> _logger;
    private readonly WorkflowOptions _options;
    
    // 内存存储计划数据（生产环境中应使用数据库）
    // In-memory storage for plan data (should use database in production)
    private readonly Dictionary<string, WorkflowPlan> _plans = new();
    private readonly object _lockObject = new();

    public WorkflowService(
        ILogger<WorkflowService> logger,
        IOptions<WorkflowOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Create a new workflow plan
    /// 创建新的工作流计划
    /// 
    /// 对应AI-Agent中的_create_initial_plan方法
    /// Corresponds to _create_initial_plan method in AI-Agent
    /// </summary>
    public async Task<WorkflowPlan> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var planId = $"plan_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            
            var plan = new WorkflowPlan
            {
                Id = planId,
                Title = request.Title,
                Description = request.Description,
                ExecutorKeys = request.ExecutorKeys,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 创建步骤对象 - Create step objects
            for (int i = 0; i < request.Steps.Count; i++)
            {
                var stepText = request.Steps[i];
                var step = new WorkflowStep
                {
                    Index = i,
                    Text = stepText,
                    Type = ExtractStepType(stepText), // 提取步骤类型 - Extract step type
                    Status = PlanStepStatus.NotStarted
                };
                
                plan.Steps.Add(step);
                plan.StepStatuses.Add(PlanStepStatus.NotStarted);
            }

            // 设置第一个步骤为当前步骤 - Set first step as current
            if (plan.Steps.Count > 0)
            {
                plan.CurrentStepIndex = 0;
            }

            lock (_lockObject)
            {
                _plans[planId] = plan;
            }

            _logger.LogInformation("Created workflow plan with ID: {PlanId}, Title: {Title}", planId, request.Title);
            
            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow plan: {Title}", request.Title);
            throw;
        }
    }

    /// <summary>
    /// Get workflow plan by ID
    /// 根据ID获取工作流计划
    /// </summary>
    public async Task<WorkflowPlan?> GetPlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return _plans.TryGetValue(planId, out var plan) ? plan : null;
        }
    }

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    public async Task<List<WorkflowPlan>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return _plans.Values.ToList();
        }
    }

    /// <summary>
    /// Update step status in a plan
    /// 更新计划中的步骤状态
    /// 
    /// 对应AI-Agent中的mark_step功能
    /// Corresponds to mark_step functionality in AI-Agent
    /// </summary>
    public async Task<bool> UpdateStepStatusAsync(string planId, int stepIndex, PlanStepStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lockObject)
            {
                if (!_plans.TryGetValue(planId, out var plan))
                {
                    _logger.LogWarning("Plan not found: {PlanId}", planId);
                    return false;
                }

                if (stepIndex < 0 || stepIndex >= plan.Steps.Count)
                {
                    _logger.LogWarning("Invalid step index: {StepIndex} for plan: {PlanId}", stepIndex, planId);
                    return false;
                }

                // 更新步骤状态 - Update step status
                plan.Steps[stepIndex].Status = status;
                plan.StepStatuses[stepIndex] = status;
                plan.UpdatedAt = DateTime.UtcNow;

                // 更新时间戳 - Update timestamps
                var step = plan.Steps[stepIndex];
                switch (status)
                {
                    case PlanStepStatus.InProgress:
                        step.StartedAt = DateTime.UtcNow;
                        break;
                    case PlanStepStatus.Completed:
                        step.CompletedAt = DateTime.UtcNow;
                        break;
                }

                _logger.LogDebug("Updated step {StepIndex} status to {Status} for plan {PlanId}", 
                    stepIndex, status, planId);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step status for plan: {PlanId}, step: {StepIndex}", planId, stepIndex);
            return false;
        }
    }

    /// <summary>
    /// Get current active step in a plan
    /// 获取计划中当前活动步骤
    /// 
    /// 对应AI-Agent中的_get_current_step_info方法
    /// Corresponds to _get_current_step_info method in AI-Agent
    /// </summary>
    public async Task<WorkflowStep?> GetCurrentStepAsync(string planId, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lockObject)
            {
                if (!_plans.TryGetValue(planId, out var plan))
                {
                    _logger.LogWarning("Plan not found: {PlanId}", planId);
                    return null;
                }

                // 查找第一个活动步骤 - Find first active step
                for (int i = 0; i < plan.Steps.Count; i++)
                {
                    var step = plan.Steps[i];
                    if (step.Status.IsActive())
                    {
                        // 如果步骤未开始，标记为进行中 - If step not started, mark as in progress
                        if (step.Status == PlanStepStatus.NotStarted)
                        {
                            UpdateStepStatusAsync(planId, i, PlanStepStatus.InProgress, cancellationToken);
                        }
                        
                        plan.CurrentStepIndex = i;
                        return step;
                    }
                }

                // 没有找到活动步骤 - No active step found
                plan.CurrentStepIndex = null;
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current step for plan: {PlanId}", planId);
            return null;
        }
    }

    /// <summary>
    /// Mark step as completed and move to next
    /// 标记步骤为已完成并移动到下一步
    /// </summary>
    public async Task<bool> CompleteStepAsync(string planId, int stepIndex, string? result = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await UpdateStepStatusAsync(planId, stepIndex, PlanStepStatus.Completed, cancellationToken);
            
            if (success && !string.IsNullOrEmpty(result))
            {
                lock (_lockObject)
                {
                    if (_plans.TryGetValue(planId, out var plan) && stepIndex < plan.Steps.Count)
                    {
                        plan.Steps[stepIndex].Result = result;
                    }
                }
            }

            _logger.LogInformation("Completed step {StepIndex} for plan {PlanId}", stepIndex, planId);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step for plan: {PlanId}, step: {StepIndex}", planId, stepIndex);
            return false;
        }
    }

    /// <summary>
    /// Generate todo list file content for a plan
    /// 为计划生成待办事项列表文件内容
    /// 
    /// 这是AI-Agent中的核心功能，生成markdown格式的任务列表
    /// This is a core feature from AI-Agent, generating markdown format task lists
    /// </summary>
    public async Task<string> GenerateToDoListAsync(string planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await GetPlanAsync(planId, cancellationToken);
            if (plan == null)
            {
                throw new ArgumentException($"Plan not found: {planId}");
            }

            var todoContent = new List<string>
            {
                $"# {plan.Title}",
                "",
                $"**计划ID**: {plan.Id}",
                $"**创建时间**: {plan.CreatedAt:yyyy-MM-dd HH:mm:ss}",
                $"**最后更新**: {plan.UpdatedAt:yyyy-MM-dd HH:mm:ss}",
                ""
            };

            if (!string.IsNullOrEmpty(plan.Description))
            {
                todoContent.Add($"**描述**: {plan.Description}");
                todoContent.Add("");
            }

            // 添加进度信息 - Add progress information
            var progress = await GetProgressAsync(planId, cancellationToken);
            todoContent.Add("## 进度概览 (Progress Overview)");
            todoContent.Add("");
            todoContent.Add($"- **总步骤数**: {progress.TotalSteps}");
            todoContent.Add($"- **已完成**: {progress.CompletedSteps}");
            todoContent.Add($"- **进行中**: {progress.InProgressSteps}");
            todoContent.Add($"- **已阻塞**: {progress.BlockedSteps}");
            todoContent.Add($"- **完成度**: {progress.ProgressPercentage:F1}%");
            todoContent.Add("");

            // 添加任务列表 - Add task list
            todoContent.Add("## 任务列表 (Task List)");
            todoContent.Add("");

            for (int i = 0; i < plan.Steps.Count; i++)
            {
                var step = plan.Steps[i];
                var marker = step.Status.GetMarker();
                var stepLine = $"{marker} **步骤 {i + 1}**: {step.Text}";
                
                // 添加步骤类型标识 - Add step type identifier
                if (!string.IsNullOrEmpty(step.Type))
                {
                    stepLine += $" `[{step.Type.ToUpper()}]`";
                }

                todoContent.Add(stepLine);

                // 添加步骤详细信息 - Add step details
                if (step.Status == PlanStepStatus.InProgress && step.StartedAt.HasValue)
                {
                    todoContent.Add($"  - *开始时间*: {step.StartedAt.Value:yyyy-MM-dd HH:mm:ss}");
                }
                else if (step.Status == PlanStepStatus.Completed && step.CompletedAt.HasValue)
                {
                    todoContent.Add($"  - *完成时间*: {step.CompletedAt.Value:yyyy-MM-dd HH:mm:ss}");
                    if (!string.IsNullOrEmpty(step.Result))
                    {
                        todoContent.Add($"  - *执行结果*: {step.Result}");
                    }
                }
                else if (step.Status == PlanStepStatus.Blocked)
                {
                    todoContent.Add($"  - *状态*: 已阻塞，需要处理");
                }

                todoContent.Add("");
            }

            // 添加状态说明 - Add status legend
            todoContent.Add("## 状态说明 (Status Legend)");
            todoContent.Add("");
            todoContent.Add("- `[ ]` 未开始 (Not Started)");
            todoContent.Add("- `[→]` 进行中 (In Progress)");
            todoContent.Add("- `[✓]` 已完成 (Completed)");
            todoContent.Add("- `[!]` 已阻塞 (Blocked)");
            todoContent.Add("");

            // 添加元数据 - Add metadata
            if (plan.Metadata.Count > 0)
            {
                todoContent.Add("## 元数据 (Metadata)");
                todoContent.Add("");
                foreach (var kvp in plan.Metadata)
                {
                    todoContent.Add($"- **{kvp.Key}**: {kvp.Value}");
                }
                todoContent.Add("");
            }

            // 添加生成时间戳 - Add generation timestamp
            todoContent.Add("---");
            todoContent.Add($"*生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");

            return string.Join(Environment.NewLine, todoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating todo list for plan: {PlanId}", planId);
            throw;
        }
    }

    /// <summary>
    /// Save todo list to file
    /// 将待办事项列表保存到文件
    /// </summary>
    public async Task<bool> SaveToDoListToFileAsync(string planId, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var todoContent = await GenerateToDoListAsync(planId, cancellationToken);
            
            // 确保目录存在 - Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, todoContent, cancellationToken);
            
            _logger.LogInformation("Saved todo list for plan {PlanId} to file: {FilePath}", planId, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving todo list to file for plan: {PlanId}, file: {FilePath}", planId, filePath);
            return false;
        }
    }

    /// <summary>
    /// Load todo list from file and update plan status
    /// 从文件加载待办事项列表并更新计划状态
    /// 
    /// 这个功能允许从markdown文件中解析任务状态并更新计划
    /// This feature allows parsing task status from markdown files and updating plans
    /// </summary>
    public async Task<string?> LoadToDoListFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Todo list file not found: {FilePath}", filePath);
                return null;
            }

            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            // 解析计划ID - Parse plan ID
            var planIdMatch = Regex.Match(content, @"\*\*计划ID\*\*:\s*(.+)");
            if (!planIdMatch.Success)
            {
                _logger.LogWarning("Could not find plan ID in todo list file: {FilePath}", filePath);
                return null;
            }

            var planId = planIdMatch.Groups[1].Value.Trim();
            
            lock (_lockObject)
            {
                if (!_plans.TryGetValue(planId, out var plan))
                {
                    _logger.LogWarning("Plan not found for ID from file: {PlanId}", planId);
                    return null;
                }

                // 解析任务状态 - Parse task statuses
                var taskMatches = Regex.Matches(content, @"(\[[ →✓!]\])\s*\*\*步骤\s*(\d+)\*\*:\s*(.+)");
                
                foreach (Match match in taskMatches)
                {
                    var statusMarker = match.Groups[1].Value;
                    var stepNumber = int.Parse(match.Groups[2].Value);
                    var stepIndex = stepNumber - 1; // 转换为0基索引 - Convert to 0-based index

                    if (stepIndex >= 0 && stepIndex < plan.Steps.Count)
                    {
                        var status = ParseStatusFromMarker(statusMarker);
                        plan.Steps[stepIndex].Status = status;
                        plan.StepStatuses[stepIndex] = status;
                    }
                }

                plan.UpdatedAt = DateTime.UtcNow;
            }

            _logger.LogInformation("Loaded todo list from file and updated plan: {PlanId}", planId);
            return planId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading todo list from file: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Delete a workflow plan
    /// 删除工作流计划
    /// </summary>
    public async Task<bool> DeletePlanAsync(string planId, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lockObject)
            {
                var removed = _plans.Remove(planId);
                if (removed)
                {
                    _logger.LogInformation("Deleted workflow plan: {PlanId}", planId);
                }
                return removed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow plan: {PlanId}", planId);
            return false;
        }
    }

    /// <summary>
    /// Get plan execution progress
    /// 获取计划执行进度
    /// </summary>
    public async Task<WorkflowProgress> GetProgressAsync(string planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(planId, cancellationToken);
        if (plan == null)
        {
            return new WorkflowProgress { PlanId = planId };
        }

        var progress = new WorkflowProgress
        {
            PlanId = planId,
            TotalSteps = plan.Steps.Count,
            CurrentStepIndex = plan.CurrentStepIndex
        };

        foreach (var status in plan.StepStatuses)
        {
            switch (status)
            {
                case PlanStepStatus.Completed:
                    progress.CompletedSteps++;
                    break;
                case PlanStepStatus.InProgress:
                    progress.InProgressSteps++;
                    break;
                case PlanStepStatus.Blocked:
                    progress.BlockedSteps++;
                    break;
            }
        }

        return progress;
    }

    #region Private Helper Methods

    /// <summary>
    /// Extract step type from step text
    /// 从步骤文本中提取步骤类型
    /// 
    /// 对应AI-Agent中的类型提取逻辑
    /// Corresponds to type extraction logic in AI-Agent
    /// </summary>
    private static string? ExtractStepType(string stepText)
    {
        // 查找类似 [SEARCH] 或 [CODE] 的模式 - Look for patterns like [SEARCH] or [CODE]
        var match = Regex.Match(stepText, @"\[([A-Z_]+)\]");
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    /// <summary>
    /// Parse status from marker symbol
    /// 从标记符号解析状态
    /// </summary>
    private static PlanStepStatus ParseStatusFromMarker(string marker)
    {
        return marker switch
        {
            "[ ]" => PlanStepStatus.NotStarted,
            "[→]" => PlanStepStatus.InProgress,
            "[✓]" => PlanStepStatus.Completed,
            "[!]" => PlanStepStatus.Blocked,
            _ => PlanStepStatus.NotStarted
        };
    }

    #endregion
}

/// <summary>
/// Workflow service configuration options
/// 工作流服务配置选项
/// </summary>
public class WorkflowOptions
{
    /// <summary>
    /// Default todo list file directory
    /// 默认待办事项列表文件目录
    /// </summary>
    public string DefaultToDoDirectory { get; set; } = "todo_lists";

    /// <summary>
    /// Enable file auto-save
    /// 启用文件自动保存
    /// </summary>
    public bool EnableAutoSave { get; set; } = true;

    /// <summary>
    /// Auto-save interval in minutes
    /// 自动保存间隔（分钟）
    /// </summary>
    public int AutoSaveIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Maximum number of plans to keep in memory
    /// 内存中保留的最大计划数
    /// </summary>
    public int MaxPlansInMemory { get; set; } = 100;

    /// <summary>
    /// Enable detailed logging
    /// 启用详细日志记录
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
}


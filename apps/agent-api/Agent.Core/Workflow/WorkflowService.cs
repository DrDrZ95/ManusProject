using Agent.Core.Data.Repositories;
using Agent.Core.Data.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Agent.Core.Workflow.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Agent.Core.Workflow;

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
    private readonly IWorkflowRepository _repository;

    public WorkflowService(
        ILogger<WorkflowService> logger,
        IOptions<WorkflowOptions> options,
        IWorkflowRepository repository)
    {
        _logger = logger;
        _options = options.Value;
        _repository = repository;
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
            // 1. 创建业务模型 (Create business model)
            var plan = new WorkflowPlan
            {
                // ID将在Repository中生成
                Title = request.Title,
                Description = request.Description,
                ExecutorKeys = request.ExecutorKeys,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = PlanStatus.InProgress // 默认创建后即为进行中
            };

            // 2. 创建步骤对象 - Create step objects
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
            }
            
            // 3. 转换为实体并持久化 (Convert to entity and persist)
            var planEntity = plan.ToEntity();
            var addedEntity = await _repository.AddPlanAsync(planEntity, cancellationToken);

            // 4. 转换回业务模型并返回 (Convert back to business model and return)
            var resultPlan = addedEntity.ToModel();
            
            _logger.LogInformation("Created workflow plan with ID: {PlanId}, Title: {Title}", resultPlan.Id, request.Title);
            
            return resultPlan;
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
        if (!Guid.TryParse(planId, out var id))
        {
            _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
            return null;
        }
        
        var entity = await _repository.GetPlanByIdAsync(id, cancellationToken);
        
        return entity?.ToModel();
    }

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    public async Task<List<WorkflowPlan>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllPlansAsync(cancellationToken);
        
        return entities.Select(e => e.ToModel()).ToList();
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
            if (!Guid.TryParse(planId, out var id))
            {
                _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
                return false;
            }

            var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
            if (planEntity == null)
            {
                _logger.LogWarning("Plan not found: {PlanId}", planId);
                return false;
            }

            var stepEntity = planEntity.Steps.FirstOrDefault(s => s.Index == stepIndex);
            if (stepEntity == null)
            {
                _logger.LogWarning("Invalid step index: {StepIndex} for plan: {PlanId}", stepIndex, planId);
                return false;
            }

            // 更新时间戳 - Update timestamps
            DateTime? startedAt = null;
            DateTime? completedAt = null;
            if (status == PlanStepStatus.InProgress)
            {
                startedAt = DateTime.UtcNow;
            }
            else if (status == PlanStepStatus.Completed)
            {
                completedAt = DateTime.UtcNow;
            }

            // 使用 Repository 的专用方法更新状态 (Use Repository's dedicated method to update status)
            await _repository.UpdateStepStatusAndResultAsync(
                stepEntity.Id, 
                status, 
                stepEntity.Result, // 保持 Result 不变
                startedAt, 
                completedAt,
                cancellationToken);

            _logger.LogDebug("Updated step {StepIndex} status to {Status} for plan {PlanId}", 
                stepIndex, status, planId);
            
            return true;
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
        if (!Guid.TryParse(planId, out var id))
        {
            _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
            return null;
        }

        var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
        if (planEntity == null)
        {
            _logger.LogWarning("Plan not found: {PlanId}", planId);
            return null;
        }

        // 查找第一个活动步骤 - Find first active step
        var activeStepEntity = planEntity.Steps
            .OrderBy(s => s.Index)
            .FirstOrDefault(s => s.Status.IsActive());

        if (activeStepEntity != null)
        {
            // 如果步骤未开始，标记为进行中 - If step not started, mark as in progress
            if (activeStepEntity.Status == PlanStepStatus.NotStarted)
            {
                // 自动将状态更新为 InProgress
                await UpdateStepStatusAsync(planId, activeStepEntity.Index, PlanStepStatus.InProgress, cancellationToken);
                // 重新获取更新后的实体 (Re-fetch the updated entity)
                planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
                activeStepEntity = planEntity?.Steps.FirstOrDefault(s => s.Index == activeStepEntity.Index);
            }
            
            return activeStepEntity?.ToModel();
        }

        // 没有找到活动步骤 - No active step found
        return null;
    }

    /// <summary>
    /// Mark step as completed and move to next
    /// 标记步骤为已完成并移动到下一步
    /// </summary>
    public async Task<bool> CompleteStepAsync(string planId, int stepIndex, string? result = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. 更新状态为 Completed (Update status to Completed)
            var success = await UpdateStepStatusAsync(planId, stepIndex, PlanStepStatus.Completed, cancellationToken);
            
            // 2. 如果成功且有结果，则更新结果 (If successful and result exists, update result)
            if (success && !string.IsNullOrEmpty(result))
            {
                if (!Guid.TryParse(planId, out var id))
                {
                    _logger.LogWarning("Invalid Plan ID format: {PlanId}", planId);
                    return false;
                }

                var planEntity = await _repository.GetPlanByIdAsync(id, cancellationToken);
                var stepEntity = planEntity?.Steps.FirstOrDefault(s => s.Index == stepIndex);

                if (stepEntity != null)
                {
                    // 使用 Repository 的专用方法更新结果 (Use Repository's dedicated method to update result)
                    await _repository.UpdateStepStatusAndResultAsync(
                        stepEntity.Id, 
                        PlanStepStatus.Completed, // 确保状态是 Completed
                        result, 
                        null, 
                        DateTime.UtcNow,
                        cancellationToken);
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
                $"**描述**: {plan.Description}",
                "",
                "## 步骤列表 (Steps List)",
                ""
            };

            foreach (var step in plan.Steps)
            {
                var statusChar = step.Status.ToMarkdownStatusChar();
                var resultLine = string.IsNullOrEmpty(step.Result) ? "" : $" (结果: {step.Result})";
                todoContent.Add($"- [{statusChar}] {step.Text}{resultLine}");
            }

            return string.Join(Environment.NewLine, todoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating todo list for plan: {PlanId}", planId);
            throw;
        }
    }

    /// <summary>
    /// Helper method to extract step type from text (e.g., [CODE], [SEARCH])
    /// 辅助方法：从文本中提取步骤类型
    /// </summary>
    private string? ExtractStepType(string stepText)
    {
        var match = System.Text.RegularExpressions.Regex.Match(stepText, @"^\[(\w+)\]");
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }
}

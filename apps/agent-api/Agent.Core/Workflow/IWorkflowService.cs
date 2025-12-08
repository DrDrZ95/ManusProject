using Agent.Core.Workflow.Models;

namespace Agent.Core.Workflow;

/// <summary>
/// Interface for workflow management operations
/// 工作流管理操作接口
/// 
/// 基于AI-Agent项目的planning.py转换而来
/// Converted from AI-Agent project's planning.py
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Create a new workflow plan
    /// 创建新的工作流计划
    /// </summary>
    /// <param name="request">Plan creation request - 计划创建请求</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Created plan information - 创建的计划信息</returns>
    Task<WorkflowPlan> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow plan by ID
    /// 根据ID获取工作流计划
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Workflow plan or null if not found - 工作流计划或null（如果未找到）</returns>
    Task<WorkflowPlan?> GetPlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all workflow plans
    /// 获取所有工作流计划
    /// </summary>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>List of workflow plans - 工作流计划列表</returns>
    Task<List<WorkflowPlan>> GetAllPlansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update step status in a plan
    /// 更新计划中的步骤状态
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="stepIndex">Step index - 步骤索引</param>
    /// <param name="status">New status - 新状态</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> UpdateStepStatusAsync(string planId, int stepIndex, PlanStepStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current active step in a plan
    /// 获取计划中当前活动步骤
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Current step information or null if no active step - 当前步骤信息或null（如果没有活动步骤）</returns>
    Task<WorkflowStep?> GetCurrentStepAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark step as completed and move to next
    /// 标记步骤为已完成并移动到下一步
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="stepIndex">Step index - 步骤索引</param>
    /// <param name="result">Step execution result - 步骤执行结果</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> CompleteStepAsync(string planId, int stepIndex, string? result = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate todo list file content for a plan
    /// 为计划生成待办事项列表文件内容
    /// 
    /// 这是AI-Agent中的核心功能，用于生成可视化的任务列表
    /// This is a core feature from AI-Agent for generating visual task lists
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Todo list content - 待办事项列表内容</returns>
    Task<string> GenerateToDoListAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save todo list to file
    /// 将待办事项列表保存到文件
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="filePath">File path - 文件路径</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> SaveToDoListToFileAsync(string planId, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load todo list from file and update plan status
    /// 从文件加载待办事项列表并更新计划状态
    /// </summary>
    /// <param name="filePath">File path - 文件路径</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Loaded plan ID or null if failed - 加载的计划ID或null（如果失败）</returns>
    Task<string?> LoadToDoListFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a workflow plan
    /// 删除工作流计划
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> DeletePlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get plan execution progress
    /// 获取计划执行进度
    /// </summary>
    /// <param name="planId">Plan ID - 计划ID</param>
    /// <param name="cancellationToken">Cancellation token - 取消令牌</param>
    /// <returns>Progress information - 进度信息</returns>
    Task<WorkflowProgress> GetProgressAsync(string planId, CancellationToken cancellationToken = default);

    Task<string> CreateWorkflowAsync(string llmResponse);
}

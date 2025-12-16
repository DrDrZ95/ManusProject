using Agent.Core.Data.Entities;
using Agent.Core.Workflow.Models;

namespace Agent.Core.Data.Mappers;

/// <summary>
/// 工作流模型映射器 (Workflow Model Mapper)
/// 负责在业务模型 (WorkflowPlan/WorkflowStep) 和持久化实体 (WorkflowPlanEntity/WorkflowStepEntity) 之间进行转换。
/// </summary>
public static class WorkflowMapper
{
    // --- Entity to Model (实体到业务模型) ---

    public static WorkflowPlan ToModel(this WorkflowPlanEntity entity)
    {
        var model = new WorkflowPlan
        {
            Id = entity.Id.ToString(),
            Title = entity.Title,
            Description = entity.Description,
            // ExecutorKeys 和 Metadata 假设在 Entity 中是 string，需要反序列化，这里简化处理
            // In a real app, Metadata/ExecutorKeys would need JSON deserialization
            ExecutorKeys = new List<string>(), 
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Status = entity.Status,
            Steps = entity.Steps.Select(s => s.ToModel()).ToList(),
            StepStatuses = entity.Steps.Select(s => s.Status).ToList(),
            CurrentStepIndex = entity.Steps.FirstOrDefault(s => s.Status.IsActive())?.Index
        };
        return model;
    }

    public static WorkflowStep ToModel(this WorkflowStepEntity entity)
    {
        return new WorkflowStep
        {
            Index = entity.Index,
            Text = entity.Text,
            Type = entity.Type,
            Status = entity.Status,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            Result = entity.Result
        };
    }

    // --- Model to Entity (业务模型到实体) ---

    public static WorkflowPlanEntity ToEntity(this WorkflowPlan model)
    {
        var entity = new WorkflowPlanEntity
        {
            Id = Guid.TryParse(model.Id, out var id) ? id : Guid.Empty,
            Title = model.Title,
            Description = model.Description,
            // ExecutorKeys 和 Metadata 假设在 Entity 中是 string，需要序列化，这里简化处理
            Metadata = model.Metadata,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            Status = model.Status,
            Steps = model.Steps.Select(s => s.ToEntity(model.Id)).ToList()
        };
        return entity;
    }

    public static WorkflowStepEntity ToEntity(this WorkflowStep model, Guid planId)
    {
        return new WorkflowStepEntity
        {
            PlanId = planId,
            Index = model.Index,
            Text = model.Text,
            Type = model.Type,
            Status = model.Status,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt,
            Result = model.Result
        };
    }
}

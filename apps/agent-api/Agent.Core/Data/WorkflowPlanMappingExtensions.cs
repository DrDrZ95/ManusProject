using Mapster;

namespace Agent.Core.Data;

public static class WorkflowPlanMappingExtensions
{
    public static WorkflowPlan ToModel(this WorkflowPlanEntity entity)
    {
        return entity.Adapt<WorkflowPlan>();
    }

    public static WorkflowPlanEntity ToEntity(this WorkflowPlan model)
    {
        return model.Adapt<WorkflowPlanEntity>();
    }
}

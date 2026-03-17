namespace Agent.Core.Data.Repositories;

public static class WorkflowMappingExtensions
{
    public static WorkflowStepEntity ToEntity(this WorkflowStep model)
    {
        return model.Adapt<WorkflowStepEntity>();
    }
}

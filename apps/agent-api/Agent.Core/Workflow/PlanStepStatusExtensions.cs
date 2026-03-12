namespace Agent.Core.Workflow;

public static class PlanStepStatusExtensions
{
    public static bool IsActive(this PlanStepStatus status)
    {
        return status == PlanStepStatus.NotStarted || status == PlanStepStatus.InProgress;
    }
}

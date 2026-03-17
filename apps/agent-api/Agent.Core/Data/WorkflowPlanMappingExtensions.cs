namespace Agent.Core.Data;

public static class WorkflowPlanMappingExtensions
{
    static WorkflowPlanMappingExtensions()
    {
        // 配置 Mapster 映射 (Configure Mapster mapping)
        TypeAdapterConfig<WorkflowStepEntity, WorkflowStep>.NewConfig()
            .Map(dest => dest.DependsOn, src => !string.IsNullOrEmpty(src.DependsOnJson) 
                ? JsonSerializer.Deserialize<List<int>>(src.DependsOnJson, (JsonSerializerOptions?)null) 
                : new List<int>())
            .Map(dest => dest.HasBreakpoint, src => src.IsBreakpoint);

        TypeAdapterConfig<WorkflowStep, WorkflowStepEntity>.NewConfig()
            .Map(dest => dest.DependsOnJson, src => JsonSerializer.Serialize(src.DependsOn, (JsonSerializerOptions?)null))
            .Map(dest => dest.IsBreakpoint, src => src.HasBreakpoint);

        TypeAdapterConfig<WorkflowPlanEntity, WorkflowPlan>.NewConfig()
            .Map(dest => dest.ExecutorKeys, src => !string.IsNullOrEmpty(src.ExecutorKeys) 
                ? JsonSerializer.Deserialize<List<string>>(src.ExecutorKeys, (JsonSerializerOptions?)null) 
                : new List<string>())
            .Map(dest => dest.Metadata, src => !string.IsNullOrEmpty(src.Metadata) 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Metadata, (JsonSerializerOptions?)null) 
                : new Dictionary<string, object>());

        TypeAdapterConfig<WorkflowPlan, WorkflowPlanEntity>.NewConfig()
            .Map(dest => dest.ExecutorKeys, src => JsonSerializer.Serialize(src.ExecutorKeys, (JsonSerializerOptions?)null))
            .Map(dest => dest.Metadata, src => JsonSerializer.Serialize(src.Metadata, (JsonSerializerOptions?)null));
    }

    public static WorkflowPlan ToModel(this WorkflowPlanEntity entity)
    {
        if (entity == null) return null!;
        return entity.Adapt<WorkflowPlan>();
    }

    public static WorkflowPlanEntity ToEntity(this WorkflowPlan model)
    {
        if (model == null) return null!;
        return model.Adapt<WorkflowPlanEntity>();
    }

    public static WorkflowStep ToModel(this WorkflowStepEntity entity)
    {
        if (entity == null) return null!;
        return entity.Adapt<WorkflowStep>();
    }

    public static WorkflowStepEntity ToEntity(this WorkflowStep model)
    {
        if (model == null) return null!;
        return model.Adapt<WorkflowStepEntity>();
    }
}

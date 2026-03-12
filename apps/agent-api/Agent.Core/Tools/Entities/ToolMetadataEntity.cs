namespace Agent.Core.Tools.Entities;

public class ToolMetadataEntity
{
    public Guid Id { get; set; }
    public string PluginName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

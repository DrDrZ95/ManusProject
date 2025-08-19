
namespace Agent.Core.McpTools;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<ToolOutput> ExecuteAsync(ToolInput toolInput, CancellationToken cancellationToken = default);
    ToolDefinition GetDefinition();
}
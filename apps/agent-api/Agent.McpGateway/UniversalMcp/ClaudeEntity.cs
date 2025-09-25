namespace Agent.McpGateway.UniversalMcp
{
    /// <summary>
    /// Claude 实体
    /// Claude Entity
    /// </summary>
    public class ClaudeEntity : IMcpEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        // Add Claude-specific properties here
        public string ThreadId { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
    }
}


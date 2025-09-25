namespace Agent.McpGateway.UniversalMcp
{
    /// <summary>
    /// Chrome 实体
    /// Chrome Entity
    /// </summary>
    public class ChromeEntity : IMcpEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        // Add Chrome-specific properties here
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}


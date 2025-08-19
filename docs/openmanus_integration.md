# AI-Agent Integration Guide

This document provides a comprehensive guide for integrating the AI-Agent framework with the ai-agent solution, focusing on the Model Context Protocol (MCP) compatibility, agent orchestration, and extensibility points.

## Introduction to AI-Agent

AI-Agent is an open-source framework for building general AI agents, designed to provide similar functionality to proprietary agent systems. It offers a flexible architecture for creating, configuring, and deploying AI agents with various capabilities, including:

- Tool-based agent interactions
- Model Context Protocol (MCP) integration
- Browser automation
- Multi-agent workflows
- Extensible tool ecosystem

## Integration Architecture

The integration between our ai-agent solution and AI-Agent can be structured in several ways, depending on your specific requirements:

### 1. MCP Protocol Integration

The most direct integration path leverages the Model Context Protocol (MCP) compatibility in both systems:

```
┌─────────────┐     ┌───────────────┐     ┌────────────┐
│ ai-agent    │     │ MCP Protocol  │     │ AI-Agent  │
│ (.NET API)  ├────►│ Communication ├────►│ Agents     │
└─────────────┘     └───────────────┘     └────────────┘
```

Our ai-agent solution includes `DynamicExternalAccessTool.cs` and `QwenDialogueTool.cs` in the `Agent.Core/McpTools/` directory, which can be extended to communicate with AI-Agent agents through the MCP protocol.

### 2. Shared Tool Ecosystem

Both frameworks can share and extend the same tool ecosystem:

```
                 ┌───────────────────┐
                 │ Shared Tool       │
                 │ Ecosystem         │
                 └─┬─────────────────┘
                   │
       ┌───────────┴───────────┐
       │                       │
┌──────▼──────┐         ┌──────▼──────┐
│ ai-agent    │         │ AI-Agent   │
│ Framework   │         │ Framework   │
└─────────────┘         └─────────────┘
```

This approach allows for consistent tool behavior across both frameworks while maintaining independent agent implementations.

### 3. Hybrid Agent Deployment

For more complex scenarios, a hybrid deployment can be implemented:

```
┌─────────────────────────────────────────────┐
│                                             │
│  ┌─────────────┐           ┌────────────┐   │
│  │ ai-agent    │           │ AI-Agent  │   │
│  │ Components  │◄────────►│ Components │   │
│  └─────────────┘           └────────────┘   │
│                                             │
│           Integrated Application            │
└─────────────────────────────────────────────┘
```

## Implementation Guide

### Setting Up AI-Agent

1. **Installation**:
   ```bash
   git clone https://github.com/mannaandpoem/AI-Agent.git
   cd AI-Agent
   pip install -r requirements.txt
   ```

2. **Configuration**:
   Create a `config.toml` file in the `config` directory:
   ```toml
   # Global LLM configuration
   [llm]
   model = "gpt-4o"
   base_url = "https://api.openai.com/v1"
   api_key = "sk-..."  # Your API key
   max_tokens = 4096
   temperature = 0.0
   ```

### MCP Integration

#### 1. Running AI-Agent as an MCP Server

AI-Agent can be run as an MCP server that our ai-agent solution can connect to:

```bash
python run_mcp_server.py
```

This starts an MCP server on the default port (8000) that exposes AI-Agent tools through the MCP protocol.

#### 2. Connecting from ai-agent

To connect our ai-agent solution to the AI-Agent MCP server:

1. **Update the MCP Client Configuration**:

   Modify the `QwenServiceClient.cs` to connect to the AI-Agent MCP server:

   ```csharp
   // Example configuration for connecting to AI-Agent MCP server
   var mpcConfig = new McpConfiguration
   {
       ServerUrl = "http://localhost:8000/sse",
       ConnectionType = "sse"
   };
   ```

2. **Extend the DynamicExternalAccessTool**:

   The existing `DynamicExternalAccessTool.cs` can be extended to handle AI-Agent-specific tools:

   ```csharp
   // Example extension for AI-Agent tools
   public async Task<ToolResponse> HandleAI-AgentToolAsync(string toolName, JObject parameters)
   {
       // Implementation for handling AI-Agent-specific tools
       // ...
   }
   ```

### Tool Ecosystem Integration

To create a shared tool ecosystem:

1. **Define Common Tool Interfaces**:

   Create shared interfaces that both frameworks can implement:

   ```csharp
   // In ai-agent
   public interface ISharedTool
   {
       string Name { get; }
       string Description { get; }
       Task<object> ExecuteAsync(JObject parameters);
   }
   ```

2. **Implement Tools in Both Frameworks**:

   Implement the same tools in both frameworks following the shared interfaces.

3. **Tool Registration**:

   Register the tools in both frameworks:

   ```csharp
   // In ai-agent
   toolRegistry.RegisterTool(new SharedSearchTool());
   ```

   ```python
   # In AI-Agent
   tools.register(SharedSearchTool())
   ```

## Advanced Integration Scenarios

### 1. Bidirectional Agent Communication

For complex workflows requiring bidirectional communication between agents:

```
┌─────────────┐     ┌───────────────┐     ┌────────────┐
│ ai-agent    │◄───►│ Message Queue │◄───►│ AI-Agent  │
│ Agents      │     │ (e.g., Redis) │     │ Agents     │
└─────────────┘     └───────────────┘     └────────────┘
```

Implementation steps:
1. Set up a message queue (Redis, RabbitMQ, etc.)
2. Implement message producers and consumers in both frameworks
3. Define a common message format for agent communication

### 2. Shared State Management

For maintaining consistent state across frameworks:

```
┌─────────────┐     ┌───────────────┐     ┌────────────┐
│ ai-agent    │◄───►│ Shared State  │◄───►│ AI-Agent  │
│ Framework   │     │ Database      │     │ Framework  │
└─────────────┘     └───────────────┘     └────────────┘
```

Implementation steps:
1. Set up a shared database (PostgreSQL, MongoDB, etc.)
2. Implement data access layers in both frameworks
3. Define common data models and state transition rules

## Best Practices

1. **Consistent Configuration Management**:
   - Use environment variables for shared configuration
   - Implement configuration validation in both frameworks

2. **Error Handling and Logging**:
   - Implement consistent error handling patterns
   - Use structured logging with correlation IDs across frameworks

3. **Testing Integration Points**:
   - Create integration tests that verify cross-framework communication
   - Implement contract tests for API boundaries

4. **Deployment Considerations**:
   - Use Docker Compose for local development
   - Consider Kubernetes for production deployments
   - Implement health checks for all components

## Example: Integrating AI-Agent Browser Automation

AI-Agent includes browser automation capabilities that can be integrated with our ai-agent solution:

```csharp
// Example: Invoking AI-Agent browser automation from ai-agent
public async Task<string> PerformBrowserAutomation(string url, string action)
{
    var parameters = new JObject
    {
        ["url"] = url,
        ["action"] = action
    };
    
    var result = await mcpClient.InvokeTool("browser_navigate", parameters);
    return result.ToString();
}
```

## Conclusion

Integrating AI-Agent with our ai-agent solution provides several benefits:

1. **Extended Tool Ecosystem**: Access to AI-Agent's growing tool collection
2. **Flexible Agent Architectures**: Combine different agent approaches for optimal solutions
3. **Open-Source Foundation**: Build on a transparent, community-driven framework
4. **MCP Compatibility**: Leverage the standardized Model Context Protocol for seamless integration

By following this integration guide, you can create powerful hybrid agent systems that leverage the strengths of both frameworks while maintaining flexibility and extensibility.

## References

- [AI-Agent GitHub Repository](https://github.com/mannaandpoem/AI-Agent)
- [Model Context Protocol Specification](https://github.com/microsoft/mcp)
- [ai-agent Dynamic External Access Documentation](dynamic_external_access.md)

namespace Agent.Core.McpTools;

// 此方案为手动添加，在于解决短暂的冲突

// ========= 基础原语（方便在 Tool 参数/结果中传递简单值） =========
public abstract record McpPrimitive;

public sealed record McpString(string Value) : McpPrimitive;
public sealed record McpNumber(double Value) : McpPrimitive;  // 如需整数，可用整型转成 double 保存
public sealed record McpBoolean(bool Value)  : McpPrimitive;

// ========= Tool 输入 / 输出 =========
/// <summary>工具输入：用字典承载参数（与现有代码匹配）。</summary>
public sealed class ToolInput
{
    public Dictionary<string, McpPrimitive>? Parameters { get; init; }
}

/// <summary>工具输出：是否成功 + 可选错误 + 结果字典（与现有代码匹配）。</summary>
public sealed class ToolOutput
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, McpPrimitive>? Results { get; init; }
}

// ========= 便于工具自描述的简单 Schema（与 GetDefinition() 用法匹配） =========
public enum McpSchemaType
{
    Object, String, Number, Integer, Boolean, Array, Null
}

public sealed class McpSchema
{
    public McpSchemaType Type { get; init; }
    public string? Description { get; init; }
    public Dictionary<string, McpSchema>? Properties { get; init; }
    public List<string>? Required { get; init; }

    public McpSchema() { }

    public McpSchema(
        McpSchemaType type,
        string? description = null,
        Dictionary<string, McpSchema>? properties = null,
        List<string>? required = null)
    {
        Type = type;
        Description = description;
        Properties = properties;
        Required = required;
    }
}

/// <summary>工具定义：名称/描述/输入输出结构（供 UI/校验用）。</summary>
public sealed class ToolDefinition
{
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public McpSchema? InputSchema { get; init; }
    public McpSchema? OutputSchema { get; init; }
}

// ========= 兼容占位：CallToolResponse =========
// 某些旧代码/示例可能引用了该类型；这里给出简化版以消除 CS0246。
// 你可在后续替换为官方 MCP 的 CallToolResult（若迁移到 MCP 正式模型）。
public sealed class CallToolResponse
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    // 与 ToolOutput 保持一致，便于互换/过渡
    public Dictionary<string, McpPrimitive>? Results { get; init; }
}

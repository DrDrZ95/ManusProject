namespace Agent.Core.Services.Prompts;

/// <summary>
/// Professional prompt management service interface
/// 专业提示词管理服务接口
/// 
/// 提供AI-Agent系统中各种专业工具的提示词模板和管理功能
/// Provides prompt templates and management for various professional tools in AI-Agent system
/// </summary>
public interface IPromptsService
{
    /// <summary>
    /// Get prompt template by category and name
    /// 根据类别和名称获取提示词模板
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <param name="name">Prompt name - 提示词名称</param>
    /// <returns>Prompt template - 提示词模板</returns>
    Task<PromptTemplate?> GetPromptAsync(string category, string name);

    /// <summary>
    /// Get all prompts in a category
    /// 获取某个类别下的所有提示词
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <returns>List of prompt templates - 提示词模板列表</returns>
    Task<List<PromptTemplate>> GetPromptsByCategoryAsync(string category);

    /// <summary>
    /// Get all available categories
    /// 获取所有可用的类别
    /// </summary>
    /// <returns>List of categories - 类别列表</returns>
    Task<List<string>> GetCategoriesAsync();

    /// <summary>
    /// Render prompt with variables
    /// 使用变量渲染提示词
    /// </summary>
    /// <param name="template">Prompt template - 提示词模板</param>
    /// <param name="variables">Template variables - 模板变量</param>
    /// <returns>Rendered prompt - 渲染后的提示词</returns>
    string RenderPrompt(PromptTemplate template, Dictionary<string, object> variables);

    /// <summary>
    /// Add or update prompt template
    /// 添加或更新提示词模板
    /// </summary>
    /// <param name="template">Prompt template - 提示词模板</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> SavePromptAsync(PromptTemplate template);

    /// <summary>
    /// Delete prompt template
    /// 删除提示词模板
    /// </summary>
    /// <param name="category">Prompt category - 提示词类别</param>
    /// <param name="name">Prompt name - 提示词名称</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> DeletePromptAsync(string category, string name);

    /// <summary>
    /// Search prompts by keywords
    /// 根据关键词搜索提示词
    /// </summary>
    /// <param name="keywords">Search keywords - 搜索关键词</param>
    /// <returns>List of matching prompts - 匹配的提示词列表</returns>
    Task<List<PromptTemplate>> SearchPromptsAsync(string keywords);

    /// <summary>
    /// Get professional tool examples
    /// 获取专业工具示例
    /// </summary>
    /// <param name="toolType">Tool type - 工具类型</param>
    /// <returns>List of tool examples - 工具示例列表</returns>
    Task<List<ToolExample>> GetToolExamplesAsync(string toolType);

    /// <summary>
    /// Get all available tool types
    /// 获取所有可用的工具类型
    /// </summary>
    /// <returns>List of tool types - 工具类型列表</returns>
    Task<List<string>> GetToolTypesAsync();
}

/// <summary>
/// Prompt template model
/// 提示词模板模型
/// </summary>
public class PromptTemplate
{
    /// <summary>
    /// Unique identifier - 唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Prompt category - 提示词类别
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Prompt name - 提示词名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Prompt title - 提示词标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Prompt description - 提示词描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Prompt template content - 提示词模板内容
    /// </summary>
    public string Template { get; set; } = string.Empty;

    /// <summary>
    /// Template variables - 模板变量
    /// </summary>
    public List<PromptVariable> Variables { get; set; } = new();

    /// <summary>
    /// Usage examples - 使用示例
    /// </summary>
    public List<string> Examples { get; set; } = new();

    /// <summary>
    /// Tags for categorization - 分类标签
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Creation timestamp - 创建时间戳
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp - 最后更新时间戳
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Template version - 模板版本
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Author information - 作者信息
    /// </summary>
    public string Author { get; set; } = "AI-Agent";
}

/// <summary>
/// Prompt template variable
/// 提示词模板变量
/// </summary>
public class PromptVariable
{
    /// <summary>
    /// Variable name - 变量名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Variable description - 变量描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Variable type - 变量类型
    /// </summary>
    public string Type { get; set; } = "string";

    /// <summary>
    /// Default value - 默认值
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Is required - 是否必需
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Validation pattern - 验证模式
    /// </summary>
    public string? ValidationPattern { get; set; }
}

/// <summary>
/// Professional tool example
/// 专业工具示例
/// </summary>
public class ToolExample
{
    /// <summary>
    /// Tool type - 工具类型
    /// </summary>
    public string ToolType { get; set; } = string.Empty;

    /// <summary>
    /// Tool name - 工具名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tool description - 工具描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Usage example - 使用示例
    /// </summary>
    public string Example { get; set; } = string.Empty;

    /// <summary>
    /// Input parameters - 输入参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Expected output - 预期输出
    /// </summary>
    public string ExpectedOutput { get; set; } = string.Empty;

    /// <summary>
    /// Use cases - 使用场景
    /// </summary>
    public List<string> UseCases { get; set; } = new();

    /// <summary>
    /// Best practices - 最佳实践
    /// </summary>
    public List<string> BestPractices { get; set; } = new();
}


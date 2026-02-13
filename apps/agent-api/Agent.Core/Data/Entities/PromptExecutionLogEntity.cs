namespace Agent.Core.Data.Entities;

/// <summary>
/// Prompt Execution Log Entity
/// 提示词执行日志实体
/// 
/// Records execution details for prompt templates
/// 记录提示词模板的执行详情
/// </summary>
[Table("prompt_execution_logs")]
public class PromptExecutionLogEntity
{
    /// <summary>
    /// Unique identifier
    /// 唯一标识符
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Associated Prompt Template ID
    /// 关联模板 ID
    /// </summary>
    [Required]
    [Column("prompt_template_id")]
    public Guid PromptTemplateId { get; set; }

    /// <summary>
    /// Input Variables
    /// 输入变量
    /// </summary>
    [Column("input_variables", TypeName = "jsonb")]
    public string InputVariables { get; set; } = "{}";

    /// <summary>
    /// Rendered Prompt
    /// 渲染后的 Prompt
    /// </summary>
    [Required]
    [Column("rendered_prompt")]
    public string RenderedPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Model Response
    /// 模型响应
    /// </summary>
    [Required]
    [Column("model_response")]
    public string ModelResponse { get; set; } = string.Empty;

    /// <summary>
    /// Tokens Used
    /// 使用的 token 数
    /// </summary>
    [Column("tokens_used")]
    public int TokensUsed { get; set; }

    /// <summary>
    /// Response Time (ms)
    /// 响应时间（毫秒）
    /// </summary>
    [Column("response_time")]
    public int ResponseTime { get; set; }

    /// <summary>
    /// User Feedback Rating (1-5)
    /// 用户反馈评分（1-5）
    /// </summary>
    [Column("user_feedback")]
    public int? UserFeedback { get; set; }

    /// <summary>
    /// Executed At
    /// 执行时间
    /// </summary>
    [Column("executed_at")]
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    // Navigation property could be added here if needed, but keeping it simple as requested
    // public virtual PromptTemplateEntity PromptTemplate { get; set; }
}


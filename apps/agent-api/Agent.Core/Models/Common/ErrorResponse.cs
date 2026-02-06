namespace Agent.Core.Models.Common;

/// <summary>
/// Standard error response
/// 标准错误响应
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code
    /// 错误代码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error description
    /// 详细错误描述
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Request ID for tracing
    /// 用于追踪的请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;
}

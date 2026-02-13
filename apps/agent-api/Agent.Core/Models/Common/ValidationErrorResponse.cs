namespace Agent.Core.Models.Common;

/// <summary>
/// Validation error response
/// 验证错误响应
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Validation errors grouped by field name
    /// 按字段名分组的验证错误
    /// </summary>
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

    public ValidationErrorResponse()
    {
        Code = "ValidationFailed";
        Message = "One or more validation errors occurred.";
    }
}


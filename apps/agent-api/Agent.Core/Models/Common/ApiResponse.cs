namespace Agent.Core.Models.Common;

/// <summary>
/// Unified API response wrapper
/// 统一API响应包装器
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// 指示请求是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Request ID for tracing
    /// 用于追踪的请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    public ApiResponse() { }

    public ApiResponse(T data, string message = "Success")
    {
        Success = true;
        Data = data;
        Message = message;
    }

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse<T> Fail(string message, string requestId = "")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            RequestId = requestId
        };
    }
}

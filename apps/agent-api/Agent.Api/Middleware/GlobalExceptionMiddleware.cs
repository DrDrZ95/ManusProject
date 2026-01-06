namespace Agent.Api.Middleware;

/// <summary>
/// Global middleware for handling exceptions, logging them, and returning a standardized error response.
/// 全局异常处理中间件，用于捕获异常、记录日志并返回标准化的错误响应。
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    // The user wants to use threads for streaming collection for future Elastic integration.
    // We will use a simple background thread to process a queue of exceptions.
    // This is a placeholder to show the idea.

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    { 
        // 1. Capture request body for context if needed (e.g., for logging/tracing)
        // 捕获请求体以获取上下文信息（例如，用于日志/跟踪）
        // This is a placeholder for the user's requirement to add context information to the collection pipeline.
        // This pattern is typically used in a separate logging/tracing middleware *before* the exception handler.
        // For a simple exception handler, we'll focus on the response stream.

        // 2. Ensure the response body is readable for logging/tracing in case of an exception
        // 确保响应体在发生异常时可读，以便进行日志/跟踪
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        finally
        {
            // 3. Restore the original stream and copy the response content back
            // 恢复原始流并复制响应内容
            responseBody.Seek(0, SeekOrigin.Begin); // Refer to stream.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        RecordExceptionSpan(exception);

        var (statusCode, errorCode, message) = CategorizeException(exception);

        _logger.LogError(exception, "An unhandled exception has occurred. Error Code: {ErrorCode}, Message: {Message}", errorCode, message);

        // Elastic APM Integration:
            // The Elastic.Apm SDK is now included. For full APM integration, the agent needs to be started
            // in Program.cs. For now, we will use the SDK to manually capture the exception.
            // Agent.Tracer.CaptureException(exception); // Manual capture for demonstration.

            // Future Elastic integration idea (User's requirement):
        // 1. Create a concurrent queue to hold exception data (e.g., request/response context, exception details).
        // 2. A background thread would dequeue items and send them to Elastic Search in batches.
        // 3. This decouples the request pipeline from the logging infrastructure, improving performance.
        // 4. The use of threads for streaming collection is a key part of this future integration. (This is where the APM agent's background thread would handle the data transmission.)
        //    Example: Task.Run(() => ElasticClient.IndexDocument(exceptionContext));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = new
            {
                code = errorCode,
                message = message,
                details = exception.Message
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private void RecordExceptionSpan(Exception exception)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.RecordException(exception);
            activity.SetStatus(ActivityStatusCode.Error, "An unhandled exception has occurred.");
        }
    }

    private (HttpStatusCode, string, string) CategorizeException(Exception exception)
    {
        return exception switch
        {
            ArgumentException _ => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", "Invalid argument provided."),
            UnauthorizedAccessException _ => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Unauthorized access."),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR", "An internal server error has occurred.")
        };
    }
}
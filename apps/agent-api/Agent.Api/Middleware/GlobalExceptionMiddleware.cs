
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Agent.Api.Middleware
{
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
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            RecordExceptionSpan(exception);

            var (statusCode, errorCode, message) = CategorizeException(exception);

            _logger.LogError(exception, "An unhandled exception has occurred. Error Code: {ErrorCode}, Message: {Message}", errorCode, message);

            // Future Elastic integration idea:
            // 1. Create a concurrent queue to hold exception data.
            // 2. A background thread would dequeue items and send them to Elastic Search in batches.
            // 3. This decouples the request pipeline from the logging infrastructure, improving performance.

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
}


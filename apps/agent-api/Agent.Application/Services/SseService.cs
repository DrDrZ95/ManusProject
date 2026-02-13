namespace Agent.Application.Services;

/// <summary>
/// SSE (Server-Sent Events) 服务 (SSE Service)
/// </summary>
public static class SseService
{
    /// <summary>
    /// 发送 SSE 流式响应 (Send SSE streaming response)
    /// </summary>
    /// <param name="context">HTTP 上下文 (HTTP context)</param>
    /// <param name="data">要发送的数据 (Data to send)</param>
    public static async Task SendSseResponse(HttpContext context, string data)
    {
        var response = context.Response;
        response.Headers.Append("Content-Type", "text/event-stream");
        response.Headers.Append("Cache-Control", "no-cache");
        response.Headers.Append("Connection", "keep-alive");

        await response.WriteAsync($"data: {data}\n\n");
        await response.Body.FlushAsync();
    }
}


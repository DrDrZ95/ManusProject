using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Agent.Core.McpTools
{
    /// <summary>
    /// Example MCP server tools implementation based on README examples.
    /// </summary>
    [McpServerToolType]
    public class McpServerExamples
    {
        private readonly ILogger<McpServerExamples> _logger;

        public McpServerExamples(ILogger<McpServerExamples> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Echoes the message back to the client.
        /// </summary>
        /// <param name="message">The message to echo.</param>
        /// <returns>The echoed message.</returns>
        [McpServerTool, Description("Echoes the message back to the client.")]
        public string Echo(string message)
        {
            _logger.LogInformation("Echo tool called with message: {Message}", message);
            return $"Echo: {message}";
        }

        /// <summary>
        /// Summarizes content downloaded from a specific URI.
        /// </summary>
        /// <param name="thisServer">The MCP server instance.</param>
        /// <param name="httpClient">HTTP client for downloading content.</param>
        /// <param name="url">The URL from which to download the content to summarize.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A summary of the downloaded content.</returns>
        [McpServerTool(Name = "SummarizeContentFromUrl"), Description("Summarizes content downloaded from a specific URI")]
        public async Task<string> SummarizeDownloadedContent(
            IMcpServer thisServer,
            HttpClient httpClient,
            [Description("The url from which to download the content to summarize")] string url,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Summarizing content from URL: {Url}", url);
            
            string content = await httpClient.GetStringAsync(url, cancellationToken);

            ChatMessage[] messages =
            [
                new(ChatRole.User, "Briefly summarize the following downloaded content:"),
                new(ChatRole.User, content),
            ];
            
            ChatOptions options = new()
            {
                MaxOutputTokens = 256,
                Temperature = 0.3f,
            };

            var summary = await thisServer.AsSamplingChatClient().GetResponseAsync((IEnumerable<Microsoft.Extensions.AI.ChatMessage>)messages, options, cancellationToken);
            _logger.LogInformation("Content summarized successfully");
            
            return $"Summary: {summary}";
        }
    }

    /// <summary>
    /// Example MCP server prompts implementation based on README examples.
    /// </summary>
    [McpServerPromptType]
    public static class McpServerPrompts
    {
        /// <summary>
        /// Creates a prompt to summarize the provided content.
        /// </summary>
        /// <param name="content">The content to summarize.</param>
        /// <returns>A chat message with the summarization prompt.</returns>
        [McpServerPrompt, Description("Creates a prompt to summarize the provided message.")]
        public static ChatMessage Summarize([Description("The content to summarize")] string content) =>
            new(ChatRole.User, $"Please summarize this content into a single sentence: {content}");
    }
}

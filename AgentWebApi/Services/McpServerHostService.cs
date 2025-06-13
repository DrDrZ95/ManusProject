using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Threading.Tasks;

namespace AgentWebApi.Services
{
    /// <summary>
    /// Service for hosting MCP servers.
    /// </summary>
    public class McpServerHostService : IMcpServerHostService
    {
        private readonly ILogger<McpServerHostService> _logger;

        public McpServerHostService(ILogger<McpServerHostService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates and configures an MCP server host.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A configured host builder.</returns>
        public IHostBuilder CreateMcpServerHost(string[] args)
        {
            _logger.LogInformation("Creating MCP server host");
            
            var builder = Host.CreateApplicationBuilder(args);
            
            builder.Logging.AddConsole(consoleLogOptions =>
            {
                // Configure all logs to go to stderr
                consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
            });
            
            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();
            
            _logger.LogInformation("MCP server host created successfully");
            
            return builder;
        }

        /// <summary>
        /// Runs an MCP server host.
        /// </summary>
        /// <param name="hostBuilder">The host builder to run.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RunMcpServerHostAsync(IHostBuilder hostBuilder)
        {
            _logger.LogInformation("Running MCP server host");
            
            var host = hostBuilder.Build();
            await host.RunAsync();
            
            _logger.LogInformation("MCP server host stopped");
        }
    }

    /// <summary>
    /// Interface for MCP server host service.
    /// </summary>
    public interface IMcpServerHostService
    {
        IHostBuilder CreateMcpServerHost(string[] args);
        Task RunMcpServerHostAsync(IHostBuilder hostBuilder);
    }
}

using Agent.McpGateway.Clients;

namespace Agent.McpGateway.Clients
{
    public interface IMcpClientFactory
    {
        IMcpClient CreateClient(string clientType);
    }
}


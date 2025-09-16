using System.Threading.Tasks;

namespace Agent.McpGateway
{
    public interface IMcpClientFactory
    {
        IMcpClient CreateClient(string clientType);
    }
}


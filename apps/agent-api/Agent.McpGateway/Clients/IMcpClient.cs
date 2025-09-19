using System.Threading.Tasks;

namespace Agent.McpGateway.Clients
{
    public interface IMcpClient
    {
        Task<string> ExecuteJsonRpc(string method, string parametersJson);
    }
}


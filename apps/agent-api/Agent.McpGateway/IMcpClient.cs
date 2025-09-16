using System.Threading.Tasks;

namespace Agent.McpGateway
{
    public interface IMcpClient
    {
        Task<string> ExecuteJsonRpc(string method, string parametersJson);
    }
}


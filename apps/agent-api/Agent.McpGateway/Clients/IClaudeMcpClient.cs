using System.Threading.Tasks;
using Agent.McpGateway.Clients;

namespace Agent.McpGateway.Clients
{
    public interface IClaudeMcpClient : IMcpClient
    {
        Task<string> StartThread(string initialMessage);
        Task<string> ContinueThread(string threadId, string newMessage);
        Task<string> GetThreadMessages(string threadId);
        new Task<string> ExecuteJsonRpc(string method, string parametersJson);
    }
}


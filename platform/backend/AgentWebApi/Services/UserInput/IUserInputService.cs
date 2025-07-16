namespace AgentWebApi.Services.UserInput
{
    public interface IUserInputService
    {
        Task<string> ProcessUserInputAsync(string input);
    }
}


namespace Agent.Application.Services.UserInput;

/// <summary>
/// User input processing service interface
/// 用户输入处理服务接口
/// 
/// 负责处理用户输入并协调各个AI-Agent模块的执行流程
/// Responsible for processing user input and orchestrating the execution flow of various AI-Agent modules
/// </summary>
public interface IUserInputService
{
    /// <summary>
    /// Process user input and orchestrate agent execution flow
    /// 处理用户输入并协调智能体执行流程
    /// </summary>
    /// <param name="input">User input text - 用户输入文本</param>
    /// <returns>Processed response - 处理后的响应</returns>
    Task<string> ProcessUserInputAsync(string input);
}

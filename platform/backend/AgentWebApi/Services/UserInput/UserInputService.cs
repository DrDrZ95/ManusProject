using AgentWebApi.Services.SemanticKernel;
using AgentWebApi.Services.RAG;
using AgentWebApi.Services.Workflow;
using AgentWebApi.Services.Sandbox;
using AgentWebApi.Services.Prompts;

namespace AgentWebApi.Services.UserInput
{
    /// <summary>
    /// User input processing service implementation
    /// 用户输入处理服务实现
    /// 
    /// 作为AI-Agent系统的核心协调器，负责处理用户输入并按序调用各个模块
    /// Acts as the core orchestrator of the AI-Agent system, responsible for processing user input and calling various modules in sequence
    /// </summary>
    public class UserInputService : IUserInputService
    {
        private readonly ISemanticKernelService _semanticKernelService;  // 语义内核服务 - Semantic Kernel service
        private readonly IRagService _ragService;                        // RAG检索增强生成服务 - RAG retrieval augmented generation service
        private readonly IWorkflowService _workflowService;              // 工作流服务 - Workflow service
        private readonly ISandboxTerminalService _sandboxTerminalService; // 沙箱终端服务 - Sandbox terminal service
        private readonly IPromptsService _promptsService;                // 提示词服务 - Prompts service

        /// <summary>
        /// Constructor with dependency injection
        /// 构造函数，使用依赖注入
        /// </summary>
        public UserInputService(
            ISemanticKernelService semanticKernelService,
            IRagService ragService,
            IWorkflowService workflowService,
            ISandboxTerminalService sandboxTerminalService,
            IPromptsService promptsService)
        {
            _semanticKernelService = semanticKernelService;
            _ragService = ragService;
            _workflowService = workflowService;
            _sandboxTerminalService = sandboxTerminalService;
            _promptsService = promptsService;
        }

        /// <summary>
        /// Process user input and orchestrate agent execution flow
        /// 处理用户输入并协调智能体执行流程
        /// 
        /// 执行序列：用户输入 -> LLM交互 -> RAG(可选) -> 生成待办事项 -> 流程交互 -> 最终响应
        /// Execution sequence: User Input -> LLM Interaction -> RAG (optional) -> Generate To-do List -> Process Interaction -> Final Response
        /// </summary>
        /// <param name="input">User input text - 用户输入文本</param>
        /// <returns>Processed response - 处理后的响应</returns>
        public async Task<string> ProcessUserInputAsync(string input)
        {
            // 步骤1: 用户输入处理 (由此服务接收输入时隐式处理)
            // Step 1: User Input Processing (Implicitly handled by this service receiving the input)
            // 这是用户交互的入口点 - This is the entry point for user interaction.

            // 步骤2: LLM交互
            // Step 2: LLM Interaction
            // 调用语义内核与LLM交互，基于用户输入进行初始理解或任务规划
            // Call Semantic Kernel to interact with the LLM based on user input for initial understanding or task planning.
            // 模拟调用LLM进行初始理解或任务规划 - Simulate a call to the LLM for initial understanding or task planning.
            var llmResponse = await _semanticKernelService.ExecutePromptAsync("Initial LLM interaction for: " + input);

            // 步骤3: RAG (检索增强生成) - 可选
            // Step 3: RAG (Retrieval Augmented Generation) - Optional
            // 如果LLM交互建议需要外部知识，则调用RAG服务
            // If the LLM interaction suggests a need for external knowledge, call the RAG service.
            // 模拟可能需要RAG的条件 - Simulate a condition where RAG might be needed.
            if (llmResponse.Contains("knowledge base", StringComparison.OrdinalIgnoreCase))
            {
                // 从知识库检索相关信息并生成增强响应 - Retrieve relevant information from knowledge base and generate enhanced response
                var ragResult = await _ragService.RetrieveAndGenerateAsync(input);
                llmResponse += "\n(RAG Result: " + ragResult + ")";
            }

            // 步骤4: 生成待办事项列表 (或计划)
            // Step 4: Generate To-Do List (or plan)
            // 基于LLM的响应，生成计划或待办事项列表
            // Based on the LLM's response, generate a plan or a to-do list.
            // 基于LLM的理解模拟生成简单的工作流 - Simulate generating a simple workflow based on the LLM's understanding.
            var todoList = await _workflowService.CreateWorkflowAsync("Plan for: " + llmResponse);

            // 步骤5: 流程交互 (例如：沙箱终端、工作流执行)
            // Step 5: Process Interaction (e.g., Sandbox Terminal, Workflow Execution)
            // 执行生成的待办事项列表或与沙箱交互
            // Execute the generated to-do list or interact with the sandbox.
            // 基于计划模拟在沙箱中执行命令 - Simulate executing a command in the sandbox based on the plan.
            var sandboxOutput = await _sandboxTerminalService.ExecuteCommandAsync("echo \"Executing: " + todoList + "\"");

            // 步骤6: 最终LLM交互 / 响应生成
            // Step 6: Final LLM Interaction / Response Generation
            // 再次使用LLM制定最终响应给用户，整合所有步骤的结果
            // Use LLM again to formulate a final response to the user, incorporating all steps.
            var finalResponse = await _semanticKernelService.ExecutePromptAsync(
                "Summarize the process and provide a user-friendly response based on: " +
                llmResponse + "\n" + todoList + "\n" + sandboxOutput);

            // 返回最终处理结果 - Return the final processed result
            return finalResponse;
        }
    }
}


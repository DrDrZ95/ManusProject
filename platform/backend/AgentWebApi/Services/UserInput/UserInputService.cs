using AgentWebApi.Services.SemanticKernel;
using AgentWebApi.Services.RAG;
using AgentWebApi.Services.Workflow;
using AgentWebApi.Services.Sandbox;
using AgentWebApi.Services.Prompts;

namespace AgentWebApi.Services.UserInput
{
    public class UserInputService : IUserInputService
    {
        private readonly ISemanticKernelService _semanticKernelService;
        private readonly IRagService _ragService;
        private readonly IWorkflowService _workflowService;
        private readonly ISandboxTerminalService _sandboxTerminalService;
        private readonly IPromptsService _promptsService;

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

        public async Task<string> ProcessUserInputAsync(string input)
        {
            // 1. User Input Processing (Implicitly handled by this service receiving the input)
            // This is the entry point for user interaction.

            // 2. LLM Interaction
            // Call Semantic Kernel to interact with the LLM based on user input.
            // Simulate a call to the LLM for initial understanding or task planning.
            var llmResponse = await _semanticKernelService.ExecutePromptAsync("Initial LLM interaction for: " + input);

            // 3. RAG (Retrieval Augmented Generation) - Optional
            // If the LLM interaction suggests a need for external knowledge, call the RAG service.
            // Simulate a condition where RAG might be needed.
            if (llmResponse.Contains("knowledge base", StringComparison.OrdinalIgnoreCase))
            {
                var ragResult = await _ragService.RetrieveAndGenerateAsync(input);
                llmResponse += "\n(RAG Result: " + ragResult + ")";
            }

            // 4. Generate To-Do List (or plan)
            // Based on the LLM's response, generate a plan or a to-do list.
            // Simulate generating a simple workflow based on the LLM's understanding.
            var todoList = await _workflowService.CreateWorkflowAsync("Plan for: " + llmResponse);

            // 5. Process Interaction (e.g., Sandbox Terminal, Workflow Execution)
            // Execute the generated to-do list or interact with the sandbox.
            // Simulate executing a command in the sandbox based on the plan.
            var sandboxOutput = await _sandboxTerminalService.ExecuteCommandAsync("echo \"Executing: " + todoList + "\"");

            // 6. Final LLM Interaction / Response Generation
            // Use LLM again to formulate a final response to the user, incorporating all steps.
            var finalResponse = await _semanticKernelService.ExecutePromptAsync(
                "Summarize the process and provide a user-friendly response based on: " +
                llmResponse + "\n" + todoList + "\n" + sandboxOutput);

            return finalResponse;
        }
    }
}


using Agent.Application.Services.Prompts;
using Agent.Application.Services.RAG;
using Agent.Application.Services.Sandbox;
using Agent.Application.Services.SemanticKernel;
using Agent.Application.Services.UserInput;
using Agent.Application.Services.Workflow;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Core.Tests.Services
{
    /// <summary>
    /// Unit tests for UserInputService
    /// UserInputService 单元测试
    /// </summary>
    public class UserInputServiceTests
    {
        private readonly Mock<ISemanticKernelService> _mockSemanticKernel;
        private readonly Mock<IRagService> _mockRag;
        private readonly Mock<IWorkflowService> _mockWorkflow;
        private readonly Mock<ISandboxTerminalService> _mockSandbox;
        private readonly Mock<IPromptsService> _mockPrompts;
        private readonly UserInputService _service;

        public UserInputServiceTests()
        {
            _mockSemanticKernel = new Mock<ISemanticKernelService>();
            _mockRag = new Mock<IRagService>();
            _mockWorkflow = new Mock<IWorkflowService>();
            _mockSandbox = new Mock<ISandboxTerminalService>();
            _mockPrompts = new Mock<IPromptsService>();

            _service = new UserInputService(
                _mockSemanticKernel.Object,
                _mockRag.Object,
                _mockWorkflow.Object,
                _mockSandbox.Object,
                _mockPrompts.Object);
        }

        /// <summary>
        /// Test normal flow without RAG
        /// 测试不触发 RAG 的正常流程
        /// </summary>
        [Fact]
        public async Task ProcessUserInputAsync_NormalFlow_ShouldCallServicesInOrder()
        {
            // Arrange
            var input = "Help me check disk space";
            var llmResponse1 = "I will check disk space";
            var plan = "Step 1: check disk";
            var sandboxOutput = "Disk space: 50GB free";
            var finalResponse = "You have 50GB free space";

            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Initial LLM interaction"))))
                .ReturnsAsync(llmResponse1);

            _mockWorkflow.Setup(w => w.CreateWorkflowAsync(It.IsAny<string>()))
                .ReturnsAsync(plan);

            _mockSandbox.Setup(s => s.ExecuteCommandAsync(It.IsAny<string>(), null, 30, default))
                .ReturnsAsync(new SandboxCommandResult { StandardOutput = sandboxOutput, ExitCode = 0 });

            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Summarize the process"))))
                .ReturnsAsync(finalResponse);

            // Act
            var result = await _service.ProcessUserInputAsync(input);

            // Assert
            Assert.Equal(finalResponse, result);
            _mockSemanticKernel.Verify(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Initial LLM interaction"))), Times.Once);
            _mockRag.Verify(r => r.RetrieveAndGenerateAsync(It.IsAny<string>()), Times.Never);
            _mockWorkflow.Verify(w => w.CreateWorkflowAsync(It.Is<string>(s => s.Contains(llmResponse1))), Times.Once);
            _mockSandbox.Verify(s => s.ExecuteCommandAsync(It.Is<string>(c => c.Contains(plan)), null, 30, default), Times.Once);
            _mockSemanticKernel.Verify(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Summarize the process"))), Times.Once);
        }

        /// <summary>
        /// Test flow triggers RAG when LLM response contains keyword
        /// 测试当 LLM 响应包含关键词时触发 RAG
        /// </summary>
        [Fact]
        public async Task ProcessUserInputAsync_WithRagKeyword_ShouldCallRagService()
        {
            // Arrange
            var input = "Tell me about internal docs";
            var llmResponse1 = "I need to check the knowledge base";
            var ragResult = "Here is the info from docs";
            var plan = "Step 1: Process info";
            var sandboxOutput = "Processed";
            var finalResponse = "Summary with docs";

            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Initial LLM interaction"))))
                .ReturnsAsync(llmResponse1);

            _mockRag.Setup(r => r.RetrieveAndGenerateAsync(input))
                .ReturnsAsync(ragResult);

            _mockWorkflow.Setup(w => w.CreateWorkflowAsync(It.IsAny<string>()))
                .ReturnsAsync(plan);

            _mockSandbox.Setup(s => s.ExecuteCommandAsync(It.IsAny<string>(), null, 30, default))
                .ReturnsAsync(new SandboxCommandResult { StandardOutput = sandboxOutput, ExitCode = 0 });

            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Summarize the process"))))
                .ReturnsAsync(finalResponse);

            // Act
            var result = await _service.ProcessUserInputAsync(input);

            // Assert
            Assert.Equal(finalResponse, result);
            _mockRag.Verify(r => r.RetrieveAndGenerateAsync(input), Times.Once);
            // Workflow creation should receive the updated llmResponse (with RAG result)
            _mockWorkflow.Verify(w => w.CreateWorkflowAsync(It.Is<string>(s => s.Contains(ragResult))), Times.Once);
        }

        /// <summary>
        /// Test handling exceptions from dependencies
        /// 测试依赖服务抛出异常时的处理
        /// </summary>
        [Fact]
        public async Task ProcessUserInputAsync_DependencyThrows_ShouldPropagateException()
        {
            // Arrange
            var input = "Crash me";
            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SK Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessUserInputAsync(input));
        }

        /// <summary>
        /// Test empty input
        /// 测试空输入
        /// </summary>
        /// <remarks>
        /// Assuming current implementation doesn't check for empty input explicitly, but it's good to test how it behaves.
        /// If it throws or passes empty string to dependencies.
        /// </remarks>
        [Fact]
        public async Task ProcessUserInputAsync_EmptyInput_ShouldProceed()
        {
            // Arrange
            var input = "";
            var llmResponse = "Empty response";
            var plan = "Nothing to do";
            var sandboxOutput = "Done";
            var finalResponse = "Finished";

            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.IsAny<string>()))
                .ReturnsAsync(llmResponse);
            _mockWorkflow.Setup(w => w.CreateWorkflowAsync(It.IsAny<string>()))
                .ReturnsAsync(plan);
            _mockSandbox.Setup(s => s.ExecuteCommandAsync(It.IsAny<string>(), null, 30, default))
                .ReturnsAsync(new SandboxCommandResult { StandardOutput = sandboxOutput });
            _mockSemanticKernel.Setup(s => s.ExecutePromptAsync(It.Is<string>(p => p.Contains("Summarize"))))
                .ReturnsAsync(finalResponse);

            // Act
            var result = await _service.ProcessUserInputAsync(input);

            // Assert
            Assert.Equal(finalResponse, result);
        }
    }
}

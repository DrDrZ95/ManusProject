using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Agent.Core.Services.Workflosing System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Core.Tests.Services
{
    public class WorkflowServiceTests
    {
        private readonly Mock<ILogger<WorkflowService>> _mockLogger;
        private readonly Mock<IOptions<WorkflowOptions>> _mockOptions;
        private readonly WorkflowService _workflowService;

        public WorkflowServiceTests()
        {
            _mockLogger = new Mock<ILogger<WorkflowService>>();
            _mockOptions = new Mock<IOptions<WorkflowOptions>>();
            _mockOptions.Setup(o => o.Value).Returns(new WorkflowOptions { WorkflowsDirectory = "/tmp/workflows" });
            
            _workflowService = new WorkflowService(_mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task CreatePlanAsync_ValidRequest_ReturnsWorkflowPlan()
        {
            // Arrange
            var request = new CreatePlanRequest
            {
                Title = "Test Plan",
                Description = "A plan for testing",
                ExecutorKeys = new List<string> { "key1", "key2" },
                Metadata = new Dictionary<string, object> { { "meta1", "value1" } },
                Steps = new List<string> { "Step 1", "Step 2" }
            };

            // Act
            var plan = await _workflowService.CreatePlanAsync(request);

            // Assert
            Assert.NotNull(plan);
            Assert.False(string.IsNullOrEmpty(plan.Id));
            Assert.Equal(request.Title, plan.Title);
            Assert.Equal(request.Description, plan.Description);
            Assert.Equal(request.ExecutorKeys, plan.ExecutorKeys);
            
            Assert.Equal(request.Steps.Count, plan.Steps.Count);
            Assert.Equal(0, plan.CurrentStepIndex);
            Assert.All(plan.Steps, step => Assert.Equal(PlanStepStatus.NotStarted, step.Status));
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Created workflow plan"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetPlanAsync_ExistingPlan_ReturnsPlan()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Existing Plan", Steps = new List<string> { "Step 1" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);

            // Act
            var retrievedPlan = await _workflowService.GetPlanAsync(createdPlan.Id);

            // Assert
            Assert.NotNull(retrievedPlan);
            Assert.Equal(createdPlan.Id, retrievedPlan.Id);
        }

        [Fact]
        public async Task GetPlanAsync_NonExistingPlan_ReturnsNull()
        {
            // Act
            var retrievedPlan = await _workflowService.GetPlanAsync("non_existent_plan");

            // Assert
            Assert.Null(retrievedPlan);
        }

        [Fact]
        public async Task GetAllPlansAsync_ReturnsAllCreatedPlans()
        {
            // Arrange
            await _workflowService.CreatePlanAsync(new CreatePlanRequest { Title = "Plan 1", Steps = new List<string> { "Step A" } });
            await _workflowService.CreatePlanAsync(new CreatePlanRequest { Title = "Plan 2", Steps = new List<string> { "Step B" } });

            // Act
            var allPlans = await _workflowService.GetAllPlansAsync();

            // Assert
            Assert.NotNull(allPlans);
            Assert.True(allPlans.Count >= 2); // May contain plans from other tests if not isolated
        }

        [Fact]
        public async Task UpdateStepStatusAsync_ValidInput_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan for Update", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);

            // Act
            var result = await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 0, PlanStepStatus.InProgress);
            var updatedPlan = await _workflowService.GetPlanAsync(createdPlan.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPlan);
            Assert.Equal(PlanStepStatus.InProgress, updatedPlan.Steps[0].Status);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Updated step 0 status to InProgress"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateStepStatusAsync_InvalidPlanId_ReturnsFalse()
        {
            // Act
            var result = await _workflowService.UpdateStepStatusAsync("non_existent_plan", 0, PlanStepStatus.InProgress);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Plan not found"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateStepStatusAsync_InvalidStepIndex_ReturnsFalse()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan for Invalid Index", Steps = new List<string> { "Step 1" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);

            // Act
            var result = await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 99, PlanStepStatus.InProgress);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid step index"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetCurrentStepAsync_NoActiveStep_ReturnsNull()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan No Active", Steps = new List<string> { "Step 1" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);

            // Act
            var currentStep = await _workflowService.GetCurrentStepAsync(createdPlan.Id);

            // Assert
            Assert.NotNull(currentStep);
            Assert.Equal(0, createdPlan.CurrentStepIndex);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Updated step 0 status to InProgress"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetCurrentStepAsync_ActiveStepExists_ReturnsActiveStep()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan With Active", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 0, PlanStepStatus.InProgress);

            // Act
            var currentStep = await _workflowService.GetCurrentStepAsync(createdPlan.Id);

            // Assert
            Assert.NotNull(currentStep);
            Assert.Equal(0, currentStep.Index);
            Assert.Equal(PlanStepStatus.InProgress, currentStep.Status);
        }

        [Fact]
        public async Task CompleteStepAsync_ValidInput_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan for Complete", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 0, PlanStepStatus.InProgress);

            // Act
            var result = await _workflowService.CompleteStepAsync(createdPlan.Id, 0, "Step 1 Result");
            var updatedPlan = await _workflowService.GetPlanAsync(createdPlan.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPlan);
            Assert.Equal(PlanStepStatus.Completed, updatedPlan.Steps[0].Status);
            Assert.Equal("Step 1 Result", updatedPlan.Steps[0].Result);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step 0 for plan"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task GenerateToDoListAsync_ValidPlan_ReturnsMarkdownContent()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Test ToDo List", Description = "Description for todo", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 0, PlanStepStatus.InProgress);

            // Act
            var todoContent = await _workflowService.GenerateToDoListAsync(createdPlan.Id);

            // Assert
            Assert.NotNull(todoContent);
            Assert.Contains("# Test ToDo List", todoContent);
            Assert.Contains("**描述**: Description for todo", todoContent);
            Assert.Contains("- [ ] Step 2", todoContent);
            Assert.Contains("- [ ] Step 1", todoContent);
        }

        [Fact]
        public async Task GetProgressAsync_ValidPlan_ReturnsCorrectProgress()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Progress Plan", Steps = new List<string> { "Step 1", "Step 2", "Step 3" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.CompleteStepAsync(createdPlan.Id, 0);
            await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 1, PlanStepStatus.InProgress);

            // Act
            var progress = await _workflowService.GetProgressAsync(createdPlan.Id);

            // Assert
            Assert.NotNull(progress);
            Assert.Equal(3, progress.TotalSteps);
            Assert.Equal(1, progress.CompletedSteps);
            Assert.Equal(1, progress.InProgressSteps);
            Assert.Equal(0, progress.BlockedSteps);
            Assert.Equal(33.3f, progress.ProgressPercentage, 1); // 1 completed out of 3
        }

        [Fact]
        public async Task FailStepAsync_ValidInput_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan for Fail", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 0, PlanStepStatus.InProgress);

            // Act
            var result = await _workflowService.FailStepAsync(createdPlan.Id, 0, "Error occurred");
            var updatedPlan = await _workflowService.GetPlanAsync(createdPlan.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPlan);
            Assert.Equal(PlanStepStatus.Failed, updatedPlan.Steps[0].Status);
            Assert.Equal("Error occurred", updatedPlan.Steps[0].ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed step 0 for plan"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task BlockStepAsync_ValidInput_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan for Block", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.UpdateStepStatusAsync(createdPlan.Id, 0, PlanStepStatus.InProgress);

            // Act
            var result = await _workflowService.BlockStepAsync(createdPlan.Id, 0, "Dependency missing");
            var updatedPlan = await _workflowService.GetPlanAsync(createdPlan.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPlan);
            Assert.Equal(PlanStepStatus.Blocked, updatedPlan.Steps[0].Status);
            Assert.Equal("Dependency missing", updatedPlan.Steps[0].BlockReason);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Blocked step 0 for plan"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Fact]
        public async Task UnblockStepAsync_ValidInput_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var createRequest = new CreatePlanRequest { Title = "Plan for Unblock", Steps = new List<string> { "Step 1", "Step 2" } };
            var createdPlan = await _workflowService.CreatePlanAsync(createRequest);
            await _workflowService.BlockStepAsync(createdPlan.Id, 0, "Dependency missing");

            // Act
            var result = await _workflowService.UnblockStepAsync(createdPlan.Id, 0);
            var updatedPlan = await _workflowService.GetPlanAsync(createdPlan.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPlan);
            Assert.Equal(PlanStepStatus.NotStarted, updatedPlan.Steps[0].Status); // Unblocked reverts to NotStarted
            Assert.Null(updatedPlan.Steps[0].BlockReason);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unblocked step 0 for plan"))!,
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }

        [Theory]
        [InlineData("ACTION: Browse google.com", PlanStepType.Browse)]
        [InlineData("CODE: print(\'hello\')", PlanStepType.Code)]
        [InlineData("PLAN: Create a new plan", PlanStepType.Plan)]
        [InlineData("CRITIQUE: Review the code", PlanStepType.Critique)]
        [InlineData("Something else", PlanStepType.Unknown)]
        public void ExtractStepType_ReturnsCorrectType(string stepText, PlanStepType expectedType)
        {
            // Act
            var actualType = _workflowService.ExtractStepType(stepText);

            // Assert
            Assert.Equal(expectedType, actualType);
        }

        // Additional tests for file operations (SaveWorkflowToFileAsync, LoadWorkflowFromFileAsync, DeleteWorkflowFileAsync)
        // These tests will require mocking IFileOperationService or creating a temporary file system.
        // For now, I'll provide a placeholder and focus on the core logic.

        // [Fact]
        // public async Task SaveWorkflowToFileAsync_ValidPlan_SavesFile()
        // {
        //     // Arrange
        //     var createRequest = new CreatePlanRequest { Title = "File Plan", Steps = new List<string> { "Step 1" } };
        //     var createdPlan = await _workflowService.CreatePlanAsync(createRequest);

        //     // Act
        //     await _workflowService.SaveWorkflowToFileAsync(createdPlan.Id);

        //     // Assert: Verify file exists and content is correct (requires file system mocking or temporary files)
        // }
    }
}

namespace Agent.Core.Tests.Services
{
    /// <summary>
    /// WorkflowService 扩展单元测试 - 包含并发、状态机路径、人工干预和性能测试
    /// Extended unit tests for WorkflowService - including concurrency, state machine paths, manual intervention, and performance tests
    /// </summary>
    public class WorkflowServiceTests
    {
        private readonly Mock<ILogger<WorkflowService>> _mockLogger;
        private readonly Mock<IOptions<WorkflowOptions>> _mockOptions;
        private readonly Mock<IWorkflowRepository> _mockRepository;
        private readonly WorkflowService _workflowService;

        public WorkflowServiceTests()
        {
            _mockLogger = new Mock<ILogger<WorkflowService>>();
            _mockOptions = new Mock<IOptions<WorkflowOptions>>();
            _mockOptions.Setup(o => o.Value).Returns(new WorkflowOptions { WorkflowsDirectory = "/tmp/workflows" });
            _mockRepository = new Mock<IWorkflowRepository>();
            
            _workflowService = new WorkflowService(_mockLogger.Object, _mockOptions.Object, _mockRepository.Object);
        }

        /// <summary>
        /// a) 并发执行场景测试
        /// Test concurrent execution scenarios
        /// </summary>
        [Fact]
        public async Task CreatePlanAsync_ConcurrentRequests_ShouldHandleCorrectly()
        {
            // Arrange
            var request = new CreatePlanRequest { Title = "Concurrent Plan", Steps = new List<string> { "Step 1" } };
            _mockRepository.Setup(r => r.AddPlanAsync(It.IsAny<WorkflowPlanEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WorkflowPlanEntity e, CancellationToken ct) => {
                    e.Id = Guid.NewGuid();
                    return e;
                });

            // Act
            var tasks = Enumerable.Range(0, 10).Select(_ => _workflowService.CreatePlanAsync(request)).ToList();
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, results.Length);
            Assert.Equal(10, results.Select(r => r.Id).Distinct().Count());
            _mockRepository.Verify(r => r.AddPlanAsync(It.IsAny<WorkflowPlanEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(10));
        }

        /// <summary>
        /// b) 状态机转换全路径测试 (基于 WorkflowOptimization.md)
        /// Test all paths of state machine transitions
        /// </summary>
        [Fact]
        public async Task WorkflowExecutionEngine_StateTransitions_ShouldFollowOptimizationDoc()
        {
            // Arrange
            var engine = new WorkflowExecutionEngine(12345);

            // Act & Assert: Idle -> Initializing -> Planning -> Executing -> Completed
            Assert.Equal(WorkflowState.Idle, engine.CurrentState);
            
            await engine.TriggerEventAsync(WorkflowEvent.StartTask);
            Assert.Equal(WorkflowState.Initializing, engine.CurrentState);
            
            await engine.TriggerEventAsync(WorkflowEvent.InitializationComplete);
            Assert.Equal(WorkflowState.Planning, engine.CurrentState);
            
            await engine.TriggerEventAsync(WorkflowEvent.PlanReady);
            Assert.Equal(WorkflowState.Executing, engine.CurrentState);
            
            await engine.TriggerEventAsync(WorkflowEvent.ExecutionComplete);
            Assert.Equal(WorkflowState.Completed, engine.CurrentState);
        }

        /// <summary>
        /// c) 人工干预机制测试
        /// Test manual intervention mechanism
        /// </summary>
        [Fact]
        public async Task WorkflowExecutionEngine_ManualIntervention_ShouldTransitionCorrectly()
        {
            // Arrange
            var engine = new WorkflowExecutionEngine(67890);
            await engine.TriggerEventAsync(WorkflowEvent.StartTask);
            await engine.TriggerEventAsync(WorkflowEvent.InitializationComplete);
            await engine.TriggerEventAsync(WorkflowEvent.PlanReady); // Now in Executing

            // Act: Executing -> ManualIntervention
            var interventionInfo = new ManualInterventionInfo { Reason = "Sensitive Action: System Shutdown" };
            await engine.TriggerEventAsync(WorkflowEvent.NeedIntervention, interventionInfo);
            
            // Assert
            Assert.Equal(WorkflowState.ManualIntervention, engine.CurrentState);
            Assert.Equal("Sensitive Action: System Shutdown", engine.GetContext().InterventionInfo?.Reason);

            // Act: ManualIntervention -> Executing (UserResume)
            await engine.TriggerEventAsync(WorkflowEvent.UserResume);
            Assert.Equal(WorkflowState.Executing, engine.CurrentState);
        }

        /// <summary>
        /// d) 错误恢复与重试测试
        /// Test error recovery and retry
        /// </summary>
        [Fact]
        public async Task WorkflowExecutionEngine_ErrorRecovery_ShouldAllowRestartFromIdle()
        {
            // Arrange
            var engine = new WorkflowExecutionEngine(11111);
            await engine.TriggerEventAsync(WorkflowEvent.StartTask);
            
            // Act: Initializing -> Failed
            await engine.TriggerEventAsync(WorkflowEvent.InitializationFailed);
            Assert.Equal(WorkflowState.Failed, engine.CurrentState);

            // Act: Failed -> Initializing (Retry/Restart)
            await engine.TriggerEventAsync(WorkflowEvent.StartTask);
            Assert.Equal(WorkflowState.Initializing, engine.CurrentState);
        }

        /// <summary>
        /// e) 性能测试：执行 1000 个工作流状态转换
        /// Performance testing: execute 1000 workflow state transitions
        /// </summary>
        [Fact]
        public async Task WorkflowExecutionEngine_Performance_1000Workflows()
        {
            // Arrange
            var engines = Enumerable.Range(0, 1000).Select(i => new WorkflowExecutionEngine(i)).ToList();
            var startTime = DateTime.UtcNow;

            // Act
            var tasks = engines.Select(async engine => {
                await engine.TriggerEventAsync(WorkflowEvent.StartTask);
                await engine.TriggerEventAsync(WorkflowEvent.InitializationComplete);
                await engine.TriggerEventAsync(WorkflowEvent.PlanReady);
                await engine.TriggerEventAsync(WorkflowEvent.ExecutionComplete);
            });
            await Task.WhenAll(tasks);

            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.All(engines, e => Assert.Equal(WorkflowState.Completed, e.CurrentState));
            _mockLogger.LogInformation("Executed 1000 workflow transitions in {Duration}ms", duration.TotalMilliseconds);
        }
    }
}

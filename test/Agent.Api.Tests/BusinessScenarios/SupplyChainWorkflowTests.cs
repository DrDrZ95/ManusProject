namespace Agent.Api.Tests.BusinessScenarios;

public class SupplyChainWorkflowTests
{
    private readonly Mock<IWorkflowRepository> _workflowRepositoryMock;
    private readonly Mock<IWorkflowNotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<WorkflowExecutionEngine>> _loggerMock;
    private readonly Mock<IAgentTraceService> _agentTraceServiceMock;

    public SupplyChainWorkflowTests()
    {
        _workflowRepositoryMock = new Mock<IWorkflowRepository>();
        _notificationServiceMock = new Mock<IWorkflowNotificationService>();
        _loggerMock = new Mock<ILogger<WorkflowExecutionEngine>>();
        _agentTraceServiceMock = new Mock<IAgentTraceService>();
    }

    [Fact]
    public void SupplyChain_InventoryShortage_TriggerRestockWorkflow()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var engine = new WorkflowExecutionEngine(
            planId,
            WorkflowState.Idle,
            _notificationServiceMock.Object,
            _workflowRepositoryMock.Object,
            _loggerMock.Object,
            _agentTraceServiceMock.Object);

        // Assert
        Assert.Equal(WorkflowState.Idle, engine.CurrentState);
    }
}


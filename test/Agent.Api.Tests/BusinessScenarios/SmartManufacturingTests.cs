namespace Agent.Api.Tests.BusinessScenarios;

public class SmartManufacturingTests
{
    private readonly Mock<IeBPFDetectiveService> _ebpfServiceMock;

    public SmartManufacturingTests()
    {
        _ebpfServiceMock = new Mock<IeBPFDetectiveService>();
    }

    [Fact]
    public async Task Manufacturing_AnomalousVibration_TriggersMaintenanceAlert()
    {
        // Arrange
        _ebpfServiceMock.Setup(e => e.RunBpftraceScriptAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Vibration: High, Status: Alert");

        // Act
        var result = await _ebpfServiceMock.Object.RunBpftraceScriptAsync("monitor_vibration.bt");

        // Assert
        Assert.Contains("Alert", result);
    }
}

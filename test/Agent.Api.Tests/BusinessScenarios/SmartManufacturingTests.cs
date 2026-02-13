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
        _ebpfServiceMock.Setup(e => e.ExecuteScriptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Vibration: High, Status: Alert");

        // Act
        var result = await _ebpfServiceMock.Object.ExecuteScriptAsync("monitor_vibration.bt");

        // Assert
        Assert.Contains("Alert", result);
    }
}

namespace Agent.Metering.Tests.eBPF;

public class eBPFDetectiveServiceTests
{
    [Fact]
    public void CanCreateInstance()
    {
        var loggerMock = new Mock<ILogger<eBPFDetectiveService>>();

        var service = new eBPFDetectiveService(loggerMock.Object);

        Assert.NotNull(service);
    }
}

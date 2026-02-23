namespace Agent.Metering.Tests.eBPF;

public class EbpfServiceTests
{
    [Fact]
    public async Task LoadAndAttachEbpfProgramAsync_CompletesWithoutException()
    {
        var loggerMock = new Mock<ILogger<EbpfService>>();
        var service = new EbpfService(loggerMock.Object);

        await service.LoadAndAttachEbpfProgramAsync();
    }

    [Fact]
    public async Task DetachAndUnloadEbpfProgramAsync_CompletesWithoutException()
    {
        var loggerMock = new Mock<ILogger<EbpfService>>();
        var service = new EbpfService(loggerMock.Object);

        await service.DetachAndUnloadEbpfProgramAsync();
    }

    [Fact]
    public async Task GetAndStoreMetricAsync_ReturnsValue()
    {
        var loggerMock = new Mock<ILogger<EbpfService>>();
        var service = new EbpfService(loggerMock.Object);

        var result = await service.GetAndStoreMetricAsync("test-metric");

        Assert.True(result >= 100);
    }
}


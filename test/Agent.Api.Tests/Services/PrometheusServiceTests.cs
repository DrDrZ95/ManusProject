namespace Agent.Api.Tests.Services;

public class PrometheusServiceTests
{
    private readonly Mock<IPrometheusService> _mockService;

    public PrometheusServiceTests()
    {
        _mockService = new Mock<IPrometheusService>();
    }

    [Fact]
    public void IncrementRequestCounter_ShouldCallServiceMethod()
    {
        // Arrange
        string endpoint = "/api/v1/test";

        // Act
        _mockService.Object.IncrementRequestCounter(endpoint);

        // Assert
        _mockService.Verify(s => s.IncrementRequestCounter(endpoint), Times.Once);
    }

    [Fact]
    public void ObserveRequestDuration_ShouldCallServiceMethod()
    {
        // Arrange
        string endpoint = "/api/v1/test";
        double duration = 0.5;

        // Act
        _mockService.Object.ObserveRequestDuration(endpoint, duration);

        // Assert
        _mockService.Verify(s => s.ObserveRequestDuration(endpoint, duration), Times.Once);
    }
}


using Agent.Core.Services;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Prometheus;

namespace Agent.Api.Tests.Services
{
    /// <summary>
    /// PrometheusMetricService的单元测试
    /// Unit tests for PrometheusMetricService
    /// </summary>
    public class PrometheusMetricServiceTests
    {
        private readonly Mock<IPrometheusMetricService> _mockMetricService;

        public PrometheusMetricServiceTests()
        {
            _mockMetricService = new Mock<IPrometheusMetricService>();
        }

        /// <summary>
        /// 测试指标收集服务（例如，计数器递增）
        /// Test metric collection service (e.g., counter increment)
        /// </summary>
        [Fact]
        public void IncrementRequestCounter_ShouldCallServiceMethod()
        {
            // Arrange
            string counterName = "http_requests_total";
            string[] labels = { "get", "/api/v1/test" };

            // Act
            _mockMetricService.Object.IncrementCounter(counterName, labels);

            // Assert
            // 验证服务方法是否被调用 - Verify that the service method was called
            _mockMetricService.Verify(s => s.IncrementCounter(counterName, labels), Times.Once);
        }

        /// <summary>
        /// 测试指标收集服务（例如，直方图观察）
        /// Test metric collection service (e.g., histogram observation)
        /// </summary>
        [Fact]
        public void ObserveRequestDuration_ShouldCallServiceMethod()
        {
            // Arrange
            string histogramName = "http_request_duration_seconds";
            double duration = 0.5;
            string[] labels = { "get", "/api/v1/test" };

            // Act
            _mockMetricService.Object.ObserveHistogram(histogramName, duration, labels);

            // Assert
            // 验证服务方法是否被调用 - Verify that the service method was called
            _mockMetricService.Verify(s => s.ObserveHistogram(histogramName, duration, labels), Times.Once);
        }

        // TODO: 补充其他指标类型的测试，例如Gauge、Summary等
        // TODO: Supplement other metric type tests, such as Gauge, Summary, etc.
    }
}


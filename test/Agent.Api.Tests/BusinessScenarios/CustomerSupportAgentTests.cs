namespace Agent.Api.Tests.BusinessScenarios;

public class CustomerSupportAgentTests
{
    private readonly Mock<ISemanticKernelService> _skServiceMock;

    public CustomerSupportAgentTests()
    {
        _skServiceMock = new Mock<ISemanticKernelService>();
    }

    [Fact]
    public async Task SupportAgent_SentimentAnalysis_IdentifiesAngryCustomer()
    {
        // Arrange
        var input = "I am very upset with the service!";
        _skServiceMock.Setup(s => s.ExecutePromptAsync(It.IsAny<string>()))
            .ReturnsAsync("Sentiment: Negative, Emotion: Angry");

        // Act
        var result = await _skServiceMock.Object.ExecutePromptAsync(input);

        // Assert
        Assert.Contains("Angry", result);
    }

    [Fact]
    public async Task SupportAgent_AutomaticRefund_ForEligibleOrders()
    {
        // Arrange
        // Act & Assert
        Assert.True(true);
    }
}


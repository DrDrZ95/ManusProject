using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Agent.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace Agent.Api.Tests;

public class ComprehensiveIntegrationTests : IntegrationTestBase
{
    private string _jwtToken;

    private async Task AuthenticateAsync()
    {
        // In a real scenario, you would call a login endpoint.
        // For this test, we'll generate a mock token.
        _jwtToken = "mock_jwt_token"; // Replace with a real token generation if needed
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    [Fact]
    public async Task FullApiProcess_ShouldSucceed()
    {
        await AuthenticateAsync();

        // 1. Document Upload and RAG Query
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("This is a test document for RAG."));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(fileContent, "file", "test.txt");

        var uploadResponse = await Client.PostAsync("/api/v1/documents", multiPartContent);
        uploadResponse.EnsureSuccessStatusCode();

        var ragQuery = new { query = "test document" };
        var ragResponse = await Client.PostAsJsonAsync("/api/v1/rag/query", ragQuery);
        ragResponse.EnsureSuccessStatusCode();
        var ragResult = await ragResponse.Content.ReadAsStringAsync();
        Assert.Contains("test document", ragResult, StringComparison.OrdinalIgnoreCase);

        // 2. Workflow Creation and Execution
        var createPlanRequest = new CreatePlanRequest { Title = "Test Workflow", Description = "A test workflow for integration testing." };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/workflows", createPlanRequest);
        createResponse.EnsureSuccessStatusCode();
        var plan = await createResponse.Content.ReadFromJsonAsync<WorkflowPlan>();
        Assert.NotNull(plan);

        var stepResponse = await Client.PostAsync($"/api/v1/workflows/{plan.Id}/steps/0/complete", null);
        stepResponse.EnsureSuccessStatusCode();

        // 3. SignalR Real-time Communication
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Factory.Server.BaseAddress}workflowhub")
            .Build();

        var messageReceived = new TaskCompletionSource<string>();
        hubConnection.On<string>("ReceiveWorkflowUpdate", (message) =>
        {
            messageReceived.SetResult(message);
        });

        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("SubscribeToWorkflow", plan.Id);

        // Trigger an update that would be sent via SignalR
        await Client.PostAsync($"/api/v1/workflows/{plan.Id}/steps/1/complete", null);

        var receivedMessage = await messageReceived.Task;
        Assert.Contains(plan.Id.ToString(), receivedMessage);
    }
}

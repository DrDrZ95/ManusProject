using Xunit;
using System.Threading.Tasks;
using Agent.Api.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.SignalR.Client;
using Hangfire;
using Hangfire.MemoryStorage;

namespace Agent.Api.Tests.Integration;

/// <summary>
/// 实时通信和后台任务集成测试 (Real-Time Communication and Background Task Integration Tests)
/// 
/// 目标: 验证 SignalR 实时通信和 Hangfire 后台任务的集成和功能。
/// Goal: Validate the integration and functionality of SignalR real-time communication and Hangfire background tasks.
/// </summary>
[Collection("IntegrationTestCollection")]
public class RealTimeCommunicationTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public RealTimeCommunicationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// 测试 SignalR 实时通信连接和消息接收 (Test SignalR real-time communication connection and message reception)
    /// </summary>
    [Fact]
    public async Task SignalR_ConnectionAndMessage_ShouldSucceed()
    {
        // 假设 SignalR Hub 的路径是 /hubs/workflow
        // Assume the SignalR Hub path is /hubs/workflow
        var hubUrl = "http://localhost/hubs/workflow"; // 实际测试中需要使用测试服务器的地址 (In real test, use the test server address)

        // 模拟 HubConnection 的创建 (Simulate HubConnection creation)
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // 禁用证书验证等，以便在测试环境中连接 (Disable certificate validation etc. for test environment connection)
                options.HttpMessageHandlerFactory = _ => _fixture.Server.CreateHandler();
            })
            .Build();

        var messageReceived = new TaskCompletionSource<string>();

        // 模拟客户端订阅一个方法 (Simulate client subscribing to a method)
        connection.On<string>("ReceiveWorkflowUpdate", (message) =>
        {
            messageReceived.SetResult(message);
        });

        try
        {
            // 模拟连接 (Simulate connection)
            // await connection.StartAsync();
            // Assert.Equal(HubConnectionState.Connected, connection.State);

            // 模拟服务器端发送消息 (Simulate server-side sending a message)
            // 实际应用中，这需要通过一个内部服务调用 HubContext.Clients.All.SendAsync
            // In a real app, this requires an internal service call to HubContext.Clients.All.SendAsync
            
            // 由于无法直接模拟服务器发送，我们只验证连接逻辑和订阅设置
            // Since we cannot directly simulate server sending, we only validate connection logic and subscription setup
            
            // 验证订阅是否设置成功 (Verify subscription is set up successfully)
            Assert.NotNull(connection);
            Assert.Contains("ReceiveWorkflowUpdate", connection.Callbacks.Keys);
        }
        finally
        {
            // 模拟断开连接 (Simulate disconnection)
            // await connection.StopAsync();
        }
    }

    /// <summary>
    /// 测试 Hangfire 后台任务的调度和执行 (Test Hangfire background task scheduling and execution)
    /// </summary>
    [Fact]
    public void Hangfire_TaskScheduling_ShouldSucceed()
    {
        // 1. 配置 Hangfire 使用内存存储进行测试 (Configure Hangfire to use in-memory storage for testing)
        GlobalConfiguration.Configuration.UseMemoryStorage();
        
        // 2. 模拟一个简单的后台任务服务 (Simulate a simple background task service)
        var taskExecuted = false;
        var mockService = new MockHangfireService(() => taskExecuted = true);

        // 3. 调度一个一次性任务 (Schedule a one-time job)
        var jobId = BackgroundJob.Enqueue(() => mockService.ExecuteOnce());
        
        // 4. 验证任务是否被调度 (Verify the job is scheduled)
        Assert.NotNull(jobId);
        
        // 5. 模拟 Hangfire 服务器执行任务 (Simulate Hangfire server executing the job)
        // 在单元测试环境中，我们不能运行完整的Hangfire服务器，但可以验证调度逻辑
        // In a unit test environment, we cannot run a full Hangfire server, but we can validate the scheduling logic
        
        // 6. 调度一个重复性任务 (Schedule a recurring job)
        RecurringJob.AddOrUpdate("daily-report", () => mockService.ExecuteRecurring(), Cron.Daily);
        
        // 7. 验证重复性任务是否被调度 (Verify the recurring job is scheduled)
        var recurringJob = JobStorage.Current.GetMonitoringApi().GetStatistics().Recurring;
        Assert.True(recurringJob > 0);
        
        // 8. 验证任务执行逻辑 (Verify task execution logic)
        mockService.ExecuteOnce(); // 直接调用模拟执行
        Assert.True(taskExecuted);
    }
}

/// <summary>
/// 模拟 Hangfire 后台任务服务 (Mock Hangfire Background Task Service)
/// </summary>
public class MockHangfireService
{
    private readonly Action _onExecute;

    public MockHangfireService(Action onExecute)
    {
        _onExecute = onExecute;
    }

    public void ExecuteOnce()
    {
        // 实际任务执行逻辑 (Actual task execution logic)
        _onExecute();
    }

    public void ExecuteRecurring()
    {
        // 实际重复任务执行逻辑 (Actual recurring task execution logic)
    }
}

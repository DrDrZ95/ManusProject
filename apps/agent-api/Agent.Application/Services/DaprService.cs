namespace Agent.Application.Services;

/// <summary>
/// Service for Dapr integration.
/// </summary>
public class DaprService : IDaprService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprService> _logger;
    private readonly ITelemetryService _telemetryService;

    public DaprService(DaprClient daprClient, ILogger<DaprService> logger, ITelemetryService telemetryService)
    {
        _daprClient = daprClient;
        _logger = logger;
        _telemetryService = telemetryService;
    }

    /// <summary>
    /// Invokes a method on another service using Dapr.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="appId">The application ID of the service to invoke.</param>
    /// <param name="methodName">The name of the method to invoke.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the invoked method.</returns>
    public async Task<TResponse> InvokeMethodAsync<TRequest, TResponse>(
        string appId,
        string methodName,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _telemetryService.TraceActivityAsync<TResponse>(
            $"DaprInvoke_{appId}_{methodName}",
            async () =>
            {
                _logger.LogInformation("Invoking method {MethodName} on app {AppId}", methodName, appId);
                
                var response = await _daprClient.InvokeMethodAsync<TRequest, TResponse>(
                    appId,
                    methodName,
                    request,
                    cancellationToken);
                
                _logger.LogInformation("Method {MethodName} on app {AppId} invoked successfully", methodName, appId);
                
                return response;
            },
            ("appId", appId),
            ("methodName", methodName));
    }

    /// <summary>
    /// Saves state using Dapr state management.
    /// </summary>
    /// <typeparam name="T">The state value type.</typeparam>
    /// <param name="storeName">The name of the state store.</param>
    /// <param name="key">The state key.</param>
    /// <param name="value">The state value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveStateAsync<T>(
        string storeName,
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        await _telemetryService.TraceActivityAsync<bool>(
            $"DaprSaveState_{storeName}",
            async () =>
            {
                _logger.LogInformation("Saving state with key {Key} to store {StoreName}", key, storeName);
                
                await _daprClient.SaveStateAsync(
                    storeName,
                    key,
                    value,
                    cancellationToken: cancellationToken);
                
                _logger.LogInformation("State with key {Key} saved to store {StoreName}", key, storeName);
                
                return true;
            },
            ("storeName", storeName),
            ("key", key));
    }

    /// <summary>
    /// Gets state using Dapr state management.
    /// </summary>
    /// <typeparam name="T">The state value type.</typeparam>
    /// <param name="storeName">The name of the state store.</param>
    /// <param name="key">The state key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The state value.</returns>
    public async Task<T> GetStateAsync<T>(
        string storeName,
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _telemetryService.TraceActivityAsync<T>(
            $"DaprGetState_{storeName}",
            async () =>
            {
                _logger.LogInformation("Getting state with key {Key} from store {StoreName}", key, storeName);
                
                var result = await _daprClient.GetStateAsync<T>(
                    storeName,
                    key,
                    cancellationToken: cancellationToken);
                
                _logger.LogInformation("State with key {Key} retrieved from store {StoreName}", key, storeName);
                
                return result;
            },
            ("storeName", storeName),
            ("key", key));
    }

    /// <summary>
    /// Publishes an event using Dapr pub/sub.
    /// </summary>
    /// <typeparam name="T">The event data type.</typeparam>
    /// <param name="pubsubName">The name of the pub/sub component.</param>
    /// <param name="topicName">The name of the topic.</param>
    /// <param name="eventData">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishEventAsync<T>(
        string pubsubName,
        string topicName,
        T eventData,
        CancellationToken cancellationToken = default)
    {
        await _telemetryService.TraceActivityAsync<bool>(
            $"DaprPublishEvent_{pubsubName}_{topicName}",
            async () =>
            {
                _logger.LogInformation("Publishing event to topic {TopicName} on pubsub {PubsubName}", topicName, pubsubName);
                
                await _daprClient.PublishEventAsync(
                    pubsubName,
                    topicName,
                    eventData,
                    cancellationToken);
                
                _logger.LogInformation("Event published to topic {TopicName} on pubsub {PubsubName}", topicName, pubsubName);
                
                return true;
            },
            ("pubsubName", pubsubName),
            ("topicName", topicName));
    }
}

/// <summary>
/// Interface for Dapr service.
/// </summary>
public interface IDaprService
{
    Task<TResponse> InvokeMethodAsync<TRequest, TResponse>(
        string appId,
        string methodName,
        TRequest request,
        CancellationToken cancellationToken = default);
        
    Task SaveStateAsync<T>(
        string storeName,
        string key,
        T value,
        CancellationToken cancellationToken = default);
        
    Task<T> GetStateAsync<T>(
        string storeName,
        string key,
        CancellationToken cancellationToken = default);
        
    Task PublishEventAsync<T>(
        string pubsubName,
        string topicName,
        T eventData,
        CancellationToken cancellationToken = default);
}

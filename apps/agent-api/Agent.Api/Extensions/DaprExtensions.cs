namespace Agent.Api.Extensions;

/// <summary>
/// Extension methods for configuring Dapr in the application.
/// </summary>
public static class DaprExtensions
{
    /// <summary>
    /// Adds Dapr services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDaprServices(this IServiceCollection services)
    {
        // Add Dapr client
        services.AddDaprClient();

        // Add Dapr service
        services.AddScoped<IDaprService, DaprService>();

        return services;
    }

    /// <summary>
    /// Configures Dapr middleware in the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseDaprMiddleware(this IApplicationBuilder app)
    {
        // Use Dapr cloud events middleware
        app.UseCloudEvents();

        // Map Dapr subscriber endpoints
        //app.MapSubscribeHandler();

        return app;
    }
}

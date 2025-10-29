namespace Agent.Api.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry in the application.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="serviceVersion">The version of the service.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        string serviceName = "Agent.Api",
        string serviceVersion = "1.0.0")
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddSource("Agent.Api")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317");
                    });
            });

        services.AddSingleton<ITelemetryService, TelemetryService>();

        return services;
    }
}


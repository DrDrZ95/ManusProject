namespace Agent.Metering.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        string serviceName = "Agent.Metering",
        string serviceVersion = "1.0.0")
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddSource("Agent.Metering")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317");
                    });
            });

        return services;
    }
}

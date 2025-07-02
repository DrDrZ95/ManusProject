# AI-Agent YARP Gateway Integration

This document outlines the integration of YARP (Yet Another Reverse Proxy) as a gateway mechanism within the `AgentWebApi` project, including a circuit breaker mechanism for enhanced resilience.

## 1. Overview

The YARP gateway acts as an API Gateway, routing incoming requests to various backend services (e.g., Semantic Kernel, RAG, ChromaDB, SignalR). It provides features like load balancing, health checks, and request transformation. The integrated circuit breaker mechanism, powered by Polly, prevents cascading failures by temporarily stopping requests to unhealthy services.

## 2. Key Features

- **Reverse Proxy**: Routes requests to different backend services based on defined rules.
- **Load Balancing**: Distributes incoming traffic across multiple instances of backend services.
- **Health Checks**: Actively monitors the health of backend services and removes unhealthy instances from the routing pool.
- **Circuit Breaker**: Implements the circuit breaker pattern to protect services from being overwhelmed by repeated failures, improving overall system resilience.
- **Modular Integration**: Designed as an independent module that can be easily enabled or disabled in `Program.cs`.
- **Customizable Configuration**: Flexible configuration for routes, clusters, and circuit breaker policies.

## 3. Implementation Details

### 3.1. YARP Configuration (`Gateway/YarpConfig.cs`)

This static class defines the routing rules (`GetRoutes()`) and cluster configurations (`GetClusters()`).

- **Routes**: Define how incoming paths are matched and transformed before being forwarded to a cluster. Examples include routes for AI-Agent API, Semantic Kernel, RAG, ChromaDB, and SignalR.
- **Clusters**: Define the backend services, including their addresses, load balancing policies (e.g., RoundRobin, LeastRequests), and health check configurations.

```csharp
// Example Route Configuration
new RouteConfig
{
    RouteId = "ai-agent-api",
    ClusterId = "ai-agent-cluster",
    Match = new RouteMatch
    {
        Path = "/api/{**catch-all}"
    },
    Transforms = new[]
    {
        new Dictionary<string, string>
        {
            ["PathPattern"] = "/api/{**catch-all}"
        }
    }
},

// Example Cluster Configuration
new ClusterConfig
{
    ClusterId = "ai-agent-cluster",
    LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
    HealthCheck = new HealthCheckConfig
    {
        Active = new ActiveHealthCheckConfig
        {
            Enabled = true,
            Interval = TimeSpan.FromSeconds(30),
            Timeout = TimeSpan.FromSeconds(5),
            Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
            Path = "/health"
        },
        Passive = new PassiveHealthCheckConfig
        {
            Enabled = true,
            Policy = HealthCheckConstants.PassivePolicy.TransportFailureRate,
            ReactivationPeriod = TimeSpan.FromMinutes(2)
        }
    },
    Destinations = new Dictionary<string, DestinationConfig>
    {
        ["ai-agent-1"] = new DestinationConfig
        {
            Address = "https://localhost:5001",
            Health = "https://localhost:5001/health"
        },
        ["ai-agent-2"] = new DestinationConfig
        {
            Address = "https://localhost:5002",
            Health = "https://localhost:5002/health"
        }
    }
}
```

### 3.2. Circuit Breaker Middleware (`Gateway/CircuitBreakerMiddleware.cs`)

This middleware implements the circuit breaker pattern using Polly. It intercepts HTTP requests and applies resilience policies (circuit breaker, retry, timeout) based on the request path.

- **Circuit Breaker Policies**: Configured for different services (AI service, RAG, ChromaDB, SignalR) with varying failure ratios, minimum throughputs, sampling durations, and break durations.
- **Retry Policy**: Retries failed requests a specified number of times with exponential backoff.
- **Timeout Policy**: Sets a maximum duration for request execution.

```csharp
public class CircuitBreakerMiddleware
{
    // ... (constructor and other methods)

    private void InitializePipelines()
    {
        // AI服务熔断器 - AI Service Circuit Breaker
        _pipelines["ai-service"] = CreatePipeline(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5, // 50%失败率触发熔断 - 50% failure rate triggers circuit breaker
            MinimumThroughput = 10, // 最少10个请求 - Minimum 10 requests
            SamplingDuration = TimeSpan.FromSeconds(30), // 30秒采样周期 - 30 second sampling period
            BreakDuration = TimeSpan.FromSeconds(60), // 熔断持续60秒 - Circuit break for 60 seconds
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        });

        // ... (other service pipelines)
    }

    private ResiliencePipeline CreatePipeline(CircuitBreakerStrategyOptions circuitBreakerOptions)
    {
        return new ResiliencePipelineBuilder()
            .AddCircuitBreaker(circuitBreakerOptions)
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
            })
            .AddTimeout(TimeSpan.FromSeconds(30)) // 30秒超时 - 30 second timeout
            .Build();
    }
}
```

### 3.3. Extension Methods (`Extensions/YarpExtensions.cs`)

These extension methods provide a clean and modular way to add YARP services and middleware to the application's dependency injection container and request pipeline.

- `AddAiAgentYarp()`: Configures YARP reverse proxy and circuit breaker options.
- `UseAiAgentYarp()`: Adds the `CircuitBreakerMiddleware` and YARP's routing and endpoint mapping to the application pipeline.

```csharp
public static class YarpExtensions
{
    public static IServiceCollection AddAiAgentYarp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("Yarp"));

        services.Configure<CircuitBreakerOptions>(configuration.GetSection(CircuitBreakerOptions.SectionName));

        return services;
    }

    public static IApplicationBuilder UseAiAgentYarp(this IApplicationBuilder app)
    {
        app.UseMiddleware<CircuitBreakerMiddleware>();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapReverseProxy();
        });

        return app;
    }
}
```

### 3.4. Program.cs Integration

To enable the YARP gateway and circuit breaker, uncomment the following lines in `Program.cs`:

```csharp
// Add YARP services
builder.Services.AddAiAgentYarp(builder.Configuration); // Optional AI-Agent gateway with circuit breaker - 可选的AI-Agent网关与熔断器

// Use YARP middleware
app.UseAiAgentYarp(); // Optional AI-Agent gateway middleware - 可选的AI-Agent网关中间件
```

## 4. Configuration (`appsettings.json`)

The YARP configuration is typically defined in `appsettings.json` under the `Yarp` section. This includes `Routes` and `Clusters`.

```json
{
  "Yarp": {
    "Routes": {
      "ai-agent-api": {
        "ClusterId": "ai-agent-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
      // ... other routes
    },
    "Clusters": {
      "ai-agent-cluster": {
        "Destinations": {
          "ai-agent-1": {
            "Address": "https://localhost:5001"
          }
        }
      }
      // ... other clusters
    }
  },
  "CircuitBreaker": {
    "Enabled": true,
    "DefaultFailureRatio": 0.5
    // ... other circuit breaker options
  }
}
```

## 5. Usage

Once configured and enabled, all requests to the `AgentWebApi` will pass through the YARP gateway. The circuit breaker will automatically monitor the health of the backend services and apply resilience policies as defined.

For example, a request to `/api/semantickernel/chat` would be routed to the `semantic-kernel-cluster` and be subject to the `ai-service` circuit breaker policy defined in `CircuitBreakerMiddleware.cs`.


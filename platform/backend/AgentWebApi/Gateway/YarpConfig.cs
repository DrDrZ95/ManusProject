using Yarp.ReverseProxy.Configuration;

namespace AgentWebApi.Gateway;

/// <summary>
/// YARP 网关配置类 - AI-Agent Gateway Configuration Class
/// 用于配置反向代理路由和集群 - Used to configure reverse proxy routes and clusters
/// </summary>
public static class YarpConfig
{
    /// <summary>
    /// 获取路由配置 - Get route configuration
    /// 定义请求如何路由到不同的后端服务 - Define how requests are routed to different backend services
    /// </summary>
    public static RouteConfig[] GetRoutes()
    {
        return new[]
        {
            // AI-Agent API 路由 - AI-Agent API Routes
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
            
            // SemanticKernel 服务路由 - SemanticKernel Service Routes
            new RouteConfig
            {
                RouteId = "semantic-kernel-route",
                ClusterId = "semantic-kernel-cluster",
                Match = new RouteMatch
                {
                    Path = "/sk/{**catch-all}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/api/semantickernel/{**catch-all}"
                    }
                }
            },
            
            // RAG 服务路由 - RAG Service Routes
            new RouteConfig
            {
                RouteId = "rag-route",
                ClusterId = "rag-cluster",
                Match = new RouteMatch
                {
                    Path = "/rag/{**catch-all}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/api/rag/{**catch-all}"
                    }
                }
            },
            
            // ChromaDB 服务路由 - ChromaDB Service Routes
            new RouteConfig
            {
                RouteId = "chromadb-route",
                ClusterId = "chromadb-cluster",
                Match = new RouteMatch
                {
                    Path = "/chromadb/{**catch-all}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/api/chromadb/{**catch-all}"
                    }
                }
            },
            
            // SignalR Hub 路由 - SignalR Hub Routes
            new RouteConfig
            {
                RouteId = "signalr-route",
                ClusterId = "signalr-cluster",
                Match = new RouteMatch
                {
                    Path = "/hubs/{**catch-all}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = "/hubs/{**catch-all}"
                    }
                }
            }
        };
    }

    /// <summary>
    /// 获取集群配置 - Get cluster configuration
    /// 定义后端服务的负载均衡和健康检查 - Define load balancing and health checks for backend services
    /// </summary>
    public static ClusterConfig[] GetClusters()
    {
        return new[]
        {
            // AI-Agent 主集群 - AI-Agent Main Cluster
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
            },
            
            // SemanticKernel 集群 - SemanticKernel Cluster
            new ClusterConfig
            {
                ClusterId = "semantic-kernel-cluster",
                LoadBalancingPolicy = LoadBalancingPolicies.LeastRequests,
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(10),
                        Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["sk-service-1"] = new DestinationConfig
                    {
                        Address = "https://localhost:5001",
                        Health = "https://localhost:5001/health"
                    }
                }
            },
            
            // RAG 集群 - RAG Cluster
            new ClusterConfig
            {
                ClusterId = "rag-cluster",
                LoadBalancingPolicy = LoadBalancingPolicies.PowerOfTwoChoices,
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(15),
                        Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["rag-service-1"] = new DestinationConfig
                    {
                        Address = "https://localhost:5001",
                        Health = "https://localhost:5001/health"
                    }
                }
            },
            
            // ChromaDB 集群 - ChromaDB Cluster
            new ClusterConfig
            {
                ClusterId = "chromadb-cluster",
                LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(60),
                        Timeout = TimeSpan.FromSeconds(10),
                        Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
                        Path = "/api/v1/heartbeat"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["chromadb-1"] = new DestinationConfig
                    {
                        Address = "http://localhost:8000",
                        Health = "http://localhost:8000/api/v1/heartbeat"
                    }
                }
            },
            
            // SignalR 集群 - SignalR Cluster
            new ClusterConfig
            {
                ClusterId = "signalr-cluster",
                LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
                SessionAffinity = new SessionAffinityConfig
                {
                    Enabled = true,
                    Policy = SessionAffinityConstants.Policies.Cookie,
                    AffinityKeyName = "ai-agent-signalr"
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["signalr-1"] = new DestinationConfig
                    {
                        Address = "https://localhost:5001",
                        Health = "https://localhost:5001/health"
                    }
                }
            }
        };
    }
}


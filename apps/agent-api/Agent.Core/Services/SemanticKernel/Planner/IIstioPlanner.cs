using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agent.Core.Services.SemanticKernel.Planner;

/// <summary>
/// Istio Planner Interface
/// </summary>
public interface IIstioPlanner
{
    /// <summary>
    /// Deploys an Istio VirtualService for traffic management.
    /// </summary>
    /// <param name="yamlContent">The Istio VirtualService YAML content.</param>
    /// <returns>A message indicating the deployment status.</returns>
    [KernelFunction, Description("Deploys an Istio VirtualService for traffic management.")]
    Task<string> DeployVirtualService([Description("The Istio VirtualService YAML content.")] string yamlContent);

    /// <summary>
    /// Configures traffic shifting for a service.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="namespaceName">The namespace of the service.</param>
    /// <param name="subsetWeights">A JSON string representing subset weights (e.g., {"v1": 90, "v2": 10}).</param>
    /// <returns>A message indicating the configuration status.</returns>
    [KernelFunction, Description("Configures traffic shifting for a service using Istio DestinationRule.")]
    Task<string> ConfigureTrafficShifting(
        [Description("The name of the service.")] string serviceName,
        [Description("The namespace of the service.")] string namespaceName,
        [Description("A JSON string representing subset weights (e.g., {\"v1\": 90, \"v2\": 10}).")] string subsetWeights);

    /// <summary>
    /// Injects Istio sidecar into a deployment.
    /// </summary>
    /// <param name="deploymentName">The name of the deployment.</param>
    /// <param name="namespaceName">The namespace of the deployment.</param>
    /// <returns>A message indicating the injection status.</returns>
    [KernelFunction, Description("Injects Istio sidecar into a Kubernetes deployment.")]
    Task<string> InjectSidecar(
        [Description("The name of the deployment.")] string deploymentName,
        [Description("The namespace of the deployment.")] string namespaceName);

    /// <summary>
    /// Gets the status of Istio gateways.
    /// </summary>
    /// <param name="namespaceName">The namespace to check.</param>
    /// <returns>A JSON string representing the status of gateways.</returns>
    [KernelFunction, Description("Gets the status of Istio gateways in a given namespace.")]
    Task<string> GetGatewayStatus([Description("The namespace to check.")] string namespaceName);
}



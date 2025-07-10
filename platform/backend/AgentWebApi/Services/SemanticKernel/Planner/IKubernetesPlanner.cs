using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AgentWebApi.Services.SemanticKernel.Planner;

/// <summary>
/// Kubernetes Planner Interface
/// </summary>
public interface IKubernetesPlanner
{
    /// <summary>
    /// Deploys a Kubernetes application.
    /// </summary>
    /// <param name="yamlContent">The Kubernetes YAML content for deployment.</param>
    /// <returns>A message indicating the deployment status.</returns>
    [KernelFunction, Description("Deploys a Kubernetes application from YAML content.")]
    Task<string> DeployApplication([Description("The Kubernetes YAML content for deployment.")] string yamlContent);

    /// <summary>
    /// Scales a Kubernetes deployment.
    /// </summary>
    /// <param name="deploymentName">The name of the deployment to scale.</param>
    /// <param name="namespaceName">The namespace of the deployment.</param>
    /// <param name="replicas">The desired number of replicas.</param>
    /// <returns>A message indicating the scaling status.</returns>
    [KernelFunction, Description("Scales a Kubernetes deployment to a specified number of replicas.")]
    Task<string> ScaleDeployment(
        [Description("The name of the deployment to scale.")] string deploymentName,
        [Description("The namespace of the deployment.")] string namespaceName,
        [Description("The desired number of replicas.")] int replicas);

    /// <summary>
    /// Gets the status of Kubernetes pods in a given namespace.
    /// </summary>
    /// <param name="namespaceName">The namespace to check.</param>
    /// <returns>A JSON string representing the status of pods.</returns>
    [KernelFunction, Description("Gets the status of Kubernetes pods in a given namespace.")]
    Task<string> GetPodStatus([Description("The namespace to check.")] string namespaceName);

    /// <summary>
    /// Executes a command in a Kubernetes pod.
    /// </summary>
    /// <param name="podName">The name of the pod.</param>
    /// <param name="namespaceName">The namespace of the pod.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>The output of the command execution.</returns>
    [KernelFunction, Description("Executes a command in a Kubernetes pod.")]
    Task<string> ExecInPod(
        [Description("The name of the pod.")] string podName,
        [Description("The namespace of the pod.")] string namespaceName,
        [Description("The command to execute.")] string command);
}



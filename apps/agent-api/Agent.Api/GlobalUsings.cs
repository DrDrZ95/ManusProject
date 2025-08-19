global using Agent.Core.Authorization;
global using Agent.Api.Controllers;
global using Agent.Api.Extensions;
global using Agent.Api.Domain;
global using Agent.Core.Domain;
global using Agent.Core.Data;
global using Agent.Core.Data.Repositories;
global using Agent.Core.Extensions;
global using Agent.Core.Gateway;
global using Agent.Core.Hubs;
global using Agent.Core.Identity;
global using Agent.Core.Models.Identity;
global using Agent.Core.Services;
global using Agent.Core.Services.Authorization;
global using Agent.Core.Services.Finetune;
global using Agent.Core.Services.Hdfs;
global using Agent.Core.Services.Prometheus;
global using Agent.Core.Services.Prompts;
global using Agent.Core.Services.Qwen;
global using Agent.Core.Services.RAG;
global using Agent.Core.Services.Sandbox;
global using Agent.Core.Services.SemanticKernel;
global using Agent.Core.Services.SemanticKernel.Planner;
global using Agent.Core.Services.Telemetry;
global using Agent.Core.Services.UserInput;
global using Agent.Core.Services.VectorDatabase;
global using Agent.Core.Services.Workflow;

global using ChromaDB.Client;
global using ChromaDB.Client.Models;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.SemanticKernel;
global using ModelContextProtocol.Protocol;
global using System.Diagnostics;


namespace Agent.Api.Domain;
public class AddDocumentsRequest
{
    public IEnumerable<VectorDocument> Documents { get; set; } = new List<VectorDocument>();
    public string CollectionName { get; set; } = string.Empty;
    public IEnumerable<string>? Ids { get; set; }
    public IEnumerable<Dictionary<string, object>>? Metadatas { get; set; }
}


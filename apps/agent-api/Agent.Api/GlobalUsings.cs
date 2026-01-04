global using Agent.Api;
global using Agent.Api.Controllers;
global using Agent.Api.Extensions;

global using Agent.Core;
global using Agent.Core.Authorization;
global using Agent.Core.Data;
global using Agent.Core.Data.Entities;
global using Agent.Core.Data.Repositories;
global using Agent.Core.Extensions;
global using Agent.Core.eBPF.Detective;
global using Agent.Core.Gateway;
global using Agent.Core.Hubs;
global using Agent.Core.Identity;
global using Agent.Core.Models.Identity;
global using Agent.Core.Services;
global using Agent.Core.Services.Authorization;
global using Agent.Core.Services.Finetune;
global using Agent.Core.Services.FileUpload;
global using Agent.Core.Services.Hdfs;
global using Agent.Core.Services.Prometheus;
global using Agent.Core.Services.Prompts;
global using Agent.Core.Services.RAG;
global using Agent.Core.Services.Sandbox;
global using Agent.Core.Services.SemanticKernel;
global using Agent.Core.Services.SemanticKernel.Planner;
global using Agent.Core.Services.Telemetry;
global using Agent.Core.Services.UserInput;
global using Agent.Core.Services.VectorDatabase;

global using Agent.Core.Workflow;
global using Agent.Core.Workflow.Models;
global using Agent.McpGateway;
global using Agent.McpGateway.UniversalMcp;

global using ChromaDB.Client;
global using ChromaDB.Client.Models;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ApiExplorer;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.AspNetCore.Http.Features;

global using Microsoft.OpenApi.Models;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel;
global using Microsoft.SemanticKernel.ChatCompletion;
global using Microsoft.SemanticKernel.Connectors.OpenAI;
global using Microsoft.SemanticKernel.Embeddings;
global using ModelContextProtocol.Protocol;


global using System.Diagnostics;
global using System.Reflection;
global using System.Text;

global using Asp.Versioning;
global using Swashbuckle.AspNetCore.SwaggerGen;
global using StackExchange.Redis;
global using Asp.Versioning.ApiExplorer;
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;
global using Hangfire;
global using Hangfire.Redis.StackExchange;
global using Prometheus;

namespace Agent.Api;

public class AddDocumentsRequest
{
    public IEnumerable<VectorDocument> Documents { get; set; } = new List<VectorDocument>();
    public string CollectionName { get; set; } = string.Empty;
    public IEnumerable<string>? Ids { get; set; }
    public IEnumerable<Dictionary<string, object>>? Metadatas { get; set; }
}



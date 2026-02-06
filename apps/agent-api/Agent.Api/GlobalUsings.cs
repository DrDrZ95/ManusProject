global using Agent.Core.Models.Common;
global using Agent.Core.Models.Finetune;
global using Swashbuckle.AspNetCore.Annotations;
global using Microsoft.AspNetCore.Http;

global using Agent.Api.Extensions;

global using Agent.Core;
global using Agent.Core.Authorization;
global using Agent.Core.Data;
global using Agent.Core.Data.Entities;
global using Agent.Core.Data.Repositories;
global using Agent.Core.eBPF.Detective;
global using Agent.Core.Gateway;
global using Agent.Core.Identity;
global using Agent.Core.Models.Identity;

global using Agent.Application.Hubs;
global using Agent.Application.Services;
global using Agent.Application.Services.Finetune;
global using Agent.Application.Services.FileUpload;
global using Agent.Application.Services.Hdfs;
global using Agent.Application.Services.Prometheus;
global using Agent.Application.Services.Prompts;
global using Agent.Application.Services.RAG;
global using Agent.Application.Services.Sandbox;
global using Agent.Application.Services.SemanticKernel;
global using Agent.Application.Services.SemanticKernel.Planner;
global using Agent.Application.Services.Telemetry;
global using Agent.Application.Services.UserInput;
global using Agent.Application.Services.VectorDatabase;
global using Agent.Application.Services.Workflow;
    
global using Agent.Core.Workflow;
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
global using Microsoft.SemanticKernel.Embeddings;


global using System.Net;
global using System.Text;
global using System.Text.Json;
global using System.Diagnostics;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.Reflection;


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



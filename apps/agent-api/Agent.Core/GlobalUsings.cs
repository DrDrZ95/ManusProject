global using Agent.Core.Authorization;
global using Agent.Core.Data;
global using Agent.Core.Data.Repositories;
global using Agent.Core.Data.Entities;
global using Agent.Core.Domain;
global using Agent.Core.eBPF;
global using Agent.Core.Gateway;
global using Agent.Core.Hubs;
global using Agent.Core.Identity;

global using Agent.Core.Models;
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

global using HealthChecks.NpgSql;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel;
global using System.Diagnostics;

global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Data.SqlClient;
global using System.Collections.Generic;
global using System;
global using System.Threading.Tasks;
global using System.Net.Http.Json;

global using ChromaDB.Client;
global using ChromaDB.Client.Models;
global using ModelContextProtocol;
global using ModelContextProtocol.Protocol;
global using ModelContextProtocol.Server;

global using Microsoft.AspNetCore.SignalR;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.Diagnostics.HealthChecks;

global using Yarp.ReverseProxy.LoadBalancing;
global using Yarp.ReverseProxy.Health;
global using Yarp.ReverseProxy.SessionAffinity;

global using Polly;
global using Polly.Retry;

global using Prometheus;

global using PromCounter   = Prometheus.Counter;
global using PromHistogram = Prometheus.Histogram;

namespace Agent.Core.Domain;

/// <summary>
/// Chat message for conversation history
/// 对话历史的聊天消息
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Message role (system, user, assistant) - 消息角色（系统、用户、助手）
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Message content - 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Metadata { get; set; }
}


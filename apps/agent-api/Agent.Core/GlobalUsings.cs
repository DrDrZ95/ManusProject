global using Agent.Core.Authorization;
global using Agent.Core.Cache;
global using Agent.Core.Data;
global using Agent.Core.Data.Entities;
global using Agent.Core.Data.Repositories;
global using Agent.Core.eBPF;
global using Agent.Core.Gateway;
global using Agent.Core.Identity;
global using Agent.Core.Memory.Interfaces;
global using Agent.Core.Models;
global using Agent.Core.Models.Identity;
global using Agent.Core.Notifications;
global using Agent.Core.Workflow;
global using Flurl;
global using Flurl.Http;
global using HealthChecks.NpgSql;
global using Mapster;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Data.SqlClient;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel;
global using Microsoft.SemanticKernel.Connectors.Chroma;
global using ModelContextProtocol;
global using ModelContextProtocol.Protocol;
global using ModelContextProtocol.Server;
global using Polly;
global using Polly.CircuitBreaker;
global using Polly.Retry;
global using Prometheus;
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Diagnostics;
global using System.Linq.Expressions;
global using System.Net.Http.Json;
global using System.Runtime.InteropServices;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Threading.Tasks;
global using Yarp.ReverseProxy.Configuration;
global using Yarp.ReverseProxy.Health;
global using Yarp.ReverseProxy.LoadBalancing;
global using Yarp.ReverseProxy.SessionAffinity;

//可能存在功能不完善,不使用但保留
//global using ChromaDB.Client;
//global using ChromaDB.Client.Models;

global using PromCounter   = Prometheus.Counter;
global using PromHistogram = Prometheus.Histogram;

namespace Agent.Core;

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

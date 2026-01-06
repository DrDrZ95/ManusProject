global using Agent.Core.Authorization;
global using Agent.Core.Cache;
global using Agent.Core.Data;
global using Agent.Core.Data.Repositories;
global using Agent.Core.Data.Entities;
global using Agent.Core.eBPF;
global using Agent.Core.Gateway;
global using Agent.Core.Hubs;
global using Agent.Core.Identity;

global using Agent.Core.Models;
global using Agent.Core.Models.Identity;
global using Agent.Core.Workflow;

global using Agent.Application.Services;
global using Agent.Application.Services.Authorization;
global using Agent.Application.Services.Finetune;
global using Agent.Application.Services.Hdfs;
global using Agent.Application.Services.Multimodal;
global using Agent.Application.Services.Prometheus;
global using Agent.Application.Services.Prompts;
global using Agent.Application.Services.RAG;
global using Agent.Application.Services.Sandbox;
global using Agent.Application.Services.SemanticKernel;
global using Agent.Application.Services.SemanticKernel.Planner;
global using Agent.Application.Services.Telemetry;
global using Agent.Application.Services.UserInput;
global using Agent.Application.Services.VectorDatabase;

global using HealthChecks.NpgSql;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel;
global using Microsoft.SemanticKernel.Connectors.Chroma;
global using System.Diagnostics;

global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Data.SqlClient;
global using System.Collections.Generic;
global using System;
global using System.Text.Json;
global using System.Threading.Tasks;
global using System.Net.Http.Json;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;

global using ModelContextProtocol;
global using ModelContextProtocol.Protocol;
global using ModelContextProtocol.Server;

global using Microsoft.AspNetCore.SignalR;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.Diagnostics.HealthChecks;

global using Yarp.ReverseProxy.LoadBalancing;
global using Yarp.ReverseProxy.Health;
global using Yarp.ReverseProxy.SessionAffinity;

global using Flurl;
global using Flurl.Http;

global using Polly;
global using Polly.Retry;

global using Prometheus;

global using PromCounter   = Prometheus.Counter;
global using PromHistogram = Prometheus.Histogram;

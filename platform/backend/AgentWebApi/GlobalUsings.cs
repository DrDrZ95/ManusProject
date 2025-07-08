global using AgentWebApi.Extensions;
global using AgentWebApi.McpTools;
global using AgentWebApi.Services;
global using AgentWebApi.Services.Qwen;
global using AgentWebApi.Services.SemanticKernel;
global using AgentWebApi.Services.VectorDatabase;
global using AgentWebApi.Services.RAG; // 添加RAG服务的全局引用
global using AgentWebApi.Services.Sandbox; // 添加沙盒终端服务的全局引用
global using AgentWebApi.Services.Workflow; // 添加工作流服务的全局引用
global using AgentWebApi.Services.Prompts; // 添加提示词服务的全局引用
global using AgentWebApi.Services.Finetune; // 添加Python微调服务的全局引用
global using AgentWebApi.Identity; // 添加身份验证的全局引用
global using AgentWebApi.Hubs; // 添加SignalR集线器的全局引用
global using AgentWebApi.Gateway; // 添加YARP网关的全局引用
global using AgentWebApi.Data; // 添加数据访问的全局引用
global using AgentWebApi.Data.Repositories; // 添加仓储模式的全局引用
global using ChromaDB.Client;
global using ChromaDB.Client.Models;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.SemanticKernel;
global using ModelContextProtocol.Extensions;



global using System.Diagnostics;


global using AgentWebApi.Services.Hdfs;


global using AgentWebApi.Services.Telemetry;


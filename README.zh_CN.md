# AI-Agent 应用 (集成 .NET Web API)

[![GitHub stars](https://img.shields.io/github/stars/DrDrZ95/ManusProject?style=social)](https://github.com/DrDrZ95/ManusProject/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[English Documentation](README.md)

本代码仓库包含一个全面的 AI Agent 应用程序，它使用 .NET 8.0 Web API 后端和 React 前端构建。它集成了各种先进的 AI 和系统监控功能。

## 项目概述

本项目旨在提供一个健壮且可扩展的解决方案，用于：

1.  **核心 AI 服务**：与 Microsoft Semantic Kernel 集成以进行 LLM 交互，RAG（检索增强生成）以增强上下文，以及向量数据库（ChromaDB）以实现高效数据检索。
2.  **Agent 编排**：工作流管理，用于定义和跟踪复杂的多步骤任务，以及用于安全进程交互的沙盒终端。
3.  **系统可观测性**：基于 eBPF 的侦探模块，用于低级系统监控（CPU、内存、网络、进程活动）。
4.  **数据管理**：PostgreSQL 集成，用于持久存储应用程序数据和微调记录。
5.  **实时通信**：SignalR 用于后端和前端之间的实时交互。
6.  **API 网关**：YARP（Yet Another Reverse Proxy）用于智能路由、负载均衡和熔断。
7.  **认证与授权**：ASP.NET Core Identity with JWT Bearer 用于安全访问控制和基于角色的分发。
8.  **模型微调**：Python.NET 集成，用于管理和跟踪微调作业。
9.  **前端**：基于 React 的用户界面 (`platform/frontend/agent-chat/`)，用于直观交互。

## 代码仓库结构

```
ai-agent/
├── apps/                         # 核心应用组件
│   ├── agent-api/                # .NET 8.0 后端项目
│   │   ├── Agent.Api/            # ASP.NET Core Web API 入口点
│   │   │   ├── Controllers/      # API 端点（重构后保留）
│   │   │   ├── Extensions/       # 扩展方法（重构后保留）
│   │   │   ├── GlobalUsings.cs   # Agent.Api 的全局 using 指令
│   │   │   ├── Program.cs        # 应用程序启动和配置
│   │   │   └── Agent.Api.csproj
│   │   ├── Agent.Core/           # 核心业务逻辑和共享模块
│   │   │   ├── Authorization/    # 自定义授权策略和处理器
│   │   │   ├── Controllers/      # 核心 API 端点（从 Agent.Api 移动）
│   │   │   ├── Data/             # EF Core DbContext、仓储和实体 (PostgreSQL)
│   │   │   ├── eBPF/             # eBPF 侦探模块（服务、控制器、脚本）
│   │   │   ├── Extensions/       # 模块化配置的扩展方法
│   │   │   ├── Gateway/          # YARP 网关和熔断组件
│   │   │   ├── Hubs/             # SignalR Hubs
│   │   │   ├── Identity/         # ASP.NET Core Identity 模型和配置
│   │   ├── Agent.McpGateway/   # MCP 网关实现，简化了命名空间并集成了 GlobalUsings
│   │   │   ├── McpTools/         # 模型上下文协议集成工具
│   │   │   ├── Models/           # 共享数据模型
│   │   │   ├── Services/         # 核心服务实现（Semantic Kernel, RAG, Sandbox, Workflow, Prompts, Finetune, HDFS, FileUpload, Prometheus, Qwen, Telemetry, UserInput, VectorDatabase）
│   │   │   ├── WebSearch/        # Web 搜索模块 (SearXNG, SerpApi)
│   │   │   └── Agent.Core.csproj
│   └── agent-ui/                 # React 前端应用程序
├── infra/                        # 基础设施（部署配置和环境设置）
│   ├── docker/                   # Docker 部署配置
│   │   ├── Dockerfile.webapi     # .NET Web API 的 Dockerfile
│   │   ├── Dockerfile.react      # React UI 的 Dockerfile
│   │   ├── docker-compose.yml    # Docker Compose 配置
│   │   ├── examples/             # Docker Compose 示例配置
│   │   └── nginx.conf            # React UI 的 Nginx 配置
│   ├── envsetup/                 # 环境设置脚本（例如，download_model.sh, install_dependencies.sh）
│   ├── helm/                     # Helm charts 用于部署
│   └── kubernetes/               # 原始 Kubernetes 清单
├── llm/                          # 大型语言模型相关组件
│   ├── deploy/                   # 部署脚本和模型服务器（例如，api_examples.py, model_server.py）
│   └── finetune/                 # 模型微调脚本和工具（例如，install_dependencies.sh, utils.py）
├── test/                         # 单元测试
│   └── Agent.Core.Tests/         # Agent.Core 的单元测试
├── docs/                         # 综合文档
│   ├── chromadb_integration.md
│   ├── ebpf_integration.md
│   ├── identity_signalr_integration.md
│   ├── kubernetes_istio_grayscale_release.zh_CN.md
│   ├── mlflow_integration.md
│   ├── mlflow_integration.zh_CN.md
│   ├── rag_prompt_engineering.md
│   ├── sandbox_terminal_integration.md
│   ├── semantic_kernel_examples.md
│   ├── workflow_integration.md
│   └── yarp_gateway_integration.md
├── README.md                     # 主项目文档（英文）
├── README.zh_CN.md               # 主项目文档（简体中文）
└── .gitignore                    # 指定 Git 应忽略的有意未跟踪文件
```

## 快速开始

### Docker 部署 (推荐)

为了最快速的设置，请使用 Docker 一起部署所有组件：

```bash
cd docker
docker-compose up -d
```

这将构建并启动所有服务。有关详细说明，请参阅 `docs/` 目录中的各个模块文档。

### 手动设置

#### 系统要求

*   **.NET 8.0 SDK**：适用于 `platform/backend/`。
*   **Node.js 和 pnpm**：适用于 `platform/frontend/agent-chat/`。
*   **Python 3.x**：适用于 Python.NET 集成和 `finetune/` 工具。
*   **Linux 环境**：适用于 eBPF 模块（需要 `bpftrace`）。

#### 设置与运行

请参阅具体的 `docs/` 以获取每个模块的详细设置和运行说明：

*   **`platform/backend/`**：请参阅 `docs/semantic_kernel_examples.md`、`docs/rag_prompt_engineering.md` 等。
*   **`platform/frontend/agent-chat/`**：请参阅 `platform/frontend/agent-chat/README.md`（如果存在，否则为标准 React 设置）。
*   **`finetune/`**：请参阅 `docs/python_finetune_integration.md`（如果存在，否则请参阅 `finetune/README.md`）。

## OpenTelemetry 跟踪

`platform/backend/Agent.Api` 项目集成了 OpenTelemetry，用于分布式跟踪，提供对应用程序执行流程的洞察。典型的 Agent 应用程序执行序列的检测如下：

```csharp
// 1. 定义用于跟踪的 ActivitySource
using var activitySource = new ActivitySource("AI-Agent.Application");

// ... 服务配置 ...

var app = builder.Build();

// 2. 启动主应用程序活动 Span
using (var activity = activitySource.StartActivity("AI-Agent.ApplicationStartup"))
{
    // ... 管道配置 ...

    // 3. 模拟典型的 Agent 应用程序执行序列
    using (var agentFlowActivity = activitySource.StartActivity("AI-Agent.ExecutionFlow"))
    {
        agentFlowActivity?.SetTag("flow.description", "用户输入 -> LLM 交互 -> RAG (可选) -> 生成待办事项列表 -> 进程交互");

        // 3.1 用户输入处理
        using (var userInputActivity = activitySource.StartActivity("AI-Agent.UserInputProcessing"))
        {
            // ... 标签 ...
        }

        // 3.2 LLM 交互
        using (var llmInteractionActivity = activitySource.StartActivity("AI-Agent.LLMInteraction"))
        {
            // ... 标签 ...
        }

        // 3.3 RAG (检索增强生成) - 可选
        using (var ragActivity = activitySource.StartActivity("AI-Agent.RAG"))
        {
            // ... 标签 ...
        }

        // 3.4 生成待办事项列表
        using (var todoListActivity = activitySource.StartActivity("AI-Agent.GenerateTodoList"))
        {
            // ... 标签 ...
        }

        // 3.5 进程交互 (例如, 沙盒终端, 工作流执行)
        using (var processInteractionActivity = activitySource.StartActivity("AI-Agent.ProcessInteraction"))
        {
            // ... 标签 ...
        }
    }
}

app.Run();
```

此检测提供了以下 Span：

-   `AI-Agent.ApplicationStartup`：应用程序的整体启动。
-   `AI-Agent.ExecutionFlow`：Agent 操作的主要序列。
-   `AI-Agent.UserInputProcessing`：用户输入处理。
-   `AI-Agent.LLMInteraction`：与大型语言模型的交互。
-   `AI-Agent.RAG`：检索增强生成过程（可选）。
-   `AI-Agent.GenerateTodoList`：任务列表的生成。
-   `AI-Agent.ProcessInteraction`：与外部进程或沙盒环境的交互。

对于 Kubernetes 部署，请考虑使用 OpenTelemetry Collectors 收集跟踪并将其导出到集中式跟踪后端（例如 Jaeger、Zipkin、Prometheus）。

## 详细文档

*   **核心模块**：
    *   [ChromaDB 集成](docs/chromadb_integration.md)
    *   [Semantic Kernel 示例](docs/semantic_kernel_examples.md)
    *   [RAG Prompt 工程](docs/rag_prompt_engineering.md)
    *   [沙盒终端集成](docs/sandbox_terminal_integration.md)
    *   [工作流集成](docs/workflow_integration.md)
    *   [Identity & SignalR 集成](docs/identity_signalr_integration.md)
    *   [YARP 网关集成](docs/yarp_gateway_integration.md)
    *   [eBPF 集成](docs/ebpf_integration.md)
    *   [Kubernetes、Istio 与灰度发布指南](docs/kubernetes_istio_grayscale_release.zh_CN.md)
*   **部署**：
    *   [Docker 快速入门指南](docs/docker_quickstart.md)

## 许可证

项目框架：MIT 许可证。各个组件和模型可能受其自身许可证的约束。

## 致谢

Microsoft .NET 团队、OpenTelemetry 社区、YARP 项目、Polly 项目、SignalR 团队、ChromaDB、Microsoft Semantic Kernel、bpftrace 以及所有贡献的开源项目。



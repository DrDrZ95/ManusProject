# AI-Agent Application with .NET Web API

[![GitHub stars](https://img.shields.io/github/stars/DrDrZ95/ManusProject?style=social)](https://github.com/DrDrZ95/ManusProject/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[中文文档](README.zh_CN.md)

This repository contains a comprehensive AI Agent application built with a .NET 8.0 Web API backend and a React frontend. It integrates various advanced AI and system monitoring capabilities.

## Project Overview

This project aims to provide a robust and extensible solution for:

1.  **Core AI Services**: Integration with Microsoft Semantic Kernel for LLM interactions, RAG (Retrieval Augmented Generation) for enhanced context, and vector databases (ChromaDB) for efficient data retrieval.
2.  **Agent Orchestration**: Workflow management for defining and tracking complex multi-step tasks, and a sandbox terminal for secure process interaction.
3.  **System Observability**: An eBPF-based detective module for low-level system monitoring (CPU, memory, network, process activity).
4.  **Data Management**: PostgreSQL integration for persistent storage of application data and fine-tuning records.
5.  **Real-time Communication**: SignalR for real-time interactions between the backend and frontend.
6.  **API Gateway**: YARP (Yet Another Reverse Proxy) for intelligent routing, load balancing, and circuit breaking.
7.  **Authentication & Authorization**: ASP.NET Core Identity with JWT Bearer for secure access control and role-based distribution.
8.  **Model Fine-tuning**: Python.NET integration for managing and tracking fine-tuning jobs.
9.  **Frontend**: A React-based user interface (`platform/frontend/agent-chat/`) for intuitive interaction.

## Repository Structure

```
ai-agent/
├── apps/                         # Core application components
│   ├── agent-api/                # .NET 8.0 Backend Projects
│   │   ├── Agent.Api/            # ASP.NET Core Web API Entry Point
│   │   │   ├── Controllers/      # API Endpoints (remaining after refactoring)
│   │   │   ├── Extensions/       # Extension Methods (remaining after refactoring)
│   │   │   ├── GlobalUsings.cs   # Global using directives for Agent.Api
│   │   │   ├── Program.cs        # Application startup and configuration
│   │   │   └── Agent.Api.csproj
│   │   ├── Agent.Core/           # Core business logic and shared modules
│   │   │   ├── Authorization/    # Custom authorization policies and handlers
│   │   │   ├── Controllers/      # Core API Endpoints (moved from Agent.Api)
│   │   │   ├── Data/             # EF Core DbContext, Repositories, and Entities (PostgreSQL)
│   │   │   ├── eBPF/             # eBPF Detective Module (Services, Controllers, Scripts)
│   │   │   ├── Extensions/       # Extension Methods for modular configuration
│   │   │   ├── Gateway/          # YARP Gateway and Circuit Breaker components
│   │   │   ├── Hubs/             # SignalR Hubs
│   │   │   ├── Identity/         # ASP.NET Core Identity models and configurations
│   │   │   ├── McpTools/         # Model Context Protocol integration tools
│   │   │   ├── Models/           # Shared data models
│   │   │   ├── Services/         # Core Service Implementations (Semantic Kernel, RAG, Sandbox, Workflow, Prompts, Finetune, HDFS, FileUpload, Prometheus, Qwen, Telemetry, UserInput, VectorDatabase)
│   │   │   ├── WebSearch/        # Web Search module (SearXNG, SerpApi)
│   │   │   └── Agent.Core.csproj
│   └── agent-ui/                 # React Frontend application
├── infra/                        # Infrastructure (deployment configuration and environment setup)
│   ├── docker/                   # Docker deployment configuration
│   │   ├── Dockerfile.webapi     # Dockerfile for .NET Web API
│   │   ├── Dockerfile.react      # Dockerfile for React UI
│   │   ├── docker-compose.yml    # Docker Compose configuration
│   │   ├── examples/             # Example docker-compose configurations
│   │   └── nginx.conf            # Nginx configuration for React UI
│   ├── envsetup/                 # Environment setup scripts (e.g., download_model.sh, install_dependencies.sh)
│   ├── helm/                     # Helm charts for deployment
│   └── kubernetes/               # Raw Kubernetes manifests
├── llm/                          # Large Language Model related components
│   ├── deploy/                   # Deployment scripts and model server (e.g., api_examples.py, model_server.py)
│   └── finetune/                 # Model fine-tuning scripts and utilities (e.g., install_dependencies.sh, utils.py)
├── test/                         # Unit tests
│   └── Agent.Core.Tests/         # Unit tests for Agent.Core
├── docs/                         # Comprehensive Documentation
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
├── README.md                     # Main project documentation (English)
├── README.zh_CN.md               # Main project documentation (Simplified Chinese)
└── .gitignore                    # Specifies intentionally untracked files that Git should ignore
```

## Quick Start

### Docker Deployment (Recommended)

For the fastest setup, use Docker to deploy all components together:

```bash
cd docker
docker-compose up -d
```

This will build and start all services. For detailed instructions, refer to the individual module documentation in the `docs/` directory.

### Manual Setup

#### Prerequisites

*   **.NET 8.0 SDK**: For `platform/backend/`.
*   **Node.js and pnpm**: For `platform/frontend/agent-chat/`.
*   **Python 3.x**: For Python.NET integration and `finetune/` utilities.
*   **Linux Environment**: For eBPF module (`bpftrace` required).

#### Setup & Running

Refer to the specific `docs/` for detailed setup and running instructions for each module:

*   **`platform/backend/`**: See `docs/semantic_kernel_examples.md`, `docs/rag_prompt_engineering.md`, etc.
*   **`platform/frontend/agent-chat/`**: See `platform/frontend/agent-chat/README.md` (if exists, otherwise standard React setup).
*   **`finetune/`**: See `docs/python_finetune_integration.md` (if exists, otherwise refer to `finetune/README.md`).

## OpenTelemetry Tracing

The `platform/backend/Agent.Api` project integrates OpenTelemetry for distributed tracing, providing insights into the application's execution flow. A typical Agent application execution sequence is instrumented as follows:

```csharp
// 1. Define ActivitySource for tracing
using var activitySource = new ActivitySource("AI-Agent.Application");

// ... service configurations ...

var app = builder.Build();

// 2. Start main application activity span
using (var activity = activitySource.StartActivity("AI-Agent.ApplicationStartup"))
{
    // ... pipeline configurations ...

    // 3. Simulate a typical Agent application execution sequence
    using (var agentFlowActivity = activitySource.StartActivity("AI-Agent.ExecutionFlow"))
    {
        agentFlowActivity?.SetTag("flow.description", "User input -> LLM interaction -> RAG (optional) -> Generate to-do list -> Process interaction");

        // 3.1 User Input Processing
        using (var userInputActivity = activitySource.StartActivity("AI-Agent.UserInputProcessing"))
        {
            // ... tags ...
        }

        // 3.2 LLM Interaction
        using (var llmInteractionActivity = activitySource.StartActivity("AI-Agent.LLMInteraction"))
        {
            // ... tags ...
        }

        // 3.3 RAG (Retrieval Augmented Generation) - Optional
        using (var ragActivity = activitySource.StartActivity("AI-Agent.RAG"))
        {
            // ... tags ...
        }

        // 3.4 Generate To-Do List
        using (var todoListActivity = activitySource.StartActivity("AI-Agent.GenerateTodoList"))
        {
            // ... tags ...
        }

        // 3.5 Process Interaction (e.g., Sandbox Terminal, Workflow Execution)
        using (var processInteractionActivity = activitySource.StartActivity("AI-Agent.ProcessInteraction"))
        {
            // ... tags ...
        }
    }
}

app.Run();
```

This instrumentation provides spans for:

-   `AI-Agent.ApplicationStartup`: Overall application startup.
-   `AI-Agent.ExecutionFlow`: The main sequence of agent operations.
-   `AI-Agent.UserInputProcessing`: Handling of user input.
-   `AI-Agent.LLMInteraction`: Interactions with Large Language Models.
-   `AI-Agent.RAG`: Retrieval Augmented Generation process (optional).
-   `AI-Agent.GenerateTodoList`: Generation of task lists.
-   `AI-Agent.ProcessInteraction`: Interactions with external processes or sandbox environments.

For Kubernetes deployments, consider using OpenTelemetry Collectors to gather traces and export them to a centralized tracing backend (e.g., Jaeger, Zipkin, Prometheus).

## Detailed Documentation

*   **Core Modules**:
    *   [ChromaDB Integration](docs/chromadb_integration.md)
    *   [Semantic Kernel Examples](docs/semantic_kernel_examples.md)
    *   [RAG Prompt Engineering](docs/rag_prompt_engineering.md)
    *   [Sandbox Terminal Integration](docs/sandbox_terminal_integration.md)
    *   [Workflow Integration](docs/workflow_integration.md)
    *   [Identity & SignalR Integration](docs/identity_signalr_integration.md)
    *   [YARP Gateway Integration](docs/yarp_gateway_integration.md)
    *   [eBPF Integration](docs/ebpf_integration.md)
    *   [Kubernetes, Istio, and Gray-scale Release Guide](docs/kubernetes_istio_grayscale_release.zh_CN.md)
*   **Deployment**:
    *   [Docker Quick Start Guide](docs/docker_quickstart.md)

## License

Project framework: MIT License. Individual components and models may be subject to their own licenses.

## Acknowledgements

Microsoft .NET Team, OpenTelemetry Community, YARP Project, Polly Project, SignalR Team, ChromaDB, Microsoft Semantic Kernel, bpftrace, and all contributing open-source projects.



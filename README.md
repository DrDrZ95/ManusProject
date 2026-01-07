# ManusProject - AI-Powered Intelligent Agent Platform

> A professional-grade AI agent framework built with .NET 8.0 and React, designed for autonomous task execution and intelligent workflow automation.
> All files and solution logic are generated from Manus. reference: https://manus.im/

### 📢 Author's Message

This project is continuously being optimized, and the author strives for 3+ updates and optimizations per week.

**C# never lost online, never won in reality. Man! what can i say?** 🚀

---

[中文版本](./README.zh_CN.md)

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [System Architecture](#system-architecture)
- [System Requirements](#system-requirements)
- [Project Structure](#project-structure)
- [Quick Start](#quick-start)
- [Deployment Guide](#deployment-guide)
- [Core Modules](#core-modules)
- [Documentation](#documentation)
- [Development Guide](#development-guide)
- [Contributing Guide](#contributing-guide)
- [License](#license)

---

## 🎯 Project Overview

ManusProject is an enterprise-grade AI agent framework that combines cutting-edge Large Language Model (LLM) technologies with robust backend infrastructure and intuitive frontend interfaces. The platform enables autonomous task execution through intelligent workflow management, Retrieval-Augmented Generation (RAG), and sandbox-based task processing.

### Key Highlights

- **🤖 Multi-Model Support**: Integration with OpenAI, Azure OpenAI, and Alibaba Qwen
- **🏗️ Distributed Architecture**: Native support for Kubernetes and Docker, inherently scalable
- **🔒 Advanced Security**: eBPF system monitoring, ASP.NET Core Identity integration, fine-grained access control
- **⚡ Real-time Communication**: SignalR enables instant updates and push notifications
- **📊 Enterprise-Ready**: Comprehensive logging, distributed tracing, and observability

---

## ✨ Key Features

### 🤖 AI & LLM Capabilities
- **Semantic Kernel Integration** - Unified LLM abstraction layer supporting multiple model providers
- **Retrieval-Augmented Generation (RAG)** - Intelligent knowledge base with ChromaDB and custom vector store integration
- **Advanced Prompt Engineering** - Prompt system with dynamic variable substitution and template management
- **Model Fine-tuning Tools** - Complete scripts and utilities for custom model adaptation

### ⚙️ Workflow & Automation
- **Intelligent Workflow Engine** - Orchestration and execution of complex multi-step tasks
- **Sandbox Terminal Integration** - Secure isolated command execution environment preventing malicious operations
- **Dynamic Task Planning** - AI-driven automatic to-do list generation and task decomposition
- **Flexible Interaction Handling** - Support for interaction patterns across diverse task types

### 🔐 System & Security
- **eBPF Detection Module** - Low-level system monitoring and security threat analysis
- **Identity & Authorization** - Complete ASP.NET Core Identity implementation
- **Custom Policy Engine** - Fine-grained role and permission management
- **Web Search Integration** - Real-time information retrieval via SearXNG and SerpApi

### 📈 Observability & Operations
- **Distributed Tracing** - OpenTelemetry integration for end-to-end request visualization
- **Prometheus Metrics** - Comprehensive application and system health metrics
- **MLflow Experiment Management** - Model training and experiment tracking
- **Structured Logging** - Correlation IDs and contextual logging across the call stack

### 🚀 Infrastructure & Deployment
- **Docker Containerization** - Complete Docker Compose multi-container orchestration solution
- **Kubernetes Support** - Helm charts and raw manifests for cloud deployment
- **YARP Reverse Proxy** - Intelligent gateway with circuit breaker pattern support
- **High Availability Design** - Load balancing and failover mechanisms

---

## 🛠 Technology Stack

### 📱 Backend Technologies
| Component | Version | Purpose |
|-----------|---------|---------|
| .NET | 8.0+ | Modern high-performance web framework |
| ASP.NET Core | 8.0+ | Web API and real-time communication |
| Entity Framework Core | 8.0+ | PostgreSQL ORM mapping |
| SignalR | 8.0+ | Real-time bidirectional communication |
| OpenTelemetry | Latest | Observability and distributed tracing |
| Semantic Kernel | Latest | LLM abstraction and orchestration |
| YARP | Latest | Reverse proxy and gateway |

### 🎨 Frontend Technologies
| Component | Version | Purpose |
|-----------|---------|---------|
| React | 18.0+ | Modern UI framework |
| TypeScript | 5.0+ | Type-safe JavaScript development |
| SignalR Client | 8.0+ | Real-time notification client |
| Notion UI | Custom | Notion-style design system |

### 💾 Data & Storage
| Component | Purpose |
|-----------|---------|
| PostgreSQL 12+ | Primary relational database for metadata storage |
| ChromaDB | Vector database supporting RAG functionality |
| Redis (Optional) | Caching layer for improved query performance |

### 🐳 Containerization & Orchestration
| Component | Purpose |
|-----------|---------|
| Docker | Application and service containerization |
| Docker Compose | Local development multi-container orchestration |
| Kubernetes 1.21+ | Production cloud deployment |
| Helm 3.0+ | Kubernetes package management and templating |

### 📊 Monitoring & Operations
| Component | Purpose |
|-----------|---------|
| Prometheus | Metrics collection and storage |
| Grafana (Optional) | Metrics visualization dashboard |
| MLflow | Machine learning experiment tracking |
| Elasticsearch (Optional) | Log indexing and search |

### 🔗 Integration & Extensions
| Component | Purpose |
|-----------|---------|
| Model Context Protocol (MCP) | Standardized tool integration framework |
| Nginx | Web server and load balancing |
| SearXNG / SerpApi | Web search integration |

---

## 🏗 System Architecture

### Layered Architecture Design (Layered Architecture Pattern)

```
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃                      Layer 1: Presentation                          ┃
┃  ┌─────────────────────────────────────────────────────────────┐   ┃
┃  │  React 18+ Application Interface                            │   ┃
┃  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────┐   │   ┃
┃  │  │  Dashboard       │  │  Workflow        │  │  Task    │   │   ┃
┃  │  │  - Analytics     │  │  - Editor        │  │  - Board │   │   ┃
┃  │  │  - Overview      │  │  - Visualizer    │  │  - Cards │   │   ┃
┃  │  └──────────────────┘  └──────────────────┘  └──────────┘   │   ┃
┃  │                                                               │   ┃
┃  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────┐   │   ┃
┃  │  │  AI Chat         │  │  Settings        │  │  Search  │   │   ┃
┃  │  │  - LLM Prompt    │  │  - Profile       │  │  - Query │   │   ┃
┃  │  │  - Response      │  │  - Preferences   │  │  - Filter│   │   ┃
┃  │  └──────────────────┘  └──────────────────┘  └──────────┘   │   ┃
┃  └─────────────────────────────────────────────────────────────┘   ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                                  ↕
                      HTTP/HTTPS + WebSocket
                                  ↕
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃                    API Gateway Layer                                ┃
┃              (Nginx / YARP - Load Balancing)                        ┃
└─────────────────────────────┬───────────────────────────────┘
                              │
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━┻━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃         Business Logic Layer (Application Layer)              ┃
┃                    ASP.NET Core Backend                       ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.Api (Startup & Configuration)                 │   ┃
┃  │  ├─ Program.cs: Application Entry Point & DI Setup   │   ┃
┃  │  ├─ GlobalUsings.cs: Global Namespace Declarations   │   ┃
┃  │  ├─ Controllers/*: API Route Endpoints               │   ┃
┃  │  └─ Extensions/*: Modular Extension Configuration    │   ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.Core (Core Business Logic)                    │   ┃
┃  │  ├─ Authorization/          - Auth Policies & Handlers│  ┃
┃  │  ├─ Controllers/            - API Endpoint Impl       │  ┃
┃  │  ├─ Data/                   - EF Core DbContext       │  ┃
┃  │  ├─ eBPF/                   - System Detection Module │  ┃
┃  │  ├─ Extensions/             - Modular Config Ext      │  ┃
┃  │  ├─ Gateway/                - YARP Gateway Config     │  ┃
┃  │  ├─ Hubs/                   - SignalR Real-time Hub   │  ┃
┃  │  └─ Identity/               - ASP.NET Core Identity   │  ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.McpGateway (AI Orchestration Engine)          │   ┃
┃  │  ├─ SemanticKernelService     - LLM Integration      │   ┃
┃  │  ├─ RAGService                - RAG Implementation    │   ┃
┃  │  ├─ WorkflowService           - Workflow Orchestr     │   ┃
┃  │  ├─ SandboxService            - Isolated Execution    │   ┃
┃  │  ├─ PromptService             - Prompt Management     │   ┃
┃  │  ├─ WebSearchService          - Web Search Integration│  ┃
┃  │  ├─ VectorDatabaseService     - Vector Operations     │   ┃
┃  │  ├─ FinetuneService           - Model Fine-tuning     │   ┃
┃  │  ├─ TelemetryService          - OpenTelemetry        │   ┃
┃  │  ├─ UserInputService          - User Input Processing │   ┃
┃  │  └─ FileUploadService         - File Management       │   ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                                  ↕
          ┌───────────────────────┼───────────────────┐
          │                       │                   │
    ┌─────▼──────┐        ┌──────▼─────┐    ┌─────▼──────┐
    │ PostgreSQL │        │  ChromaDB   │    │ External   │
    │ (Metadata) │        │  (Vectors)  │    │ Services   │
    │            │        │             │    │            │
    │ - Users    │        │ - Embeddings│    │ - OpenAI   │
    │ - Workflows│        │ - Documents │    │ - Azure AI  │
    │ - Tasks    │        │ - RAG Index │    │ - Qwen     │
    │ - Config   │        │             │    │ - SearXNG  │
    └────────────┘        └─────────────┘    └────────────┘
```

### Architecture Characteristics

#### 📌 Separation of Concerns
- Single responsibility per layer, clear boundaries
- Dependencies flow from top to bottom
- Each layer independently testable and maintainable

#### 🔄 Data Flow
1. **Request Flow**: Presentation → Gateway → Business → Data → Storage
2. **Response Flow**: Storage → Data → Business → Gateway → Presentation
3. **Notification Flow**: SignalR Hub → Gateway → Presentation (Real-time Push)

#### 🛡️ Cross-Cutting Concerns
- **Authentication/Authorization**: Enforced at gateway and business layers
- **Logging/Tracing**: Collected across all layers
- **Error Handling**: Unified handling at gateway layer
- **Caching**: Implemented at data layer

---

## 💻 System Requirements

### Minimum Hardware Configuration
```
CPU:     4 cores (8+ cores recommended)
Memory:  8 GB (16+ GB recommended)
Storage: 50 GB SSD (100+ GB recommended)
Network: Stable 1Mbps+ network connection
```

### Software Dependencies

#### Required Components
```
.NET SDK         8.0.0 or higher
Node.js          16.x or higher
npm / yarn       7.0+ or 1.22.x+
Docker           20.10+ (for containerized deployment)
Docker Compose   1.29+ (for container orchestration)
PostgreSQL       12.x or higher
Git              2.20+
```

#### Optional Components
```
Kubernetes       1.21+ (for cloud deployment)
Helm             3.0+ (for Kubernetes package management)
NVIDIA CUDA      11.0+ (for GPU acceleration)
Prometheus       Latest (for monitoring)
MLflow           Latest (for experiment tracking)
Elasticsearch    7.0+ (for log analysis)
Redis            6.0+ (for caching)
```

### Network Requirements
```
- Network connectivity to Azure/OpenAI services
- Firewall rules allowing:
  * Port 3000 (Frontend)
  * Port 5000 (Backend API)
  * Port 5432 (PostgreSQL)
  * Port 8000 (ChromaDB)
  * Port 9090 (Prometheus)
```

---

## 📁 Project Structure

### Complete Project Tree Structure

```
ManusProject/                                 # 📦 Project Root
│
├─ README.md                                  # 📄 English Documentation
├─ README.zh_CN.md                            # 📄 Chinese Documentation
├─ .gitignore                                 # 🚫 Git Ignore Rules
├─ LICENSE                                    # 📜 MIT License
├─ .editorconfig                              # 🎯 Editor Configuration
│
├─ apps/                                      # 💼 Application Code
│  │
│  ├─ agent-api/                              # 🔧 Backend API Service
│  │  │
│  │  ├─ Agent.Api/                           # 🎯 Startup & Configuration
│  │  │  ├─ Controllers/
│  │  │  │  ├─ HealthCheckController.cs       # Health Check
│  │  │  │  ├─ WorkflowController.cs          # Workflow Management
│  │  │  │  ├─ TaskController.cs              # Task Management
│  │  │  │  ├─ AIController.cs                # AI/LLM Operations
│  │  │  │  ├─ SearchController.cs            # Search Features
│  │  │  │  └─ AdminController.cs             # Admin Functions
│  │  │  │
│  │  │  ├─ Extensions/
│  │  │  │  ├─ ServiceCollectionExtensions.cs # DI Extensions
│  │  │  │  ├─ AuthenticationExtensions.cs    # Auth Config
│  │  │  │  ├─ CorsExtensions.cs              # CORS Config
│  │  │  │  └─ MiddlewareExtensions.cs        # Middleware Extensions
│  │  │  │
│  │  │  ├─ GlobalUsings.cs                   # Global Usings
│  │  │  ├─ Program.cs                        # Application Entry Point
│  │  │  ├─ Agent.Api.csproj                  # Project File
│  │  │  ├─ appsettings.json                  # Default Config
│  │  │  ├─ appsettings.Development.json      # Development Config
│  │  │  └─ appsettings.Production.json       # Production Config
│  │  │
│  │  ├─ Agent.Core/                          # 💎 Core Business Logic
│  │  │  │
│  │  │  ├─ Authorization/                    # 🔐 Authorization Module
│  │  │  │  ├─ Handlers/
│  │  │  │  │  ├─ RoleBasedHandler.cs         # Role-based Auth
│  │  │  │  │  ├─ PolicyBasedHandler.cs       # Policy-based Auth
│  │  │  │  │  └─ ClaimBasedHandler.cs        # Claim-based Auth
│  │  │  │  ├─ Policies/
│  │  │  │  │  ├─ AdminOnlyPolicy.cs
│  │  │  │  │  ├─ WorkflowOwnerPolicy.cs
│  │  │  │  │  └─ RateLimitPolicy.cs
│  │  │  │  └─ Requirements/
│  │  │  │     └─ CustomRequirement.cs
│  │  │  │
│  │  │  ├─ Controllers/                      # 📍 API Endpoints
│  │  │  │  ├─ BaseController.cs              # Base Class
│  │  │  │  ├─ WorkflowController.cs
│  │  │  │  ├─ TaskController.cs
│  │  │  │  ├─ AIController.cs
│  │  │  │  ├─ RAGController.cs
│  │  │  │  ├─ SearchController.cs
│  │  │  │  └─ AdminController.cs
│  │  │  │
│  │  │  ├─ Data/                             # 💾 Data Access Layer
│  │  │  │  ├─ Contexts/
│  │  │  │  │  ├─ ApplicationDbContext.cs     # Main DbContext
│  │  │  │  │  └─ SeedData.cs                 # Initial Data
│  │  │  │  │
│  │  │  │  ├─ Entities/                      # 📦 Data Entities
│  │  │  │  │  ├─ User.cs
│  │  │  │  │  ├─ Workflow.cs
│  │  │  │  │  ├─ Task.cs
│  │  │  │  │  ├─ Document.cs
│  │  │  │  │  ├─ AuditLog.cs
│  │  │  │  │  └─ Attachment.cs
│  │  │  │  │
│  │  │  │  ├─ Repositories/                  # 🗂️ Repository Implementation
│  │  │  │  │  ├─ IRepository.cs              # Generic Interface
│  │  │  │  │  ├─ Repository.cs               # Base Implementation
│  │  │  │  │  ├─ WorkflowRepository.cs
│  │  │  │  │  ├─ TaskRepository.cs
│  │  │  │  │  ├─ DocumentRepository.cs
│  │  │  │  │  └─ UserRepository.cs
│  │  │  │  │
│  │  │  │  ├─ Migrations/                    # 📝 EF Core Migrations
│  │  │  │  │  ├─ 20260107_InitialCreate.cs
│  │  │  │  │  ├─ 20260110_AddWorkflowTables.cs
│  │  │  │  │  └─ ApplicationDbContextModelSnapshot.cs
│  │  │  │  │
│  │  │  │  └─ Seeds/                         # 🌱 Data Seeds
│  │  │  │     └─ DataSeeder.cs
│  │  │  │
│  │  │  ├─ eBPF/                             # 🔍 eBPF Monitoring
│  │  │  │  ├─ Services/
│  │  │  │  │  ├─ IEbpfService.cs
│  │  │  │  │  ├─ EbpfService.cs
│  │  │  │  │  ├─ ProcessMonitor.cs
│  │  │  │  │  └─ SecurityDetector.cs
│  │  │  │  │
│  │  │  │  ├─ Controllers/
│  │  │  │  │  └─ EbpfController.cs
│  │  │  │  │
│  │  │  │  └─ Scripts/
│  │  │  │     ├─ process_monitor.c
│  │  │  │     ├─ network_monitor.c
│  │  │  │     └─ security_check.c
│  │  │  │
│  │  │  ├─ Extensions/                       # 🔧 Module Extensions
│  │  │  │  ├─ ServiceCollectionExtensions.cs
│  │  │  │  ├─ AuthorizationExtensions.cs
│  │  │  │  ├─ DataAccessExtensions.cs
│  │  │  │  └─ TelemetryExtensions.cs
│  │  │  │
│  │  │  ├─ Gateway/                          # 🚪 Gateway Configuration
│  │  │  │  ├─ Services/
│  │  │  │  │  ├─ IRouteService.cs
│  │  │  │  │  └─ RouteService.cs
│  │  │  │  │
│  │  │  │  └─ Configuration/
│  │  │  │     └─ YarpConfiguration.cs
│  │  │  │
│  │  │  ├─ Hubs/                             # 💬 SignalR Hubs
│  │  │  │  ├─ IHubClient.cs
│  │  │  │  ├─ WorkflowHub.cs
│  │  │  │  ├─ TaskHub.cs
│  │  │  │  └─ NotificationHub.cs
│  │  │  │
│  │  │  ├─ Identity/                         # 👤 Identity Management
│  │  │  │  ├─ ApplicationUser.cs
│  │  │  │  ├─ ApplicationRole.cs
│  │  │  │  ├─ IdentityService.cs
│  │  │  │  ├─ TokenService.cs
│  │  │  │  └─ JwtOptions.cs
│  │  │  │
│  │  │  ├─ Exceptions/                       # ⚠️ Exception Handling
│  │  │  │  ├─ ApplicationException.cs
│  │  │  │  ├─ BusinessException.cs
│  │  │  │  ├─ ValidationException.cs
│  │  │  │  ├─ UnauthorizedException.cs
│  │  │  │  └─ ResourceNotFoundException.cs
│  │  │  │
│  │  │  ├─ Middlewares/                      # 🔌 Custom Middlewares
│  │  │  │  ├─ ErrorHandlingMiddleware.cs
│  │  │  │  ├─ LoggingMiddleware.cs
│  │  │  │  ├─ CorrelationIdMiddleware.cs
│  │  │  │  └─ RequestTimingMiddleware.cs
│  │  │  │
│  │  │  ├─ Models/                           # 📦 DTO Models
│  │  │  │  ├─ Requests/
│  │  │  │  │  ├─ CreateWorkflowRequest.cs
│  │  │  │  │  ├─ CreateTaskRequest.cs
│  │  │  │  │  └─ UpdateWorkflowRequest.cs
│  │  │  │  │
│  │  │  │  ├─ Responses/
│  │  │  │  │  ├─ WorkflowResponse.cs
│  │  │  │  │  ├─ TaskResponse.cs
│  │  │  │  │  └─ ApiResponse.cs
│  │  │  │  │
│  │  │  │  └─ Constants/
│  │  │  │     ├─ ErrorCodes.cs
│  │  │  │     └─ MessageConstants.cs
│  │  │  │
│  │  │  ├─ Services/                         # 🛠️ Business Services
│  │  │  │  ├─ IWorkflowService.cs
│  │  │  │  ├─ WorkflowService.cs
│  │  │  │  ├─ ITaskService.cs
│  │  │  │  ├─ TaskService.cs
│  │  │  │  ├─ INotificationService.cs
│  │  │  │  ├─ NotificationService.cs
│  │  │  │  └─ ICacheService.cs
│  │  │  │
│  │  │  ├─ Agent.Core.csproj                 # Project File
│  │  │  └─ GlobalUsings.cs                   # Global Usings
│  │  │
│  │  └─ Agent.McpGateway/                    # 🤖 AI Orchestration Engine
│  │     │
│  │     ├─ Services/                         # 🛠️ AI Core Services
│  │     │  │
│  │     │  ├─ SemanticKernelService/
│  │     │  │  ├─ ISemanticKernelService.cs
│  │     │  │  ├─ SemanticKernelService.cs
│  │     │  │  ├─ PluginManager.cs
│  │     │  │  ├─ PromptOptimizer.cs
│  │     │  │  └─ ModelRouter.cs
│  │     │  │
│  │     │  ├─ RAGService/
│  │     │  │  ├─ IRagService.cs
│  │     │  │  ├─ RagService.cs
│  │     │  │  ├─ DocumentProcessor.cs
│  │     │  │  ├─ EmbeddingGenerator.cs
│  │     │  │  ├─ SimilaritySearcher.cs
│  │     │  │  └─ ChunkingStrategy.cs
│  │     │  │
│  │     │  ├─ WorkflowService/
│  │     │  │  ├─ IWorkflowService.cs
│  │     │  │  ├─ WorkflowService.cs
│  │     │  │  ├─ WorkflowExecutor.cs
│  │     │  │  ├─ WorkflowParser.cs
│  │     │  │  ├─ StateManager.cs
│  │     │  │  └─ ExecutionContext.cs
│  │     │  │
│  │     │  ├─ SandboxService/
│  │     │  │  ├─ ISandboxService.cs
│  │     │  │  ├─ SandboxService.cs
│  │     │  │  ├─ ProcessExecutor.cs
│  │     │  │  ├─ EnvironmentManager.cs
│  │     │  │  └─ SecurityManager.cs
│  │     │  │
│  │     │  ├─ PromptService/
│  │     │  │  ├─ IPromptService.cs
│  │     │  │  ├─ PromptService.cs
│  │     │  │  ├─ TemplateEngine.cs
│  │     │  │  ├─ VariableResolver.cs
│  │     │  │  └─ PromptCache.cs
│  │     │  │
│  │     │  ├─ WebSearchService/
│  │     │  │  ├─ IWebSearchService.cs
│  │     │  │  ├─ WebSearchService.cs
│  │     │  │  ├─ SearXngClient.cs
│  │     │  │  ├─ SerpApiClient.cs
│  │     │  │  └─ SearchResultProcessor.cs
│  │     │  │
│  │     │  ├─ VectorDatabaseService/
│  │     │  │  ├─ IVectorDatabaseService.cs
│  │     │  │  ├─ ChromaDBClient.cs
│  │     │  │  ├─ EmbeddingCache.cs
│  │     │  │  ├─ IndexManager.cs
│  │     │  │  └─ VectorQueryBuilder.cs
│  │     │  │
│  │     │  ├─ FinetuneService/
│  │     │  │  ├─ IFinetuneService.cs
│  │     │  │  ├─ FinetuneService.cs
│  │     │  │  ├─ DatasetPreparer.cs
│  │     │  │  ├─ ModelTrainer.cs
│  │     │  │  └─ MetricsCalculator.cs
│  │     │  │
│  │     │  ├─ TelemetryService/
│  │     │  │  ├─ ITelemetryService.cs
│  │     │  │  ├─ OpenTelemetryService.cs
│  │     │  │  ├─ MetricsCollector.cs
│  │     │  │  ├─ TraceExporter.cs
│  │     │  │  └─ HealthChecker.cs
│  │     │  │
│  │     │  ├─ UserInputService/
│  │     │  │  ├─ IUserInputService.cs
│  │     │  │  ├─ UserInputService.cs
│  │     │  │  ├─ InputValidator.cs
│  │     │  │  ├─ ContextAnalyzer.cs
│  │     │  │  └─ IntentClassifier.cs
│  │     │  │
│  │     │  └─ FileUploadService/
│  │     │     ├─ IFileUploadService.cs
│  │     │     ├─ FileUploadService.cs
│  │     │     ├─ FileValidator.cs
│  │     │     ├─ StorageManager.cs
│  │     │     └─ VirusScanner.cs
│  │     │
│  │     ├─ McpTools/                         # 🔗 MCP Tools
│  │     │  ├─ IMcpTool.cs
│  │     │  ├─ MusicTool.cs
│  │     │  ├─ WeatherTool.cs
│  │     │  ├─ CustomTool.cs
│  │     │  └─ ToolRegistry.cs
│  │     │
│  │     ├─ Models/                           # 📦 Data Models
│  │     │  ├─ WorkflowModel.cs
│  │     │  ├─ TaskModel.cs
│  │     │  ├─ RAGQuery.cs
│  │     │  ├─ SearchResult.cs
│  │     │  ├─ ExecutionResult.cs
│  │     │  └─ EmbeddingModel.cs
│  │     │
│  │     ├─ WebSearch/                        # 🔍 Search Module
│  │     │  ├─ Interfaces/
│  │     │  │  └─ ISearchProvider.cs
│  │     │  ├─ Providers/
│  │     │  │  ├─ SearXngProvider.cs
│  │     │  │  └─ SerpApiProvider.cs
│  │     │  └─ Models/
│  │     │     └─ SearchResult.cs
│  │     │
│  │     ├─ Agent.McpGateway.csproj           # Project File
│  │     └─ GlobalUsings.cs
│  │
│  └─ agent-ui/                               # 🎨 React Frontend
│     │
│     ├─ public/                              # 📊 Static Assets
│     │  ├─ index.html
│     │  ├─ favicon.ico
│     │  └─ manifest.json
│     │
│     ├─ src/                                 # 💻 Source Code
│     │  ├─ index.tsx                         # Application Entry
│     │  ├─ App.tsx                           # Root Component
│     │  ├─ App.css                           # Global Styles
│     │  │
│     │  ├─ components/                       # ⚛️ React Components
│     │  │  ├─ Layout/
│     │  │  │  ├─ Header.tsx
│     │  │  │  ├─ Sidebar.tsx
│     │  │  │  ├─ Footer.tsx
│     │  │  │  └─ LayoutWrapper.tsx
│     │  │  │
│     │  │  ├─ Workflow/
│     │  │  │  ├─ WorkflowList.tsx
│     │  │  │  ├─ WorkflowEditor.tsx
│     │  │  │  ├─ WorkflowViewer.tsx
│     │  │  │  └─ WorkflowNode.tsx
│     │  │  │
│     │  │  ├─ Task/
│     │  │  │  ├─ TaskBoard.tsx
│     │  │  │  ├─ TaskCard.tsx
│     │  │  │  ├─ TaskModal.tsx
│     │  │  │  └─ TaskForm.tsx
│     │  │  │
│     │  │  ├─ AI/
│     │  │  │  ├─ ChatBox.tsx
│     │  │  │  ├─ PromptEditor.tsx
│     │  │  │  ├─ ResultDisplay.tsx
│     │  │  │  └─ ModelSelector.tsx
│     │  │  │
│     │  │  └─ Common/
│     │  │     ├─ Button.tsx
│     │  │     ├─ Modal.tsx
│     │  │     ├─ Notification.tsx
│     │  │     ├─ Loading.tsx
│     │  │     └─ ErrorBoundary.tsx
│     │  │
│     │  ├─ pages/                           # 📄 Pages
│     │  │  ├─ Dashboard.tsx
│     │  │  ├─ Workflows.tsx
│     │  │  ├─ Tasks.tsx
│     │  │  ├─ AIChat.tsx
│     │  │  ├─ Settings.tsx
│     │  │  ├─ Search.tsx
│     │  │  ├─ NotFound.tsx
│     │  │  └─ Unauthorized.tsx
│     │  │
│     │  ├─ services/                        # 🔌 API Services
│     │  │  ├─ api.ts
│     │  │  ├─ workflowApi.ts
│     │  │  ├─ taskApi.ts
│     │  │  ├─ aiApi.ts
│     │  │  ├─ searchApi.ts
│     │  │  └─ authApi.ts
│     │  │
│     │  ├─ hooks/                           # 🎣 Custom Hooks
│     │  │  ├─ useWorkflows.ts
│     │  │  ├─ useTasks.ts
│     │  │  ├─ useAuth.ts
│     │  │  ├─ useSignalR.ts
│     │  │  ├─ useNotification.ts
│     │  │  └─ useLocalStorage.ts
│     │  │
│     │  ├─ store/                           # 📦 State Management
│     │  │  ├─ slices/
│     │  │  │  ├─ workflowSlice.ts
│     │  │  │  ├─ taskSlice.ts
│     │  │  │  ├─ authSlice.ts
│     │  │  │  ├─ uiSlice.ts
│     │  │  │  └─ notificationSlice.ts
│     │  │  └─ store.ts
│     │  │
│     │  ├─ styles/                          # 🎨 Styles
│     │  │  ├─ variables.css
│     │  │  ├─ themes.css
│     │  │  ├─ notion-ui.css
│     │  │  ├─ responsive.css
│     │  │  └─ animations.css
│     │  │
│     │  ├─ utils/                           # 🔧 Utilities
│     │  │  ├─ formatters.ts
│     │  │  ├─ validators.ts
│     │  │  ├─ constants.ts
│     │  │  ├─ storage.ts
│     │  │  └─ logger.ts
│     │  │
│     │  └─ types/                           # 📝 Type Definitions
│     │     ├─ workflow.ts
│     │     ├─ task.ts
│     │     ├─ api.ts
│     │     ├─ user.ts
│     │     └─ index.ts
│     │
│     ├─ package.json                        # 📋 Dependencies
│     ├─ tsconfig.json                       # ⚙️ TS Config
│     ├─ vite.config.ts                      # 🔨 Build Config
│     ├─ .env.example                        # 🔑 Environment Variables
│     └─ .eslintrc.json                      # 📏 Lint Config
│
├─ infra/                                    # 🏗️ Infrastructure
│  │
│  ├─ docker/                                # 🐳 Docker
│  │  ├─ Dockerfile.webapi                   # Backend Image
│  │  ├─ Dockerfile.react                    # Frontend Image
│  │  ├─ docker-compose.yml                  # Orchestration Config
│  │  ├─ docker-compose.dev.yml              # Dev Config
│  │  ├─ docker-compose.prod.yml             # Prod Config
│  │  ├─ nginx.conf                          # Proxy Config
│  │  ├─ nginx.ssl.conf                      # SSL Config
│  │  ├─ .dockerignore                       # Docker Ignore
│  │  └─ examples/
│  │     ├─ docker-compose.dev.yml
│  │     └─ docker-compose.prod.yml
│  │
│  ├─ kubernetes/                            # ☸️ K8s Manifests
│  │  ├─ namespace.yaml
│  │  ├─ configmap.yaml
│  │  ├─ secrets.yaml
│  │  ├─ deployments.yaml
│  │  ├─ services.yaml
│  │  ├─ ingress.yaml
│  │  ├─ persistentvolumes.yaml
│  │  ├─ hpa.yaml
│  │  └─ rbac.yaml
│  │
│  ├─ helm/                                  # 📦 Helm Charts
│  │  └─ manus-project/
│  │     ├─ Chart.yaml
│  │     ├─ values.yaml
│  │     ├─ values.dev.yaml
│  │     ├─ values.prod.yaml
│  │     ├─ values.staging.yaml
│  │     └─ templates/
│  │        ├─ deployment.yaml
│  │        ├─ service.yaml
│  │        ├─ ingress.yaml
│  │        ├─ configmap.yaml
│  │        ├─ secrets.yaml
│  │        └─ hpa.yaml
│  │
│  └─ envsetup/                              # 🔧 Environment Scripts
│     ├─ install_dependencies.sh
│     ├─ download_model.sh
│     ├─ setup_database.sh
│     ├─ configure_ssl.sh
│     ├─ health_check.sh
│     └─ monitoring_setup.sh
│
├─ llm/                                      # 🤖 ML Components
│  │
│  ├─ deploy/                                # 🚀 Deployment
│  │  ├─ model_server.py
│  │  ├─ api_examples.py
│  │  ├─ requirements.txt
│  │  └─ Dockerfile
│  │
│  └─ finetune/                              # 🎓 Fine-tuning
│     ├─ train.py
│     ├─ evaluate.py
│     ├─ dataset_loader.py
│     ├─ utils.py
│     ├─ install_dependencies.sh
│     └─ config.yaml
│
├─ test/                                     # 🧪 Tests
│  │
│  └─ Agent.Core.Tests/
│     ├─ Unit/
│     │  ├─ Services/
│     │  │  ├─ WorkflowServiceTests.cs
│     │  │  ├─ RAGServiceTests.cs
│     │  │  └─ PromptServiceTests.cs
│     │  ├─ Controllers/
│     │  │  ├─ WorkflowControllerTests.cs
│     │  │  └─ TaskControllerTests.cs
│     │  └─ Repositories/
│     │     └─ WorkflowRepositoryTests.cs
│     │
│     ├─ Integration/
│     │  ├─ ApiIntegrationTests.cs
│     │  ├─ DatabaseIntegrationTests.cs
│     │  └─ WorkflowIntegrationTests.cs
│     │
│     ├─ MockData/
│     │  ├─ TestDataFactory.cs
│     │  └─ MockServices.cs
│     │
│     └─ Agent.Core.Tests.csproj
│
├─ docs/                                     # 📚 Documentation
│  │
│  ├─ README.md
│  ├─ Architecture/
│  │  ├─ system-architecture.md
│  │  ├─ components-overview.md
│  │  └─ data-flow.md
│  │
│  ├─ Setup/
│  │  ├─ docker-deployment.md
│  │  ├─ kubernetes-deployment.md
│  │  ├─ helm-deployment.md
│  │  └─ configuration-guide.md
│  │
│  ├─ Features/
│  │  ├─ chromadb_integration.md
│  │  ├─ ebpf_integration.md
│  │  ├─ identity_signalr_integration.md
│  │  ├─ mlflow_integration.md
│  │  ├─ rag_prompt_engineering.md
│  │  ├─ sandbox_terminal_integration.md
│  │  ├─ semantic_kernel_examples.md
│  │  ├─ workflow_integration.md
│  │  └─ yarp_gateway_integration.md
│  │
│  ├─ API/
│  │  ├─ workflow-api.md
│  │  ├─ task-api.md
│  │  ├─ ai-api.md
│  │  └─ search-api.md
│  │
│  ├─ Development/
│  │  ├─ getting-started.md
│  │  ├─ development-setup.md
│  │  ├─ code-structure.md
│  │  ├─ coding-standards.md
│  │  └─ testing-guide.md
│  │
│  └─ CHANGELOG.md
│
├─ .github/                                  # 🔄 CI/CD
│  ├─ workflows/
│  │  ├─ build.yml
│  │  ├─ test.yml
│  │  ├─ docker-build.yml
│  │  └─ deploy.yml
│  │
│  ├─ ISSUE_TEMPLATE/
│  │  ├─ bug_report.md
│  │  ├─ feature_request.md
│  │  └─ documentation.md
│  │
│  └─ PULL_REQUEST_TEMPLATE.md
│
└─ LICENSE                                   # 📜 MIT License
```

### Project Structure Key Optimization Points

#### 📌 **Modularization Optimization**
- ✅ Clear layered design with low coupling, high cohesion
- ✅ Single responsibility per module
- ✅ Complete dependency injection support

#### 🔄 **Service Decoupling**
- ✅ Interface-Implementation separation (IService + Service)
- ✅ Factory pattern supporting multiple implementations
- ✅ Async/concurrent operation optimization

#### 💾 **Data Access Improvements**
- ✅ Generic repository base class reduces duplication
- ✅ Standardized query building
- ✅ Integrated caching strategy

#### 🔐 **Security Enhancements**
- ✅ Multi-layer authorization mechanism
- ✅ Standardized input validation
- ✅ Complete audit logging

---

## 🚀 Quick Start

### Option 1️⃣: Docker Deployment (Recommended)

```bash
# Clone repository
git clone https://github.com/DrDrZ95/ManusProject.git
cd ManusProject

# Navigate to Docker directory
cd infra/docker

# Start all services
docker-compose up -d

# Check service status
docker-compose ps

# View live logs
docker-compose logs -f

# Stop services
docker-compose down
```

**Service Access Points:**
- 🌐 Frontend UI: http://localhost:3000
- 📡 Backend API: http://localhost:5000
- 📚 API Documentation: http://localhost:5000/swagger
- 📊 Prometheus: http://localhost:9090

### Option 2️⃣: Local Development Deployment

```bash
# Clone repository
git clone https://github.com/DrDrZ95/ManusProject.git
cd ManusProject

# 1. Configure backend
cd apps/agent-api/Agent.Api
dotnet restore
dotnet build
dotnet run

# 2. In another terminal, configure frontend
cd apps/agent-ui
npm install
npm start

# 3. Setup database (requires PostgreSQL running)
# Update connection string in appsettings.json
# Then run migrations
dotnet ef database update
```

### Option 3️⃣: Kubernetes Deployment

```bash
# Create namespace
kubectl create namespace manus-project

# Install using Helm
cd infra/helm
helm install manus-project ./manus-project-chart \
  -n manus-project \
  -f values.yaml

# Verify deployment
kubectl get pods -n manus-project
kubectl get svc -n manus-project

# View deployment logs
kubectl logs -n manus-project -l app=manus-project -f
```

---

## 📦 Deployment Guide

### Docker Compose Configuration

```yaml
# Service List
services:
  agent-api:           # ASP.NET Core Backend
  agent-ui:            # React Frontend
  postgres:            # Relational Database
  chromadb:            # Vector Database
  nginx:               # Reverse Proxy
  prometheus:          # Monitoring (Optional)
  mlflow:              # Experiment Tracking (Optional)
```

### Environment Variables Configuration

```env
# infra/docker/.env

# PostgreSQL Database
POSTGRES_PASSWORD=your_secure_password
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=manus;...

# API Configuration
API_ENDPOINT=https://your-domain.com
API_PORT=5000

# LLM Service Configuration
OPENAI_API_KEY=sk-xxxxxxxxxxxxx
AZURE_OPENAI_ENDPOINT=https://xxx.openai.azure.com/
QWEN_API_KEY=xxxxxxxxxxxxx

# Authentication
JWT_SECRET_KEY=your-super-secret-key-min-32-chars
IDENTITY_SEED_ADMIN_PASSWORD=Admin@123456

# Vector Database
CHROMADB_HOST=chromadb
CHROMADB_PORT=8000

# Monitoring & Telemetry
ENABLE_PROMETHEUS=true
ENABLE_MLFLOW=true
```

### Production Deployment Checklist

- [ ] Configure HTTPS/TLS certificates
- [ ] Setup database backups and replication
- [ ] Configure external authentication (OIDC/LDAP)
- [ ] Enable audit logging
- [ ] Deploy monitoring and alerting system
- [ ] Configure log aggregation
- [ ] Test disaster recovery procedures
- [ ] Establish CI/CD automation pipeline
- [ ] Perform performance and load testing
- [ ] Conduct security audit

---

## 🔧 Core Modules

### Agent.Api - Application Entry Point
- **Responsibility**: Application startup, dependency injection, middleware configuration
- **Key Files**: Program.cs, GlobalUsings.cs
- **Extension Points**: ServiceCollectionExtensions, MiddlewareExtensions

### Agent.Core - Core Business Logic
- **Authorization Module**: Role-based, policy-based, claim-based authorization
- **Data Access**: EF Core repositories, database migrations
- **eBPF Module**: System monitoring and security detection
- **SignalR**: Real-time communication and push notifications
- **Identity Management**: User, role, and permission management

### Agent.McpGateway - AI Orchestration Engine
- **Semantic Kernel Service**: LLM integration and abstraction
- **RAG Service**: Knowledge base, document processing, similarity search
- **Workflow Service**: Task orchestration and execution
- **Sandbox Service**: Isolated command execution
- **Prompt Service**: Prompt template management and optimization
- **Web Search Service**: Information retrieval
- **Vector Database Service**: Embedding storage
- **Fine-tune Service**: Model training
- **Telemetry Service**: Distributed tracing

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| `chromadb_integration.md` | Vector database setup, RAG configuration |
| `ebpf_integration.md` | eBPF module setup, system monitoring |
| `identity_signalr_integration.md` | Authentication, real-time communication |
| `kubernetes_istio_grayscale_release.md` | Advanced Kubernetes deployment |
| `mlflow_integration.md` | Experiment tracking, model management |
| `rag_prompt_engineering.md` | Prompt optimization, RAG best practices |
| `sandbox_terminal_integration.md` | Sandbox execution, security isolation |
| `semantic_kernel_examples.md` | LLM integration examples |
| `workflow_integration.md` | Workflow design and implementation |
| `yarp_gateway_integration.md` | Gateway configuration, route management |

---

## 👨‍💻 Development Guide

### Build from Source

```bash
# Backend
cd apps/agent-api
dotnet restore
dotnet build -c Release
dotnet test

# Frontend
cd apps/agent-ui
npm install
npm run build
npm test
```

### Development Workflow

1. **Create feature branch**: `git checkout -b feature/your-feature`
2. **Write code**: Follow coding standards
3. **Write tests**: Unit and integration tests
4. **Commit changes**: `git commit -am 'Add feature'`
5. **Push code**: `git push origin feature/your-feature`
6. **Create PR**: Detailed description of changes
7. **Code review**: Wait for maintainer review
8. **Merge**: Merge after approval

### Coding Standards

- **C#**: Follow Microsoft C# coding guidelines
- **TypeScript**: Use ESLint and Prettier
- **Commits**: `[feat|fix|docs|style|refactor|test]: description`

---

## 🤝 Contributing Guide

We welcome any form of contribution!

### Contribution Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Contribution Requirements

- ✅ All tests must pass
- ✅ Code conforms to project style guidelines
- ✅ Commit messages are clear and concise
- ✅ PR includes detailed description
- ✅ New features require documentation updates

---

## 📄 License

This project is licensed under the **MIT License**. See [LICENSE](./LICENSE) file for details.

MIT License allows:
- ✅ Commercial use
- ✅ Code modification
- ✅ Distribution
- ✅ Private use

Conditions:
- 📌 Must include license copy
- 📌 Must state significant changes

---

## 🔗 Related Resources

### Official Documentation
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [React Official Documentation](https://react.dev/)
- [TypeScript Documentation](https://www.typescriptlang.org/)

### Related Projects
- [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/)
- [ChromaDB](https://www.trychroma.com/)
- [OpenTelemetry](https://opentelemetry.io/)
- [Docker Documentation](https://docs.docker.com/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)

### Reference Links
- **GitHub Repository**: https://github.com/DrDrZ95/ManusProject
- **Manus Project**: https://manus.im/ (code generation tool)

---

## 📞 Support & Feedback

### Getting Help

- 📖 Check [complete documentation](./docs/)
- 🐛 [Report Bug](https://github.com/DrDrZ95/ManusProject/issues)
- 💡 [Request Feature](https://github.com/DrDrZ95/ManusProject/issues)
- 💬 [Discuss Issues](https://github.com/DrDrZ95/ManusProject/discussions)

### Community Support

- Review existing Issues and Discussions
- Participate in GitHub Discussions
- Contribute improvements and bug fixes

---

## 📊 Project Statistics

- **Programming Languages**: C#, TypeScript, Python
- **Framework Versions**: .NET 8.0, React 18+
- **Lines of Code**: 15,000+
- **Module Count**: 20+
- **Documentation Pages**: 60+

---

## 🎯 Roadmap

### Q1 Completed ✅
- Core AI agent framework
- Workflow management system
- RAG implementation
- Docker deployment support
- System architecture optimization
- Modularization refactor

### Q1-Q2 In Progress 🚀
- Complete Notion UI redesign
- Advanced caching strategy optimization
- WebSearch enhancement and expansion
- Model fine-tuning tool refinement
- Performance benchmark testing

### Q2-Q3 Planned 🔮
- Multi-language support (Chinese, English, Japanese, Korean)
- Additional LLM integrations (Claude, Gemini)
- Community plugin system
- Desktop client (Electron)
- Mobile app support (React Native)
- GraphQL API layer

---

**All files and solution logic generated with assistance from Manus. Reference: https://manus.im/**
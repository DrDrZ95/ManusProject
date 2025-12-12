# ManusProject - AI-Powered Intelligent Agent Platform

> A professional-grade AI agent framework built with .NET 8.0 and React, designed for autonomous task execution and intelligent workflow automation.
> All files and solution logic are generated from Manus. reference: https://manus.im/

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

### Layered Architecture Design

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Presentation Layer (UI)                          │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  React 18+ Frontend (Notion-style UI)                        │   │
│  │  - Task Management Dashboard                                │   │
│  │  - Real-time Collaborative Editing (SignalR)                │   │
│  │  - Workflow Visualization                                   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────┬──────────────────────────────────┘
                                   │ HTTPS / WebSocket
┌──────────────────────────────────┴──────────────────────────────────┐
│                        Gateway Layer                                │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  Nginx / YARP (Reverse Proxy)                               │   │
│  │  - Load Balancing                                           │   │
│  │  - Request Routing                                          │   │
│  │  - SSL/TLS Termination                                      │   │
│  │  - Circuit Breaker Pattern                                  │   │
│  └─────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────┬──────────────────────────────────┘
                                   │
┌──────────────────────────────────┴──────────────────────────────────┐
│                   Business Logic Layer                              │
│                      ASP.NET Core Backend                           │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  Agent.Api (Startup & Configuration)                        │   │
│  │  - Program.cs: Application entry point & DI config          │   │
│  │  - GlobalUsings.cs: Global namespace declarations           │   │
│  │  - Controllers/*: API routing endpoints                     │   │
│  │  - Extensions/*: Modular extension configuration            │   │
│  └─────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  Agent.Core (Core Business Logic)                           │   │
│  │  ├─ Authorization/          - Authorization policies        │   │
│  │  ├─ Controllers/            - API endpoint implementations  │   │
│  │  ├─ Data/                   - EF Core DbContext & repos     │   │
│  │  ├─ eBPF/                   - System detection module        │   │
│  │  ├─ Extensions/             - Modular configuration         │   │
│  │  ├─ Gateway/                - YARP gateway setup            │   │
│  │  ├─ Hubs/                   - SignalR real-time hubs        │   │
│  │  └─ Identity/               - ASP.NET Core Identity         │   │
│  └─────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  Agent.McpGateway (AI Orchestration Engine)                 │   │
│  │  ├─ Services/                                               │   │
│  │  │  ├─ SemanticKernelService     - LLM integration         │   │
│  │  │  ├─ RagService                - RAG functionality        │   │
│  │  │  ├─ WorkflowService           - Task orchestration       │   │
│  │  │  ├─ SandboxService            - Isolated execution       │   │
│  │  │  ├─ PromptService             - Prompt management        │   │
│  │  │  ├─ FinetuneService           - Model fine-tuning        │   │
│  │  │  ├─ WebSearchService          - Web search integration   │   │
│  │  │  ├─ VectorDatabaseService     - Vector DB operations     │   │
│  │  │  ├─ TelemetryService          - OpenTelemetry           │   │
│  │  │  ├─ UserInputService          - User input handling      │   │
│  │  │  └─ FileUploadService         - File operations         │   │
│  │  ├─ McpTools/                    - MCP tool integration     │   │
│  │  ├─ Models/                      - Shared data models       │   │
│  │  └─ WebSearch/                   - Search implementations   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────┬──────────────────────────────────┘
                                   │
        ┌──────────────────────────┼──────────────────────────────┐
        │                          │                              │
┌───────▼─────────┐       ┌────────▼──────┐         ┌───────────▼─┐
│   PostgreSQL    │       │   ChromaDB     │         │ External AI │
│ (Relational DB) │       │ (Vector DB)    │         │  Services   │
├─────────────────┤       ├────────────────┤         ├─────────────┤
│ - User info     │       │ - Document     │         │ - OpenAI    │
│ - Workflows     │       │   embeddings   │         │ - Azure AI  │
│ - Tasks         │       │ - Retrieval    │         │ - Qwen      │
│ - Configuration │       │   index        │         │ - SearXNG   │
│ - Audit logs    │       │ - RAG cache    │         │ - SerpApi   │
└─────────────────┘       └────────────────┘         └─────────────┘
```

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

### Complete Project Tree

```
ManusProject/                              # Project root directory
│
├── README.md                              # English documentation (entry point)
├── README.zh_CN.md                        # Chinese documentation (entry point)
├── .gitignore                             # Git ignore configuration
├── LICENSE                                # MIT open-source license
│
├── apps/                                  # 📦 Application layer
│   │
│   ├── agent-api/                         # 🔌 Backend API service
│   │   │
│   │   ├── Agent.Api/                     # ⚙️ ASP.NET Core application entry
│   │   │   ├── Controllers/               # 📍 API routing controllers
│   │   │   ├── Extensions/                # 🔧 Modular configuration extensions
│   │   │   ├── GlobalUsings.cs            # 📌 Global using declarations
│   │   │   ├── Program.cs                 # 🚀 Application startup entry point
│   │   │   ├── Agent.Api.csproj           # 📋 Project file
│   │   │   └── appsettings*.json          # ⚙️ Configuration files
│   │   │
│   │   ├── Agent.Core/                    # 💡 Core business logic
│   │   │   ├── Authorization/             # 🔐 Authentication & authorization
│   │   │   ├── Controllers/               # 📍 API controller collection
│   │   │   ├── Data/                      # 💾 Data access layer
│   │   │   │   ├── Contexts/              # EF Core DbContext
│   │   │   │   ├── Entities/              # Data model entities
│   │   │   │   ├── Repositories/          # Data repository implementations
│   │   │   │   ├── Migrations/            # EF Core migration scripts
│   │   │   │   └── Seeds/                 # Data initialization scripts
│   │   │   ├── eBPF/                      # 🔍 eBPF system detection module
│   │   │   ├── Extensions/                # 🔧 Modular extensions
│   │   │   ├── Gateway/                   # 🚪 YARP gateway configuration
│   │   │   ├── Hubs/                      # 💬 SignalR real-time communication
│   │   │   ├── Identity/                  # 👤 Identity management
│   │   │   ├── Exceptions/                # ⚠️ Custom exceptions
│   │   │   ├── Middlewares/               # 🔌 Custom middlewares
│   │   │   ├── Models/                    # 📦 Data models & DTOs
│   │   │   ├── Agent.Core.csproj          # 📋 Project file
│   │   │   └── GlobalUsings.cs            # Global using declarations
│   │   │
│   │   └── Agent.McpGateway/              # 🤖 AI orchestration engine
│   │       ├── Services/                  # 🛠️ Core service implementations
│   │       │   ├── SemanticKernelService/ - LLM integration
│   │       │   ├── RAGService/            - RAG functionality
│   │       │   ├── WorkflowService/       - Task orchestration
│   │       │   ├── SandboxService/        - Isolated execution
│   │       │   ├── PromptService/         - Prompt management
│   │       │   ├── WebSearchService/      - Web search
│   │       │   ├── VectorDatabaseService/ - Vector DB operations
│   │       │   ├── FinetuneService/       - Model fine-tuning
│   │       │   ├── TelemetryService/      - OpenTelemetry
│   │       │   ├── UserInputService/      - User input handling
│   │       │   └── FileUploadService/     - File operations
│   │       ├── McpTools/                  # 🔗 MCP tool integration
│   │       ├── Models/                    # 📦 Shared data models
│   │       ├── WebSearch/                 # 🔍 Web search module
│   │       ├── Agent.McpGateway.csproj    # 📋 Project file
│   │       └── GlobalUsings.cs            # Global using declarations
│   │
│   └── agent-ui/                          # 🎨 React frontend application
│       ├── public/                        # Static assets
│       ├── src/                           # Source code
│       │   ├── components/                # React components
│       │   ├── pages/                     # Page components
│       │   ├── services/                  # API services
│       │   ├── hooks/                     # Custom React hooks
│       │   ├── store/                     # State management
│       │   ├── styles/                    # Style files
│       │   ├── utils/                     # Utility functions
│       │   └── types/                     # TypeScript types
│       ├── package.json                   # Dependencies & scripts
│       ├── tsconfig.json                  # TypeScript config
│       ├── vite.config.ts                 # Vite build config
│       └── .env.example                   # Environment variables example
│
├── infra/                                 # 🏗️ Infrastructure & deployment
│   ├── docker/                            # 🐳 Docker containerization
│   │   ├── Dockerfile.webapi              # Backend image
│   │   ├── Dockerfile.react               # Frontend image
│   │   ├── docker-compose.yml             # Container orchestration
│   │   ├── nginx.conf                     # Nginx configuration
│   │   └── examples/                      # Example configurations
│   │
│   ├── kubernetes/                        # ☸️ Kubernetes manifests
│   │   ├── namespace.yaml                 # Namespace definition
│   │   ├── configmap.yaml                 # Configuration maps
│   │   ├── secrets.yaml                   # Sensitive information
│   │   ├── deployments.yaml               # Deployment configurations
│   │   ├── services.yaml                  # Service definitions
│   │   ├── ingress.yaml                   # Ingress configuration
│   │   └── hpa.yaml                       # Horizontal Pod Autoscaler
│   │
│   ├── helm/                              # 📦 Helm charts
│   │   └── manus-project/
│   │       ├── Chart.yaml                 # Chart metadata
│   │       ├── values.yaml                # Default values
│   │       ├── values.dev.yaml            # Development values
│   │       ├── values.prod.yaml           # Production values
│   │       └── templates/                 # Kubernetes templates
│   │
│   └── envsetup/                          # 🔧 Environment setup scripts
│       ├── install_dependencies.sh        # Dependency installation
│       ├── download_model.sh              # Model download script
│       ├── setup_database.sh              # Database initialization
│       └── health_check.sh                # Health check script
│
├── llm/                                   # 🤖 LLM & ML components
│   ├── deploy/                            # 🚀 Deployment & services
│   │   ├── model_server.py                # Model server
│   │   ├── api_examples.py                # API examples
│   │   ├── requirements.txt               # Python dependencies
│   │   └── Dockerfile                     # Model service container
│   │
│   └── finetune/                          # 🎓 Model fine-tuning
│       ├── train.py                       # Training script
│       ├── evaluate.py                    # Evaluation script
│       ├── dataset_loader.py              # Data loading
│       ├── utils.py                       # Utility functions
│       └── config.yaml                    # Training configuration
│
├── test/                                  # 🧪 Test suite
│   └── Agent.Core.Tests/                  # Unit tests
│       ├── Unit/                          # Unit tests
│       ├── Integration/                   # Integration tests
│       ├── MockData/                      # Test data
│       └── Agent.Core.Tests.csproj        # Test project file
│
├── docs/                                  # 📚 Comprehensive documentation
│   ├── Architecture/                      # Architecture documentation
│   ├── Setup/                             # Deployment documentation
│   ├── Features/                          # Feature documentation
│   ├── API/                               # API documentation
│   ├── Development/                       # Development guides
│   └── CHANGELOG.md                       # Changelog
│
├── .github/                               # GitHub configuration
│   ├── workflows/                         # CI/CD workflows
│   ├── ISSUE_TEMPLATE/                    # Issue templates
│   └── PULL_REQUEST_TEMPLATE.md           # PR template
│
└── LICENSE                                # MIT license
```

### Key Structure Explanations

#### 1️⃣ **apps/** - Application Layer
- **agent-api/**: .NET backend application
  - `Agent.Api/`: ASP.NET Core entry point
  - `Agent.Core/`: Core business logic
  - `Agent.McpGateway/`: AI orchestration engine
- **agent-ui/**: React frontend application

#### 2️⃣ **infra/** - Infrastructure
- **docker/**: Docker and Docker Compose configuration
- **kubernetes/**: Kubernetes manifest files
- **helm/**: Helm charts for K8s deployment
- **envsetup/**: Environment initialization scripts

#### 3️⃣ **llm/** - Machine Learning
- **deploy/**: Model server deployment
- **finetune/**: Fine-tuning and training scripts

#### 4️⃣ **test/** - Testing
- **Agent.Core.Tests/**: Unit and integration tests

#### 5️⃣ **docs/** - Documentation
- Complete feature, deployment, and development documentation
- API documentation
- Architecture design documentation

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
  agent-api:           # ASP.NET Core backend
  agent-ui:            # React frontend
  postgres:            # Relational database
  chromadb:            # Vector database
  nginx:               # Reverse proxy
  prometheus:          # Monitoring (optional)
  mlflow:              # Experiment tracking (optional)
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
- **Lines of Code**: 10,000+
- **Module Count**: 15+
- **Documentation Pages**: 50+

---

## 🎯 Roadmap

### Completed ✅
- Core AI agent framework
- Workflow management system
- RAG implementation
- Docker deployment support
- Kubernetes integration
- Basic monitoring and logging

### In Progress 🚀
- Complete Notion UI redesign
- Advanced caching strategy
- WebSearch enhancement
- Model fine-tuning tool optimization

### Planned 🔮
- Multi-language support
- Additional LLM integrations
- Community plugin system
- Desktop client
- Mobile app support

---

**All files and solution logic generated with assistance from Manus. Reference: https://manus.im/**

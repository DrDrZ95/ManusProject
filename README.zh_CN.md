# ManusProject - AI 智能代理平台

> 一个企业级 AI 代理框架，基于 .NET 8.0 和 React 构建，专为自主任务执行和智能工作流自动化设计。
> All files and solution logic are generated from Manus. reference: https://manus.im/

### 📢 作者留言

本项目仍然不断在优化中，作者争取一周做到 3+ 次更新和优化。

**C# 网上没输过，现实没赢过. Man! what can i say?** 🚀

## 2026-02-01 心得
由于Manus近期对关键功能进行阉割，以及其reddit社区愈演愈烈的问题暴露，导致作者对Manus的期望越来越低。

因为它变得更面向非专业用户，不管是支付体系的漏洞还是集成模型对于项目的改动效率和算力变得越来越**拉跨**，
开始怀念刚发布不久的那个版本。

迫使作者在近段时间需要重新思考，并更改为 Cursor / Claude Code / Antigravity / Codex 等专业的平台和工具进行继续构建。

**失望的不止是这个：**

![唉](./docs/photo.png)
---

[English Version](./README.md)

## 📋 目录

- [项目概述](#项目概述)
- [核心特性](#核心特性)
- [技术栈](#技术栈)
- [系统架构](#系统架构)
- [系统需求](#系统需求)
- [项目结构详解](#项目结构详解)
- [快速开始](#快速开始)
- [部署指南](#部署指南)
- [核心模块](#核心模块)
- [文档资源](#文档资源)
- [开发指南](#开发指南)
- [贡献指南](#贡献指南)
- [许可证](#许可证)

---

## 🎯 项目概述

ManusProject 是一个企业级 AI 代理框架，融合了最先进的大语言模型（LLM）技术与强大的后端基础设施以及直观的前端界面。该平台通过智能工作流管理、检索增强生成（RAG）和沙箱任务处理，实现自主任务执行。

### 核心亮点

- **🤖 多模型支持**：集成 OpenAI、DeepSeek、Kimi 和 Llama 4 等多种开源和闭源模型
- **🏗️ 分布式架构**：原生支持 Kubernetes 和 Docker，天生可扩展
- **🔒 高级安全**：eBPF 系统监控、ASP.NET Core Identity 集成、细粒度访问控制
- **⚡ 实时通信**：SignalR 实现即时更新和推送通知
- **📊 企业级就绪**：完善的日志、分布式追踪和可观测性

---

## ✨ 核心特性

### 🤖 AI & LLM 能力
- **Semantic Kernel 集成** - 统一的 LLM 抽象层，支持多个模型提供商
- **检索增强生成（RAG）** - 与 ChromaDB 和自定义向量存储集成的智能知识库
- **高级提示工程** - 具有动态变量替换和模板管理的提示系统
- **模型微调工具** - 完整的脚本和工具支持自定义模型适配

### ⚙️ 工作流与自动化
- **智能工作流引擎** - 支持复杂多步骤任务的编排和执行
- **沙箱终端集成** - 安全隔离的命令执行环境，防止恶意操作
- **动态任务规划** - AI 驱动的自动待办清单生成和任务分解
- **灵活交互处理** - 支持多种任务类型的交互模式

### 🔐 系统与安全
- **eBPF 检测模块** - 低级系统监控和安全威胁分析
- **身份认证与授权** - ASP.NET Core Identity 完整实现
- **自定义策略引擎** - 细粒度的角色和权限管理
- **Web 搜索集成** - 支持 SearXNG 和 SerpApi 的实时信息检索

### 📈 可观测性与运维
- **分布式追踪** - OpenTelemetry 集成，端到端请求可视化
- **Prometheus 指标** - 全面的应用和系统健康指标
- **MLflow 实验管理** - 模型训练和实验追踪
- **结构化日志** - 关联 ID 和上下文贯穿整个调用栈

### 🚀 基础设施与部署
- **Docker 容器化** - 完整的 Docker Compose 多容器编排方案
- **Kubernetes 支持** - Helm 图表和原始清单用于云部署
- **YARP 反向代理** - 支持熔断器模式的智能网关
- **高可用性设计** - 负载均衡和故障转移机制

---

## 🛠 技术栈

### 📱 后端技术
| 组件 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0+ | 现代高性能 Web 框架 |
| ASP.NET Core | 8.0+ | Web API 和实时通信 |
| Entity Framework Core | 8.0+ | PostgreSQL ORM 映射 |
| SignalR | 8.0+ | 实时双向通信 |
| OpenTelemetry | Latest | 可观测性和分布式追踪 |
| Semantic Kernel | Latest | LLM 抽象和编排 |
| YARP | Latest | 反向代理和网关 |

### 🎨 前端技术
| 组件 | 版本 | 用途 |
|------|------|------|
| React | 18.0+ | 现代 UI 框架 |
| TypeScript | 5.0+ | 类型安全的 JavaScript |
| SignalR Client | 8.0+ | 实时通知客户端 |
| Tailwind CSS | Latest | 现代 CSS 框架 |

### 💾 数据与存储
| 组件 | 用途 |
|------|------|
| PostgreSQL 12+ | 主关系数据库，存储元数据 |
| ChromaDB | 向量数据库，支持 RAG 功能 |
| Redis (可选) | 缓存层，提升查询性能 |

### 🐳 容器化与编排
| 组件 | 用途 |
|------|------|
| Docker | 容器化应用和服务 |
| Docker Compose | 本地开发多容器编排 |
| Kubernetes 1.21+ | 生产环境云部署 |
| Helm 3.0+ | K8s 包管理和模板化 |

### 📊 监控与运维
| 组件 | 用途 |
|------|------|
| Prometheus | 指标收集和存储 |
| Grafana (可选) | 指标可视化面板 |
| MLflow | 机器学习实验追踪 |
| Elasticsearch (可选) | 日志索引和搜索 |

### 🔗 集成与扩展
| 组件 | 用途 |
|------|------|
| Model Context Protocol (MCP) | 标准化工具集成框架 |
| Nginx | Web 服务器和负载均衡 |
| SearXNG / SerpApi | Web 搜索集成 |

---

## 🏗 系统架构

### 分层架构设计 (Layered Architecture Pattern)

```
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃                        Layer 1: Presentation                          ┃
┃  ┌─────────────────────────────────────────────────────────────┐   ┃
┃  │  React 18+ Application Interface (agent-ui)                 │   ┃
┃  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────┐  │   ┃
┃  │  │  Dashboard       │  │  Workflow        │  │  Task    │  │   ┃
┃  │  │  - Analytics     │  │  - Editor        │  │  - Board │  │   ┃
┃  │  │  - Overview      │  │  - Visualizer    │  │  - Cards │  │   ┃
┃  │  └──────────────────┘  └──────────────────┘  └──────────┘  │   ┃
┃  └─────────────────────────────────────────────────────────────┘   ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                                    ↕                                   
                        HTTP/HTTPS + WebSocket                          
                                    ↕                                   
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃                    API 网关层                                        ┃
┃              (Nginx / YARP - 负载均衡)                              ┃
└─────────────────────────────┬───────────────────────────────┘       
                              │                                         
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━┻━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃           业务逻辑层 (Application Layer)                      ┃
┃                      ASP.NET Core Backend                     ┃
┃                                                                ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.Api (启动、配置、路由)                         │   ┃
┃  │  ├─ Program.cs: 应用入口点和 DI 配置               │   ┃
┃  │  ├─ GlobalUsings.cs: 全局命名空间声明              │   ┃
┃  │  ├─ Controllers: REST API 路由端点                │   ┃
┃  │  └─ Extensions: 模块化服务注册和配置              │   ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┃                                                                ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.Application (应用编排和业务流程)              │   ┃
┃  │  ├─ DTOs: 请求/响应数据传输对象                   │   ┃
┃  │  ├─ Mappers: 实体和 DTO 映射逻辑                  │   ┃
┃  │  ├─ Validators: 业务规则验证                       │   ┃
┃  │  ├─ Commands: CQRS 命令处理                        │   ┃
┃  │  ├─ Queries: CQRS 查询处理                         │   ┃
┃  │  ├─ Events: 领域事件定义                           │   ┃
┃  │  └─ Behaviors: 管道行为和拦截                     │   ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┃                                                                ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.Core (核心业务逻辑 - Domain Layer)           │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 身份认证与授权 (Identity & Authorization)        │   ┃
┃  │    ├─ Authorization Handlers (角色、策略、声明)      │   ┃
┃  │    ├─ Authorization Policies (权限规则)             │   ┃
┃  │    └─ Identity Services (用户、角色管理)            │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 工作流与任务 (Workflow & Task Management)        │   ┃
┃  │    ├─ WorkflowService (工作流编排)                 │   ┃
┃  │    ├─ TaskService (任务管理)                        │   ┃
┃  │    ├─ StateManager (状态跟踪)                       │   ┃
┃  │    └─ ExecutionContext (执行上下文)                │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 系统监控与检测 (System Monitoring)              │   ┃
┃  │    ├─ eBPF Services (进程监控)                     │   ┃
┃  │    ├─ SecurityDetector (威胁检测)                  │   ┃
┃  │    └─ HealthChecker (健康检查)                     │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 数据访问层 (Data Access & Persistence)          │   ┃
┃  │    ├─ Entity Models (数据实体)                      │   ┃
┃  │    ├─ DbContext (EF Core 上下文)                   │   ┃
┃  │    ├─ Repositories (通用仓储模式)                  │   ┃
┃  │    └─ Migrations (数据库迁移)                       │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 实时通信 (Real-Time Communication)              │   ┃
┃  │    ├─ SignalR Hubs (实时消息推送)                  │   ┃
┃  │    ├─ NotificationService (通知服务)               │   ┃
┃  │    └─ ConnectionManager (连接管理)                │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 网关与路由 (Gateway & Routing)                  │   ┃
┃  │    ├─ YARP Configuration (YARP 配置)              │   ┃
┃  │    ├─ RouteService (路由服务)                      │   ┃
┃  │    └─ CircuitBreaker (熔断器模式)                  │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 异常与日志 (Exception & Logging)                │   ┃
┃  │    ├─ Exception Handlers (异常处理)                │   ┃
┃  │    ├─ Logging Middleware (日志中间件)              │   ┃
┃  │    └─ Correlation ID (关联 ID)                     │   ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┃                                                                ┃
┃  ┌──────────────────────────────────────────────────────┐   ┃
┃  │  Agent.McpGateway (AI 编排引擎 - AI Orchestration)  │   ┃
┃  │                                                       │   ┃
┃  │  ▶ LLM 集成与管理 (LLM Integration & Management)    │   ┃
┃  │    ├─ SemanticKernelService (SK 封装)              │   ┃
┃  │    ├─ ModelRouter (模型路由)                        │   ┃
┃  │    ├─ PluginManager (插件管理)                      │   ┃
┃  │    └─ PromptOptimizer (提示优化)                    │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 知识库与 RAG (Knowledge Base & RAG)              │   ┃
┃  │    ├─ RagService (RAG 核心服务)                    │   ┃
┃  │    ├─ DocumentProcessor (文档处理)                 │   ┃
┃  │    ├─ EmbeddingGenerator (向量生成)                │   ┃
┃  │    ├─ VectorDatabaseService (向量库操作)           │   ┃
┃  │    └─ SimilaritySearcher (相似度搜索)              │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 工作流编排 (Workflow Orchestration)              │   ┃
┃  │    ├─ WorkflowService (工作流引擎)                 │   ┃
┃  │    ├─ WorkflowExecutor (执行器)                    │   ┃
┃  │    ├─ WorkflowParser (解析器)                      │   ┃
┃  │    └─ StateManager (状态管理)                       │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 提示与模板 (Prompt Engineering)                 │   ┃
┃  │    ├─ PromptService (提示管理)                     │   ┃
┃  │    ├─ TemplateEngine (模板引擎)                    │   ┃
┃  │    ├─ VariableResolver (变量解析)                  │   ┃
┃  │    └─ PromptCache (提示缓存)                        │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 沙箱与隔离执行 (Sandbox & Isolated Execution)    │   ┃
┃  │    ├─ SandboxService (沙箱服务)                    │   ┃
┃  │    ├─ ProcessExecutor (进程执行)                   │   ┃
┃  │    ├─ EnvironmentManager (环境管理)                │   ┃
┃  │    └─ SecurityManager (安全隔离)                   │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 搜索与集成 (Search & Integration)                │   ┃
┃  │    ├─ WebSearchService (Web 搜索)                  │   ┃
┃  │    ├─ McpTools (MCP 工具集)                        │   ┃
┃  │    ├─ FileUploadService (文件管理)                 │   ┃
┃  │    └─ UserInputService (用户输入)                  │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 模型微调 (Model Fine-Tuning)                     │   ┃
┃  │    ├─ FinetuneService (微调服务)                   │   ┃
┃  │    ├─ DatasetPreparer (数据集准备)                 │   ┃
┃  │    ├─ ModelTrainer (模型训练)                      │   ┃
┃  │    └─ MetricsCalculator (指标计算)                │   ┃
┃  │                                                       │   ┃
┃  │  ▶ 可观测性 (Observability)                         │   ┃
┃  │    ├─ TelemetryService (遥测服务)                 │   ┃
┃  │    ├─ MetricsCollector (指标收集)                 │   ┃
┃  │    └─ TraceExporter (追踪导出)                     │   ┃
┃  └──────────────────────────────────────────────────────┘   ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                                    ↕                                   
          ┌───────────────────────┼───────────────────┐                
          │                       │                   │                
    ┌─────▼──────┐        ┌──────▼─────┐    ┌─────▼──────┐             
    │ PostgreSQL │        │  ChromaDB   │    │ 外部服务   │             
    │ (元数据)   │        │  (向量)     │    │            │             
    │            │        │             │    │ - OpenAI   │             
    │ - 用户     │        │ - 嵌入向量  │    │ - DeepSeek │             
    │ - 工作流   │        │ - 文档      │    │ - Kimi     │             
    │ - 任务     │        │ - RAG 索引  │    │ - Llama 4  │             
    │ - 配置     │        │             │    │ - SearXNG  │             
    └────────────┘        └─────────────┘    └────────────┘             
```

### 架构特点说明

#### 📌 分离关注点 (Separation of Concerns)
- 各层职责单一，清晰界定
- 依赖方向从上到下
- 每层可独立测试和维护

#### 🔄 数据流向
1. **请求流向**：表现层 → 网关 → 应用层 → 业务层 → 数据层 → 存储
2. **响应流向**：存储 → 数据层 → 业务层 → 应用层 → 网关 → 表现层
3. **通知流向**：SignalR Hub → 网关 → 表现层 (实时推送)

#### 🛡️ 横切关注点
- **认证/授权**：在网关和业务层进行
- **日志/追踪**：在所有层收集
- **错误处理**：统一在网关层处理
- **缓存**：在数据层实现

---

## 💻 系统需求

### 硬件最低配置
```
CPU:    4 核心 (建议 8 核或更多)
内存:    8 GB (建议 16 GB 或更多)
存储:    50 GB SSD (建议 100 GB+)
网络:    稳定的 1Mbps+ 网络连接
```

### 软件依赖

#### 必需组件
```
.NET SDK         8.0.0 或更高
Node.js          16.x 或更高
npm / yarn       7.0+ 或 1.22.x+
Docker           20.10+ (容器部署)
Docker Compose   1.29+ (容器编排)
PostgreSQL       12.x 或更高
Git              2.20+
```

#### 可选组件
```
Kubernetes       1.21+ (云部署)
Helm             3.0+ (K8s 包管理)
NVIDIA CUDA      11.0+ (GPU 加速)
Prometheus       最新版 (监控)
MLflow           最新版 (实验追踪)
Elasticsearch    7.0+ (日志分析)
Redis            6.0+ (缓存加速)
```

### 网络要求
```
- 能够访问各类 LLM 服务的网络环境
- 防火墙规则允许：
  * 3000 (前端)
  * 5000 (后端 API)
  * 5432 (PostgreSQL)
  * 8000 (ChromaDB)
  * 9090 (Prometheus)
```

---

## 📁 项目结构详解

### apps/ - 应用程序代码

```
apps/
├─ agent-api/                              # 🔧 后端 API 服务
│  ├─ Agent.Api/                           # 🎯 应用启动和配置
│  │  ├─ Controllers/
│  │  │  ├─ HealthCheckController.cs       # 健康检查
│  │  │  ├─ WorkflowController.cs          # 工作流管理
│  │  │  ├─ TaskController.cs              # 任务管理
│  │  │  ├─ AIController.cs                # AI/LLM 操作
│  │  │  ├─ SearchController.cs            # 搜索功能
│  │  │  └─ AdminController.cs             # 管理员功能
│  │  ├─ Extensions/
│  │  │  ├─ ServiceCollectionExtensions.cs # DI 扩展
│  │  │  ├─ AuthenticationExtensions.cs    # 认证配置
│  │  │  ├─ CorsExtensions.cs              # CORS 配置
│  │  │  └─ MiddlewareExtensions.cs        # 中间件扩展
│  │  ├─ GlobalUsings.cs                   # 全局 using
│  │  ├─ Program.cs                        # 启动入口
│  │  ├─ Agent.Api.csproj
│  │  ├─ appsettings.json
│  │  ├─ appsettings.Development.json
│  │  └─ appsettings.Production.json
│  │
│  ├─ Agent.Application/                   # 💼 应用编排和流程
│  │  ├─ DTOs/                             # 📦 数据传输对象
│  │  │  ├─ Request/
│  │  │  │  ├─ CreateWorkflowDto.cs
│  │  │  │  ├─ CreateTaskDto.cs
│  │  │  │  └─ UpdateWorkflowDto.cs
│  │  │  └─ Response/
│  │  │     ├─ WorkflowDto.cs
│  │  │     ├─ TaskDto.cs
│  │  │     └─ ApiResponseDto.cs
│  │  │
│  │  ├─ Mappers/                          # 🗺️ 实体映射
│  │  │  ├─ WorkflowMappingProfile.cs
│  │  │  ├─ TaskMappingProfile.cs
│  │  │  └─ UserMappingProfile.cs
│  │  │
│  │  ├─ Validators/                       # ✅ 业务验证
│  │  │  ├─ CreateWorkflowValidator.cs
│  │  │  ├─ UpdateTaskValidator.cs
│  │  │  └─ UserInputValidator.cs
│  │  │
│  │  ├─ Commands/                         # ⚡ CQRS 命令
│  │  │  ├─ CreateWorkflowCommand.cs
│  │  │  ├─ ExecuteTaskCommand.cs
│  │  │  ├─ UpdateWorkflowCommand.cs
│  │  │  └─ DeleteWorkflowCommand.cs
│  │  │
│  │  ├─ Queries/                          # 🔍 CQRS 查询
│  │  │  ├─ GetWorkflowQuery.cs
│  │  │  ├─ ListWorkflowsQuery.cs
│  │  │  ├─ SearchTasksQuery.cs
│  │  │  └─ GetUserQuery.cs
│  │  │
│  │  ├─ CommandHandlers/                  # 🎯 命令处理器
│  │  │  ├─ CreateWorkflowCommandHandler.cs
│  │  │  ├─ ExecuteTaskCommandHandler.cs
│  │  │  └─ DeleteWorkflowCommandHandler.cs
│  │  │
│  │  ├─ QueryHandlers/                    # 📊 查询处理器
│  │  │  ├─ GetWorkflowQueryHandler.cs
│  │  │  ├─ ListWorkflowsQueryHandler.cs
│  │  │  └─ SearchTasksQueryHandler.cs
│  │  │
│  │  ├─ Events/                           # 🔔 领域事件
│  │  │  ├─ WorkflowCreatedEvent.cs
│  │  │  ├─ TaskCompletedEvent.cs
│  │  │  └─ WorkflowDeletedEvent.cs
│  │  │
│  │  ├─ EventHandlers/                    # 📡 事件处理
│  │  │  ├─ WorkflowCreatedEventHandler.cs
│  │  │  └─ TaskCompletedEventHandler.cs
│  │  │
│  │  ├─ Behaviors/                        # 🔧 管道行为
│  │  │  ├─ ValidationBehavior.cs
│  │  │  ├─ LoggingBehavior.cs
│  │  │  ├─ CachingBehavior.cs
│  │  │  └─ PerformanceBehavior.cs
│  │  │
│  │  ├─ Specifications/                   # 📋 查询规范
│  │  │  ├─ WorkflowSpecification.cs
│  │  │  ├─ TaskSpecification.cs
│  │  │  └─ UserSpecification.cs
│  │  │
│  │  └─ Agent.Application.csproj
│  │
│  ├─ Agent.Core/                          # 💎 核心业务逻辑
│  │  ├─ Authorization/                    # 🔐 授权模块
│  │  │  ├─ Handlers/
│  │  │  │  ├─ RoleBasedHandler.cs
│  │  │  │  ├─ PolicyBasedHandler.cs
│  │  │  │  └─ ClaimBasedHandler.cs
│  │  │  ├─ Policies/
│  │  │  │  ├─ AdminOnlyPolicy.cs
│  │  │  │  ├─ WorkflowOwnerPolicy.cs
│  │  │  │  └─ RateLimitPolicy.cs
│  │  │  └─ Requirements/
│  │  │     └─ CustomRequirement.cs
│  │  │
│  │  ├─ WorkflowAndTask/                  # ⚙️ 工作流与任务
│  │  │  ├─ Services/
│  │  │  │  ├─ IWorkflowService.cs
│  │  │  │  ├─ WorkflowService.cs
│  │  │  │  ├─ ITaskService.cs
│  │  │  │  ├─ TaskService.cs
│  │  │  │  ├─ StateManager.cs
│  │  │  │  └─ ExecutionContext.cs
│  │  │  └─ Models/
│  │  │     ├─ WorkflowModel.cs
│  │  │     └─ TaskModel.cs
│  │  │
│  │  ├─ SystemMonitoring/                 # 🔍 系统监控
│  │  │  ├─ Services/
│  │  │  │  ├─ IEbpfService.cs
│  │  │  │  ├─ EbpfService.cs
│  │  │  │  ├─ ProcessMonitor.cs
│  │  │  │  ├─ SecurityDetector.cs
│  │  │  │  └─ HealthChecker.cs
│  │  │  └─ Scripts/
│  │  │     ├─ process_monitor.c
│  │  │     └─ security_check.c
│  │  │
│  │  ├─ Data/                             # 💾 数据访问
│  │  │  ├─ Contexts/
│  │  │  │  ├─ ApplicationDbContext.cs
│  │  │  │  └─ SeedData.cs
│  │  │  ├─ Entities/
│  │  │  │  ├─ User.cs
│  │  │  │  ├─ Workflow.cs
│  │  │  │  ├─ Task.cs
│  │  │  │  ├─ Document.cs
│  │  │  │  └─ AuditLog.cs
│  │  │  ├─ Repositories/
│  │  │  │  ├─ IRepository.cs
│  │  │  │  ├─ Repository.cs
│  │  │  │  ├─ WorkflowRepository.cs
│  │  │  │  ├─ TaskRepository.cs
│  │  │  │  └─ DocumentRepository.cs
│  │  │  ├─ Migrations/
│  │  │  │  └─ [EF Core 迁移文件]
│  │  │  └─ Seeds/
│  │  │     └─ DataSeeder.cs
│  │  │
│  │  ├─ RealTimeComm/                     # ⚡ 实时通信
│  │  │  ├─ Hubs/
│  │  │  │  ├─ IHubClient.cs
│  │  │  │  ├─ WorkflowHub.cs
│  │  │  │  ├─ TaskHub.cs
│  │  │  │  └─ NotificationHub.cs
│  │  │  └─ Services/
│  │  │     ├─ INotificationService.cs
│  │  │     ├─ NotificationService.cs
│  │  │     └─ ConnectionManager.cs
│  │  │
│  │  ├─ Gateway/                          # 🚪 网关与路由
│  │  │  ├─ Services/
│  │  │  │  ├─ IRouteService.cs
│  │  │  │  └─ RouteService.cs
│  │  │  └─ Configuration/
│  │  │     ├─ YarpConfiguration.cs
│  │  │     └─ CircuitBreakerPolicy.cs
│  │  │
│  │  ├─ ExceptionAndLogging/              # ⚠️ 异常与日志
│  │  │  ├─ Handlers/
│  │  │  │  ├─ GlobalExceptionHandler.cs
│  │  │  │  ├─ ValidationExceptionHandler.cs
│  │  │  │  └─ BusinessExceptionHandler.cs
│  │  │  ├─ Exceptions/
│  │  │  │  ├─ ApplicationException.cs
│  │  │  │  ├─ BusinessException.cs
│  │  │  │  ├─ ValidationException.cs
│  │  │  │  └─ ResourceNotFoundException.cs
│  │  │  ├─ Middlewares/
│  │  │  │  ├─ ErrorHandlingMiddleware.cs
│  │  │  │  ├─ LoggingMiddleware.cs
│  │  │  │  ├─ CorrelationIdMiddleware.cs
│  │  │  │  └─ RequestTimingMiddleware.cs
│  │  │  └─ Logging/
│  │  │     ├─ ILogger.cs
│  │  │     └─ LoggerImpl.cs
│  │  │
│  │  ├─ Identity/                         # 👤 身份管理
│  │  │  ├─ Entities/
│  │  │  │  ├─ ApplicationUser.cs
│  │  │  │  ├─ ApplicationRole.cs
│  │  │  │  └─ ApplicationUserRole.cs
│  │  │  ├─ Services/
│  │  │  │  ├─ IIdentityService.cs
│  │  │  │  ├─ IdentityService.cs
│  │  │  │  ├─ ITokenService.cs
│  │  │  │  └─ TokenService.cs
│  │  │  └─ Options/
│  │  │     ├─ JwtOptions.cs
│  │  │     └─ IdentityOptions.cs
│  │  │
│  │  ├─ Extensions/
│  │  │  ├─ ServiceCollectionExtensions.cs
│  │  │  ├─ AuthorizationExtensions.cs
│  │  │  ├─ DataAccessExtensions.cs
│  │  │  └─ TelemetryExtensions.cs
│  │  │
│  │  └─ Agent.Core.csproj
│  │
│  └─ Agent.McpGateway/                    # 🤖 AI 编排引擎
│     ├─ LLMIntegration/                    # 🧠 LLM 集成与管理
│     │  ├─ SemanticKernelService/
│     │  │  ├─ ISemanticKernelService.cs
│     │  │  ├─ SemanticKernelService.cs
│     │  │  ├─ PluginManager.cs
│     │  │  ├─ PromptOptimizer.cs
│     │  │  └─ ModelRouter.cs
│     │  └─ Models/
│     │     └─ LLMConfig.cs
│     │
│     ├─ KnowledgeBase/                     # 📚 知识库与 RAG
│     │  ├─ RagService/
│     │  │  ├─ IRagService.cs
│     │  │  ├─ RagService.cs
│     │  │  ├─ DocumentProcessor.cs
│     │  │  ├─ EmbeddingGenerator.cs
│     │  │  ├─ SimilaritySearcher.cs
│     │  │  └─ ChunkingStrategy.cs
│     │  ├─ VectorDatabase/
│     │  │  ├─ IVectorDatabaseService.cs
│     │  │  ├─ ChromaDBClient.cs
│     │  │  ├─ EmbeddingCache.cs
│     │  │  ├─ IndexManager.cs
│     │  │  └─ VectorQueryBuilder.cs
│     │  └─ Models/
│     │     ├─ Document.cs
│     │     └─ Embedding.cs
│     │
│     ├─ WorkflowOrchestration/             # 🔄 工作流编排
│     │  ├─ WorkflowService/
│     │  │  ├─ IWorkflowService.cs
│     │  │  ├─ WorkflowService.cs
│     │  │  ├─ WorkflowExecutor.cs
│     │  │  ├─ WorkflowParser.cs
│     │  │  ├─ StateManager.cs
│     │  │  └─ ExecutionContext.cs
│     │  └─ Models/
│     │     └─ WorkflowDefinition.cs
│     │
│     ├─ PromptEngineering/                 # 📝 提示与模板
│     │  ├─ PromptService/
│     │  │  ├─ IPromptService.cs
│     │  │  ├─ PromptService.cs
│     │  │  ├─ TemplateEngine.cs
│     │  │  ├─ VariableResolver.cs
│     │  │  └─ PromptCache.cs
│     │  └─ Templates/
│     │     └─ PromptTemplate.cs
│     │
│     ├─ SandboxExecution/                  # 🔒 沙箱隔离执行
│     │  ├─ SandboxService/
│     │  │  ├─ ISandboxService.cs
│     │  │  ├─ SandboxService.cs
│     │  │  ├─ ProcessExecutor.cs
│     │  │  ├─ EnvironmentManager.cs
│     │  │  └─ SecurityManager.cs
│     │  └─ Policies/
│     │     └─ SandboxPolicy.cs
│     │
│     ├─ SearchAndIntegration/              # 🔍 搜索与集成
│     │  ├─ WebSearch/
│     │  │  ├─ IWebSearchService.cs
│     │  │  ├─ WebSearchService.cs
│     │  │  ├─ SearXngClient.cs
│     │  │  ├─ SerpApiClient.cs
│     │  │  └─ SearchResultProcessor.cs
│     │  ├─ McpTools/
│     │  │  ├─ IMcpTool.cs
│     │  │  ├─ ToolRegistry.cs
│     │  │  ├─ MusicTool.cs
│     │  │  ├─ WeatherTool.cs
│     │  │  └─ CustomTool.cs
│     │  ├─ FileUpload/
│     │  │  ├─ IFileUploadService.cs
│     │  │  ├─ FileUploadService.cs
│     │  │  ├─ FileValidator.cs
│     │  │  ├─ StorageManager.cs
│     │  │  └─ VirusScanner.cs
│     │  └─ UserInput/
│     │     ├─ IUserInputService.cs
│     │     ├─ UserInputService.cs
│     │     ├─ InputValidator.cs
│     │     └─ ContextAnalyzer.cs
│     │
│     ├─ ModelFinetuning/                   # 🎓 模型微调
│     │  ├─ FinetuneService/
│     │  │  ├─ IFinetuneService.cs
│     │  │  ├─ FinetuneService.cs
│     │  │  ├─ DatasetPreparer.cs
│     │  │  ├─ ModelTrainer.cs
│     │  │  └─ MetricsCalculator.cs
│     │  └─ Models/
│     │     └─ FinetuneConfig.cs
│     │
│     ├─ Observability/                     # 📊 可观测性
│     │  ├─ TelemetryService/
│     │  │  ├─ ITelemetryService.cs
│     │  │  ├─ OpenTelemetryService.cs
│     │  │  ├─ MetricsCollector.cs
│     │  │  └─ TraceExporter.cs
│     │  └─ Metrics/
│     │     └─ PerformanceMetrics.cs
│     │
│     ├─ Common/
│     │  ├─ Models/
│     │  │  ├─ ExecutionResult.cs
│     │  │  ├─ SearchResult.cs
│     │  │  └─ EmbeddingModel.cs
│     │  └─ Extensions/
│     │     └─ ServiceCollectionExtensions.cs
│     │
│     └─ Agent.McpGateway.csproj
│
└─ agent-ui/                               # 🎨 React 前端
   ├─ public/                              # 📊 静态资源
   │  ├─ index.html
   │  ├─ favicon.ico
   │  └─ manifest.json
   │
   ├─ src/                                 # 💻 源代码
   │  ├─ index.tsx
   │  ├─ App.tsx
   │  ├─ App.css
   │  ├─ components/
   │  │  ├─ Layout/ (Header, Sidebar, Footer, LayoutWrapper)
   │  │  ├─ Workflow/ (List, Editor, Viewer, Node)
   │  │  ├─ Task/ (Board, Card, Modal, Form)
   │  │  ├─ AI/ (ChatBox, PromptEditor, ResultDisplay, ModelSelector)
   │  │  └─ Common/ (Button, Modal, Notification, Loading, ErrorBoundary)
   │  ├─ pages/ (Dashboard, Workflows, Tasks, AIChat, Settings, Search)
   │  ├─ services/ (api, workflowApi, taskApi, aiApi, searchApi, authApi)
   │  ├─ hooks/ (useWorkflows, useTasks, useAuth, useSignalR, useNotification)
   │  ├─ store/ (Redux slices, store.ts)
   │  ├─ styles/ (variables.css, themes.css, responsive.css)
   │  ├─ utils/ (formatters, validators, constants, storage, logger)
   │  └─ types/ (workflow, task, api, user)
   │
   ├─ package.json
   ├─ tsconfig.json
   ├─ vite.config.ts
   ├─ .env.example
   └─ .eslintrc.json
```

### docs/ - 文档

```
docs/
├─ README.md
├─ Architecture/
│  ├─ system-architecture.md
│  ├─ components-overview.md
│  └─ data-flow.md
├─ Setup/
│  ├─ docker-deployment.md
│  ├─ kubernetes-deployment.md
│  ├─ helm-deployment.md
│  └─ configuration-guide.md
├─ Features/
│  ├─ chromadb_integration.md
│  ├─ ebpf_integration.md
│  ├─ identity_signalr_integration.md
│  ├─ mlflow_integration.md
│  ├─ rag_prompt_engineering.md
│  ├─ sandbox_terminal_integration.md
│  ├─ semantic_kernel_examples.md
│  ├─ workflow_integration.md
│  └─ yarp_gateway_integration.md
├─ API/
│  ├─ workflow-api.md
│  ├─ task-api.md
│  ├─ ai-api.md
│  └─ search-api.md
├─ Development/
│  ├─ getting-started.md
│  ├─ development-setup.md
│  ├─ code-structure.md
│  ├─ coding-standards.md
│  └─ testing-guide.md
└─ CHANGELOG.md
```

### infra/ - 基础设施

```
infra/
├─ docker/
│  ├─ Dockerfile.webapi
│  ├─ Dockerfile.react
│  ├─ docker-compose.yml
│  ├─ docker-compose.dev.yml
│  ├─ docker-compose.prod.yml
│  ├─ nginx.conf
│  ├─ nginx.ssl.conf
│  ├─ .dockerignore
│  └─ examples/
├─ kubernetes/
│  ├─ namespace.yaml
│  ├─ configmap.yaml
│  ├─ secrets.yaml
│  ├─ deployments.yaml
│  ├─ services.yaml
│  ├─ ingress.yaml
│  ├─ persistentvolumes.yaml
│  ├─ hpa.yaml
│  └─ rbac.yaml
├─ helm/
│  └─ manus-project/
│     ├─ Chart.yaml
│     ├─ values.yaml
│     ├─ values.dev.yaml
│     ├─ values.prod.yaml
│     ├─ values.staging.yaml
│     └─ templates/
└─ envsetup/
   ├─ install_dependencies.sh
   ├─ download_model.sh
   ├─ setup_database.sh
   ├─ configure_ssl.sh
   ├─ health_check.sh
   └─ monitoring_setup.sh
```

### llm/ - ML 组件

```
llm/
├─ deploy/
│  ├─ model_server.py
│  ├─ api_examples.py
│  ├─ requirements.txt
│  └─ Dockerfile
└─ finetune/
   ├─ train.py
   ├─ evaluate.py
   ├─ dataset_loader.py
   ├─ utils.py
   ├─ install_dependencies.sh
   └─ config.yaml
```

### test/ - 测试

```
test/
└─ Agent.Core.Tests/
   ├─ Unit/
   │  ├─ Services/
   │  │  ├─ WorkflowServiceTests.cs
   │  │  ├─ RAGServiceTests.cs
   │  │  └─ PromptServiceTests.cs
   │  ├─ Controllers/
   │  │  ├─ WorkflowControllerTests.cs
   │  │  └─ TaskControllerTests.cs
   │  └─ Repositories/
   │     └─ WorkflowRepositoryTests.cs
   ├─ Integration/
   │  ├─ ApiIntegrationTests.cs
   │  ├─ DatabaseIntegrationTests.cs
   │  └─ WorkflowIntegrationTests.cs
   ├─ MockData/
   │  ├─ TestDataFactory.cs
   │  └─ MockServices.cs
   └─ Agent.Core.Tests.csproj
```

---

## 🚀 快速开始

### 选项 1️⃣：Docker 部署（推荐）

```bash
# 克隆仓库
git clone https://github.com/DrDrZ95/ManusProject.git
cd ManusProject

# 进入 Docker 目录
cd infra/docker

# 启动所有服务
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看实时日志
docker-compose logs -f

# 停止服务
docker-compose down
```

**服务访问地址：**
- 🌐 前端 UI: http://localhost:3000
- 📡 后端 API: http://localhost:5000
- 📚 API 文档: http://localhost:5000/swagger
- 📊 Prometheus: http://localhost:9090

### 选项 2️⃣：本地开发部署

```bash
# 克隆仓库
git clone https://github.com/DrDrZ95/ManusProject.git
cd ManusProject

# 1. 配置后端
cd apps/agent-api/Agent.Api
dotnet restore
dotnet build
dotnet run

# 2. 在另一个终端配置前端
cd apps/agent-ui
npm install
npm start

# 3. 配置数据库 (需要 PostgreSQL 运行)
# 更新 appsettings.json 中的数据库连接字符串
# 然后运行迁移
dotnet ef database update
```

### 选项 3️⃣：Kubernetes 部署

```bash
# 创建命名空间
kubectl create namespace manus-project

# 使用 Helm 安装
cd infra/helm
helm install manus-project ./manus-project-chart \
  -n manus-project \
  -f values.yaml

# 验证部署
kubectl get pods -n manus-project
kubectl get svc -n manus-project

# 查看部署日志
kubectl logs -n manus-project -l app=manus-project -f
```

---

## 📦 部署指南

### Docker Compose 完整配置

```yaml
# 服务清单
services:
  agent-api:           # ASP.NET Core 后端
  agent-ui:            # React 前端
  postgres:            # 关系型数据库
  chromadb:            # 向量数据库
  nginx:               # 反向代理
  prometheus:          # 监控 (可选)
  mlflow:              # 实验追踪 (可选)
```

### 环境变量配置

```env
# infra/docker/.env 文件

# PostgreSQL 数据库
POSTGRES_PASSWORD=your_secure_password
POSTGRES_USER=manus_user
POSTGRES_DB=manus_db
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=manus_db;Username=manus_user;Password=your_secure_password

# API 配置
API_ENDPOINT=https://your-domain.com
API_PORT=5000
API_LOG_LEVEL=Information

# LLM 服务配置
OPENAI_API_KEY=sk-xxxxxxxxxxxxx
OPENAI_MODEL=gpt-4
DEEPSEEK_API_KEY=xxxxxxxxxxxxx
DEEPSEEK_MODEL=deepseek-chat
KIMI_API_KEY=xxxxxxxxxxxxx
KIMI_MODEL=moonshot-v1
LLAMA_API_ENDPOINT=http://localhost:8000
LLAMA_MODEL=llama-4-70b

# 身份认证
JWT_SECRET_KEY=your-super-secret-key-min-32-chars
JWT_EXPIRATION_MINUTES=60
IDENTITY_SEED_ADMIN_PASSWORD=Admin@123456

# SignalR
SIGNALR_ENABLE=true

# 向量数据库
CHROMADB_HOST=chromadb
CHROMADB_PORT=8000

# Web 搜索
ENABLE_WEB_SEARCH=true
SEARXNG_ENDPOINT=http://searxng:8888

# 监控和遥测
ENABLE_PROMETHEUS=true
PROMETHEUS_ENDPOINT=http://prometheus:9090
ENABLE_MLFLOW=true
MLFLOW_ENDPOINT=http://mlflow:5000

# 日志
LOG_LEVEL=Information
ELASTICSEARCH_ENDPOINT=http://elasticsearch:9200 (可选)
```

### 生产部署检查清单

- [ ] 配置 HTTPS/TLS 证书
- [ ] 设置数据库备份和点对点复制
- [ ] 配置外部身份认证 (OIDC/LDAP)
- [ ] 启用审计日志记录
- [ ] 部署监控告警系统
- [ ] 配置日志聚合和分析
- [ ] 测试灾难恢复程序
- [ ] 建立 CI/CD 自动化管道
- [ ] 性能和压力测试
- [ ] 安全审计和渗透测试

---

## 🔧 核心模块详解

### Agent.Api - 应用入口
- **职责**：应用启动、依赖注入、中间件配置
- **关键文件**：Program.cs, GlobalUsings.cs
- **扩展点**：ServiceCollectionExtensions, MiddlewareExtensions

### Agent.Application - 应用编排
- **职责**：CQRS 模式实现、DTO 映射、业务流程编排
- **关键特性**：MediatR 命令/查询处理、自动映射、验证管道
- **扩展点**：CommandHandlers, QueryHandlers, Behaviors

### Agent.Core - 核心业务逻辑
- **授权模块**：角色、策略、声明授权
- **工作流与任务**：编排和执行复杂流程
- **系统监控**：eBPF 和安全检测
- **数据访问**：EF Core 仓储
- **实时通信**：SignalR 集成
- **网关与路由**：YARP 反向代理
- **异常日志**：统一异常处理和结构化日志
- **身份管理**：用户、角色、权限

### Agent.McpGateway - AI 编排引擎
- **LLM 集成**：Semantic Kernel 封装、模型路由
- **知识库与 RAG**：文档处理、向量数据库、相似度搜索
- **工作流编排**：复杂任务编排和执行
- **提示工程**：模板管理和优化
- **沙箱执行**：隔离命令运行
- **搜索与集成**：Web 搜索、工具集成、文件管理
- **模型微调**：数据集准备、模型训练
- **可观测性**：遥测和指标收集

---

## 📚 文档资源

| 文档 | 说明 |
|------|------|
| `chromadb_integration.md` | 向量数据库设置、RAG 配置 |
| `ebpf_integration.md` | eBPF 模块、系统监控 |
| `identity_signalr_integration.md` | 身份认证、实时通信 |
| `kubernetes_istio_grayscale_release.md` | K8s 灰度发布策略 |
| `mlflow_integration.md` | 实验追踪、模型管理 |
| `rag_prompt_engineering.md` | 提示优化、RAG 最佳实践 |
| `sandbox_terminal_integration.md` | 沙箱执行、安全隔离 |
| `semantic_kernel_examples.md` | LLM 集成示例 |
| `workflow_integration.md` | 工作流设计和实现 |
| `yarp_gateway_integration.md` | 网关配置、路由管理 |

---

## 👨‍💻 开发指南

### 本地构建

```bash
# 后端
cd apps/agent-api
dotnet restore
dotnet build -c Release
dotnet test

# 前端
cd apps/agent-ui
npm install
npm run build
npm test
```

### 开发工作流

1. **创建功能分支**：`git checkout -b feature/your-feature`
2. **编写代码**：遵循编码规范
3. **编写测试**：单元和集成测试
4. **提交更改**：`git commit -am 'Add feature'`
5. **推送代码**：`git push origin feature/your-feature`
6. **创建 PR**：详细描述改动内容
7. **代码审查**：等待维护者审查
8. **合并**：通过审查后合并到主分支

### 编码规范

- **C#**：遵循 Microsoft C# 编码指南
- **TypeScript**：使用 ESLint 和 Prettier
- **提交信息**：`[feat|fix|docs|style|refactor|test]: description`

---

## 🤝 贡献指南

我们欢迎任何形式的贡献！无论是代码改进、文档更新、Bug 修复还是新功能建议，都非常欢迎。

### 贡献流程

1. Fork 本项目
2. 创建您的功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开一个 Pull Request

### 欢迎反馈与建议

- 💡 **在 PR 中分享您的优化建议**：如果您有改进代码、性能或用户体验的想法，欢迎在 Pull Request 中详细说明
- 🐛 **报告 Bug**：如果您发现任何问题，请通过 Issue 反馈，并提供复现步骤
- ✨ **功能建议**：对于新功能或改进建议，欢迎在 Issues 或 Discussions 中讨论
- 📝 **文档改进**：如果文档有不清楚或缺失的地方，欢迎提交改进建议

### 贡献要求

- ✅ 所有测试必须通过
- ✅ 代码符合项目风格指南
- ✅ 提交信息清晰明确
- ✅ PR 包含详细的改动说明
- ✅ 涉及新功能的 PR 需要提供相应文档更新

### 审查流程

我们的维护团队会定期审查 Pull Request，您可能会收到以下反馈：

- 代码审查意见
- 要求补充测试
- 建议文档更新
- 性能或安全方面的优化建议

请耐心对待审查过程，我们致力于保持代码质量和项目的长期可维护性。

---

## 📄 许可证

本项目采用 **MIT 许可证**。详见 [LICENSE](./LICENSE) 文件。

MIT License 允许：
- ✅ 商业使用
- ✅ 修改代码
- ✅ 分发
- ✅ 私人使用

条件：
- 📌 必须包含许可证副本
- 📌 必须说明重大改动

---

## 🔗 相关资源

### 官方文档
- [.NET 8.0 文档](https://docs.microsoft.com/zh-cn/dotnet/)
- [ASP.NET Core 文档](https://docs.microsoft.com/zh-cn/aspnet/core/)
- [React 官方文档](https://zh-hans.react.dev/)
- [TypeScript 文档](https://www.typescriptlang.org/zh/)

### 相关项目
- [Semantic Kernel](https://learn.microsoft.com/zh-cn/semantic-kernel/)
- [ChromaDB](https://www.trychroma.com/)
- [OpenTelemetry](https://opentelemetry.io/zh/)
- [Docker 文档](https://docs.docker.com/)
- [Kubernetes 文档](https://kubernetes.io/zh-cn/docs/)

### 参考链接
- **GitHub 仓库**：https://github.com/DrDrZ95/ManusProject
- **Manus 项目**：https://manus.im/ (代码生成工具)

---

## 📞 支持与反馈

### 获取帮助

- 📖 查看 [完整文档](./docs/)
- 🐛 [报告 Bug](https://github.com/DrDrZ95/ManusProject/issues)
- 💡 [请求功能](https://github.com/DrDrZ95/ManusProject/issues)
- 💬 [讨论问题](https://github.com/DrDrZ95/ManusProject/discussions)

### 社区支持

- 查看现有 Issues 和 Discussions
- 在 GitHub Discussions 参与讨论
- 贡献改进和错误修复

---

## 📊 项目统计

- **编程语言**：C#, TypeScript, Python
- **框架版本**：.NET 8.0, React 18+
- **代码行数**：15,000+
- **模块数量**：20+
- **文档页数**：60+

---

## 🎯 路线图

### Q1 已完成 ✅
- 核心 AI 代理框架
- 工作流管理系统
- RAG 实现
- Docker 部署支持
- 系统架构优化
- 模块化重构

### Q1-Q2 进行中 🚀
- 高级缓存策略优化
- WebSearch 增强和扩展
- 模型微调工具完善
- 性能基准测试

### Q2-Q3 计划中 🔮
- 多语言支持（中英日韩）
- 更多 LLM 集成
- 社区插件系统
- 桌面客户端 (Electron)
- 移动应用支持 (React Native)
- GraphQL API 层

---

**所有文件和解决方案逻辑由 Manus 协助生成。参考：https://manus.im/**

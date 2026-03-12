# RAG 部署教程（Docker / Linux）

本文面向运维/IT 人员，描述如何在 Linux 上以 Docker（推荐）或原生进程方式部署本项目的 RAG（Retrieval-Augmented Generation，检索增强生成）能力，并给出生产环境注意事项。

## 1. 架构概览

RAG 的典型链路：

- 文档导入（Ingestion）：将文档切分（chunking）→ 计算 embedding → 写入向量库
- 在线检索（Retrieval）：对用户 query 计算 embedding → 向量相似检索 → 取回相关片段
- 生成（Generation）：把“检索到的片段 + 用户问题 + 指令”交给 LLM 生成最终回答

本项目中，通常由以下组件组成：

- Agent WebAPI（.NET 8）：提供业务 API、RAG 调用入口、工作流能力
- PostgreSQL：存储业务元数据/工作流/任务等结构化数据
- Redis：缓存、后台任务/队列相关存储（项目内也用于部分中间状态）
- ChromaDB：向量数据库（存储 embedding、向量索引、相似度检索）
- LLM/Embedding 模型服务：
  - OpenAI / Azure OpenAI（直接对接云服务）
  - 或者自建模型服务（仅当代码中接入了自建推理端点时）

关键配置位置：

- 向量库：`ConnectionStrings:ChromaDbConnection`（示例见 [appsettings.json](file:///c:/Code/TraeProject/apps/agent-api/Agent.Api/appsettings.json#L16-L21)）
- 关系库：`ConnectionStrings:DefaultConnection`
- Redis：`Redis:ConnectionString`
- LLM/Embedding：`SemanticKernel:*`（绑定逻辑见 [SemanticKernelExtensions.cs](file:///c:/Code/TraeProject/apps/agent-api/Agent.Api/Extensions/SemanticKernelExtensions.cs#L16-L101)）

## 2. Docker 部署（推荐）

### 2.1 前置条件

- Linux x86_64
- Docker Engine + docker compose plugin
- 对外访问策略：至少开放 API 端口（80/443 或自定义），以及内部组件端口（PostgreSQL 5432、Redis 6379、ChromaDB 8000）建议仅在内网可达

### 2.2 启动依赖组件（PostgreSQL / Redis / ChromaDB 等）

仓库已内置基础设施编排文件：

- 主编排： [infra/docker/docker-compose.yml](file:///c:/Code/TraeProject/infra/docker/docker-compose.yml)
- PostgreSQL： [infra/docker/database/postgres/docker-compose.yml](file:///c:/Code/TraeProject/infra/docker/database/postgres/docker-compose.yml)
- Redis： [infra/docker/cache/redis/docker-compose.yml](file:///c:/Code/TraeProject/infra/docker/cache/redis/docker-compose.yml)
- ChromaDB： [infra/docker/vector/chromadb/docker-compose.yml](file:///c:/Code/TraeProject/infra/docker/vector/chromadb/docker-compose.yml)

在仓库根目录执行：

```bash
cd infra/docker
docker compose up -d
```

验证组件健康状况：

```bash
docker ps
docker compose ps
curl -f http://localhost:8000/api/v1/heartbeat
```

### 2.3 构建 Agent WebAPI 镜像

仓库提供了 .NET WebAPI 的 Dockerfile： [Dockerfile.webapi](file:///c:/Code/TraeProject/infra/docker/Dockerfile.webapi)

在仓库根目录执行（注意 build context 选择 `apps/agent-api`）：

```bash
docker build -f infra/docker/Dockerfile.webapi -t agentwebapi:local apps/agent-api
```

### 2.4 启动 Agent WebAPI 容器（示例）

推荐用环境变量覆盖敏感信息（API Key、密码、JWT Secret 等），避免写入镜像或仓库配置文件。

```bash
docker run -d --name agentwebapi \
  -p 8080:80 \
  -e ASPNETCORE_URLS=http://+:80 \
  -e ConnectionStrings__DefaultConnection="Host=127.0.0.1;Port=5432;Database=ai_agent_db;Username=ai_agent_user;Password=ai_agent_password" \
  -e ConnectionStrings__ChromaDbConnection="http://127.0.0.1:8000" \
  -e Redis__ConnectionString="127.0.0.1:6379" \
  -e JwtSettings__Secret="REPLACE_WITH_A_LONG_RANDOM_SECRET" \
  -e SemanticKernel__OpenAIApiKey="REPLACE_WITH_KEY" \
  -e SemanticKernel__ChatModel="gpt-3.5-turbo" \
  -e SemanticKernel__EmbeddingModel="text-embedding-3-small" \
  agentwebapi:local
```

如果走 Azure OpenAI，则使用以下变量（见 [SemanticKernelOptions.cs](file:///c:/Code/TraeProject/apps/agent-api/Agent.Application/Services/SemanticKernel/SemanticKernelOptions.cs#L7-L43)）：

- `SemanticKernel__AzureOpenAIEndpoint`
- `SemanticKernel__AzureOpenAIApiKey`
- `SemanticKernel__AzureChatDeploymentName`
- `SemanticKernel__AzureEmbeddingDeploymentName`

### 2.5 数据持久化（非常重要）

数据库/向量库的数据不能放在容器可写层，否则容器重建会丢失数据。

当前 compose 中已为关键组件定义了卷（示例）：

- PostgreSQL：`postgres_data:/var/lib/postgresql/data`
- Redis：`redis_data:/data`
- ChromaDB：`chromadb_data:/chroma/data`（见 [vector/chromadb/docker-compose.yml](file:///c:/Code/TraeProject/infra/docker/vector/chromadb/docker-compose.yml#L12-L25)）

生产建议：

- 使用显式绑定目录，例如 `/data/postgres`、`/data/redis`、`/data/chromadb`
- 对这些目录做常规备份与容量监控

## 3. Linux 原生部署（不使用 Docker）

适用于需要接入企业进程管理（systemd）、或容器受限的环境。此模式仍然建议把 PostgreSQL/Redis/ChromaDB 以托管服务或容器方式提供。

### 3.1 发布 WebAPI

```bash
cd apps/agent-api/Agent.Api
dotnet publish -c Release -o /opt/agentwebapi
```

### 3.2 以 systemd 管理

示例（路径可根据实际调整）：

- 工作目录：`/opt/agentwebapi`
- 环境变量：放入 `/etc/agentwebapi/agentwebapi.env`
- 日志：由 journald 收集

运行时关键环境变量与 Docker 一致（`ConnectionStrings__*`、`Redis__*`、`SemanticKernel__*`、`JwtSettings__Secret`）。

## 4. IT 运维注意点（RAG 与 RDS/NoSQL 的差异）

### 4.1 向量库不是关系库（也不是典型 NoSQL）

向量库（如 ChromaDB）和 RDS/NoSQL 的主要差异：

- 查询方式不同：核心是“相似度检索”（top-k / cosine / dot-product），不是 SQL 的 join/filter 为主
- 更新语义不同：通常是 upsert 文档/向量；如果原文修改，往往需要重新切分 + 重算 embedding 才能保持检索质量
- 索引构建与一致性：向量索引可能存在重建/增量更新成本，写入后检索生效可能受内部机制影响（实现依赖向量库）
- 备份策略：不要只备份 PostgreSQL；向量库卷也必须纳入备份/快照范围
- 容量评估：向量维度、chunk 数量、冗余存储会显著影响磁盘和内存，需要提前压测

### 4.2 “不能直接更新 /xxx”的常见原因与正确做法

在容器化部署中，经常会遇到“无法更新 /xxx、修改容器内文件无效、重启后丢失”等问题，原因是：

- 镜像层通常视为只读或不可变，容器重建会恢复到镜像内容
- 正确的变更方式应该是“改配置 → 重新发布镜像/重启服务”，而不是进入容器 `vi` 改文件

建议约定：

- `/app`（应用目录）视为只读，不做在线修改
- 所有需要持久化/可更新的内容（数据、索引、缓存、上传文件）统一挂载到明确目录，例如 `/data`
- 配置用环境变量/配置中心/secret 注入，而不是改容器内 `appsettings.json`

### 4.3 变更与发布策略

- 模型变更（Embedding 模型/维度变更）会导致历史向量不可直接复用：通常需要重新生成 embedding 并重建索引
- 文档清洗/切分策略变更同样会影响检索效果：建议版本化并可回滚
- 生产环境不要使用仓库内示例密码/JWT secret；必须替换为安全值并走 secret 管理

### 4.4 网络与安全

- PostgreSQL/Redis/ChromaDB 建议仅内网访问，或通过安全组/防火墙限制来源
- API Key（OpenAI/Azure OpenAI）只通过环境变量/secret 注入，不写入镜像与仓库
- 若需要审计：建议开启 API 网关/反向代理层访问日志，并对敏感参数脱敏

## 5. 常见故障排查

- ChromaDB 连接失败：检查 `ConnectionStrings__ChromaDbConnection`、容器网络、端口映射、健康检查 `heartbeat`
- 检索无结果或质量差：检查是否完成导入、embedding 模型是否一致、切分策略是否过大/过小、向量库是否持久化丢失
- 认证失败：检查 `JwtSettings__Secret` 是否一致（多副本必须相同），以及时钟偏差

## 6. 推荐的最小生产清单

- PostgreSQL：主从/托管 RDS + 定期备份 + 监控
- Redis：哨兵/集群或托管服务 + 持久化策略明确
- ChromaDB：持久化卷 + 备份策略 + 容量/性能压测
- Agent WebAPI：至少 2 副本 + 健康检查 + 灰度发布
- Secrets：集中管理（Vault / KMS / K8s Secrets 等）


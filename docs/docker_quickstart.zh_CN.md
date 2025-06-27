# Docker 部署和快速入门指南

本指南提供了使用 Docker 部署 AI Agent 应用程序的说明，包括 .NET Web API 后端、React 前端以及可选的基于 Python 的 Qwen3 模型服务器。

## 前提条件

- 系统上已安装 [Docker](https://docs.docker.com/get-docker/) 和 [Docker Compose](https://docs.docker.com/compose/install/)
- Git 用于克隆仓库

## 使用 Docker 快速开始

### 1. 克隆仓库

```bash
git clone https://github.com/DrDrZ95/AI-AgentProject.git
cd AI-AgentProject
```

### 2. 使用 Docker Compose 构建和运行

项目包含用于轻松部署所有组件的 Docker 配置：

```bash
cd docker
docker-compose up -d
```

这将：
- 构建并启动端口 5000 上的 .NET Web API
- 构建并启动端口 3000 上的 React UI
- （可选）构建并启动端口 2025 上的 Python 模型服务器（如果在 docker-compose.yml 中取消注释）

### 3. 访问应用程序

- React UI：http://localhost:3000
- .NET Web API：http://localhost:5000
- Swagger UI：http://localhost:5000/swagger（如果启用）

## Docker 组件

Docker 设置包括：

1. **Web API 容器**：.NET 8.0 后端服务
   - Dockerfile：`docker/Dockerfile.webapi`
   - 内部暴露端口 80/443，外部映射到 5000/5001

2. **React UI 容器**：带有银色主题聊天界面的前端
   - Dockerfile：`docker/Dockerfile.react`
   - 使用 Nginx 提供静态文件服务
   - 配置为将 API 请求代理到后端
   - 内部暴露端口 80，外部映射到 3000

3. **Python 模型服务器容器**（可选）：
   - Dockerfile：`docker/Dockerfile.python`
   - 托管 Qwen3-4B-Instruct 模型
   - 暴露端口 2025

## 开发设置

对于开发，您可以分别运行每个组件：

### .NET Web API

```bash
cd AgentWebApi
dotnet run
```

### React UI

```bash
cd AgentUI/agent-chat
pnpm install
pnpm run dev
```

### Python 模型服务器

```bash
# 设置 Python 环境
./scripts/setup_environment.sh
source venv/bin/activate

# 下载模型（如果尚未下载）
./scripts/download_model.sh

# 运行服务器
python src/model_server.py
```

## 流式传输支持

React 应用程序内置了对 LLM API 流式响应的支持：

- 使用 `eventsource-parser` 和 `@microsoft/fetch-event-source` 库
- 支持服务器发送事件 (SSE) 进行实时流式传输
- 处理重新连接和错误场景
- 准备与后端 LLM API 集成

React 组件中的流式传输使用示例：

```typescript
import { StreamingService } from '../services/StreamingService';

// 在您的组件中：
const handleStreamingRequest = async (prompt: string) => {
  let fullResponse = '';
  
  await StreamingService.streamLLMRequest(
    'http://localhost:5000/api/llm/generate',
    { prompt },
    (chunk) => {
      // 处理每个到达的数据块
      fullResponse += chunk;
      setPartialResponse(fullResponse);
    },
    () => {
      // 处理完成
      console.log('流式传输完成');
    },
    (error) => {
      // 处理错误
      console.error('流式传输错误:', error);
    }
  );
};
```

## 配置

### 环境变量

您可以通过在 docker-compose.yml 文件中设置环境变量来自定义部署：

- `ASPNETCORE_ENVIRONMENT`：设置为 `Development` 或 `Production`
- 根据需要添加自定义环境变量

### 卷

对于持久数据，取消注释并配置 docker-compose.yml 中的卷部分。

## 故障排除

- **容器无法启动**：使用 `docker logs <容器名称>` 检查日志
- **网络问题**：确保端口未被其他应用程序使用
- **模型服务器内存**：根据您的模型大小调整 docker-compose.yml 中的内存限制

## 后续步骤

- 配置 .NET 后端与 Python 模型服务器通信
- 实现身份验证和授权
- 添加监控和日志记录
- 设置 CI/CD 管道进行自动部署

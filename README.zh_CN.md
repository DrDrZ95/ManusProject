# Qwen2-7B-Instruct AI 代理应用 (集成 .NET Web API)

[English Documentation](README.md)

本代码仓库包含一个 AI 代理应用。它先前在本地部署了 Qwen2-7B-Instruct 模型，并通过 2025 端口提供一个基于 Python 的 FastAPI 服务。现在，项目结构已更新，以包含一个 .NET 8.0 Web API 后端和一个 React 前端。

## 项目概述

本项目旨在提供一个解决方案，用于：

1.  托管 AI 模型推理 (当前通过现有 Python 脚本实现 Qwen2-7B-Instruct 服务)。
2.  提供一个 .NET 8.0 Web API 后端 (`AgentWebApi/`)，用于代理逻辑和未来的集成。
3.  为基于 React 的用户界面 (`AgentUI/`) 预留空间。
4.  支持使用 Unsloth 和 LoRA 进行模型微调以实现定制化。

该实现包含全面的文档、用于 Python 模型服务的设置脚本，以及新搭建的 .NET Web API 项目。

## 代码仓库结构

```
ai-agent/
├── AgentWebApi/            # .NET 8.0 Web API 项目
│   ├── Controllers/
│   ├── Properties/
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── AgentWebApi.csproj
│   └── Program.cs
├── AgentUI/                # React 前端应用，带聊天界面
│   └── agent-chat/         # 带银色主题的 React 聊天应用
├── README.md               # 主要项目文档 (英文)
├── README.zh_CN.md         # 主要项目文档 (简体中文)
├── config/                 # 配置文件 (如果需要，当前为空)
├── docker/                 # Docker 部署配置
│   ├── Dockerfile.webapi   # .NET Web API 的 Dockerfile
│   ├── Dockerfile.react    # React UI 的 Dockerfile
│   ├── Dockerfile.python   # Python 模型服务器的 Dockerfile
│   ├── docker-compose.yml  # Docker Compose 配置
│   └── nginx.conf          # React UI 的 Nginx 配置
├── docs/                   # 文档目录
│   ├── api_documentation.md    # API 文档 (针对 Python/FastAPI 模型服务器)
│   ├── api_documentation.zh_CN.md # API 文档 (简体中文 - 针对 Python/FastAPI)
│   ├── docker_quickstart.md    # Docker 部署指南
│   ├── docker_quickstart.zh_CN.md # Docker 部署指南 (简体中文)
│   ├── environment_setup.md    # 环境设置指南 (针对 Python 模型服务器)
│   ├── environment_setup.zh_CN.md # 环境设置指南 (简体中文 - 针对 Python 模型服务器)
│   ├── github_upload.md        # GitHub 上传指南
│   ├── github_upload.zh_CN.md  # GitHub 上传指南 (简体中文)
│   ├── ssh_setup.md            # SSH 密钥设置指南
│   ├── ssh_setup.zh_CN.md      # SSH 密钥设置指南 (简体中文)
│   ├── unsloth_lora_finetuning.md    # Unsloth+LoRA 微调指南
│   └── unsloth_lora_finetuning.zh_CN.md # Unsloth+LoRA 微调指南 (简体中文)
├── finetune/               # 模型微调工具
│   ├── install_dependencies.sh  # 安装 Unsloth 和 LoRA 依赖的脚本
│   └── utils.py                 # 微调工具函数
├── models/                 # 模型文件目录 (由 Python 脚本填充)
│   └── Qwen2-7B-Instruct/  # Qwen2-7B-Instruct 模型文件
├── scripts/                # 设置和实用工具脚本 (主要用于 Python 模型服务器)
│   ├── download_model.sh       # 下载 Qwen 模型的脚本
│   ├── install_dependencies.sh # 安装 Python 系统依赖的脚本
│   ├── run_model_server.sh     # 运行 Python/FastAPI 模型服务器的脚本
│   └── setup_environment.sh    # 设置 Python 环境的脚本
├── src/                    # 源代码 (Python/FastAPI 模型服务器)
│   ├── api_examples.py         # API 使用示例 (针对 Python/FastAPI 服务器)
│   └── model_server.py         # Python/FastAPI 服务器实现
├── .gitignore              # 指定 Git 应忽略的有意未跟踪的文件
└── venv/                   # Python 虚拟环境 (由 setup_environment.sh 创建)
```

**注意：** `src/` 和 `scripts/` 中现有的基于 Python/FastAPI 的模型服务器仍然存在。`AgentWebApi/` 中的新 `.NET Web API` 是一个独立的组件。

## 快速开始

### Docker 部署 (推荐)

对于最快速的设置，使用 Docker 一起部署所有组件：

```bash
cd docker
docker-compose up -d
```

这将构建并启动所有服务。有关详细说明，请参阅 [Docker 快速入门指南](docs/docker_quickstart.zh_CN.md)。

### 手动设置

#### 系统要求

*   **针对 Python Qwen2-7B-Instruct 模型服务器：** (请参阅 `docs/environment_setup.zh_CN.md`)
    *   基于 Linux 的操作系统，Python 3.8+，16GB+ RAM，20GB+ 磁盘空间，推荐使用 GPU。
*   **针对 .NET 8.0 Web API (`AgentWebApi/`)：**
    *   .NET 8.0 SDK (已在此环境中安装)。
*   **针对 React UI (`AgentUI/agent-chat/`)：**
    *   Node.js 和 pnpm (已在项目中设置)
*   **针对模型微调 (`finetune/`)：**
    *   推荐使用兼容 CUDA 的 GPU
    *   参见 [Unsloth+LoRA 微调指南](docs/unsloth_lora_finetuning.zh_CN.md)

#### 设置与运行

**1. Python Qwen2-7B-Instruct 模型服务器 (端口 2025)：**

   请遵循 [环境设置指南](docs/environment_setup.zh_CN.md) 中的说明来设置和运行基于 Python 的 Qwen 模型服务器。这包括：
   ```bash
   cd /home/ubuntu/ai-agent
   chmod +x scripts/*.sh
   ./scripts/install_dependencies.sh
   ./scripts/setup_environment.sh
   ./scripts/download_model.sh
   ./scripts/run_model_server.sh # 运行于 http://localhost:2025
   ```

**2. .NET 8.0 Web API (`AgentWebApi/`)：**

   要运行 .NET Web API (它将在不同的端口上运行，通常 Kestrel 默认为 5000/5001 或 HTTPS 的 7000 系列端口)：
   ```bash
   cd /home/ubuntu/ai-agent/AgentWebApi
   dotnet run
   ```
   此 API 当前是默认模板，尚未与 Python 模型服务器交互。

**3. React UI (`AgentUI/agent-chat/`)：**

   要运行 React 聊天应用：
   ```bash
   cd /home/ubuntu/ai-agent/AgentUI/agent-chat
   pnpm install
   pnpm run dev
   ```
   这将启动开发服务器，您可以在浏览器中访问聊天界面。

**4. 模型微调 (可选)：**

   要设置微调环境：
   ```bash
   cd /home/ubuntu/ai-agent
   chmod +x finetune/install_dependencies.sh
   ./finetune/install_dependencies.sh
   source finetune/venv/bin/activate
   ```
   
   有关详细使用说明，请参阅 [Unsloth+LoRA 微调指南](docs/unsloth_lora_finetuning.zh_CN.md)。

## API 使用 (Python/FastAPI 模型服务器)

一旦 Python 服务器在 2025 端口上运行，您就可以使用 `src/api_examples.py` 与其交互。请参阅 [API 文档](docs/api_documentation.zh_CN.md)。

## 流式传输支持

React 应用程序内置了对 LLM API 流式响应的支持：

- 使用 `eventsource-parser` 和 `@microsoft/fetch-event-source` 库
- 支持服务器发送事件 (SSE) 进行实时流式传输
- 处理重新连接和错误场景
- 准备与后端 LLM API 集成

## 模型微调

该项目包含使用 Unsloth 和 LoRA 微调语言模型的工具：

- 与标准方法相比，训练速度显著提高
- 通过参数高效的微调减少内存需求
- 用于数据集准备、模型加载和训练的综合工具
- 详情请参阅 [Unsloth+LoRA 微调指南](docs/unsloth_lora_finetuning.zh_CN.md)

## 详细文档

*   部署：
    *   [Docker 快速入门指南](docs/docker_quickstart.zh_CN.md)
*   Python 模型服务器：
    *   [环境设置指南](docs/environment_setup.zh_CN.md)
    *   [API 文档](docs/api_documentation.zh_CN.md)
*   模型微调：
    *   [Unsloth+LoRA 微调指南](docs/unsloth_lora_finetuning.zh_CN.md)
*   通用：
    *   [SSH 密钥设置指南](docs/ssh_setup.zh_CN.md)
    *   [GitHub 上传指南](docs/github_upload.zh_CN.md)

## 模型信息

本项目将阿里云的 Qwen2-7B-Instruct 模型用于基于 Python 的推理服务器。

## 许可证

项目框架：MIT 许可证。Qwen2-7B-Instruct 模型受其自身许可证的约束。

## 致谢

- 阿里云通义千问团队、Hugging Face、FastAPI、.NET 团队、Unsloth 团队。

# Qwen2-7B-Instruct AI 代理应用 (集成 .NET Web API)

本代码仓库包含一个 AI 代理应用。它先前在本地部署了 Qwen2-7B-Instruct 模型，并通过 2025 端口提供一个基于 Python 的 FastAPI 服务。现在，项目结构已更新，以包含一个 .NET 8.0 Web API 后端和一个 React 前端占位符。

## 项目概述

本项目旨在提供一个解决方案，用于：

1.  托管 AI 模型推理 (当前通过现有 Python 脚本实现 Qwen2-7B-Instruct 服务)。
2.  提供一个 .NET 8.0 Web API 后端 (`AgentWebApi/`)，用于代理逻辑和未来的集成。
3.  为基于 React 的用户界面 (`AgentUI/`) 预留空间。

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
├── AgentUI/                # React 前端应用占位符
├── README.md               # 主要项目文档 (英文)
├── README.zh_CN.md         # 主要项目文档 (简体中文)
├── config/                 # 配置文件 (如果需要，当前为空)
├── docs/                   # 文档目录
│   ├── api_documentation.md    # API 文档 (针对 Python/FastAPI 模型服务器)
│   ├── api_documentation.zh_CN.md # API 文档 (简体中文 - 针对 Python/FastAPI)
│   ├── environment_setup.md    # 环境设置指南 (针对 Python 模型服务器)
│   ├── environment_setup.zh_CN.md # 环境设置指南 (简体中文 - 针对 Python 模型服务器)
│   ├── github_upload.md        # GitHub 上传指南
│   ├── github_upload.zh_CN.md  # GitHub 上传指南 (简体中文)
│   ├── ssh_setup.md            # SSH 密钥设置指南
│   └── ssh_setup.zh_CN.md      # SSH 密钥设置指南 (简体中文)
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

### 系统要求

*   **针对 Python Qwen2-7B-Instruct 模型服务器：** (请参阅 `docs/environment_setup.zh_CN.md`)
    *   基于 Linux 的操作系统，Python 3.8+，16GB+ RAM，20GB+ 磁盘空间，推荐使用 GPU。
*   **针对 .NET 8.0 Web API (`AgentWebApi/`)：**
    *   .NET 8.0 SDK (已在此环境中安装)。

### 设置与运行

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

**3. React UI (`AgentUI/`)：**

   此文件夹当前是一个占位符。要在此处开发 React 应用，您通常会在 `AgentUI` 目录中使用 `npx create-react-app .` 或类似命令。

## API 使用 (Python/FastAPI 模型服务器)

一旦 Python 服务器在 2025 端口上运行，您就可以使用 `src/api_examples.py` 与其交互。请参阅 [API 文档](docs/api_documentation.zh_CN.md)。

## 详细文档

*   Python 模型服务器：
    *   [环境设置指南](docs/environment_setup.zh_CN.md)
    *   [API 文档](docs/api_documentation.zh_CN.md)
*   通用：
    *   [SSH 密钥设置指南](docs/ssh_setup.zh_CN.md)
    *   [GitHub 上传指南](docs/github_upload.zh_CN.md)

## 模型信息

本项目将阿里云的 Qwen2-7B-Instruct 模型用于基于 Python 的推理服务器。

## 许可证

项目框架：MIT 许可证。Qwen2-7B-Instruct 模型受其自身许可证的约束。

## 致谢

- 阿里云通义千问团队、Hugging Face、FastAPI、.NET 团队。

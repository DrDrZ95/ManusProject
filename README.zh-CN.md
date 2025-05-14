# Qwen2-7B-Instruct AI 代理应用

本代码仓库包含一个 AI 代理应用的完整实现，该应用在本地部署 Qwen2-7B-Instruct 模型，并通过 2025 端口提供 API 访问。

## 项目概述

本项目提供了一个完整的解决方案，用于：

1.  在本地部署 Qwen2-7B-Instruct 语言模型
2.  通过 2025 端口以 RESTful API 的形式暴露模型服务
3.  提供与 API 交互的工具

该实现包含全面的文档、设置脚本和示例代码，以帮助您快速上手。

## 代码仓库结构

```
ai-agent/
├── README.md               # 主要项目文档 (英文)
├── README.zh-CN.md         # 主要项目文档 (简体中文)
├── config/                 # 配置文件 (如果需要，当前为空)
├── docs/                   # 文档目录
│   ├── api_documentation.md    # API 文档 (英文)
│   ├── api_documentation.zh-CN.md # API 文档 (简体中文)
│   ├── environment_setup.md    # 环境设置指南 (英文)
│   ├── environment_setup.zh-CN.md # 环境设置指南 (简体中文)
│   ├── github_upload.md        # GitHub 上传指南 (英文)
│   ├── github_upload.zh-CN.md  # GitHub 上传指南 (简体中文)
│   ├── ssh_setup.md            # SSH 密钥设置指南 (英文)
│   └── ssh_setup.zh-CN.md      # SSH 密钥设置指南 (简体中文)
├── models/                 # 模型文件目录 (在设置过程中填充)
│   └── Qwen2-7B-Instruct/  # Qwen2-7B-Instruct 模型文件
├── scripts/                # 设置和实用工具脚本
│   ├── download_model.sh       # 下载 Qwen 模型的脚本
│   ├── install_dependencies.sh # 安装系统依赖的脚本
│   ├── run_model_server.sh     # 运行模型服务器的脚本
│   └── setup_environment.sh    # 设置 Python 环境的脚本
├── src/                    # 源代码目录
│   ├── api_examples.py         # API 使用示例
│   └── model_server.py         # FastAPI 服务器实现
└── venv/                   # Python 虚拟环境 (由 setup_environment.sh 创建)
```

## 快速开始

### 系统要求

- 基于 Linux 的操作系统 (推荐 Ubuntu 20.04 或更高版本)
- Python 3.8 或更高版本
- 至少 16GB RAM (Qwen2-7B 模型可能占用较多资源)
- 至少 20GB 可用磁盘空间 (用于模型和依赖项)
- 强烈推荐使用具有 >= 10GB VRAM 的 CUDA 兼容 GPU (例如 NVIDIA Tesla T4, V100, A100) 以获得合理的性能。仅 CPU 设置是可能的，但会非常慢。

### 设置步骤

1.  克隆此代码仓库 (假设您已在 GitHub 上设置了它)：
    ```bash
    git clone git@github.com:your-username/ai-agent.git
    cd ai-agent
    ```
    (如果您在本地工作且尚未使用 GitHub，只需导航到 `/home/ubuntu/ai-agent` 目录)

2.  使脚本可执行：
    ```bash
    chmod +x scripts/*.sh
    chmod +x src/*.py
    ```

3.  安装系统依赖：
    ```bash
    ./scripts/install_dependencies.sh
    ```

4.  设置 Python 环境并安装 Python 包：
    ```bash
    ./scripts/setup_environment.sh
    ```

5.  下载 Qwen2-7B-Instruct 模型：
    ```bash
    ./scripts/download_model.sh
    ```

6.  启动模型服务器：
    ```bash
    ./scripts/run_model_server.sh
    ```

API 将在 `http://localhost:2025` 上可用。

## API 使用

服务器运行后，您可以使用提供的 API 示例与其交互。打开一个新的终端并运行：

```bash
# 首先激活虚拟环境
source venv/bin/activate

# 使用默认提示进行基本使用
python src/api_examples.py

# 自定义提示
python src/api_examples.py --prompt "写一首关于星星的短诗。"

# 自定义参数
python src/api_examples.py --max-length 100 --temperature 0.5
```

有关详细的 API 文档，请参阅 [API 文档](docs/api_documentation.zh-CN.md)。

## 详细文档

- [环境设置指南](docs/environment_setup.zh-CN.md): 环境设置说明
- [API 文档](docs/api_documentation.zh-CN.md): 详细的 API 文档
- [SSH 密钥设置指南](docs/ssh_setup.zh-CN.md): GitHub SSH 密钥设置说明
- [GitHub 上传指南](docs/github_upload.zh-CN.md): 将代码上传到 GitHub 的说明

## 模型信息

本项目使用阿里云 Qwen团队 开发的 Qwen 系列中的 Qwen2-7B-Instruct 模型。它是一个强大的语言模型，专为指令遵循和聊天而设计，适用于广泛的应用，包括：

- 代码生成和解释
- 文本补全和摘要
- 问题回答
- 创意写作和内容生成
- 对话式 AI

## 许可证

本项目框架根据 MIT 许可证授权。Qwen2-7B-Instruct 模型本身受其自身许可证条款 (通常是 通义千问 LICENSE AGREEMENT) 的约束。请确保遵守模型的许可证。

## 致谢

- [阿里云通义千问团队](https://github.com/QwenLM) 提供 Qwen2 模型
- [Hugging Face](https://huggingface.co/) 提供 Transformers 库和模型托管
- [FastAPI](https://fastapi.tiangolo.com/) 提供 API 框架

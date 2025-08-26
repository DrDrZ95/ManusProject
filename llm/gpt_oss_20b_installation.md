# gpt-oss 20B 安装指南

## 概述

本指南将详细介绍如何安装和配置 gpt-oss 20B 模型，包括其命令行界面（CLI）工具。gpt-oss 20B 是一个强大的开源语言模型，适用于各种自然语言处理任务。

## 环境要求

在开始安装之前，请确保您的系统满足以下要求：

*   **操作系统**: Linux (推荐 Ubuntu 20.04 或更高版本)
*   **Python**: 3.8 或更高版本
*   **CUDA**: 11.8 或更高版本 (如果使用 GPU)
*   **cuDNN**: 对应 CUDA 版本的 cuDNN (如果使用 GPU)
*   **内存**: 至少 64GB RAM (推荐 128GB 或更高)
*   **GPU**: 至少 24GB 显存 (推荐 48GB 或更高，例如 NVIDIA A100)
*   **磁盘空间**: 至少 200GB 可用空间

## 安装步骤

### 步骤 1: 克隆仓库

首先，从 GitHub 克隆 gpt-oss 20B 的官方仓库：

```bash
git clone https://github.com/gpt-oss/gpt-oss-20b.git
cd gpt-oss-20b
```

### 步骤 2: 创建并激活 Python 虚拟环境

为了避免依赖冲突，强烈建议使用虚拟环境：

```bash
python3 -m venv venv
source venv/bin/activate
```

### 步骤 3: 安装依赖

安装所有必要的 Python 依赖：

```bash
pip install -r requirements.txt
```

如果您计划使用 GPU，请确保安装了相应的 PyTorch 版本：

```bash
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

### 步骤 4: 下载模型权重

gpt-oss 20B 的模型权重通常需要单独下载。请查阅官方仓库的 `README.md` 文件以获取最新的下载链接和说明。通常，您会找到一个下载脚本或直接的链接。

例如：

```bash
# 假设有一个下载脚本
bash download_model.sh
# 或者手动下载到指定目录，例如 ./models/
```

### 步骤 5: 配置环境变量 (可选)

如果模型权重或数据存储在非默认位置，您可能需要设置环境变量：

```bash
export GPT_OSS_MODEL_PATH=/path/to/your/model
export GPT_OSS_DATA_PATH=/path/to/your/data
```

### 步骤 6: 运行 CLI 工具

安装完成后，您可以通过以下命令运行 gpt-oss 20B 的 CLI 工具：

```bash
# 激活虚拟环境 (如果尚未激活)
source venv/bin/activate

# 运行 CLI
python -m gpt_oss.cli --help
```

一些常用的 CLI 命令示例：

*   **文本生成**: 
    ```bash
    python -m gpt_oss.cli generate --prompt "Once upon a time" --max_length 100
    ```
*   **模型评估**: 
    ```bash
    python -m gpt_oss.cli evaluate --dataset_path ./data/my_dataset.json
    ```
*   **模型微调**: 
    ```bash
    python -m gpt_oss.cli finetune --config_path ./configs/finetune_config.yaml
    ```

## 常见问题与故障排除

*   **CUDA 错误**: 确保您的 CUDA 和 cuDNN 版本与 PyTorch 兼容，并且驱动程序已正确安装。
*   **内存不足**: 尝试减少 `max_length` 或使用更小的批处理大小。考虑升级硬件。
*   **模型下载失败**: 检查网络连接，或尝试使用镜像站点下载。

如果您遇到任何问题，请查阅 gpt-oss 20B 的官方文档或 GitHub 仓库的 Issues 页面。



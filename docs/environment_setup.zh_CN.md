# Qwen3-4B-Instruct 模型环境设置

本文档提供了运行 Qwen3-4B-Instruct 模型所需的环境设置说明。

## 系统要求

- 基于 Linux 的操作系统 (推荐 Ubuntu 20.04 或更高版本)
- Python 3.8 或更高版本
- 至少 16GB RAM (Qwen3-4B 模型可能占用较多资源)
- 至少 20GB 可用磁盘空间 (用于模型和依赖项)
- 强烈推荐使用具有 >= 8GB VRAM 的 CUDA 兼容 GPU (例如 NVIDIA Tesla T4, V100, A100) 以获得合理的性能。仅 CPU 设置是可能的，但会非常慢。

## 设置过程

设置过程包括三个主要步骤，从 `/home/ubuntu/ai-agent` 目录执行：

1.  安装系统依赖项
2.  设置 Python 环境和包
3.  下载 Qwen3-4B-Instruct 模型

确保 `scripts/` 目录中的所有脚本都可执行：
```bash
chmod +x /home/ubuntu/ai-agent/scripts/*.sh
```

## 第 1 步：安装系统依赖项

运行以下脚本以安装所有必需的系统依赖项：

```bash
cd /home/ubuntu/ai-agent
./scripts/install_dependencies.sh
```

此脚本将安装：
- Python 开发包 (python3-dev, python3-pip, python3-venv)
- 构建工具 (build-essential, cmake)
- Git, wget, curl
- OpenBLAS 和 OpenMP 库 (用于潜在的基于 CPU 的操作)

## 第 2 步：设置 Python 环境

运行以下脚本以创建 Python 虚拟环境并安装所有必需的包：

```bash
cd /home/ubuntu/ai-agent
./scripts/setup_environment.sh
```

此脚本将：
- 在 `/home/ubuntu/ai-agent` 内的 `venv` 目录中创建 Python 虚拟环境。
- 安装 PyTorch 及相关库 (torchvision, torchaudio)。
- 安装 Transformers, Accelerate, BitsandBytes, SentencePiece, Protobuf, FastAPI, Uvicorn, Pydantic, Einops, 和 Tiktoken。

## 第 3 步：下载模型

运行以下脚本以下载 Qwen3-4B-Instruct 模型：

```bash
cd /home/ubuntu/ai-agent
./scripts/download_model.sh
```

此脚本将：
- 从 Hugging Face (`Qwen/Qwen3-4B-Instruct`) 下载 Qwen3-4B-Instruct 模型。
- 将模型保存到 `/home/ubuntu/ai-agent/models/Qwen3-4B-Instruct` 目录。

## 验证设置

完成上述所有步骤后，您可以通过以下方式验证设置是否成功：

1.  激活虚拟环境 (如果尚未激活)：
    ```bash
    cd /home/ubuntu/ai-agent
    source venv/bin/activate
    ```

2.  运行一个简单的测试来加载模型 (这可能需要一些时间和内存)：
    ```python
    python -c "from transformers import AutoModelForCausalLM, AutoTokenizer; import torch; model_path = '/home/ubuntu/ai-agent/models/Qwen3-4B-Instruct'; tokenizer = AutoTokenizer.from_pretrained(model_path, trust_remote_code=True); model = AutoModelForCausalLM.from_pretrained(model_path, trust_remote_code=True, device_map='auto', torch_dtype=torch.bfloat16 if torch.cuda.is_available() and torch.cuda.is_bf16_supported() else torch.float16 if torch.cuda.is_available() else torch.float32); print('Qwen3-4B-Instruct 模型加载成功！')"
    ```
    (注意：上面的 python 命令应该是一行。`torch_dtype` 部分用于在 CUDA 可用时进行优化加载。)

## 故障排除

如果在设置过程中遇到任何问题：

1.  检查所有系统依赖项是否已正确安装。
2.  确保您有足够的磁盘空间 (至少 20GB) 和 RAM (至少 16GB)。
3.  验证您的 Python 版本是否兼容 (3.8 或更高版本)。
4.  检查下载模型和包的互联网连接。
5.  如果使用 GPU，请确保 CUDA 驱动程序和工具包已正确安装并与 PyTorch 版本兼容。
6.  对于特定的错误消息，请参阅终端输出中的日志以及服务器运行后的 `model_server.log` 文件。

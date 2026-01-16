# 使用 ufoym/deepo 设置带有 Unsloth 和 CUDA 的 LLM 环境

本指南提供了使用 ufoym/deepo Docker 镜像作为基础，设置带有 Unsloth 和 CUDA 支持的大型语言模型 (LLM) 环境的分步说明。

## 简介

[ufoym/deepo](https://github.com/ufoym/deepo) 是一个预装了许多深度学习框架和工具的 Docker 镜像。我们将以此为基础，添加 Unsloth 以实现高效的 LLM 微调。

## 前提条件

- 系统上已安装 Docker
- 支持 CUDA 的 NVIDIA GPU
- 已安装 NVIDIA Container Toolkit (nvidia-docker2)

## 步骤 1：拉取 ufoym/deepo 镜像

首先拉取带有 CUDA 支持的 ufoym/deepo 镜像：

```bash
docker pull ufoym/deepo:all-py38-cu118
```

这将拉取带有 Python 3.8 和 CUDA 11.8 的镜像，它与大多数当前的机器学习库具有良好的兼容性。

## 步骤 2：创建用于 Unsloth 集成的 Dockerfile

创建一个扩展 deepo 镜像的新 Dockerfile：

```dockerfile
FROM ufoym/deepo:all-py38-cu118

# 设置工作目录
WORKDIR /workspace

# 安装系统依赖
RUN apt-get update && apt-get install -y \
    git \
    wget \
    curl \
    software-properties-common \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# 安装 Unsloth 及其依赖
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir \
    unsloth==2025.3.19 \
    bitsandbytes>=0.41.1 \
    accelerate>=0.23.0 \
    peft>=0.6.0 \
    transformers>=4.35.0 \
    trl>=0.7.4 \
    scipy \
    sentencepiece \
    protobuf \
    einops \
    wandb \
    datasets

# 安装用于微调的其他工具
RUN pip install --no-cache-dir \
    tensorboard \
    evaluate \
    scikit-learn \
    pandas \
    matplotlib \
    seaborn \
    tqdm

# 为模型和数据创建目录
RUN mkdir -p /workspace/models /workspace/data /workspace/output

# 设置环境变量
ENV PYTHONPATH="/workspace:${PYTHONPATH}"
ENV CUDA_VISIBLE_DEVICES="0"

# 创建一个验证安装的示例脚本
RUN echo 'import torch; print(f"PyTorch 版本: {torch.__version__}"); print(f"CUDA 可用: {torch.cuda.is_available()}"); print(f"CUDA 版本: {torch.version.cuda}"); print(f"GPU 数量: {torch.cuda.device_count()}"); print(f"GPU 名称: {torch.cuda.get_device_name(0) if torch.cuda.is_available() else \"None\"}")' > /workspace/verify_install.py

# 设置默认命令
CMD ["/bin/bash"]
```

## 步骤 3：构建自定义 Docker 镜像

构建您的自定义 Docker 镜像：

```bash
docker build -t unsloth-llm-env:latest -f Dockerfile .
```

## 步骤 4：使用 GPU 支持运行容器

使用 GPU 支持运行容器：

```bash
docker run --gpus all -it --rm \
  -v $(pwd)/data:/workspace/data \
  -v $(pwd)/models:/workspace/models \
  -v $(pwd)/output:/workspace/output \
  unsloth-llm-env:latest
```

此命令：
- 挂载本地目录用于数据、模型和输出
- 提供对所有 GPU 的访问
- 启动交互式终端会话

## 步骤 5：验证安装

在容器内，验证所有内容是否正确安装：

```bash
python /workspace/verify_install.py
```

您应该看到确认 PyTorch 版本、CUDA 可用性和 GPU 信息的输出。

## 步骤 6：使用 Unsloth 进行 LLM 微调

创建一个简单的微调脚本来测试环境：

```python
# 保存为 /workspace/test_unsloth.py
from unsloth import FastLanguageModel
import torch
from datasets import load_dataset

# 使用 Unsloth 优化加载模型
model, tokenizer = FastLanguageModel.from_pretrained(
    model_name="meta-llama/Llama-4-Scout-17B-16E-Instruct",
    max_seq_length=2048,
    dtype=torch.bfloat16,
    load_in_4bit=True,
    device_map="auto",
)

# 配置 LoRA
model = FastLanguageModel.get_peft_model(
    model,
    r=8,
    lora_alpha=16,
    target_modules=["q_proj", "v_proj"],
    lora_dropout=0.05,
    bias="none",
    task_type="SEQ_2_SEQ_LM"
)

# 打印模型信息
print(f"已加载模型: {model.__class__.__name__}")
print(f"使用设备: {next(model.parameters()).device}")
```

运行测试脚本：

```bash
python /workspace/test_unsloth.py
```

## 步骤 7：创建持久环境

要创建可重复使用的持久环境：

1. 创建一个 Docker Compose 文件：

```yaml
# docker-compose.yml
version: '3'
services:
  unsloth-env:
    image: unsloth-llm-env:latest
    container_name: unsloth-llm-container
    restart: unless-stopped
    volumes:
      - ./data:/workspace/data
      - ./models:/workspace/models
      - ./output:/workspace/output
      - ./scripts:/workspace/scripts
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]
    tty: true
    stdin_open: true
    command: /bin/bash
```

2. 启动环境：

```bash
docker-compose up -d
```

3. 连接到运行中的容器：

```bash
docker exec -it unsloth-llm-container bash
```

## 高级配置

### 内存优化

对于大型模型，您可能需要优化内存使用：

```python
# 内存优化设置
from accelerate import FullyShardedDataParallelPlugin, Accelerator
from torch.distributed.fsdp.fully_sharded_data_parallel import FullOptimStateDictConfig, FullStateDictConfig

fsdp_plugin = FullyShardedDataParallelPlugin(
    state_dict_config=FullStateDictConfig(offload_to_cpu=True, rank0_only=False),
    optim_state_dict_config=FullOptimStateDictConfig(offload_to_cpu=True, rank0_only=False),
)

accelerator = Accelerator(fsdp_plugin=fsdp_plugin)
```

### 多 GPU 训练

对于多 GPU 训练，修改您的脚本：

```python
# 多 GPU 训练设置
from accelerate import Accelerator

accelerator = Accelerator()
model, optimizer, training_dataloader = accelerator.prepare(
    model, optimizer, training_dataloader
)
```

## 故障排除

### CUDA 内存不足

如果遇到 CUDA 内存不足错误：

1. 减小批量大小
2. 使用梯度累积
3. 启用梯度检查点：

```python
model.gradient_checkpointing_enable()
```

### 训练速度慢

如果训练速度慢：

1. 检查是否使用了正确的 CUDA 版本
2. 确保使用混合精度训练
3. 通过适当的 num_workers 优化数据加载

## 结论

您现在拥有了一个基于 ufoym/deepo 镜像，带有 Unsloth 和 CUDA 支持的完全配置环境，用于 LLM 微调。这个设置为处理大型语言模型提供了强大而灵活的基础。

更多信息，请参考：
- [Unsloth 文档](https://github.com/unslothai/unsloth)
- [ufoym/deepo 仓库](https://github.com/ufoym/deepo)
- [PyTorch 文档](https://pytorch.org/docs/stable/index.html)

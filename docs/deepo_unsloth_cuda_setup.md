# Setting Up LLM Environment with Unsloth and CUDA Using ufoym/deepo

This guide provides step-by-step instructions for setting up a Large Language Model (LLM) environment with Unsloth and CUDA support using the ufoym/deepo Docker image as a base.

## Introduction

[ufoym/deepo](https://github.com/ufoym/deepo) is a Docker image that comes pre-installed with many deep learning frameworks and tools. We'll use this as our base and add Unsloth for efficient LLM fine-tuning.

## Prerequisites

- Docker installed on your system
- NVIDIA GPU with CUDA support
- NVIDIA Container Toolkit (nvidia-docker2) installed

## Step 1: Pull the ufoym/deepo Image

Start by pulling the ufoym/deepo image with CUDA support:

```bash
docker pull ufoym/deepo:all-py38-cu118
```

This pulls the image with Python 3.8 and CUDA 11.8, which provides good compatibility with most current ML libraries.

## Step 2: Create a Dockerfile for Unsloth Integration

Create a new Dockerfile that extends the deepo image:

```dockerfile
FROM ufoym/deepo:all-py38-cu118

# Set working directory
WORKDIR /workspace

# Install system dependencies
RUN apt-get update && apt-get install -y \
    git \
    wget \
    curl \
    software-properties-common \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install Unsloth and its dependencies
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

# Install additional utilities for fine-tuning
RUN pip install --no-cache-dir \
    tensorboard \
    evaluate \
    scikit-learn \
    pandas \
    matplotlib \
    seaborn \
    tqdm

# Create directories for models and data
RUN mkdir -p /workspace/models /workspace/data /workspace/output

# Set environment variables
ENV PYTHONPATH="/workspace:${PYTHONPATH}"
ENV CUDA_VISIBLE_DEVICES="0"

# Create a sample script to verify installation
RUN echo 'import torch; print(f"PyTorch version: {torch.__version__}"); print(f"CUDA available: {torch.cuda.is_available()}"); print(f"CUDA version: {torch.version.cuda}"); print(f"GPU count: {torch.cuda.device_count()}"); print(f"GPU name: {torch.cuda.get_device_name(0) if torch.cuda.is_available() else \"None\"}")' > /workspace/verify_install.py

# Set the default command
CMD ["/bin/bash"]
```

## Step 3: Build the Custom Docker Image

Build your custom Docker image:

```bash
docker build -t unsloth-llm-env:latest -f Dockerfile .
```

## Step 4: Run the Container with GPU Support

Run the container with GPU support:

```bash
docker run --gpus all -it --rm \
  -v $(pwd)/data:/workspace/data \
  -v $(pwd)/models:/workspace/models \
  -v $(pwd)/output:/workspace/output \
  unsloth-llm-env:latest
```

This command:
- Mounts local directories for data, models, and output
- Provides access to all GPUs
- Starts an interactive terminal session

## Step 5: Verify the Installation

Inside the container, verify that everything is installed correctly:

```bash
python /workspace/verify_install.py
```

You should see output confirming PyTorch version, CUDA availability, and GPU information.

## Step 6: Using Unsloth for LLM Fine-tuning

Create a simple fine-tuning script to test the environment:

```python
# save as /workspace/test_unsloth.py
from unsloth import FastLanguageModel
import torch
from datasets import load_dataset

# Load model with Unsloth optimization
model, tokenizer = FastLanguageModel.from_pretrained(
    model_name="Qwen/Qwen3-4B-Instruct",
    max_seq_length=2048,
    dtype=torch.bfloat16,
    load_in_4bit=True,
    device_map="auto",
)

# Configure LoRA
model = FastLanguageModel.get_peft_model(
    model,
    r=8,
    lora_alpha=16,
    target_modules=["q_proj", "v_proj"],
    lora_dropout=0.05,
    bias="none",
    task_type="SEQ_2_SEQ_LM"
)

# Print model information
print(f"Model loaded: {model.__class__.__name__}")
print(f"Using device: {next(model.parameters()).device}")
```

Run the test script:

```bash
python /workspace/test_unsloth.py
```

## Step 7: Creating a Persistent Environment

To create a persistent environment that you can reuse:

1. Create a Docker Compose file:

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

2. Start the environment:

```bash
docker-compose up -d
```

3. Connect to the running container:

```bash
docker exec -it unsloth-llm-container bash
```

## Advanced Configuration

### Memory Optimization

For large models, you may need to optimize memory usage:

```python
# Memory optimization settings
from accelerate import FullyShardedDataParallelPlugin, Accelerator
from torch.distributed.fsdp.fully_sharded_data_parallel import FullOptimStateDictConfig, FullStateDictConfig

fsdp_plugin = FullyShardedDataParallelPlugin(
    state_dict_config=FullStateDictConfig(offload_to_cpu=True, rank0_only=False),
    optim_state_dict_config=FullOptimStateDictConfig(offload_to_cpu=True, rank0_only=False),
)

accelerator = Accelerator(fsdp_plugin=fsdp_plugin)
```

### Multi-GPU Training

For multi-GPU training, modify your script:

```python
# Multi-GPU training setup
from accelerate import Accelerator

accelerator = Accelerator()
model, optimizer, training_dataloader = accelerator.prepare(
    model, optimizer, training_dataloader
)
```

## Troubleshooting

### CUDA Out of Memory

If you encounter CUDA out of memory errors:

1. Reduce batch size
2. Use gradient accumulation
3. Enable gradient checkpointing:

```python
model.gradient_checkpointing_enable()
```

### Slow Training

If training is slow:

1. Check if you're using the correct CUDA version
2. Ensure you're using mixed precision training
3. Optimize data loading with proper num_workers

## Conclusion

You now have a fully configured environment for LLM fine-tuning with Unsloth and CUDA support based on the ufoym/deepo image. This setup provides a powerful and flexible foundation for working with large language models.

For more information, refer to:
- [Unsloth Documentation](https://github.com/unslothai/unsloth)
- [ufoym/deepo Repository](https://github.com/ufoym/deepo)
- [PyTorch Documentation](https://pytorch.org/docs/stable/index.html)

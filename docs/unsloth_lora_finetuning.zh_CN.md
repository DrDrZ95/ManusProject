# 跨平台微调 Qwen3 模型指南

本指南提供了在不同平台上使用以下技术微调 Qwen3 语言模型的说明：
1. **Unsloth + LoRA**（适用于 Linux/Windows）
2. **MLX-LM**（适用于搭载 Apple Silicon 的 macOS）

## 概述

我们的跨平台微调解决方案提供了在各种硬件配置上高效微调 Qwen3 模型的灵活性：

- **Unsloth** 通过优化速度和内存效率，加速 Linux/Windows 上的 LLM 微调
- **LoRA**（低秩适应）使用低秩矩阵分解减少可训练参数
- **MLX-LM** 为搭载 Apple Silicon 的 macOS 提供原生微调功能

这种方法使得在不同平台上高效微调 Qwen3 模型成为可能，在需要时优先使用 CPU，在可用时利用 GPU 加速。

## Qwen3 模型优势

Qwen3 模型相比之前版本提供了多项改进，使其成为微调的绝佳选择：

1. **增强性能**：更好的推理能力和更准确的响应
2. **改进的上下文处理**：更有效地利用上下文窗口
3. **减少幻觉**：更加事实准确和可靠的输出
4. **更好的指令遵循**：更精确地遵循用户指令
5. **优化资源使用**：更高效的内存利用和推理速度

## 硬件考虑因素

微调环境支持多种硬件配置：

- **CPU 优先方法**：通过 `cpu_only=True` 参数优先使用 CPU
- **自动设备映射**：使用 `device_map="auto"` 智能地将模型分布在可用硬件上
- **平台特定优化**：
  - Linux/Windows：使用 Unsloth 和 LoRA 的 CUDA GPU 加速
  - macOS：使用 MLX-LM 的 Apple Silicon 优化

## 安装

### Linux/Windows 安装

```bash
# 导航到项目根目录
cd /path/to/ai-agent

# 如果需要，使脚本可执行
chmod +x finetune/install_dependencies.sh

# 运行安装脚本
./finetune/install_dependencies.sh
```

该脚本将：
1. 检查并安装 Python 3.10（如果尚未可用）
2. 在 `finetune/venv/` 中创建虚拟环境
3. 安装支持 CUDA 的 PyTorch
4. 安装 Unsloth 版本 2025.3.19
5. 安装用于微调的其他依赖项
6. 生成 `requirements.txt` 文件以确保可重现性

### macOS 安装

对于搭载 Apple Silicon 的 macOS，您需要安装 MLX-LM：

```bash
# 导航到项目根目录
cd /path/to/ai-agent

# 创建虚拟环境（如果尚未创建）
python3 -m venv finetune/venv

# 激活虚拟环境
source finetune/venv/bin/activate

# 安装 MLX-LM
pip install mlx-lm

# 安装其他依赖项
pip install transformers datasets pandas numpy
```

## 配置

微调过程通过 `finetune/utils.py` 中的 `FineTuningConfig` 类进行配置。更新后的配置包括：

```python
FineTuningConfig(
    model_name="Qwen/Qwen3-4B-Instruct",  # 更新为 Qwen3
    output_dir="./results",
    lora_r=8,                   # LoRA 秩
    lora_alpha=16,              # LoRA alpha 参数
    target_modules=["q_proj", "v_proj"],  # LoRA 目标模块
    lora_dropout=0.05,          # LoRA 层的丢弃概率
    bias="none",                # LoRA 的偏置类型
    task_type="SEQ_2_SEQ_LM",   # 任务类型
    learning_rate=2e-4,         # 训练学习率
    batch_size=4,               # 每个设备的批量大小
    gradient_accumulation_steps=4,  # 梯度累积步数
    num_train_epochs=3,         # 训练轮数
    max_seq_length=2048,        # 标记化的最大序列长度
    warmup_ratio=0.03,          # 学习率预热步数比例
    save_steps=100,             # 每 X 步保存检查点
    logging_steps=10,           # 每 X 步记录指标
    eval_steps=100,             # 每 X 步运行评估
    save_total_limit=3,         # 保留的最大检查点数
    fp16=True,                  # 是否使用 16 位浮点精度
    bf16=False,                 # 是否使用 bfloat16 精度
    seed=42,                    # 可重现性的随机种子
    use_wandb=False,            # 是否使用 Weights & Biases 进行跟踪
    device_map="auto",          # 设备映射策略（auto, cpu, balanced 等）
    cpu_only=True,              # 是否仅使用 CPU（优先使用 CPU）
    load_in_4bit=True,          # 是否以 4 位精度加载模型
    load_in_8bit=False,         # 是否以 8 位精度加载模型
)
```

### 硬件配置参数

新参数提供了对硬件使用的精细控制：

- `device_map="auto"`：自动将模型层分布在可用设备上
- `cpu_only=True`：强制模型仅在 CPU 上运行（覆盖 device_map）
- `load_in_4bit=True`：启用 4 位量化以提高内存效率
- `load_in_8bit=False`：启用 8 位量化（4 位的替代方案）

## 数据集准备

要微调 Qwen3 模型，您需要准备以下格式之一的数据集：
- 带有指令/提示和响应列的 CSV 文件
- 带有指令-响应对的 JSON/JSONL 文件
- Hugging Face 数据集

数据集应包含提示/指令及其对应响应的配对。`finetune/utils.py` 中的实用函数支持各种列命名约定：
- `instruction` 和 `response`
- `prompt` 和 `completion`
- `input` 和 `output`

## 微调过程

### Linux/Windows（Unsloth + LoRA）

在 Linux/Windows 上微调 Qwen3 模型的最简单方法是使用 `run_fine_tuning` 函数：

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# 创建配置
config = FineTuningConfig(
    model_name="Qwen/Qwen3-4B-Instruct",  # 使用 Qwen3-4B 模型
    output_dir="./my_finetuned_model",
    cpu_only=False,  # 设置为 True 强制使用 CPU
    device_map="auto",  # 自动将模型分布在设备上
)

# 运行微调
model_path = run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)

print(f"微调模型已保存至：{model_path}")
```

### 搭载 Apple Silicon 的 macOS（MLX-LM）

对于搭载 Apple Silicon 的 macOS，使用提供的示例脚本：

```bash
# 激活虚拟环境
source finetune/venv/bin/activate

# 运行 macOS 示例脚本
python finetune/examples/macos_mlx_finetune.py \
    --model_name "mlx-community/Qwen3-0.5B-Chat-mlx" \
    --output_dir ./my_mlx_model \
    --test_prompt "写一个关于机器人学习绘画的短故事。"
```

注意：MLX-LM 微调需要额外设置。请参阅 [MLX-LM 文档](https://github.com/ml-explore/mlx-examples/tree/main/llms/mlx-lm) 获取详细的微调说明。

### 使用示例脚本

仓库包含两个平台的示例脚本：

1. **Linux/Windows**：`finetune/examples/simple_finetune.py`
   ```bash
   python finetune/examples/simple_finetune.py \
       --data_path path/to/your/dataset.json \
       --model_name Qwen/Qwen3-4B-Instruct \
       --output_dir ./my_finetuned_model \
       --num_train_epochs 3 \
       --fp16
   ```

2. **macOS**：`finetune/examples/macos_mlx_finetune.py`
   ```bash
   python finetune/examples/macos_mlx_finetune.py \
       --model_name "mlx-community/Qwen3-0.5B-Chat-mlx" \
       --output_dir ./my_mlx_model
   ```

## 使用微调模型生成文本

微调后，您可以使用相同的 API 在各平台上使用微调后的 Qwen3 模型生成文本：

```python
from finetune.utils import load_model_and_tokenizer, generate_text, FineTuningConfig

# 加载配置
config = FineTuningConfig(
    model_name="./my_finetuned_model",  # 微调模型的路径
    cpu_only=True,  # 设置为 True 强制使用 CPU
    device_map="auto",  # 或 "cpu" 明确使用 CPU
)

# 加载模型和分词器
model, tokenizer = load_model_and_tokenizer(config)

# 生成文本
prompt = "写一个关于机器人学习绘画的短故事。"
generated_text = generate_text(
    model=model,
    tokenizer=tokenizer,
    prompt=prompt,
    max_new_tokens=512,
    temperature=0.7,
)

print(generated_text)
```

`generate_text` 函数自动检测平台和模型类型，使用适当的生成方法。

## 高级用法

### Qwen3 的自定义提示模板

Qwen3 模型支持特定的对话格式。您可以自定义用于微调的提示模板：

```python
from finetune.utils import prepare_dataset, FineTuningConfig

# 为 Qwen3 定义自定义提示模板
custom_template = """<|im_start|>system
您是一位专门从事{domain}的有用AI助手。<|im_end|>
<|im_start|>user
{instruction}<|im_end|>
<|im_start|>assistant
{response}<|im_end|>"""

# 创建配置
config = FineTuningConfig()

# 使用自定义模板准备数据集
train_dataset, eval_dataset = prepare_dataset(
    data_path="path/to/your/dataset.json",
    tokenizer=tokenizer,
    config=config,
    prompt_template=custom_template,
)
```

### Weights & Biases 集成

要使用 Weights & Biases 跟踪您的微调实验：

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# 创建启用 W&B 的配置
config = FineTuningConfig(
    model_name="Qwen/Qwen3-4B-Instruct",
    use_wandb=True,
    wandb_project="my-llm-finetuning",
    wandb_run_name="qwen3-lora-experiment-1",
)

# 运行微调
run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)
```

## Qwen3 模型选择指南

Qwen3 有多种规模以适应不同用例和硬件限制：

| 模型 | 参数 | 用例 | 最小显存 (GPU) | CPU 可行性 |
|-------|------------|----------|-----------------|------------|
| Qwen3-0.5B | 0.5 亿 | 测试、移动设备 | 2GB | 是（快速） |
| Qwen3-1.8B | 1.8 亿 | 轻量级任务 | 4GB | 是 |
| Qwen3-4B | 4 亿 | 通用目的 | 8GB | 是（慢） |
| Qwen3-7B | 7 亿 | 高级任务 | 14GB | 有限 |
| Qwen3-8B | 8 亿 | 高性能 | 16GB | 不推荐 |
| Qwen3-72B | 72 亿 | 企业级 | 80GB+ | 否 |

对于仅 CPU 环境，我们推荐使用 Qwen3-0.5B 或 Qwen3-1.8B 并配合 4 位量化。

## 故障排除

### 内存问题

如果遇到内存不足错误：
1. 使用 `cpu_only=True` 启用仅 CPU 模式
2. 使用 `load_in_4bit=True` 或 `load_in_8bit=True` 进行量化
3. 减小配置中的 `batch_size`
4. 增加 `gradient_accumulation_steps` 以补偿较小的批量大小
5. 如果您的数据允许，减小 `max_seq_length`
6. 使用较小的 Qwen3 模型变体（例如，使用 Qwen3-1.8B 而不是 Qwen3-4B）

### 平台特定问题

#### Linux/Windows
- 如果 CUDA 不可用，系统将自动回退到 CPU
- 为了在 CPU 上获得更好的性能，考虑使用较小的模型或增加量化
- 对于 Qwen3 模型，确保您使用最新的 transformers 库（版本 4.36.0+）

#### macOS
- 确保您使用的是兼容的 MLX 模型（例如 `mlx-community/Qwen3-0.5B-Chat-mlx`）
- 如果 MLX-LM 不可用，使用 `pip install mlx-lm` 安装
- 对于 Apple Silicon 优化，确保您使用的是 arm64 架构的 Python 3.10+
- MLX 模型专为 Apple Silicon 优化，在 macOS 上比仅 CPU 的 PyTorch 提供更好的性能

## 参考资料

- [Qwen3 模型中心](https://huggingface.co/Qwen)
- [Unsloth 文档](https://github.com/unslothai/unsloth)
- [LoRA 论文："LoRA: Low-Rank Adaptation of Large Language Models"](https://arxiv.org/abs/2106.09685)
- [PEFT 库文档](https://huggingface.co/docs/peft/index)
- [MLX-LM 文档](https://github.com/ml-explore/mlx-examples/tree/main/llms/mlx-lm)
- [Apple MLX 框架](https://github.com/ml-explore/mlx)

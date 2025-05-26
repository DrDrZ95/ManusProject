# Unsloth + LoRA 微调指南

本指南提供了在 AI Agent 平台中使用 Unsloth 和 LoRA（低秩适应）进行语言模型微调的说明。

## 概述

Unsloth 是一个通过优化速度和内存效率来加速 LLM 微调的库。结合 LoRA（使用低秩矩阵分解减少可训练参数数量），这种方法即使在计算资源有限的情况下也能高效地微调大型语言模型。

> **注意**：硬件要求（GPU/CPU）将需要在未来规划中指定。本文档假设您将根据特定需求确定适当的硬件配置。

## 安装

AI Agent 平台包含一个用于设置 Unsloth + LoRA 微调环境的脚本。该脚本安装所有必要的依赖项并创建 Python 虚拟环境。

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

## 配置

微调过程通过 `finetune/utils.py` 中的 `FineTuningConfig` 类进行配置。默认配置包括：

```python
FineTuningConfig(
    model_name="Qwen/Qwen2-7B-Instruct",
    output_dir="./results",
    lora_r=8,                   # LoRA 秩
    lora_alpha=16,              # LoRA alpha 参数
    lora_dropout=0.05,          # LoRA 层的丢弃概率
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
)
```

LoRA 配置设置为使用：
- `r=8`（秩）
- `lora_alpha=16`（缩放因子）
- `target_modules=["q_proj", "v_proj"]`（应用 LoRA 的层）
- `lora_dropout=0.05`（丢弃率）
- `bias="none"`（偏置处理）
- `task_type="SEQ_2_SEQ_LM"`（任务类型）

## 数据集准备

要微调模型，您需要准备以下格式之一的数据集：
- 带有指令/提示和响应列的 CSV 文件
- 带有指令-响应对的 JSON/JSONL 文件
- Hugging Face 数据集

数据集应包含提示/指令及其对应响应的配对。`finetune/utils.py` 中的实用函数支持各种列命名约定：
- `instruction` 和 `response`
- `prompt` 和 `completion`
- `input` 和 `output`

## 微调过程

### 基本用法

微调模型的最简单方法是使用 `run_fine_tuning` 函数：

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# 创建配置
config = FineTuningConfig(
    model_name="Qwen/Qwen2-7B-Instruct",
    output_dir="./my_finetuned_model",
    # 根据需要设置其他参数
)

# 运行微调
model_path = run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)

print(f"微调模型已保存至：{model_path}")
```

### 使用示例脚本

仓库包含一个位于 `finetune/examples/simple_finetune.py` 的示例脚本，演示了如何使用命令行参数微调模型：

```bash
# 激活虚拟环境
source finetune/venv/bin/activate

# 运行示例脚本
python finetune/examples/simple_finetune.py \
    --data_path path/to/your/dataset.json \
    --model_name Qwen/Qwen2-7B-Instruct \
    --output_dir ./my_finetuned_model \
    --num_train_epochs 3 \
    --fp16 \
    --test_prompt "写一个关于机器人学习绘画的短故事。"
```

## 使用微调模型生成文本

微调后，您可以使用微调模型生成文本：

```python
from finetune.utils import load_model_and_tokenizer, generate_text, FineTuningConfig

# 加载配置
config = FineTuningConfig(
    model_name="./my_finetuned_model",  # 微调模型的路径
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

## 高级用法

### 自定义提示模板

您可以自定义用于微调的提示模板：

```python
from finetune.utils import prepare_dataset, FineTuningConfig

# 定义自定义提示模板
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
    use_wandb=True,
    wandb_project="my-llm-finetuning",
    wandb_run_name="qwen-lora-experiment-1",
)

# 运行微调
run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)
```

## 故障排除

### 内存问题

如果遇到内存不足错误：
1. 减小配置中的 `batch_size`
2. 增加 `gradient_accumulation_steps` 以补偿较小的批量大小
3. 如果您的数据允许，减小 `max_seq_length`
4. 使用较小的基础模型

### 训练速度

要提高训练速度：
1. 确保您使用的是支持 CUDA 的 GPU
2. 使用 `fp16=True` 或 `bf16=True` 进行混合精度训练
3. 调整 LoRA 配置中的 `target_modules` 数量

## 参考资料

- [Unsloth 文档](https://github.com/unslothai/unsloth)
- [LoRA 论文："LoRA: Low-Rank Adaptation of Large Language Models"](https://arxiv.org/abs/2106.09685)
- [PEFT 库文档](https://huggingface.co/docs/peft/index)

# Unsloth + LoRA 微调指南

本指南介绍如何在 AI Agent 项目中使用 Unsloth 库和 LoRA（低秩适应）进行高效的大型语言模型微调。

## 概述

Unsloth 是一个通过优化底层操作显著加速 LLM 微调的库。当与 LoRA 结合使用时，LoRA 可以减少可训练参数的数量，使您能够以最少的计算资源微调大型模型。

主要优势：
- **速度**：与标准方法相比，训练速度最高可提高 3 倍
- **效率**：需要更少的内存和计算资源
- **质量**：通过参数高效的微调保持或提高模型质量

## 安装

要设置 Unsloth + LoRA 微调环境：

```bash
cd /path/to/ai-agent
chmod +x finetune/install_dependencies.sh
./finetune/install_dependencies.sh
source finetune/venv/bin/activate
```

此脚本在专用虚拟环境中安装所有必要的依赖项，包括 PyTorch、Transformers、Unsloth 和相关库。

## 目录结构

```
ai-agent/
└── finetune/
    ├── install_dependencies.sh  # 安装脚本
    ├── utils.py                 # 微调工具函数
    ├── venv/                    # 虚拟环境（由安装脚本创建）
    ├── requirements.txt         # 由安装脚本生成
    └── examples/                # 微调示例脚本（待添加）
```

## 基本用法

`utils.py` 模块提供了一套用于微调的综合函数。以下是一个基本示例：

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# 配置微调参数
config = FineTuningConfig(
    model_name="Qwen/Qwen2-7B-Instruct",  # 要微调的基础模型
    output_dir="./results",               # 保存结果的位置
    lora_r=16,                            # LoRA 秩
    batch_size=4,                         # 每个设备的批量大小
    num_train_epochs=3,                   # 训练轮数
    learning_rate=2e-4,                   # 学习率
    max_seq_length=2048,                  # 最大序列长度
)

# 运行微调
model_path = run_fine_tuning(
    data_path="path/to/your/dataset.json",  # 数据集路径
    config=config,
    train_test_split=0.1,                   # 10% 的数据用于评估
)

print(f"微调后的模型保存在：{model_path}")
```

## 数据集格式

您的数据集应该采用以下格式之一：

1. **JSON/JSONL**：每个示例应该有 `instruction` 和 `response` 字段（或 `prompt`/`completion` 或 `input`/`output`）。

```json
{
  "instruction": "解释微调的概念。",
  "response": "微调是指在预训练模型的基础上，使用特定数据集进一步训练，使其适应特定任务或领域的过程。"
}
```

2. **CSV**：类似结构，包含指令和响应的列。

## 高级配置

`FineTuningConfig` 类提供了许多参数来自定义您的微调：

- **模型参数**：model_name, max_seq_length
- **LoRA 参数**：lora_r, lora_alpha, lora_dropout
- **训练参数**：learning_rate, batch_size, num_train_epochs 等
- **日志和保存**：save_steps, logging_steps, eval_steps
- **精度**：fp16, bf16
- **跟踪**：use_wandb, wandb_project, wandb_run_name

## 自定义提示模板

您可以通过提供提示模板来自定义数据在训练中的格式：

```python
prompt_template = """<|im_start|>system
您是一位专注于{domain}领域的 AI 助手。<|im_end|>
<|im_start|>user
{instruction}<|im_end|>
<|im_start|>assistant
{response}<|im_end|>"""

# 在 run_fine_tuning 中使用此模板
run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
    prompt_template=prompt_template,
)
```

## 使用微调后的模型生成文本

微调后，您可以使用模型生成文本：

```python
from finetune.utils import load_model_and_tokenizer, generate_text

# 加载微调后的模型
model, tokenizer = load_model_and_tokenizer(
    FineTuningConfig(model_name="./results/final_model")
)

# 生成文本
prompt = "解释 LoRA 微调的好处。"
generated_text = generate_text(
    model=model,
    tokenizer=tokenizer,
    prompt=prompt,
    max_new_tokens=512,
    temperature=0.7,
)

print(generated_text)
```

## 与 AI Agent 集成

微调后的模型可以与现有的 AI Agent 基础设施集成：

1. 使用提供的工具微调模型
2. 将模型保存到 Python 模型服务器可访问的位置
3. 更新模型服务器配置以使用您微调的模型
4. 重启模型服务器以应用更改

## 故障排除

- **内存不足错误**：减小 batch_size、max_seq_length 或使用更小的模型
- **训练速度慢**：确保您使用 GPU 加速并正确安装了 CUDA
- **效果不佳**：尝试调整 learning_rate、num_train_epochs 或 lora_r 参数

## 后续步骤

- 为您的特定用例创建自定义数据集
- 尝试不同的基础模型（Llama、Mistral 等）
- 尝试不同的 LoRA 配置以优化您的硬件
- 实现特定于您应用的评估指标

## 参考资料

- [Unsloth GitHub 仓库](https://github.com/unslothai/unsloth)
- [LoRA 论文](https://arxiv.org/abs/2106.09685)
- [PEFT 文档](https://huggingface.co/docs/peft/index)
- [Transformers 文档](https://huggingface.co/docs/transformers/index)

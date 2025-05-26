# Unsloth + LoRA Fine-tuning Guide

This guide provides instructions for fine-tuning language models using Unsloth and LoRA (Low-Rank Adaptation) within the AI Agent platform.

## Overview

Unsloth is a library that accelerates LLM fine-tuning by optimizing the process for speed and memory efficiency. Combined with LoRA, which reduces the number of trainable parameters by using low-rank matrix decompositions, this approach enables efficient fine-tuning of large language models even with limited computational resources.

> **Note**: Hardware requirements (GPU/CPU) will need to be specified in future planning. This documentation assumes you will determine the appropriate hardware configuration based on your specific needs.

## Installation

The AI Agent platform includes a script to set up the Unsloth + LoRA fine-tuning environment. This script installs all necessary dependencies and creates a Python virtual environment.

```bash
# Navigate to the project root
cd /path/to/ai-agent

# Make the script executable if needed
chmod +x finetune/install_dependencies.sh

# Run the installation script
./finetune/install_dependencies.sh
```

The script will:
1. Check for and install Python 3.10 if not already available
2. Create a virtual environment in `finetune/venv/`
3. Install PyTorch with CUDA support
4. Install Unsloth version 2025.3.19
5. Install other dependencies for fine-tuning
6. Generate a `requirements.txt` file for reproducibility

## Configuration

The fine-tuning process is configured through the `FineTuningConfig` class in `finetune/utils.py`. The default configuration includes:

```python
FineTuningConfig(
    model_name="Qwen/Qwen2-7B-Instruct",
    output_dir="./results",
    lora_r=8,                   # LoRA rank
    lora_alpha=16,              # LoRA alpha parameter
    lora_dropout=0.05,          # Dropout probability for LoRA layers
    learning_rate=2e-4,         # Learning rate for training
    batch_size=4,               # Batch size per device
    gradient_accumulation_steps=4,  # Number of steps to accumulate gradients
    num_train_epochs=3,         # Number of training epochs
    max_seq_length=2048,        # Maximum sequence length for tokenization
    warmup_ratio=0.03,          # Ratio of steps for learning rate warmup
    save_steps=100,             # Save checkpoint every X steps
    logging_steps=10,           # Log metrics every X steps
    eval_steps=100,             # Run evaluation every X steps
    save_total_limit=3,         # Maximum number of checkpoints to keep
    fp16=True,                  # Whether to use 16-bit floating point precision
    bf16=False,                 # Whether to use bfloat16 precision
    seed=42,                    # Random seed for reproducibility
    use_wandb=False,            # Whether to use Weights & Biases for tracking
)
```

The LoRA configuration is set to use:
- `r=8` (rank)
- `lora_alpha=16` (scaling factor)
- `target_modules=["q_proj", "v_proj"]` (which layers to apply LoRA to)
- `lora_dropout=0.05` (dropout rate)
- `bias="none"` (bias handling)
- `task_type="SEQ_2_SEQ_LM"` (task type)

## Dataset Preparation

To fine-tune a model, you need to prepare a dataset in one of the following formats:
- CSV file with columns for instructions/prompts and responses
- JSON/JSONL file with instruction-response pairs
- Hugging Face dataset

The dataset should contain pairs of prompts/instructions and corresponding responses. The utility functions in `finetune/utils.py` support various column naming conventions:
- `instruction` and `response`
- `prompt` and `completion`
- `input` and `output`

## Fine-tuning Process

### Basic Usage

The simplest way to fine-tune a model is to use the `run_fine_tuning` function:

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# Create configuration
config = FineTuningConfig(
    model_name="Qwen/Qwen2-7B-Instruct",
    output_dir="./my_finetuned_model",
    # Other parameters as needed
)

# Run fine-tuning
model_path = run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)

print(f"Fine-tuned model saved to: {model_path}")
```

### Using the Example Script

The repository includes an example script at `finetune/examples/simple_finetune.py` that demonstrates how to fine-tune a model with command-line arguments:

```bash
# Activate the virtual environment
source finetune/venv/bin/activate

# Run the example script
python finetune/examples/simple_finetune.py \
    --data_path path/to/your/dataset.json \
    --model_name Qwen/Qwen2-7B-Instruct \
    --output_dir ./my_finetuned_model \
    --num_train_epochs 3 \
    --fp16 \
    --test_prompt "Write a short story about a robot learning to paint."
```

## Generating Text with Fine-tuned Models

After fine-tuning, you can generate text using the fine-tuned model:

```python
from finetune.utils import load_model_and_tokenizer, generate_text, FineTuningConfig

# Load configuration
config = FineTuningConfig(
    model_name="./my_finetuned_model",  # Path to fine-tuned model
)

# Load model and tokenizer
model, tokenizer = load_model_and_tokenizer(config)

# Generate text
prompt = "Write a short story about a robot learning to paint."
generated_text = generate_text(
    model=model,
    tokenizer=tokenizer,
    prompt=prompt,
    max_new_tokens=512,
    temperature=0.7,
)

print(generated_text)
```

## Advanced Usage

### Custom Prompt Templates

You can customize the prompt template used for fine-tuning:

```python
from finetune.utils import prepare_dataset, FineTuningConfig

# Define custom prompt template
custom_template = """<|im_start|>system
You are a helpful AI assistant specialized in {domain}.<|im_end|>
<|im_start|>user
{instruction}<|im_end|>
<|im_start|>assistant
{response}<|im_end|>"""

# Create configuration
config = FineTuningConfig()

# Prepare dataset with custom template
train_dataset, eval_dataset = prepare_dataset(
    data_path="path/to/your/dataset.json",
    tokenizer=tokenizer,
    config=config,
    prompt_template=custom_template,
)
```

### Weights & Biases Integration

To track your fine-tuning experiments with Weights & Biases:

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# Create configuration with W&B enabled
config = FineTuningConfig(
    use_wandb=True,
    wandb_project="my-llm-finetuning",
    wandb_run_name="qwen-lora-experiment-1",
)

# Run fine-tuning
run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)
```

## Troubleshooting

### Memory Issues

If you encounter out-of-memory errors:
1. Reduce `batch_size` in the configuration
2. Increase `gradient_accumulation_steps` to compensate for smaller batch size
3. Reduce `max_seq_length` if your data allows it
4. Use a smaller base model

### Training Speed

To improve training speed:
1. Ensure you're using a GPU with CUDA support
2. Use `fp16=True` or `bf16=True` for mixed precision training
3. Adjust the number of `target_modules` in the LoRA configuration

## References

- [Unsloth Documentation](https://github.com/unslothai/unsloth)
- [LoRA Paper: "LoRA: Low-Rank Adaptation of Large Language Models"](https://arxiv.org/abs/2106.09685)
- [PEFT Library Documentation](https://huggingface.co/docs/peft/index)

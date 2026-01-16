# Cross-Platform Fine-tuning Guide for DeepSeek Models

This guide provides instructions for fine-tuning DeepSeek language models across different platforms using:
1. **Unsloth + LoRA** (for Linux/Windows)
2. **MLX-LM** (for macOS with Apple Silicon)

## Overview

Our cross-platform fine-tuning solution offers flexibility to work efficiently with DeepSeek models on various hardware configurations:

- **Unsloth** accelerates LLM fine-tuning by optimizing the process for speed and memory efficiency on Linux/Windows
- **LoRA** (Low-Rank Adaptation) reduces trainable parameters using low-rank matrix decompositions
- **MLX-LM** provides native fine-tuning capabilities for macOS with Apple Silicon

This approach enables efficient fine-tuning of DeepSeek models across different platforms, prioritizing CPU usage when needed and leveraging GPU acceleration when available.

## DeepSeek Model Advantages

DeepSeek models offer several improvements over previous versions that make them excellent candidates for fine-tuning:

1. **Enhanced Performance**: Better reasoning capabilities and more accurate responses
2. **Improved Context Handling**: More effective utilization of context window
3. **Reduced Hallucinations**: More factual and reliable outputs
4. **Better Instruction Following**: More precise adherence to user instructions
5. **Optimized Resource Usage**: More efficient memory utilization and inference speed

## Hardware Considerations

The fine-tuning environment supports multiple hardware configurations:

- **CPU-first approach**: Prioritizes CPU usage with `cpu_only=True` parameter
- **Automatic device mapping**: Uses `device_map="auto"` to intelligently distribute model across available hardware
- **Platform-specific optimizations**:
  - Linux/Windows: CUDA GPU acceleration with Unsloth and LoRA
  - macOS: Apple Silicon optimization with MLX-LM

## Installation

### Linux/Windows Installation

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

### macOS Installation

For macOS with Apple Silicon, you'll need to install MLX-LM:

```bash
# Navigate to the project root
cd /path/to/ai-agent

# Create a virtual environment (if not already created)
python3 -m venv finetune/venv

# Activate the virtual environment
source finetune/venv/bin/activate

# Install MLX-LM
pip install mlx-lm

# Install other dependencies
pip install transformers datasets pandas numpy
```

## Configuration

The fine-tuning process is configured through the `FineTuningConfig` class in `finetune/utils.py`. The updated configuration includes:

```python
FineTuningConfig(
    model_name="deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B",  # Updated to DeepSeek
    output_dir="./results",
    lora_r=8,                   # LoRA rank
    lora_alpha=16,              # LoRA alpha parameter
    target_modules=["q_proj", "v_proj"],  # Target modules for LoRA
    lora_dropout=0.05,          # Dropout probability for LoRA layers
    bias="none",                # Bias type for LoRA
    task_type="SEQ_2_SEQ_LM",   # Task type
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
    device_map="auto",          # Device mapping strategy (auto, cpu, balanced, etc.)
    cpu_only=True,              # Whether to use CPU only (prioritize CPU)
    load_in_4bit=True,          # Whether to load model in 4-bit precision
    load_in_8bit=False,         # Whether to load model in 8-bit precision
)
```

### Hardware Configuration Parameters

The new parameters provide fine-grained control over hardware usage:

- `device_map="auto"`: Automatically distributes model layers across available devices
- `cpu_only=True`: Forces the model to run on CPU only (overrides device_map)
- `load_in_4bit=True`: Enables 4-bit quantization for memory efficiency
- `load_in_8bit=False`: Enables 8-bit quantization (alternative to 4-bit)

## Dataset Preparation

To fine-tune a DeepSeek model, you need to prepare a dataset in one of the following formats:
- CSV file with columns for instructions/prompts and responses
- JSON/JSONL file with instruction-response pairs
- Hugging Face dataset

The dataset should contain pairs of prompts/instructions and corresponding responses. The utility functions in `finetune/utils.py` support various column naming conventions:
- `instruction` and `response`
- `prompt` and `completion`
- `input` and `output`

## Fine-tuning Process

### Linux/Windows (Unsloth + LoRA)

The simplest way to fine-tune a DeepSeek model on Linux/Windows is to use the `run_fine_tuning` function:

```python
from finetune.utils import FineTuningConfig, run_fine_tuning

# Create configuration
config = FineTuningConfig(
    model_name="deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B",  # Using DeepSeek-4B model
    output_dir="./my_finetuned_model",
    cpu_only=False,  # Set to True to force CPU usage
    device_map="auto",  # Automatically distribute model across devices
)

# Run fine-tuning
model_path = run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)

print(f"Fine-tuned model saved to: {model_path}")
```

### macOS with Apple Silicon (MLX-LM)

For macOS with Apple Silicon, use the provided example script:

```bash
# Activate the virtual environment
source finetune/venv/bin/activate

# Run the macOS example script
python finetune/examples/macos_mlx_finetune.py \
    --model_name "mlx-community/DeepSeek-0.5B-Chat-mlx" \
    --output_dir ./my_mlx_model \
    --test_prompt "Write a short story about a robot learning to paint."
```

Note: MLX-LM fine-tuning requires additional setup. Please refer to the [MLX-LM documentation](https://github.com/ml-explore/mlx-examples/tree/main/llms/mlx-lm) for detailed fine-tuning instructions.

### Using the Example Scripts

The repository includes example scripts for both platforms:

1. **Linux/Windows**: `finetune/examples/simple_finetune.py`
   ```bash
   python finetune/examples/simple_finetune.py \
       --data_path path/to/your/dataset.json \
       --model_name deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B \
       --output_dir ./my_finetuned_model \
       --num_train_epochs 3 \
       --fp16
   ```

2. **macOS**: `finetune/examples/macos_mlx_finetune.py`
   ```bash
   python finetune/examples/macos_mlx_finetune.py \
       --model_name "mlx-community/DeepSeek-0.5B-Chat-mlx" \
       --output_dir ./my_mlx_model
   ```

## Generating Text with Fine-tuned Models

After fine-tuning, you can generate text using the fine-tuned DeepSeek model with the same API across platforms:

```python
from finetune.utils import load_model_and_tokenizer, generate_text, FineTuningConfig

# Load configuration
config = FineTuningConfig(
    model_name="./my_finetuned_model",  # Path to fine-tuned model
    cpu_only=True,  # Set to True to force CPU usage
    device_map="auto",  # Or "cpu" to explicitly use CPU
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

The `generate_text` function automatically detects the platform and model type, using the appropriate generation method.

## Advanced Usage

### Custom Prompt Templates for DeepSeek

DeepSeek models support a specific chat format. You can customize the prompt template used for fine-tuning:

```python
from finetune.utils import prepare_dataset, FineTuningConfig

# Define custom prompt template for DeepSeek
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
    model_name="deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B

",
    use_wandb=True,
    wandb_project="my-llm-finetuning",
    wandb_run_name="DeepSeek-lora-experiment-1",
)

# Run fine-tuning
run_fine_tuning(
    data_path="path/to/your/dataset.json",
    config=config,
)
```

## DeepSeek Model Selection Guide

DeepSeek comes in several sizes to fit different use cases and hardware constraints:

| Model | Parameters | Use Case | Min. VRAM (GPU) | CPU Viable |
|-------|------------|----------|-----------------|------------|
| DeepSeek-0.5B | 0.5 billion | Testing, mobile | 2GB | Yes (fast) |
| DeepSeek-1.8B | 1.8 billion | Light tasks | 4GB | Yes |
| DeepSeek-4B | 4 billion | General purpose | 8GB | Yes (slow) |
| DeepSeek-7B | 7 billion | Advanced tasks | 14GB | Limited |
| DeepSeek-8B | 8 billion | High performance | 16GB | Not recommended |
| DeepSeek-72B | 72 billion | Enterprise | 80GB+ | No |

For CPU-only environments, we recommend using DeepSeek-0.5B or DeepSeek-1.8B with 4-bit quantization.

## Troubleshooting

### Memory Issues

If you encounter out-of-memory errors:
1. Enable CPU-only mode with `cpu_only=True`
2. Use quantization with `load_in_4bit=True` or `load_in_8bit=True`
3. Reduce `batch_size` in the configuration
4. Increase `gradient_accumulation_steps` to compensate for smaller batch size
5. Reduce `max_seq_length` if your data allows it
6. Use a smaller DeepSeek model variant (e.g., DeepSeek-1.8B instead of DeepSeek-4B)

### Platform-Specific Issues

#### Linux/Windows
- If CUDA is not available, the system will automatically fall back to CPU
- For better performance on CPU, consider using a smaller model or increasing quantization
- For DeepSeek models, ensure you have the latest transformers library (version 4.36.0+)

#### macOS
- Ensure you're using a compatible MLX model (e.g., `mlx-community/DeepSeek-0.5B-Chat-mlx`)
- If MLX-LM is not available, install it with `pip install mlx-lm`
- For Apple Silicon optimization, ensure you're using Python 3.10+ with the arm64 architecture
- MLX models are optimized specifically for Apple Silicon and provide much better performance than CPU-only PyTorch on macOS

## References

- [DeepSeek Model Hub](https://huggingface.co/DeepSeek)
- [Unsloth Documentation](https://github.com/unslothai/unsloth)
- [LoRA Paper: "LoRA: Low-Rank Adaptation of Large Language Models"](https://arxiv.org/abs/2106.09685)
- [PEFT Library Documentation](https://huggingface.co/docs/peft/index)
- [MLX-LM Documentation](https://github.com/ml-explore/mlx-examples/tree/main/llms/mlx-lm)
- [Apple MLX Framework](https://github.com/ml-explore/mlx)

"""
Cross-Platform Fine-tuning Utilities

This module provides utility functions for fine-tuning language models using:
1. Unsloth and LoRA (for Linux/Windows with CUDA support)
2. MLX-LM (for macOS with Apple Silicon)

It includes functions for dataset preparation, model loading, training configuration, and evaluation.
"""

import os
import json
import platform
import pandas as pd
import numpy as np
from typing import Dict, List, Optional, Union, Tuple, Any
from datasets import Dataset, load_dataset
from transformers import (
    AutoTokenizer, 
    TrainingArguments,
    Trainer,
    DataCollatorForLanguageModeling
)

# Determine the platform
IS_MACOS = platform.system() == "Darwin" and "arm" in platform.machine()
IS_WINDOWS = platform.system() == "Windows"
IS_LINUX = platform.system() == "Linux"

# Import platform-specific modules
if not IS_MACOS:
    # For Linux/Windows
    import torch
    from peft import LoraConfig, get_peft_model
    try:
        from unsloth import FastLanguageModel
        UNSLOTH_AVAILABLE = True
    except ImportError:
        UNSLOTH_AVAILABLE = False
        print("Warning: Unsloth not available. Using standard Hugging Face transformers instead.")
else:
    # For macOS with Apple Silicon
    UNSLOTH_AVAILABLE = False
    try:
        import mlx
        import mlx.core as mx
        from mlx_lm import load, generate
        MLX_AVAILABLE = True
    except ImportError:
        MLX_AVAILABLE = False
        print("Warning: MLX-LM not available on this macOS system. Please install with 'pip install mlx-lm'.")
        # Fallback to torch for CPU-only operation
        import torch

class FineTuningConfig:
    """Configuration class for fine-tuning parameters."""
    
    def __init__(
        self,
        model_name: str = "Qwen/Qwen3-4B-Instruct",
        output_dir: str = "./results",
        lora_r: int = 8,
        lora_alpha: int = 16,
        lora_dropout: float = 0.05,
        learning_rate: float = 2e-4,
        batch_size: int = 4,
        gradient_accumulation_steps: int = 4,
        num_train_epochs: int = 3,
        max_seq_length: int = 2048,
        warmup_ratio: float = 0.03,
        save_steps: int = 100,
        logging_steps: int = 10,
        eval_steps: int = 100,
        save_total_limit: int = 3,
        fp16: bool = True,
        bf16: bool = False,
        seed: int = 42,
        use_wandb: bool = False,
        wandb_project: str = "unsloth-finetuning",
        wandb_run_name: Optional[str] = None,
        device_map: str = "auto",
        cpu_only: bool = True,
        load_in_4bit: bool = True,
        load_in_8bit: bool = False,
    ):
        """
        Initialize fine-tuning configuration.
        
        Args:
            model_name: Base model to fine-tune
            output_dir: Directory to save model checkpoints
            lora_r: LoRA rank
            lora_alpha: LoRA alpha parameter
            lora_dropout: Dropout probability for LoRA layers
            learning_rate: Learning rate for training
            batch_size: Batch size per device
            gradient_accumulation_steps: Number of steps to accumulate gradients
            num_train_epochs: Number of training epochs
            max_seq_length: Maximum sequence length for tokenization
            warmup_ratio: Ratio of steps for learning rate warmup
            save_steps: Save checkpoint every X steps
            logging_steps: Log metrics every X steps
            eval_steps: Run evaluation every X steps
            save_total_limit: Maximum number of checkpoints to keep
            fp16: Whether to use 16-bit floating point precision
            bf16: Whether to use bfloat16 precision
            seed: Random seed for reproducibility
            use_wandb: Whether to use Weights & Biases for tracking
            wandb_project: W&B project name
            wandb_run_name: W&B run name
        """
        self.model_name = model_name
        self.output_dir = output_dir
        self.lora_r = lora_r
        self.lora_alpha = lora_alpha
        self.lora_dropout = lora_dropout
        self.learning_rate = learning_rate
        self.batch_size = batch_size
        self.gradient_accumulation_steps = gradient_accumulation_steps
        self.num_train_epochs = num_train_epochs
        self.max_seq_length = max_seq_length
        self.warmup_ratio = warmup_ratio
        self.save_steps = save_steps
        self.logging_steps = logging_steps
        self.eval_steps = eval_steps
        self.save_total_limit = save_total_limit
        self.fp16 = fp16
        self.bf16 = bf16
        self.seed = seed
        self.use_wandb = use_wandb
        self.wandb_project = wandb_project
        self.wandb_run_name = wandb_run_name
        self.device_map = device_map
        self.cpu_only = cpu_only
        self.load_in_4bit = load_in_4bit
        self.load_in_8bit = load_in_8bit
        
    def to_dict(self) -> Dict[str, Any]:
        """Convert configuration to dictionary."""
        return self.__dict__
    
    def save(self, path: str) -> None:
        """Save configuration to JSON file."""
        with open(path, 'w') as f:
            json.dump(self.to_dict(), f, indent=2)
    
    @classmethod
    def from_dict(cls, config_dict: Dict[str, Any]) -> 'FineTuningConfig':
        """Create configuration from dictionary."""
        return cls(**config_dict)
    
    @classmethod
    def load(cls, path: str) -> 'FineTuningConfig':
        """Load configuration from JSON file."""
        with open(path, 'r') as f:
            config_dict = json.load(f)
        return cls.from_dict(config_dict)


def prepare_dataset(
    data_path: str,
    tokenizer,
    config: FineTuningConfig,
    prompt_template: Optional[str] = None,
    train_test_split: float = 0.1,
    shuffle: bool = True,
    seed: Optional[int] = None
) -> Tuple[Dataset, Optional[Dataset]]:
    """
    Prepare dataset for fine-tuning.
    
    Args:
        data_path: Path to dataset file or directory
        tokenizer: Tokenizer for the model
        config: Fine-tuning configuration
        prompt_template: Template for formatting prompts (if None, uses default)
        train_test_split: Fraction of data to use for evaluation
        shuffle: Whether to shuffle the dataset
        seed: Random seed for shuffling
        
    Returns:
        Tuple of (train_dataset, eval_dataset)
    """
    # Load dataset based on file extension
    if data_path.endswith('.csv'):
        df = pd.read_csv(data_path)
        dataset = Dataset.from_pandas(df)
    elif data_path.endswith('.json') or data_path.endswith('.jsonl'):
        dataset = load_dataset('json', data_files=data_path)['train']
    else:
        # Assume it's a dataset on Hugging Face Hub or a directory
        dataset = load_dataset(data_path)['train']
    
    # Split dataset if needed
    if train_test_split > 0:
        split_dataset = dataset.train_test_split(
            test_size=train_test_split, 
            shuffle=shuffle,
            seed=seed or config.seed
        )
        train_dataset = split_dataset['train']
        eval_dataset = split_dataset['test']
    else:
        train_dataset = dataset
        eval_dataset = None
    
    # Define default prompt template if none provided
    if prompt_template is None:
        prompt_template = """<|im_start|>system
You are a helpful AI assistant.<|im_end|>
<|im_start|>user
{instruction}<|im_end|>
<|im_start|>assistant
{response}<|im_end|>"""
    
    # Function to format examples according to the prompt template
    def format_example(example):
        if 'instruction' in example and 'response' in example:
            return prompt_template.format(
                instruction=example['instruction'],
                response=example['response']
            )
        elif 'prompt' in example and 'completion' in example:
            return prompt_template.format(
                instruction=example['prompt'],
                response=example['completion']
            )
        elif 'input' in example and 'output' in example:
            return prompt_template.format(
                instruction=example['input'],
                response=example['output']
            )
        else:
            raise ValueError(
                "Dataset format not recognized. Expected columns: "
                "(instruction, response) or (prompt, completion) or (input, output)"
            )
    
    # Function to tokenize examples
    def tokenize_function(examples):
        formatted_texts = [format_example(example) for example in examples]
        tokenized = tokenizer(
            formatted_texts,
            padding=False,
            truncation=True,
            max_length=config.max_seq_length,
            return_tensors=None,
        )
        return tokenized
    
    # Tokenize datasets
    train_dataset = train_dataset.map(
        tokenize_function,
        batched=True,
        remove_columns=train_dataset.column_names,
    )
    
    if eval_dataset is not None:
        eval_dataset = eval_dataset.map(
            tokenize_function,
            batched=True,
            remove_columns=eval_dataset.column_names,
        )
    
    return train_dataset, eval_dataset


def load_model_and_tokenizer(config: FineTuningConfig):
    """
    Load model and tokenizer with platform-specific optimizations.
    
    This function automatically selects the appropriate loading method based on:
    1. Platform (macOS vs Linux/Windows)
    2. Available libraries (Unsloth, MLX-LM)
    3. Configuration settings (cpu_only, device_map)
    
    Args:
        config: Fine-tuning configuration
        
    Returns:
        Tuple of (model, tokenizer)
    """
    # For macOS with Apple Silicon and MLX-LM available
    if IS_MACOS and MLX_AVAILABLE:
        print(f"Loading model {config.model_name} with MLX-LM for macOS...")
        # MLX-LM has a different loading pattern
        model, tokenizer = load(
            model_path=config.model_name,
            tokenizer_path=config.model_name,
            # MLX-LM specific parameters
            max_tokens=config.max_seq_length,
        )
        return model, tokenizer
    
    # For Linux/Windows with Unsloth available
    elif UNSLOTH_AVAILABLE and not config.cpu_only:
        print(f"Loading model {config.model_name} with Unsloth optimizations...")
        # Load model and tokenizer with Unsloth optimizations
        model, tokenizer = FastLanguageModel.from_pretrained(
            model_name=config.model_name,
            max_seq_length=config.max_seq_length,
            dtype=torch.bfloat16 if config.bf16 else torch.float16,
            load_in_4bit=config.load_in_4bit,
            load_in_8bit=config.load_in_8bit,
            device_map=config.device_map,
        )
        
        # Configure LoRA
        lora_config = LoraConfig(
            r=config.lora_r,
            lora_alpha=config.lora_alpha,
            lora_dropout=config.lora_dropout,
            bias="none",
            task_type="SEQ_2_SEQ_LM",
            target_modules=["q_proj", "v_proj"],
        )
        
        # Apply LoRA to model
        model = FastLanguageModel.get_peft_model(
            model,
            lora_config,
            use_gradient_checkpointing=True,
        )
        
        return model, tokenizer
    
    # Fallback to standard Hugging Face transformers (CPU or GPU)
    else:
        print(f"Loading model {config.model_name} with standard transformers...")
        from transformers import AutoModelForCausalLM
        
        # Load tokenizer
        tokenizer = AutoTokenizer.from_pretrained(config.model_name)
        
        # Set device map to CPU if cpu_only is True
        device_map = "cpu" if config.cpu_only else config.device_map
        
        # Load model with appropriate quantization
        model = AutoModelForCausalLM.from_pretrained(
            config.model_name,
            device_map=device_map,
            load_in_4bit=config.load_in_4bit and not config.cpu_only,
            load_in_8bit=config.load_in_8bit and not config.cpu_only,
            torch_dtype=torch.bfloat16 if config.bf16 else torch.float16,
        )
        
        # Apply LoRA if not using CPU
        if not config.cpu_only:
            from peft import prepare_model_for_kbit_training
            
            # Configure LoRA
            lora_config = LoraConfig(
                r=config.lora_r,
                lora_alpha=config.lora_alpha,
                lora_dropout=config.lora_dropout,
                bias="none",
                task_type="CAUSAL_LM",
                target_modules=["q_proj", "v_proj"],
            )
            
            # Prepare model for k-bit training if using quantization
            if config.load_in_4bit or config.load_in_8bit:
                model = prepare_model_for_kbit_training(model)
            
            # Apply LoRA
            model = get_peft_model(model, lora_config)
        
        return model, tokenizer


def setup_training_args(config: FineTuningConfig):
    """
    Set up training arguments for the Trainer.
    
    Args:
        config: Fine-tuning configuration
        
    Returns:
        TrainingArguments object
    """
    # Initialize W&B if requested
    if config.use_wandb:
        import wandb
        wandb.init(
            project=config.wandb_project,
            name=config.wandb_run_name,
            config=config.to_dict()
        )
    
    # Set up training arguments
    training_args = TrainingArguments(
        output_dir=config.output_dir,
        per_device_train_batch_size=config.batch_size,
        per_device_eval_batch_size=config.batch_size,
        gradient_accumulation_steps=config.gradient_accumulation_steps,
        learning_rate=config.learning_rate,
        num_train_epochs=config.num_train_epochs,
        weight_decay=0.01,
        adam_beta1=0.9,
        adam_beta2=0.95,
        warmup_ratio=config.warmup_ratio,
        lr_scheduler_type="cosine",
        save_strategy="steps",
        save_steps=config.save_steps,
        save_total_limit=config.save_total_limit,
        logging_strategy="steps",
        logging_steps=config.logging_steps,
        evaluation_strategy="steps" if config.eval_steps > 0 else "no",
        eval_steps=config.eval_steps if config.eval_steps > 0 else None,
        load_best_model_at_end=config.eval_steps > 0,
        fp16=config.fp16 and not config.bf16,
        bf16=config.bf16,
        seed=config.seed,
        report_to="wandb" if config.use_wandb else "none",
        ddp_find_unused_parameters=False,
    )
    
    return training_args


def train_model(
    model,
    tokenizer,
    train_dataset,
    eval_dataset,
    config: FineTuningConfig
):
    """
    Train the model using Unsloth and LoRA.
    
    Args:
        model: Model to fine-tune
        tokenizer: Tokenizer for the model
        train_dataset: Training dataset
        eval_dataset: Evaluation dataset
        config: Fine-tuning configuration
        
    Returns:
        Trained model
    """
    # Set up training arguments
    training_args = setup_training_args(config)
    
    # Create data collator
    data_collator = DataCollatorForLanguageModeling(
        tokenizer=tokenizer,
        mlm=False,
    )
    
    # Initialize trainer
    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=train_dataset,
        eval_dataset=eval_dataset,
        data_collator=data_collator,
    )
    
    # Train model
    trainer.train()
    
    # Save model
    model.save_pretrained(os.path.join(config.output_dir, "final_model"))
    tokenizer.save_pretrained(os.path.join(config.output_dir, "final_model"))
    
    # Save configuration
    config.save(os.path.join(config.output_dir, "config.json"))
    
    return model


def generate_text(
    model,
    tokenizer,
    prompt: str,
    max_new_tokens: int = 512,
    temperature: float = 0.7,
    top_p: float = 0.9,
    top_k: int = 50,
    repetition_penalty: float = 1.1,
    do_sample: bool = True,
):
    """
    Generate text using the fine-tuned model.
    
    This function automatically selects the appropriate generation method based on:
    1. Platform (macOS vs Linux/Windows)
    2. Available libraries (Unsloth, MLX-LM)
    
    Args:
        model: Fine-tuned model
        tokenizer: Tokenizer for the model
        prompt: Input prompt
        max_new_tokens: Maximum number of tokens to generate
        temperature: Sampling temperature
        top_p: Nucleus sampling parameter
        top_k: Top-k sampling parameter
        repetition_penalty: Penalty for repeating tokens
        do_sample: Whether to use sampling (vs greedy decoding)
        
    Returns:
        Generated text
    """
    # For macOS with Apple Silicon and MLX-LM available
    if IS_MACOS and MLX_AVAILABLE and hasattr(model, "generate") and hasattr(model.generate, "__module__") and "mlx_lm" in model.generate.__module__:
        print("Generating text with MLX-LM for macOS...")
        # MLX-LM has a different generation pattern
        response = generate(
            model=model,
            tokenizer=tokenizer,
            prompt=prompt,
            max_tokens=max_new_tokens,
            temp=temperature,
            top_p=top_p,
            # MLX-LM doesn't support all parameters, so we use what's available
        )
        return response
    
    # For standard Hugging Face models
    else:
        print("Generating text with transformers...")
        # Check if we're using a torch model
        if hasattr(model, "device"):
            # Prepare input
            inputs = tokenizer(prompt, return_tensors="pt")
            
            # Move inputs to the model's device
            if hasattr(inputs, "to") and hasattr(model, "device"):
                inputs = inputs.to(model.device)
            
            # Generate text
            with torch.no_grad():
                outputs = model.generate(
                    **inputs,
                    max_new_tokens=max_new_tokens,
                    temperature=temperature,
                    top_p=top_p,
                    top_k=top_k,
                    repetition_penalty=repetition_penalty,
                    do_sample=do_sample,
                    pad_token_id=tokenizer.eos_token_id,
                )
            
            # Decode and return generated text
            generated_text = tokenizer.decode(outputs[0], skip_special_tokens=True)
            
            return generated_text
        else:
            # Fallback for non-torch models
            raise ValueError("Unsupported model type for text generation")


def run_fine_tuning(
    data_path: str,
    config: Optional[FineTuningConfig] = None,
    prompt_template: Optional[str] = None,
    train_test_split: float = 0.1,
):
    """
    Run the complete fine-tuning pipeline.
    
    Args:
        data_path: Path to dataset file or directory
        config: Fine-tuning configuration (if None, uses default)
        prompt_template: Template for formatting prompts
        train_test_split: Fraction of data to use for evaluation
        
    Returns:
        Path to the saved model
    """
    # Use default config if none provided
    if config is None:
        config = FineTuningConfig()
    
    # Create output directory if it doesn't exist
    os.makedirs(config.output_dir, exist_ok=True)
    
    # Load model and tokenizer
    model, tokenizer = load_model_and_tokenizer(config)
    
    # Prepare dataset
    train_dataset, eval_dataset = prepare_dataset(
        data_path=data_path,
        tokenizer=tokenizer,
        config=config,
        prompt_template=prompt_template,
        train_test_split=train_test_split,
    )
    
    # Train model
    model = train_model(
        model=model,
        tokenizer=tokenizer,
        train_dataset=train_dataset,
        eval_dataset=eval_dataset,
        config=config,
    )
    
    # Return path to saved model
    return os.path.join(config.output_dir, "final_model")

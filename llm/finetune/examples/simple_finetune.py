#!/usr/bin/env python
# -*- coding: utf-8 -*-

"""
Simple example of fine-tuning a model with Unsloth and LoRA.
This script demonstrates how to use the utility functions to fine-tune a model.
"""

import os
import sys
import argparse
from pathlib import Path

# Add parent directory to path to import utils
sys.path.append(str(Path(__file__).parent.parent))
from utils import FineTuningConfig, run_fine_tuning, generate_text, load_model_and_tokenizer

def parse_args():
    parser = argparse.ArgumentParser(description="Fine-tune a model with Unsloth and LoRA")
    parser.add_argument(
        "--data_path", 
        type=str, 
        required=True,
        help="Path to dataset file or directory"
    )
    parser.add_argument(
        "--model_name", 
        type=str, 
        default="deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B",
        help="Base model to fine-tune"
    )
    parser.add_argument(
        "--output_dir", 
        type=str, 
        default="./results",
        help="Directory to save model checkpoints"
    )
    parser.add_argument(
        "--lora_r", 
        type=int, 
        default=8,
        help="LoRA rank"
    )
    parser.add_argument(
        "--lora_alpha", 
        type=int, 
        default=16,
        help="LoRA alpha parameter"
    )
    parser.add_argument(
        "--lora_dropout", 
        type=float, 
        default=0.05,
        help="Dropout probability for LoRA layers"
    )
    parser.add_argument(
        "--learning_rate", 
        type=float, 
        default=2e-4,
        help="Learning rate for training"
    )
    parser.add_argument(
        "--batch_size", 
        type=int, 
        default=4,
        help="Batch size per device"
    )
    parser.add_argument(
        "--num_train_epochs", 
        type=int, 
        default=3,
        help="Number of training epochs"
    )
    parser.add_argument(
        "--max_seq_length", 
        type=int, 
        default=2048,
        help="Maximum sequence length for tokenization"
    )
    parser.add_argument(
        "--use_wandb", 
        action="store_true",
        help="Whether to use Weights & Biases for tracking"
    )
    parser.add_argument(
        "--fp16", 
        action="store_true",
        help="Whether to use 16-bit floating point precision"
    )
    parser.add_argument(
        "--bf16", 
        action="store_true",
        help="Whether to use bfloat16 precision"
    )
    parser.add_argument(
        "--test_prompt", 
        type=str, 
        default="",
        help="Test prompt for generation after fine-tuning"
    )
    
    return parser.parse_args()

def main():
    # Parse arguments
    args = parse_args()
    
    # Create output directory if it doesn't exist
    os.makedirs(args.output_dir, exist_ok=True)
    
    # Create fine-tuning configuration
    config = FineTuningConfig(
        model_name=args.model_name,
        output_dir=args.output_dir,
        lora_r=args.lora_r,
        lora_alpha=args.lora_alpha,
        lora_dropout=args.lora_dropout,
        learning_rate=args.learning_rate,
        batch_size=args.batch_size,
        num_train_epochs=args.num_train_epochs,
        max_seq_length=args.max_seq_length,
        fp16=args.fp16,
        bf16=args.bf16,
        use_wandb=args.use_wandb,
    )
    
    # Run fine-tuning
    print(f"Starting fine-tuning with Unsloth and LoRA...")
    print(f"Model: {args.model_name}")
    print(f"Dataset: {args.data_path}")
    print(f"Output directory: {args.output_dir}")
    print(f"LoRA config: r={args.lora_r}, alpha={args.lora_alpha}, dropout={args.lora_dropout}")
    
    model_path = run_fine_tuning(
        data_path=args.data_path,
        config=config,
    )
    
    print(f"Fine-tuning complete! Model saved to: {model_path}")
    
    # Test generation if a prompt is provided
    if args.test_prompt:
        print("\nTesting generation with fine-tuned model...")
        print(f"Prompt: {args.test_prompt}")
        
        # Load fine-tuned model
        model, tokenizer = load_model_and_tokenizer(config)
        
        # Generate text
        generated_text = generate_text(
            model=model,
            tokenizer=tokenizer,
            prompt=args.test_prompt,
        )
        
        print(f"Generated text: {generated_text}")

if __name__ == "__main__":
    main()

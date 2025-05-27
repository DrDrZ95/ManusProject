#!/usr/bin/env python
# -*- coding: utf-8 -*-

"""
Example of fine-tuning a model with MLX-LM on macOS.
This script demonstrates how to use the utility functions to fine-tune a model on Apple Silicon.
"""

import os
import sys
import argparse
from pathlib import Path

# Add parent directory to path to import utils
sys.path.append(str(Path(__file__).parent.parent))
from utils import FineTuningConfig, load_model_and_tokenizer, generate_text, IS_MACOS, MLX_AVAILABLE

def parse_args():
    parser = argparse.ArgumentParser(description="Fine-tune a model with MLX-LM on macOS")
    parser.add_argument(
        "--model_name", 
        type=str, 
        default="mlx-community/Qwen1.5-0.5B-Chat-mlx",
        help="Base model to fine-tune (MLX compatible model)"
    )
    parser.add_argument(
        "--output_dir", 
        type=str, 
        default="./results_mlx",
        help="Directory to save model checkpoints"
    )
    parser.add_argument(
        "--test_prompt", 
        type=str, 
        default="Write a short story about a robot learning to paint.",
        help="Test prompt for generation after loading"
    )
    
    return parser.parse_args()

def main():
    # Parse arguments
    args = parse_args()
    
    # Check if running on macOS with MLX available
    if not IS_MACOS:
        print("This script is designed for macOS with Apple Silicon.")
        print("For other platforms, please use the standard fine-tuning script.")
        sys.exit(1)
    
    if not MLX_AVAILABLE:
        print("MLX-LM is not available. Please install it with:")
        print("pip install mlx-lm")
        sys.exit(1)
    
    print("Running on macOS with MLX support!")
    
    # Create output directory if it doesn't exist
    os.makedirs(args.output_dir, exist_ok=True)
    
    # Create fine-tuning configuration
    config = FineTuningConfig(
        model_name=args.model_name,
        output_dir=args.output_dir,
        # MLX-LM will ignore most LoRA parameters as it uses a different approach
        cpu_only=False,  # MLX uses the Apple Neural Engine and GPU
    )
    
    # Load model and tokenizer
    print(f"Loading model {args.model_name} with MLX-LM...")
    model, tokenizer = load_model_and_tokenizer(config)
    
    # Test generation
    if args.test_prompt:
        print("\nTesting generation with MLX-LM model...")
        print(f"Prompt: {args.test_prompt}")
        
        # Generate text
        generated_text = generate_text(
            model=model,
            tokenizer=tokenizer,
            prompt=args.test_prompt,
            temperature=0.7,
            top_p=0.9,
        )
        
        print(f"Generated text: {generated_text}")
    
    print("\nNote: MLX-LM fine-tuning requires additional setup.")
    print("Please refer to the MLX-LM documentation for fine-tuning instructions:")
    print("https://github.com/ml-explore/mlx-examples/tree/main/llms/mlx-lm")

if __name__ == "__main__":
    main()

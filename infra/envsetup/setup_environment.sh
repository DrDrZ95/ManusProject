#!/bin/bash
# setup_environment.sh
# Script to set up the environment for Qwen3-4B-Instruct model

set -e  # Exit immediately if a command exits with a non-zero status

echo "Setting up environment for Qwen3-4B-Instruct model..."

# Create virtual environment if it doesn't exist
if [ ! -d "/home/ubuntu/ai-agent/venv" ]; then
    echo "Creating Python virtual environment..."
    python3 -m venv /home/ubuntu/ai-agent/venv
else
    echo "Python virtual environment already exists."
fi

# Activate virtual environment
echo "Activating virtual environment..."
source /home/ubuntu/ai-agent/venv/bin/activate

# Install required packages
echo "Installing required packages..."
pip install --upgrade pip
# Install PyTorch with CPU support by default, or CUDA if available (user should manage CUDA setup)
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu
# Transformers, accelerate for efficient loading, sentencepiece for tokenizers, protobuf for some model formats
# einops and tiktoken are often used with Qwen3 models
pip install transformers accelerate bitsandbytes sentencepiece protobuf fastapi uvicorn pydantic einops tiktoken

# Create model directory if it doesn't exist
echo "Ensuring model directory exists..."
mkdir -p /home/ubuntu/ai-agent/models

echo "Environment setup completed successfully!"
echo "Remember to run ./scripts/download_model.sh to get the model files."

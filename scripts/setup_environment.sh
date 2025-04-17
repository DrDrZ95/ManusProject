#!/bin/bash
# setup_environment.sh
# Script to set up the environment for DeepSeek R1 1.5B model

set -e  # Exit immediately if a command exits with a non-zero status

echo "Setting up environment for DeepSeek R1 1.5B model..."

# Create virtual environment
echo "Creating Python virtual environment..."
python3 -m venv /home/ubuntu/deepseek-agent/venv

# Activate virtual environment
echo "Activating virtual environment..."
source /home/ubuntu/deepseek-agent/venv/bin/activate

# Install required packages
echo "Installing required packages..."
pip install --upgrade pip
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu
pip install transformers accelerate bitsandbytes sentencepiece protobuf fastapi uvicorn pydantic

# Create model directory
echo "Creating model directory..."
mkdir -p /home/ubuntu/deepseek-agent/models

echo "Environment setup completed successfully!"

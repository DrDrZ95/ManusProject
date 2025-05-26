#!/bin/bash
# Script to install Unsloth and LoRA fine-tuning dependencies

# Ensure Python 3.10 is available
if ! command -v python3.10 &> /dev/null; then
    echo "Python 3.10 is required but not found. Installing..."
    sudo apt-get update
    sudo apt-get install -y python3.10 python3.10-venv python3.10-dev
fi

# Create a virtual environment for fine-tuning if it doesn't exist
if [ ! -d "finetune/venv" ]; then
    echo "Creating virtual environment for fine-tuning with Python 3.10..."
    python3.10 -m venv finetune/venv
fi

# Activate the virtual environment
source finetune/venv/bin/activate

# Install PyTorch with CUDA support
echo "Installing PyTorch with CUDA support..."
pip install torch==2.1.2 torchvision==0.16.2 torchaudio==2.1.2 --index-url https://download.pytorch.org/whl/cu121

# Install Unsloth with specific version
echo "Installing Unsloth version 2025.3.19..."
pip install unsloth==2025.3.19

# Install other dependencies for fine-tuning
echo "Installing additional dependencies..."
pip install transformers==4.36.2 datasets==2.16.1 peft==0.7.1 accelerate==0.25.0 bitsandbytes==0.41.3 trl==0.7.4 wandb==0.16.1 sentencepiece==0.1.99 gradio==4.12.0

# Install additional utilities
pip install pandas numpy matplotlib scikit-learn

# Create requirements.txt for reproducibility
pip freeze > finetune/requirements.txt

echo "Installation complete! Activate the environment with: source finetune/venv/bin/activate"
echo "Note: Hardware requirements (GPU/CPU) will need to be specified in future planning."

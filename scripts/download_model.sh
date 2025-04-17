#!/bin/bash
# download_model.sh
# Script to download DeepSeek R1 1.5B model

set -e  # Exit immediately if a command exits with a non-zero status

echo "Downloading DeepSeek R1 1.5B model..."

# Create model directory if it doesn't exist
mkdir -p /home/ubuntu/deepseek-agent/models

# Activate virtual environment
source /home/ubuntu/deepseek-agent/venv/bin/activate

# Download the model using Hugging Face transformers
python3 -c "
from huggingface_hub import snapshot_download
import os

model_path = '/home/ubuntu/deepseek-agent/models/deepseek-r1-1.5b'
os.makedirs(model_path, exist_ok=True)

print('Downloading DeepSeek R1 1.5B model from Hugging Face...')
snapshot_download(
    repo_id='deepseek-ai/deepseek-coder-1.5b-base',
    local_dir=model_path,
    local_dir_use_symlinks=False
)
print('Model download completed successfully!')
"

echo "DeepSeek R1 1.5B model download completed!"

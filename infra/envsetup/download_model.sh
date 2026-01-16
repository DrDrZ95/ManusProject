#!/bin/bash
# download_model.sh
# Script to download deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B model

set -e  # Exit immediately if a command exits with a non-zero status

echo "Downloading deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B model..."

# Create model directory if it doesn't exist
mkdir -p /home/ubuntu/ai-agent/models

# Activate virtual environment
if [ -d "/home/ubuntu/ai-agent/venv" ]; then
    source /home/ubuntu/ai-agent/venv/bin/activate
else
    echo "Virtual environment not found at /home/ubuntu/ai-agent/venv. Please run setup_environment.sh first."
    exit 1
fi

# Download the model using Hugging Face transformers
python3 -c "
from huggingface_hub import snapshot_download
import os

model_name = 'deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B'
model_path = f'/home/ubuntu/ai-agent/models/{model_name.replace("deepseek-ai/", "")}'
os.makedirs(model_path, exist_ok=True)

print(f'Downloading {model_name} model from Hugging Face to {model_path}...')
snapshot_download(
    repo_id=model_name,
    local_dir=model_path,
    local_dir_use_symlinks=False,
    # Consider adding allow_patterns for specific file types if needed, e.g., ["*.safetensors", "*.json", "*.py"]
    # ignore_patterns can be used to exclude large or unnecessary files like .bin pytorch_model.bin if .safetensors is preferred
)
print('Model download completed successfully!')
"

echo "DeepSeek-R1-Distill-Qwen3-4B model download completed!"

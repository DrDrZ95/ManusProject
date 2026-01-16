#!/bin/bash
# run_model_server.sh
# Script to run the deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B model server

set -e  # Exit immediately if a command exits with a non-zero status

echo "Starting deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B model server on port 2025..."

# Check if virtual environment exists and activate it if it does
if [ -d "/home/ubuntu/ai-agent/venv/bin" ]; then
    echo "Activating virtual environment..."
    source /home/ubuntu/ai-agent/venv/bin/activate
else
    echo "No virtual environment found, using system Python..."
fi

# Run the model server
cd /home/ubuntu/ai-agent
python3 src/model_server.py

# Note: The server will continue running until manually stopped

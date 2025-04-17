#!/bin/bash
# run_model_server.sh
# Script to run the DeepSeek R1 1.5B model server

set -e  # Exit immediately if a command exits with a non-zero status

echo "Starting DeepSeek R1 1.5B model server on port 2025..."

# Activate virtual environment
source /home/ubuntu/deepseek-agent/venv/bin/activate

# Run the model server
cd /home/ubuntu/deepseek-agent
python3 src/model_server.py

# Note: The server will continue running until manually stopped

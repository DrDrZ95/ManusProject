#!/bin/bash
# install_dependencies.sh
# Script to install system-level dependencies for DeepSeek R1 1.5B model

set -e  # Exit immediately if a command exits with a non-zero status

echo "Installing system dependencies for DeepSeek R1 1.5B model..."

# Update package lists
sudo apt-get update

# Install Python development packages and other dependencies
sudo apt-get install -y \
    python3-dev \
    python3-pip \
    python3-venv \
    build-essential \
    cmake \
    git \
    wget \
    curl \
    libopenblas-dev \
    libomp-dev

echo "System dependencies installation completed successfully!"

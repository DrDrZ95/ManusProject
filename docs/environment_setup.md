# DeepSeek R1 1.5B Model Environment Setup

This document provides instructions for setting up the environment required to run the DeepSeek R1 1.5B model.

## System Requirements

- Linux-based operating system (Ubuntu 20.04 or later recommended)
- Python 3.8 or later
- At least 8GB RAM
- At least 10GB free disk space for the model and dependencies
- CUDA-compatible GPU recommended for faster inference (but CPU-only setup is possible)

## Setup Process

The setup process consists of three main steps:

1. Installing system dependencies
2. Setting up Python environment and packages
3. Downloading the DeepSeek R1 1.5B model

## Step 1: Install System Dependencies

Run the following script to install all required system dependencies:

```bash
./scripts/install_dependencies.sh
```

This script will install:
- Python development packages
- Build tools
- Git
- OpenBLAS and OpenMP libraries

## Step 2: Set Up Python Environment

Run the following script to create a Python virtual environment and install all required packages:

```bash
./scripts/setup_environment.sh
```

This script will:
- Create a Python virtual environment in the `venv` directory
- Install PyTorch and related libraries
- Install Transformers, Accelerate, and other required packages
- Install FastAPI and Uvicorn for API serving

## Step 3: Download the Model

Run the following script to download the DeepSeek R1 1.5B model:

```bash
./scripts/download_model.sh
```

This script will:
- Download the DeepSeek R1 1.5B model from Hugging Face
- Save the model to the `models/deepseek-r1-1.5b` directory

## Verifying the Setup

After completing all the steps above, you can verify that the setup was successful by:

1. Activating the virtual environment:
   ```bash
   source venv/bin/activate
   ```

2. Running a simple test to load the model:
   ```python
   python -c "from transformers import AutoModelForCausalLM, AutoTokenizer; tokenizer = AutoTokenizer.from_pretrained('./models/deepseek-r1-1.5b'); model = AutoModelForCausalLM.from_pretrained('./models/deepseek-r1-1.5b'); print('Model loaded successfully!')"
   ```

## Troubleshooting

If you encounter any issues during the setup process:

1. Check that all system dependencies are installed correctly
2. Ensure you have sufficient disk space for the model
3. Verify that your Python version is compatible (3.8 or later)
4. Check internet connectivity for downloading the model

For specific error messages, refer to the logs in the terminal output.

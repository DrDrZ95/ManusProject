# Llama3.1-8B-Instruct Model Environment Setup

This document provides instructions for setting up the environment required to run the Llama3.1-8B-Instruct model.

## System Requirements

- Linux-based operating system (Ubuntu 20.04 or later recommended)
- Python 3.8 or later
- At least 16GB RAM (Llama3.1-8B models can be resource-intensive)
- At least 20GB free disk space for the model and dependencies
- CUDA-compatible GPU (e.g., NVIDIA Tesla T4, V100, A100) with >= 8GB VRAM highly recommended for reasonable performance. CPU-only setup is possible but will be very slow.

## Setup Process

The setup process consists of three main steps, executed from the `/home/ubuntu/ai-agent` directory:

1. Installing system dependencies
2. Setting up Python environment and packages
3. Downloading the Llama3.1-8B-Instruct model

Make sure all scripts in the `scripts/` directory are executable:
```bash
chmod +x /home/ubuntu/ai-agent/scripts/*.sh
```

## Step 1: Install System Dependencies

Run the following script to install all required system dependencies:

```bash
cd /home/ubuntu/ai-agent
./scripts/install_dependencies.sh
```

This script will install:
- Python development packages (python3-dev, python3-pip, python3-venv)
- Build tools (build-essential, cmake)
- Git, wget, curl
- OpenBLAS and OpenMP libraries for potential CPU-based operations

## Step 2: Set Up Python Environment

Run the following script to create a Python virtual environment and install all required packages:

```bash
cd /home/ubuntu/ai-agent
./scripts/setup_environment.sh
```

This script will:
- Create a Python virtual environment in the `venv` directory within `/home/ubuntu/ai-agent`.
- Install PyTorch and related libraries (torchvision, torchaudio).
- Install Transformers, Accelerate, BitsandBytes, SentencePiece, Protobuf, FastAPI, Uvicorn, Pydantic, Einops, and Tiktoken.

## Step 3: Download the Model

Run the following script to download the Llama3.1-8B-Instruct model:

```bash
cd /home/ubuntu/ai-agent
./scripts/download_model.sh
```

This script will:
- Download the Llama3.1-8B-Instruct model from Hugging Face (`meta-llama/Llama3.1-8B-Instruct`).
- Save the model to the `/home/ubuntu/ai-agent/models/Llama3.1-8B-Instruct` directory.

## Verifying the Setup

After completing all the steps above, you can verify that the setup was successful by:

1. Activating the virtual environment (if not already active):
   ```bash
   cd /home/ubuntu/ai-agent
   source venv/bin/activate
   ```

2. Running a simple test to load the model (this may take some time and memory):
   ```python
   python -c "from transformers import AutoModelForCausalLM, AutoTokenizer; model_path = '/home/ubuntu/ai-agent/models/Llama3.1-8B-Instruct'; tokenizer = AutoTokenizer.from_pretrained(model_path, trust_remote_code=True); model = AutoModelForCausalLM.from_pretrained(model_path, trust_remote_code=True, device_map='auto', torch_dtype=torch.bfloat16 if torch.cuda.is_available() and torch.cuda.is_bf16_supported() else torch.float16 if torch.cuda.is_available() else torch.float32); print('Llama3.1-8B-Instruct model loaded successfully!')"
   ```
   (Note: The python command above should be a single line. The `torch_dtype` part is for optimized loading if CUDA is available.)

## Troubleshooting

If you encounter any issues during the setup process:

1. Check that all system dependencies are installed correctly.
2. Ensure you have sufficient disk space (at least 20GB) and RAM (at least 16GB).
3. Verify that your Python version is compatible (3.8 or later).
4. Check internet connectivity for downloading the model and packages.
5. If using a GPU, ensure CUDA drivers and toolkit are correctly installed and compatible with the PyTorch version.
6. For specific error messages, refer to the logs in the terminal output and the `model_server.log` file once the server is running.

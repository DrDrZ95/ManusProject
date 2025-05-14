# Qwen2-7B-Instruct AI Agent Application

This repository contains a complete implementation of an agent application that deploys the Qwen2-7B-Instruct model locally with an API accessible on port 2025.

## Project Overview

This project provides a complete solution for:

1. Deploying the Qwen2-7B-Instruct language model locally
2. Exposing the model through a RESTful API on port 2025
3. Providing tools for interacting with the API

The implementation includes comprehensive documentation, setup scripts, and example code to help you get started quickly.

## Repository Structure

```
ai-agent/
├── README.md               # Main project documentation
├── config/                 # Configuration files (if any, currently empty)
├── docs/                   # Documentation
│   ├── api_documentation.md    # API documentation
│   ├── environment_setup.md    # Environment setup guide
│   ├── github_upload.md        # GitHub upload instructions
│   └── ssh_setup.md            # SSH key setup guide
├── models/                 # Directory for model files (populated during setup)
│   └── Qwen2-7B-Instruct/  # Model files for Qwen2-7B-Instruct
├── scripts/                # Setup and utility scripts
│   ├── download_model.sh       # Script to download the Qwen model
│   ├── install_dependencies.sh # Script to install system dependencies
│   ├── run_model_server.sh     # Script to run the model server
│   └── setup_environment.sh    # Script to set up Python environment
├── src/                    # Source code
│   ├── api_examples.py         # Examples of API usage
│   └── model_server.py         # FastAPI server implementation
└── venv/                   # Python virtual environment (created by setup_environment.sh)
```

## Quick Start

### Prerequisites

- Linux-based operating system (Ubuntu 20.04 or later recommended)
- Python 3.8 or later
- At least 16GB RAM (Qwen2-7B models can be resource-intensive)
- At least 20GB free disk space for the model and dependencies
- CUDA-compatible GPU (e.g., NVIDIA Tesla T4, V100, A100) with >= 10GB VRAM highly recommended for reasonable performance. CPU-only setup is possible but will be very slow.

### Setup Steps

1. Clone this repository (assuming you have set it up on GitHub):
   ```bash
   git clone git@github.com:your-username/ai-agent.git
   cd ai-agent
   ```
   (If you are working locally without GitHub yet, just navigate to the `/home/ubuntu/ai-agent` directory)

2. Make scripts executable:
   ```bash
   chmod +x scripts/*.sh
   chmod +x src/*.py
   ```

3. Install system dependencies:
   ```bash
   ./scripts/install_dependencies.sh
   ```

4. Set up Python environment and install Python packages:
   ```bash
   ./scripts/setup_environment.sh
   ```

5. Download the Qwen2-7B-Instruct model:
   ```bash
   ./scripts/download_model.sh
   ```

6. Start the model server:
   ```bash
   ./scripts/run_model_server.sh
   ```

The API will be available at `http://localhost:2025`.

## API Usage

Once the server is running, you can interact with it using the provided API examples. Open a new terminal and run:

```bash
# Activate the virtual environment first
source venv/bin/activate

# Basic usage with default prompt
python src/api_examples.py

# Custom prompt
python src/api_examples.py --prompt "Write a short poem about the stars."

# Custom parameters
python src/api_examples.py --max-length 100 --temperature 0.5
```

For detailed API documentation, see [API Documentation](docs/api_documentation.md).

## Detailed Documentation

- [Environment Setup Guide](docs/environment_setup.md): Instructions for setting up the environment
- [API Documentation](docs/api_documentation.md): Detailed API documentation
- [SSH Setup Guide](docs/ssh_setup.md): Instructions for setting up SSH keys for GitHub
- [GitHub Upload Instructions](docs/github_upload.md): Instructions for uploading code to GitHub

## Model Information

This project uses the Qwen2-7B-Instruct model from the Qwen series by Alibaba Cloud. It is a powerful language model designed for instruction following and chat, suitable for a wide range of applications including:

- Code generation and explanation
- Text completion and summarization
- Question answering
- Creative writing and content generation
- Conversational AI

## License

This project framework is licensed under the MIT License. The Qwen2-7B-Instruct model itself is subject to its own license terms (typically Tongyi Qianwen LICENSE AGREEMENT). Please ensure compliance with the model's license.

## Acknowledgements

- [Alibaba Cloud Qwen Team](https://github.com/QwenLM) for the Qwen2 models
- [Hugging Face](https://huggingface.co/) for the Transformers library and model hosting
- [FastAPI](https://fastapi.tiangolo.com/) for the API framework

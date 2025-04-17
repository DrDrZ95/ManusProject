# DeepSeek R1 1.5B Agent Application

This repository contains a complete implementation of an agent application that deploys the DeepSeek R1 1.5B model locally with an API accessible on port 2025.

## Project Overview

This project provides a complete solution for:

1. Deploying the DeepSeek R1 1.5B language model locally
2. Exposing the model through a RESTful API on port 2025
3. Providing tools for interacting with the API

The implementation includes comprehensive documentation, setup scripts, and example code to help you get started quickly.

## Repository Structure

```
deepseek-agent/
├── config/                 # Configuration files
├── docs/                   # Documentation
│   ├── api_documentation.md    # API documentation
│   ├── environment_setup.md    # Environment setup guide
│   ├── github_upload.md        # GitHub upload instructions
│   └── ssh_setup.md            # SSH key setup guide
├── models/                 # Directory for model files (populated during setup)
├── scripts/                # Setup and utility scripts
│   ├── download_model.sh       # Script to download the model
│   ├── install_dependencies.sh # Script to install system dependencies
│   ├── run_model_server.sh     # Script to run the model server
│   └── setup_environment.sh    # Script to set up Python environment
└── src/                    # Source code
    ├── api_examples.py         # Examples of API usage
    └── model_server.py         # FastAPI server implementation
```

## Quick Start

### Prerequisites

- Linux-based operating system (Ubuntu 20.04 or later recommended)
- Python 3.8 or later
- At least 8GB RAM
- At least 10GB free disk space
- CUDA-compatible GPU recommended (but CPU-only setup is possible)

### Setup Steps

1. Clone this repository:
   ```bash
   git clone git@github.com:your-username/deepseek-agent.git
   cd deepseek-agent
   ```

2. Install system dependencies:
   ```bash
   ./scripts/install_dependencies.sh
   ```

3. Set up Python environment:
   ```bash
   ./scripts/setup_environment.sh
   ```

4. Download the model:
   ```bash
   ./scripts/download_model.sh
   ```

5. Start the model server:
   ```bash
   ./scripts/run_model_server.sh
   ```

The API will be available at `http://localhost:2025`.

## API Usage

Once the server is running, you can interact with it using the provided API examples:

```bash
# Basic usage
python src/api_examples.py

# Custom prompt
python src/api_examples.py --prompt "Write a function to calculate prime numbers in Python"

# Custom parameters
python src/api_examples.py --max-length 512 --temperature 0.8
```

For detailed API documentation, see [API Documentation](docs/api_documentation.md).

## Detailed Documentation

- [Environment Setup Guide](docs/environment_setup.md): Instructions for setting up the environment
- [API Documentation](docs/api_documentation.md): Detailed API documentation
- [SSH Setup Guide](docs/ssh_setup.md): Instructions for setting up SSH keys for GitHub
- [GitHub Upload Instructions](docs/github_upload.md): Instructions for uploading code to GitHub

## Model Information

This project uses the DeepSeek R1 1.5B model, which is a powerful language model developed by DeepSeek AI. The model is capable of generating high-quality text based on prompts, making it suitable for a wide range of applications including:

- Code generation
- Text completion
- Question answering
- Creative writing

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements

- [DeepSeek AI](https://github.com/deepseek-ai) for the DeepSeek R1 1.5B model
- [Hugging Face](https://huggingface.co/) for the Transformers library
- [FastAPI](https://fastapi.tiangolo.com/) for the API framework

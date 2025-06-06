# Qwen3-4B-Instruct AI Agent Application with .NET Web API

[中文文档](README.zh_CN.md)

This repository contains an AI agent application. It previously deployed the Qwen3-4B-Instruct model locally with a Python-based FastAPI accessible on port 2025. It is now being structured to include a .NET 8.0 Web API backend and a placeholder for a React frontend.

## Project Overview

This project aims to provide a solution for:

1.  Hosting AI model inference (currently Qwen3-4B-Instruct via existing Python scripts).
2.  Providing a .NET 8.0 Web API backend (`AgentWebApi/`) for agent logic and future integrations.
3.  Reserving a space for a React-based user interface (`AgentUI/`).
4.  Supporting model fine-tuning with Unsloth and LoRA for customization.
5.  Enabling dynamic external system access during inference via MCP integration.
6.  Providing data storage and analytics with ClickHouse and Elasticsearch integration.

The implementation includes comprehensive documentation, setup scripts for the Python-based model serving, and the newly scaffolded .NET Web API project.

## Repository Structure

```
ai-agent/
├── AgentWebApi/            # .NET 8.0 Web API Project
│   ├── Controllers/
│   ├── McpTools/           # Model Context Protocol integration tools
│   │   ├── DynamicExternalAccessTool.cs  # Dynamic external system access
│   │   └── QwenDialogueTool.cs           # Qwen dialogue integration
│   ├── Properties/
│   ├── Services/           # Service implementations
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── AgentWebApi.csproj
│   └── Program.cs
├── AgentUI/                # React Frontend application with chat interface
│   └── agent-chat/         # React chat application with silver theme
├── README.md               # Main project documentation (English)
├── README.zh_CN.md         # Main project documentation (Simplified Chinese)
├── config/                 # Configuration files (if any, currently empty)
├── docker/                 # Docker deployment configuration
│   ├── Dockerfile.webapi   # Dockerfile for .NET Web API
│   ├── Dockerfile.react    # Dockerfile for React UI
│   ├── Dockerfile.python   # Dockerfile for Python model server
│   ├── docker-compose.yml  # Docker Compose configuration
│   ├── examples/           # Example docker-compose configurations
│   │   └── clickhouse-elasticsearch-compose.yml  # ClickHouse and Elasticsearch setup
│   └── nginx.conf          # Nginx configuration for React UI
├── docs/                   # Documentation
│   ├── api_documentation.md    # API documentation (for Python/FastAPI model server)
│   ├── api_documentation.zh_CN.md # API documentation (简体中文 - for Python/FastAPI)
│   ├── docker_quickstart.md    # Docker deployment guide
│   ├── docker_quickstart.zh_CN.md # Docker deployment guide (简体中文)
│   ├── dynamic_external_access.md # Dynamic external system access guide
│   ├── environment_setup.md    # Environment setup guide (for Python model server)
│   ├── environment_setup.zh_CN.md # Environment setup guide (简体中文 - for Python model server)
│   ├── github_upload.md        # GitHub upload instructions
│   ├── github_upload.zh_CN.md  # GitHub upload instructions (简体中文)
│   ├── mcp_integration_guide.zh_CN.md # MCP integration guide (简体中文)
│   ├── ssh_setup.md            # SSH key setup guide
│   ├── ssh_setup.zh_CN.md      # SSH key setup guide (简体中文)
│   ├── unsloth_lora_finetuning.md    # Unsloth+LoRA fine-tuning guide
│   └── unsloth_lora_finetuning.zh_CN.md # Unsloth+LoRA fine-tuning guide (简体中文)
├── finetune/               # Model fine-tuning utilities
│   ├── install_dependencies.sh  # Script to install Unsloth and LoRA dependencies
│   └── utils.py                 # Utility functions for fine-tuning
├── models/                 # Directory for model files (populated by Python script)
│   └── Qwen3-4B-Instruct/  # Model files for Qwen3-4B-Instruct
├── scripts/                # Setup and utility scripts (mostly for Python model server)
│   ├── download_model.sh       # Script to download the Qwen model
│   ├── install_dependencies.sh # Script to install system dependencies for Python
│   ├── run_model_server.sh     # Script to run the Python/FastAPI model server
│   └── setup_environment.sh    # Script to set up Python environment
├── src/                    # Source code (Python/FastAPI model server)
│   ├── api_examples.py         # Examples of API usage (for Python/FastAPI server)
│   └── model_server.py         # Python/FastAPI server implementation
├── .gitignore              # Specifies intentionally untracked files that Git should ignore
└── venv/                   # Python virtual environment (created by setup_environment.sh)
```

**Note:** The existing Python/FastAPI based model server in `src/` and `scripts/` is still present. The new `.NET Web API` in `AgentWebApi/` is a separate component.

## Quick Start

### Docker Deployment (Recommended)

For the fastest setup, use Docker to deploy all components together:

```bash
cd docker
docker-compose up -d
```

This will build and start all services. For detailed instructions, see the [Docker Quick Start Guide](docs/docker_quickstart.md).

### Manual Setup

#### Prerequisites

*   **For Python Qwen3-4B-Instruct Model Server:** (Refer to `docs/environment_setup.md`)
    *   Linux-based OS, Python 3.8+, 16GB+ RAM, 20GB+ Disk, GPU recommended.
*   **For .NET 8.0 Web API (`AgentWebApi/`):**
    *   .NET 8.0 SDK (already installed in this environment).
*   **For React UI (`AgentUI/agent-chat/`):**
    *   Node.js and pnpm (already set up in the project)
*   **For Model Fine-tuning (`finetune/`):**
    *   CUDA-compatible GPU recommended
    *   See [Unsloth+LoRA Fine-tuning Guide](docs/unsloth_lora_finetuning.md)

#### Setup & Running

**1. Python Qwen3-4B-Instruct Model Server (Port 2025):**

   Follow the instructions in the [Environment Setup Guide](docs/environment_setup.md) to set up and run the Python-based Qwen model server. This includes:
   ```bash
   cd /home/ubuntu/ai-agent
   chmod +x scripts/*.sh
   ./scripts/install_dependencies.sh
   ./scripts/setup_environment.sh
   ./scripts/download_model.sh
   ./scripts/run_model_server.sh # Runs on http://localhost:2025
   ```

**2. .NET 8.0 Web API (`AgentWebApi/`):**

   To run the .NET Web API (it will run on a different port, typically Kestrel defaults like 5000/5001 or 7000-series for HTTPS):
   ```bash
   cd /home/ubuntu/ai-agent/AgentWebApi
   dotnet run
   ```
   This API is currently the default template and does not yet interact with the Python model server.

**3. React UI (`AgentUI/agent-chat/`):**

   To run the React chat application:
   ```bash
   cd /home/ubuntu/ai-agent/AgentUI/agent-chat
   pnpm install
   pnpm run dev
   ```
   This will start the development server and you can access the chat interface in your browser.

**4. Model Fine-tuning (Optional):**

   To set up the fine-tuning environment:
   ```bash
   cd /home/ubuntu/ai-agent
   chmod +x finetune/install_dependencies.sh
   ./finetune/install_dependencies.sh
   source finetune/venv/bin/activate
   ```
   
   See the [Unsloth+LoRA Fine-tuning Guide](docs/unsloth_lora_finetuning.md) for detailed usage instructions.

## API Usage (Python/FastAPI Model Server)

Once the Python server is running on port 2025, you can interact with it using `src/api_examples.py`. See [API Documentation](docs/api_documentation.md).

## Streaming Support

The React application includes built-in support for streaming responses from LLM APIs:

- Uses `eventsource-parser` and `@microsoft/fetch-event-source` libraries
- Supports Server-Sent Events (SSE) for real-time streaming
- Handles reconnection and error scenarios
- Ready for integration with the backend LLM API

## Model Fine-tuning

The project includes utilities for fine-tuning language models using Unsloth and LoRA:

- Significantly faster training compared to standard methods
- Reduced memory requirements through parameter-efficient fine-tuning
- Comprehensive utilities for dataset preparation, model loading, and training
- See the [Unsloth+LoRA Fine-tuning Guide](docs/unsloth_lora_finetuning.md) for details

## Detailed Documentation

*   Deployment:
    *   [Docker Quick Start Guide](docs/docker_quickstart.md)
*   Python Model Server:
    *   [Environment Setup Guide](docs/environment_setup.md)
    *   [API Documentation](docs/api_documentation.md)
*   Model Fine-tuning:
    *   [Unsloth+LoRA Fine-tuning Guide](docs/unsloth_lora_finetuning.md)
*   Integration:
    *   [Dynamic External Access](docs/dynamic_external_access.md)
    *   [MCP Integration Guide (中文)](docs/mcp_integration_guide.zh_CN.md)
*   Data Storage:
    *   Docker examples for [ClickHouse and Elasticsearch](docker/examples/clickhouse-elasticsearch-compose.yml)
*   General:
    *   [SSH Setup Guide](docs/ssh_setup.md)
    *   [GitHub Upload Instructions](docs/github_upload.md)

## Model Information

This project uses the Qwen3-4B-Instruct model from Alibaba Cloud for the Python-based inference server.

## License

Project framework: MIT License. Qwen3-4B-Instruct model is subject to its own license.

## Acknowledgements

- Alibaba Cloud Qwen Team, Hugging Face, FastAPI, .NET Team, Unsloth Team.

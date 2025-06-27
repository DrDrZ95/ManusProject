# Docker Deployment and Quick Start Guide

This guide provides instructions for deploying the AI Agent application using Docker, which includes the .NET Web API backend, React frontend, and optionally the Python-based Qwen3 model server.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/) installed on your system
- Git to clone the repository

## Quick Start with Docker

### 1. Clone the Repository

```bash
git clone https://github.com/DrDrZ95/AI-AgentProject.git
cd AI-AgentProject
```

### 2. Build and Run with Docker Compose

The project includes Docker configuration for easy deployment of all components:

```bash
cd docker
docker-compose up -d
```

This will:
- Build and start the .NET Web API on port 5000
- Build and start the React UI on port 3000
- (Optional) Build and start the Python model server on port 2025 (if uncommented in docker-compose.yml)

### 3. Access the Application

- React UI: http://localhost:3000
- .NET Web API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger (if enabled)

## Docker Components

The Docker setup includes:

1. **Web API Container**: .NET 8.0 backend service
   - Dockerfile: `docker/Dockerfile.webapi`
   - Exposes ports 80/443 internally, mapped to 5000/5001 externally

2. **React UI Container**: Frontend with silver-themed chat interface
   - Dockerfile: `docker/Dockerfile.react`
   - Uses Nginx to serve static files
   - Configured to proxy API requests to the backend
   - Exposes port 80 internally, mapped to 3000 externally

3. **Python Model Server Container** (Optional):
   - Dockerfile: `docker/Dockerfile.python`
   - Hosts the Qwen3-4B-Instruct model
   - Exposes port 2025

## Development Setup

For development, you can run each component separately:

### .NET Web API

```bash
cd AgentWebApi
dotnet run
```

### React UI

```bash
cd AgentUI/agent-chat
pnpm install
pnpm run dev
```

### Python Model Server

```bash
# Set up Python environment
./scripts/setup_environment.sh
source venv/bin/activate

# Download model (if not already downloaded)
./scripts/download_model.sh

# Run the server
python src/model_server.py
```

## Streaming Support

The React application includes built-in support for streaming responses from LLM APIs:

- Uses `eventsource-parser` and `@microsoft/fetch-event-source` libraries
- Supports Server-Sent Events (SSE) for real-time streaming
- Handles reconnection and error scenarios
- Ready for integration with the backend LLM API

Example usage of streaming in React components:

```typescript
import { StreamingService } from '../services/StreamingService';

// In your component:
const handleStreamingRequest = async (prompt: string) => {
  let fullResponse = '';
  
  await StreamingService.streamLLMRequest(
    'http://localhost:5000/api/llm/generate',
    { prompt },
    (chunk) => {
      // Handle each chunk as it arrives
      fullResponse += chunk;
      setPartialResponse(fullResponse);
    },
    () => {
      // Handle completion
      console.log('Stream complete');
    },
    (error) => {
      // Handle errors
      console.error('Stream error:', error);
    }
  );
};
```

## Configuration

### Environment Variables

You can customize the deployment by setting environment variables in the docker-compose.yml file:

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
- Add custom environment variables as needed

### Volumes

For persistent data, uncomment and configure the volumes section in docker-compose.yml.

## Troubleshooting

- **Container fails to start**: Check logs with `docker logs <container-name>`
- **Network issues**: Ensure ports are not in use by other applications
- **Model server memory**: Adjust memory limits in docker-compose.yml based on your model size

## Next Steps

- Configure the .NET backend to communicate with the Python model server
- Implement authentication and authorization
- Add monitoring and logging
- Set up CI/CD pipelines for automated deployment

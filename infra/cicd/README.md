# CI/CD Infrastructure

This document outlines the CI/CD (Continuous Integration/Continuous Deployment) infrastructure for the project, which is based on Jenkins.

## Jenkins Setup

The CI/CD pipelines are defined in `Jenkinsfile` files located in the `infra/cicd/jenkins` directory. There are separate pipelines for the `agent-api` and `agent-ui` projects.

### Agent-API Pipeline

The `agent-api` pipeline consists of the following stages:

1.  **Build**: Builds the .NET project.
2.  **Test**: Runs the unit tests for the `Agent.Api.Tests` project.
3.  **Publish**: Publishes the release version of the `Agent.Api` project.
4.  **Build Docker Image**: Builds a Docker image for the `agent-api` project using the `Dockerfile.webapi`.

### Agent-UI Pipeline

The `agent-ui` pipeline consists of the following stages:

1.  **Install Dependencies**: Installs the Node.js dependencies using `pnpm`.
2.  **Build**: Builds the React application.
3.  **Build Docker Image**: Builds a Docker image for the `agent-ui` project using the `Dockerfile.react`.

## One-Click Build Scripts

For local development and testing, one-click build scripts are provided in the `infra/cicd/jenkins` directory for each project.

*   `agent-api/build.sh`: Builds, tests, publishes, and creates a Docker image for the `agent-api` project.
*   `agent-ui/build.sh`: Installs dependencies, builds, and creates a Docker image for the `agent-ui` project.

To run the scripts, make them executable and then run them from their respective directories:

```bash
chmod +x build.sh
./build.sh
```

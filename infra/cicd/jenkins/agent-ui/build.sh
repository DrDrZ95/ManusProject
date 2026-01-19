#!/bin/bash
# Agent-UI 一键构建脚本 (Agent-UI One-click build script)

# 安装依赖 (Install Dependencies)
cd ../../../apps/agent-ui && pnpm install

# 代码检查 (Linting)
pnpm run lint

# 构建 (Build)
pnpm run build

# 构建 Docker 镜像 (Build Docker Image)
docker build -t agent-ui:latest -f ../../../infra/docker/Dockerfile.react .

echo "Agent-UI build completed successfully!"

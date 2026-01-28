#!/bin/bash
# 全局构建与验证脚本 (Global Build & Verification Script)
# 此脚本模拟 CI/CD 流程，用于本地验证后端、前端和 Docker 构建
# This script simulates the CI/CD pipeline for local verification of backend, frontend, and Docker builds.

set -e # 遇到错误立即停止 (Stop on error)

# 颜色定义 (Color definitions)
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}>>> 阶段 1: 后端构建与测试 (Backend Build & Test)${NC}"
# 1. 后端构建 (Backend Build)
dotnet restore ../../apps/agent-api/Agent.Api/Agent.Api.csproj
dotnet build ../../apps/agent-api/Agent.Api/Agent.Api.csproj --configuration Release

# 2. 后端测试 (Backend Test)
dotnet test ../../test/Agent.Api.Tests/Agent.Api.Tests.csproj --configuration Release
echo -e "${GREEN}后端验证通过! (Backend verification passed!)${NC}\n"

echo -e "${GREEN}>>> 阶段 2: 前端构建与测试 (Frontend Build & Test)${NC}"
# 3. 前端构建 (Frontend Build)
cd ../../apps/agent-ui
if [ -f "pnpm-lock.yaml" ]; then
    pnpm install
    pnpm run lint
    pnpm run build
    # pnpm test # 如果有测试脚本则取消注释
else
    npm install
    npm run build
fi
cd ../../infra/cicd
echo -e "${GREEN}前端验证通过! (Frontend verification passed!)${NC}\n"

echo -e "${GREEN}>>> 阶段 3: Docker 镜像构建验证 (Docker Build Verification)${NC}"
# 4. Docker 构建 (Docker Build)
cd ../..
docker build -t agent-api:verify -f infra/docker/Dockerfile.webapi .
docker build -t agent-ui:verify -f infra/docker/Dockerfile.react .

# 5. 健康检查模拟 (Health Check Simulation)
echo "正在验证 Docker 镜像... (Verifying Docker images...)"
if docker images | grep -q "agent-api" && docker images | grep -q "agent-ui"; then
    echo -e "${GREEN}Docker 镜像构建成功! (Docker images built successfully!)${NC}"
else
    echo -e "${RED}Docker 镜像构建失败! (Docker image build failed!)${NC}"
    exit 1
fi

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}所有构建验证任务已成功完成! (All build verification tasks completed successfully!)${NC}"
echo -e "${GREEN}========================================${NC}"

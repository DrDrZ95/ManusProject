#!/bin/bash
# Agent-API 一键构建脚本 (Agent-API One-click build script)

# 构建 (Build)
dotnet build ../../../apps/agent-api/Agent.Api/Agent.Api.csproj --configuration Release

# 测试与覆盖率 (Test & Coverage)
dotnet test ../../../test/Agent.Api.Tests/Agent.Api.Tests.csproj --configuration Release --collect:"XPlat Code Coverage"

# 发布 (Publish)
dotnet publish ../../../apps/agent-api/Agent.Api/Agent.Api.csproj -c Release -o ./publish

# 构建 Docker 镜像 (Build Docker Image)
docker build -t agent-api:latest -f ../../../infra/docker/Dockerfile.webapi .

echo "Agent-API build completed successfully!"

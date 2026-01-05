#!/bin/bash
# Agent-API 一键构建脚本 (Agent-API One-click build script)

# 构建 (Build)
dotnet build ../../../apps/agent-api/Agent.Api/Agent.Api.csproj

# 测试 (Test)
dotnet test ../../../test/Agent.Api.Tests/Agent.Api.Tests.csproj

# 发布 (Publish)
dotnet publish ../../../apps/agent-api/Agent.Api/Agent.Api.csproj -c Release -o ./publish

# 构建 Docker 镜像 (Build Docker Image)
docker build -t agent-api:latest -f ../../../infra/docker/Dockerfile.webapi .

echo "Agent-API build completed successfully!"

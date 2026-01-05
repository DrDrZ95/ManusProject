#!/bin/bash
# 启动所有 Docker 服务 (Start all Docker services)

# 启动主编排 (Start main orchestration)
docker-compose -f docker-compose.yml up -d

# 启动数据库 (Start database)
docker-compose -f database/postgres/docker-compose.yml up -d

# 启动缓存 (Start cache)
docker-compose -f cache/redis/docker-compose.yml up -d

# 启动向量数据库 (Start vector database)
docker-compose -f vector/chromadb/docker-compose.yml up -d

# 启动监控 (Start monitoring)
docker-compose -f monitoring/prometheus/docker-compose.yml up -d
docker-compose -f monitoring/grafana/docker-compose.yml up -d

# 启动追踪 (Start tracing)
docker-compose -f tracing/jaeger/docker-compose.yml up -d

# 启动反向代理 (Start reverse proxy)
docker-compose -f proxy/nginx/docker-compose.yml up -d

echo "All services started successfully!"

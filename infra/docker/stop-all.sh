#!/bin/bash
# 停止所有 Docker 服务 (Stop all Docker services)

# 停止主编排 (Stop main orchestration)
docker-compose -f docker-compose.yml down

# 停止数据库 (Stop database)
docker-compose -f database/postgres/docker-compose.yml down

# 停止缓存 (Stop cache)
docker-compose -f cache/redis/docker-compose.yml down

# 停止向量数据库 (Stop vector database)
docker-compose -f vector/chromadb/docker-compose.yml down

# 停止监控 (Stop monitoring)
docker-compose -f monitoring/prometheus/docker-compose.yml down
docker-compose -f monitoring/grafana/docker-compose.yml down

# 停止追踪 (Stop tracing)
docker-compose -f tracing/jaeger/docker-compose.yml down

# 停止反向代理 (Stop reverse proxy)
docker-compose -f proxy/nginx/docker-compose.yml down

echo "All services stopped successfully!"

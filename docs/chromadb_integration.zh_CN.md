# ChromaDB 集成文档

## 概述
本文档描述了 AgentApi 项目中的 ChromaDB 集成，包括设置、配置和使用示例。

## 架构

### 使用的设计模式
1. **仓储模式**: `IChromaDbService` 抽象数据访问操作
2. **依赖注入**: 服务通过 DI 容器注册和注入
3. **扩展方法模式**: 使用扩展方法模块化配置
4. **构建器模式**: 应用程序配置遵循构建器模式

### 组件
- `ChromaDbService`: 实现 `IChromaDbService` 的主要服务
- `ChromaDbExtensions`: 服务注册的扩展方法
- `ChromaDbController`: ChromaDB 操作的 REST API 端点
- 部署的 Docker 配置

## 配置

### 连接字符串
添加到 `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ChromaDb": "http://localhost:8000"
  }
}
```

### 服务注册
服务在 `Program.cs` 中自动注册:
```csharp
builder.Services.AddChromaDb(builder.Configuration);
```

## API 端点

### 集合
- `GET /api/chromadb/collections` - 列出所有集合
- `POST /api/chromadb/collections` - 创建新集合
- `GET /api/chromadb/collections/{name}` - 获取集合详情
- `DELETE /api/chromadb/collections/{name}` - 删除集合

### 文档
- `POST /api/chromadb/collections/{name}/documents` - 添加文档
- `GET /api/chromadb/collections/{name}/documents` - 获取文档
- `POST /api/chromadb/collections/{name}/query` - 查询文档

## 使用示例

### 创建集合
```http
POST /api/chromadb/collections
Content-Type: application/json

{
  "name": "my_collection",
  "metadata": {
    "description": "测试用的示例集合"
  }
}
```

### 添加文档
```http
POST /api/chromadb/collections/my_collection/documents
Content-Type: application/json

{
  "documents": [
    "这是第一个文档",
    "这是第二个文档"
  ],
  "ids": ["doc1", "doc2"],
  "metadatas": [
    {"source": "manual", "type": "text"},
    {"source": "manual", "type": "text"}
  ]
}
```

### 查询文档
```http
POST /api/chromadb/collections/my_collection/query
Content-Type: application/json

{
  "queryTexts": ["搜索文档"],
  "nResults": 5
}
```

## Docker 部署

### 使用 Docker Compose
```bash
# 启动 ChromaDB 和 AgentApi
docker-compose -f docker-compose.chromadb.yml up -d

# 查看日志
docker-compose -f docker-compose.chromadb.yml logs -f

# 停止服务
docker-compose -f docker-compose.chromadb.yml down
```

### 独立 ChromaDB
```bash
# 构建 ChromaDB 镜像
docker build -t chromadb-custom ./docker/chromadb

# 运行 ChromaDB 容器
docker run -d \
  --name chromadb \
  -p 8000:8000 \
  -v chromadb_data:/chroma/chroma \
  chromadb-custom
```

## 安全性

### 身份验证
ChromaDB 配置了基本身份验证:
- 默认管理员凭据: `admin:chromadb123`
- 默认用户凭据: `user:userpass`

**重要**: 在生产环境中更改默认凭据！

### CORS 配置
CORS 配置为允许所有来源用于开发。在生产中限制:
```csharp
builder.Services.AddChromaDb(options =>
{
    options.Url = "http://chromadb:8000";
    options.TimeoutSeconds = 30;
});
```

## 监控和健康检查

### 健康端点
- ChromaDB: `http://localhost:8000/api/v1/heartbeat`
- AgentApi: `http://localhost:5000/health`
- Nginx: `http://localhost:80/health`

### 日志记录
所有操作都使用结构化日志记录:
- 成功操作的信息级别
- 失败的错误级别，包含异常详情

## 故障排除

### 常见问题
1. **连接被拒绝**: 确保 ChromaDB 正在运行且可访问
2. **身份验证错误**: 检查 `server.htpasswd` 中的凭据
3. **CORS 错误**: 验证 ChromaDB 和 nginx 中的 CORS 配置

### 调试命令
```bash
# 检查 ChromaDB 状态
curl http://localhost:8000/api/v1/heartbeat

# 列出集合
curl http://localhost:5000/api/chromadb/collections

# 检查容器日志
docker logs chromadb
docker logs agent-webapi
```

## 性能考虑

### 优化提示
1. 为文档操作使用适当的批处理大小
2. 为高吞吐量场景实现连接池
3. 监控大型文档集合的内存使用
4. 使用元数据过滤来提高查询性能

### 扩展
- ChromaDB 通过集群支持水平扩展
- 考虑使用持久卷来保证数据持久性
- 为频繁访问的集合实现缓存


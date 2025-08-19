# ChromaDB Integration Documentation

## Overview
This document describes the ChromaDB integration in the Agent.Api project, including setup, configuration, and usage examples.

## Architecture

### Design Patterns Used
1. **Repository Pattern**: `IChromaDbService` abstracts data access operations
2. **Dependency Injection**: Services are registered and injected through DI container
3. **Extension Method Pattern**: Configuration is modularized using extension methods
4. **Builder Pattern**: Application configuration follows the builder pattern

### Components
- `ChromaDbService`: Main service implementing `IChromaDbService`
- `ChromaDbExtensions`: Extension methods for service registration
- `ChromaDbController`: REST API endpoints for ChromaDB operations
- Docker configuration for deployment

## Configuration

### Connection String
Add to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ChromaDb": "http://localhost:8000"
  }
}
```

### Service Registration
Services are automatically registered in `Program.cs`:
```csharp
builder.Services.AddChromaDb(builder.Configuration);
```

## API Endpoints

### Collections
- `GET /api/chromadb/collections` - List all collections
- `POST /api/chromadb/collections` - Create a new collection
- `GET /api/chromadb/collections/{name}` - Get collection details
- `DELETE /api/chromadb/collections/{name}` - Delete a collection

### Documents
- `POST /api/chromadb/collections/{name}/documents` - Add documents
- `GET /api/chromadb/collections/{name}/documents` - Get documents
- `POST /api/chromadb/collections/{name}/query` - Query documents

## Usage Examples

### Creating a Collection
```http
POST /api/chromadb/collections
Content-Type: application/json

{
  "name": "my_collection",
  "metadata": {
    "description": "Sample collection for testing"
  }
}
```

### Adding Documents
```http
POST /api/chromadb/collections/my_collection/documents
Content-Type: application/json

{
  "documents": [
    "This is the first document",
    "This is the second document"
  ],
  "ids": ["doc1", "doc2"],
  "metadatas": [
    {"source": "manual", "type": "text"},
    {"source": "manual", "type": "text"}
  ]
}
```

### Querying Documents
```http
POST /api/chromadb/collections/my_collection/query
Content-Type: application/json

{
  "queryTexts": ["search for documents"],
  "nResults": 5
}
```

## Docker Deployment

### Using Docker Compose
```bash
# Start ChromaDB and Agent.Api
docker-compose -f docker-compose.chromadb.yml up -d

# View logs
docker-compose -f docker-compose.chromadb.yml logs -f

# Stop services
docker-compose -f docker-compose.chromadb.yml down
```

### Standalone ChromaDB
```bash
# Build ChromaDB image
docker build -t chromadb-custom ./docker/chromadb

# Run ChromaDB container
docker run -d \
  --name chromadb \
  -p 8000:8000 \
  -v chromadb_data:/chroma/chroma \
  chromadb-custom
```

## Security

### Authentication
ChromaDB is configured with basic authentication:
- Default admin credentials: `admin:chromadb123`
- Default user credentials: `user:userpass`

**Important**: Change default credentials in production!

### CORS Configuration
CORS is configured to allow all origins for development. Restrict in production:
```csharp
builder.Services.AddChromaDb(options =>
{
    options.Url = "http://chromadb:8000";
    options.TimeoutSeconds = 30;
});
```

## Monitoring and Health Checks

### Health Endpoints
- ChromaDB: `http://localhost:8000/api/v1/heartbeat`
- Agent.Api: `http://localhost:5000/health`
- Nginx: `http://localhost:80/health`

### Logging
All operations are logged with structured logging:
- Information level for successful operations
- Error level for failures with exception details

## Troubleshooting

### Common Issues
1. **Connection refused**: Ensure ChromaDB is running and accessible
2. **Authentication errors**: Check credentials in `server.htpasswd`
3. **CORS errors**: Verify CORS configuration in both ChromaDB and nginx

### Debug Commands
```bash
# Check ChromaDB status
curl http://localhost:8000/api/v1/heartbeat

# List collections
curl http://localhost:5000/api/chromadb/collections

# Check container logs
docker logs chromadb
docker logs agent-webapi
```

## Performance Considerations

### Optimization Tips
1. Use appropriate batch sizes for document operations
2. Implement connection pooling for high-throughput scenarios
3. Monitor memory usage with large document collections
4. Use metadata filtering to improve query performance

### Scaling
- ChromaDB supports horizontal scaling through clustering
- Consider using persistent volumes for data durability
- Implement caching for frequently accessed collections


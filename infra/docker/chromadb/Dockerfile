# ChromaDB Standalone Dockerfile
FROM chromadb/chroma:latest

# Set environment variables
ENV CHROMA_HOST=0.0.0.0
ENV CHROMA_PORT=8000
ENV CHROMA_LOG_LEVEL=INFO
ENV CHROMA_CORS_ALLOW_ORIGINS=["*"]

# Expose the port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8000/api/v1/heartbeat || exit 1

# Start ChromaDB
CMD ["uvicorn", "chromadb.app:app", "--host", "0.0.0.0", "--port", "8000"]


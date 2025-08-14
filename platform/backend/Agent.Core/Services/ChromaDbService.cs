using ChromaDB.Client;
using ChromaDB.Client.Models;

namespace Agent.Core.Services;

/// <summary>
/// Service for managing ChromaDB operations
/// 服务用于管理 ChromaDB 操作
/// </summary>
public interface IChromaDbService
{
    Task<Collection> CreateCollectionAsync(string name, Dictionary<string, object>? metadata = null);
    Task<Collection> GetCollectionAsync(string name);
    Task<bool> DeleteCollectionAsync(string name);
    Task<IEnumerable<Collection>> ListCollectionsAsync();
    Task AddDocumentsAsync(string collectionName, IEnumerable<string> documents, IEnumerable<string>? ids = null, IEnumerable<Dictionary<string, object>>? metadatas = null);
    Task<QueryResponse> QueryAsync(string collectionName, IEnumerable<string> queryTexts, int nResults = 10, Dictionary<string, object>? where = null);
    Task<GetResponse> GetDocumentsAsync(string collectionName, IEnumerable<string>? ids = null, Dictionary<string, object>? where = null);
    Task UpdateDocumentsAsync(string collectionName, IEnumerable<string> ids, IEnumerable<string>? documents = null, IEnumerable<Dictionary<string, object>>? metadatas = null);
    Task DeleteDocumentsAsync(string collectionName, IEnumerable<string>? ids = null, Dictionary<string, object>? where = null);
}

/// <summary>
/// ChromaDB service implementation using Repository pattern
/// 使用仓储模式的 ChromaDB 服务实现
/// </summary>
public class ChromaDbService : IChromaDbService
{
    private readonly ChromaClient _client;
    private readonly ILogger<ChromaDbService> _logger;

    public ChromaDbService(ChromaClient client, ILogger<ChromaDbService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new collection
    /// 创建新集合
    /// </summary>
    public async Task<Collection> CreateCollectionAsync(string name, Dictionary<string, object>? metadata = null)
    {
        try
        {
            _logger.LogInformation("Creating collection: {CollectionName}", name);
            
            var collection = await _client.GetOrCreateCollection(name, metadata);
            
            _logger.LogInformation("Successfully created collection: {CollectionName}", name);
            return collection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create collection: {CollectionName}", name);
            throw;
        }
    }

    /// <summary>
    /// Get an existing collection
    /// 获取现有集合
    /// </summary>
    public async Task<Collection> GetCollectionAsync(string name)
    {
        try
        {
            _logger.LogInformation("Getting collection: {CollectionName}", name);
            
            var collection = await _client.GetOrCreateCollection(name);
            
            _logger.LogInformation("Successfully retrieved collection: {CollectionName}", name);
            return collection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection: {CollectionName}", name);
            throw;
        }
    }

    /// <summary>
    /// Delete a collection
    /// 删除集合
    /// </summary>
    public async Task<bool> DeleteCollectionAsync(string name)
    {
        try
        {
            _logger.LogInformation("Deleting collection: {CollectionName}", name);
            
            await _client.DeleteCollectionAsync(name);
            
            _logger.LogInformation("Successfully deleted collection: {CollectionName}", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete collection: {CollectionName}", name);
            return false;
        }
    }

    /// <summary>
    /// List all collections
    /// 列出所有集合
    /// </summary>
    public async Task<IEnumerable<Collection>> ListCollectionsAsync()
    {
        try
        {
            _logger.LogInformation("Listing all collections");
            
            var collections = await _client.ListCollectionsAsync();
            
            _logger.LogInformation("Successfully listed {Count} collections", collections.Count());
            return collections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list collections");
            throw;
        }
    }

    /// <summary>
    /// Add documents to a collection
    /// 向集合添加文档
    /// </summary>
    public async Task AddDocumentsAsync(string collectionName, IEnumerable<string> documents, IEnumerable<string>? ids = null, IEnumerable<Dictionary<string, object>>? metadatas = null)
    {
        try
        {
            _logger.LogInformation("Adding documents to collection: {CollectionName}", collectionName);
            
            var collection = await GetCollectionAsync(collectionName);
            await collection.AddAsync(ids, null, metadatas, documents);
            
            _logger.LogInformation("Successfully added {Count} documents to collection: {CollectionName}", documents.Count(), collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add documents to collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Query documents from a collection
    /// 从集合查询文档
    /// </summary>
    public async Task<QueryResponse> QueryAsync(string collectionName, IEnumerable<string> queryTexts, int nResults = 10, Dictionary<string, object>? where = null)
    {
        try
        {
            _logger.LogInformation("Querying collection: {CollectionName} with {Count} query texts", collectionName, queryTexts.Count());
            
            var collection = await GetCollectionAsync(collectionName);
            var result = await collection.QueryAsync(queryTexts, nResults, where);
            
            _logger.LogInformation("Successfully queried collection: {CollectionName}", collectionName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Get documents from a collection
    /// 从集合获取文档
    /// </summary>
    public async Task<GetResponse> GetDocumentsAsync(string collectionName, IEnumerable<string>? ids = null, Dictionary<string, object>? where = null)
    {
        try
        {
            _logger.LogInformation("Getting documents from collection: {CollectionName}", collectionName);
            
            var collection = await GetCollectionAsync(collectionName);
            var result = await collection.GetAsync(ids, where);
            
            _logger.LogInformation("Successfully retrieved documents from collection: {CollectionName}", collectionName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents from collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Update documents in a collection
    /// 更新集合中的文档
    /// </summary>
    public async Task UpdateDocumentsAsync(string collectionName, IEnumerable<string> ids, IEnumerable<string>? documents = null, IEnumerable<Dictionary<string, object>>? metadatas = null)
    {
        try
        {
            _logger.LogInformation("Updating documents in collection: {CollectionName}", collectionName);
            
            var collection = await GetCollectionAsync(collectionName);
            await collection.UpdateAsync(ids, null, metadatas, documents);
            
            _logger.LogInformation("Successfully updated documents in collection: {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update documents in collection: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Delete documents from a collection
    /// 从集合删除文档
    /// </summary>
    public async Task DeleteDocumentsAsync(string collectionName, IEnumerable<string>? ids = null, Dictionary<string, object>? where = null)
    {
        try
        {
            _logger.LogInformation("Deleting documents from collection: {CollectionName}", collectionName);
            
            var collection = await GetCollectionAsync(collectionName);
            await collection.DeleteAsync(ids, where);
            
            _logger.LogInformation("Successfully deleted documents from collection: {CollectionName}", collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete documents from collection: {CollectionName}", collectionName);
            throw;
        }
    }
}


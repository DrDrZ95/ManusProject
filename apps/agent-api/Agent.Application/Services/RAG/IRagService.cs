namespace Agent.Application.Services.RAG;

/// <summary>
/// RAG (Retrieval Augmented Generation) service interface
/// RAG（检索增强生成）服务接口
/// </summary>
public interface IRagService
{
    // Document Management - 文档管理
    Task<string> AddDocumentAsync(string collectionName, RagDocument document);
    Task<IEnumerable<RagDocument>> GetDocumentsAsync(string collectionName, IEnumerable<string>? ids = null);
    Task UpdateDocumentAsync(string collectionName, RagDocument document);
    Task DeleteDocumentAsync(string collectionName, string documentId);
    Task<int> GetDocumentCountAsync(string collectionName);

    // Hybrid Retrieval - 混合检索
    Task<RagRetrievalResult> HybridRetrievalAsync(string collectionName, RagQuery query);
    Task<RagRetrievalResult> VectorRetrievalAsync(string collectionName, string query, int topK = 10);
    Task<RagRetrievalResult> KeywordRetrievalAsync(string collectionName, string query, int topK = 10);
    Task<RagRetrievalResult> SemanticRetrievalAsync(string collectionName, string query, int topK = 10);

    // RAG Generation - RAG生成
    Task<RagResponse> GenerateResponseAsync(string collectionName, RagGenerationRequest request);
    Task<IAsyncEnumerable<string>> GenerateStreamingResponseAsync(string collectionName, RagGenerationRequest request);

    // Enterprise Scenarios - 企业场景
    Task<RagResponse> EnterpriseQAAsync(string knowledgeBase, string question, RagOptions? options = null);
    Task<RagResponse> DocumentSummarizationAsync(string collectionName, string documentId, RagSummaryOptions? options = null);
    Task<RagResponse> MultiDocumentAnalysisAsync(string collectionName, IEnumerable<string> documentIds, string analysisQuery);

    // Collection Management - 集合管理
    Task<string> CreateKnowledgeBaseAsync(string name, RagCollectionConfig config);
    Task DeleteKnowledgeBaseAsync(string name);
    Task<IEnumerable<string>> ListKnowledgeBasesAsync();
    Task<object> QueryAsync(string collectionName, string query, RagOptions ragOptions);
    Task<string> RetrieveAndGenerateAsync(string input);
}


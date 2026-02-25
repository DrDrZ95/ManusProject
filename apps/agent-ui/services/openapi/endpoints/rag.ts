import { apiClient } from '../client';
import { RagDocument, RagQuery, ApiResponse } from '../types';

/**
 * RAG (Retrieval-Augmented Generation) 相关 API
 * RAG related APIs
 */

/**
 * 上传文档用于 RAG
 * Upload document for RAG
 * @param document 文档内容
 * @returns 上传结果
 */
export const uploadRagDocument = async (document: RagDocument): Promise<ApiResponse<string>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/rag/upload', document);
  console.log('API Call: uploadRagDocument', document);
  return { data: 'doc_id_123', status: 200, message: 'Document uploaded successfully' };
};

/**
 * 查询 RAG 知识库
 * Query RAG knowledge base
 * @param query 查询参数
 * @returns 检索结果
 */
export const queryRagKnowledgeBase = async (query: RagQuery): Promise<ApiResponse<any[]>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/rag/query', query);
  console.log('API Call: queryRagKnowledgeBase', query);
  return { 
    data: [
      { id: '1', content: 'Relevant content 1', score: 0.95 },
      { id: '2', content: 'Relevant content 2', score: 0.88 }
    ], 
    status: 200 
  };
};

/**
 * 删除 RAG 文档
 * Delete RAG document
 * @param documentId 文档 ID
 * @returns 删除结果
 */
export const deleteRagDocument = async (documentId: string): Promise<ApiResponse<void>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.delete(`/rag/document/${documentId}`);
  console.log('API Call: deleteRagDocument', documentId);
  return { data: undefined, status: 200, message: 'Document deleted' };
};

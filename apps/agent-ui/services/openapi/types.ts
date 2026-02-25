/**
 * API 类型定义 - API Type Definitions
 * 基于 openapi.json - Based on openapi.json
 */

// 通用响应结构 - Common Response Structure
export interface ApiResponse<T> {
  data: T;
  message?: string;
  status: number;
}

// RAG 相关类型 - RAG Related Types
export interface RagDocument {
  id?: string;
  content: string;
  metadata?: Record<string, any>;
}

export interface RagQuery {
  query: string;
  topK?: number;
  filters?: Record<string, any>;
}

export interface RagGenerationRequest {
  prompt: string;
  context?: string;
  stream?: boolean;
}

// 微调相关类型 - Finetune Related Types
export interface FinetuneRequest {
  model: string;
  dataset: string;
  epochs?: number;
  learningRate?: number;
}

export interface FinetuneJobStatus {
  jobId: string;
  status: 'pending' | 'running' | 'completed' | 'failed';
  progress: number;
  logs?: string[];
}

// 沙箱终端相关类型 - Sandbox Terminal Related Types
export interface ExecuteCommandRequest {
  command: string;
  timeout?: number;
}

export interface CommandResult {
  stdout: string;
  stderr: string;
  exitCode: number;
}

// Semantic Kernel 相关类型 - Semantic Kernel Related Types
export interface ChatCompletionRequest {
  messages: Array<{ role: string; content: string }>;
  temperature?: number;
  maxTokens?: number;
  stream?: boolean;
}

export interface ChatCompletionResponse {
  content: string;
  usage?: { promptTokens: number; completionTokens: number };
}

// Prompt 相关类型 - Prompt Related Types
export interface PromptTemplate {
  name: string;
  category: string;
  template: string;
  variables: string[];
}

// 工具相关类型 - Tool Related Types
export interface ToolMetadata {
  name: string;
  description: string;
  version: string;
  parameters: Record<string, any>;
}

// 用户输入相关类型 - User Input Related Types
export interface UserInputRequest {
  input: string;
  sessionId: string;
  context?: Record<string, any>;
}

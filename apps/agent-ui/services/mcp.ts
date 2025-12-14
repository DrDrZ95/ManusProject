
import { McpTool, McpResource, McpToolCallRequest, McpToolCallResponse } from '../types';

/**
 * Model Context Protocol (MCP) Client
 * 模型上下文协议客户端
 * 
 * Concept:
 * MCP is a standard for connecting AI models to external data and tools.
 * This class simulates a client that connects to an MCP Server (e.g., a local agent runtime).
 * 
 * 概念：
 * MCP 是连接 AI 模型与外部数据/工具的标准协议。
 * 此类模拟连接到 MCP 服务器（例如本地 Agent 运行时）的客户端。
 * 
 * Protocol Flow:
 * 1. Handshake (Initialize)
 * 2. Discovery (List Tools/Resources)
 * 3. Execution (Call Tool)
 */
class McpClient {
  private static instance: McpClient;
  private isConnected: boolean = false;
  private serverName: string = 'Agent-Local-Runtime';

  private constructor() {}

  public static getInstance(): McpClient {
    if (!McpClient.instance) {
      McpClient.instance = new McpClient();
    }
    return McpClient.instance;
  }

  /**
   * Initialize MCP Connection
   * 初始化 MCP 连接 (握手)
   */
  public async connect(): Promise<boolean> {
    console.log(`[MCP] 🔌 Connecting to ${this.serverName}...`);
    await new Promise(resolve => setTimeout(resolve, 500));
    this.isConnected = true;
    console.log(`[MCP] ✅ Connected to server version 1.0.2`);
    return true;
  }

  /**
   * List Available Tools
   * 列出可用工具
   */
  public async listTools(): Promise<McpTool[]> {
    this.ensureConnection();
    console.log('[MCP] 🔍 Discovering tools...');
    
    await new Promise(resolve => setTimeout(resolve, 300));

    // Mock Tools defined by the local environment
    return [
      {
        name: 'read_file',
        description: 'Read contents of a file from the allowed workspace.',
        inputSchema: {
          type: 'object',
          properties: {
            path: { type: 'string' }
          },
          required: ['path']
        }
      },
      {
        name: 'execute_command',
        description: 'Run a shell command in the sandbox.',
        inputSchema: {
          type: 'object',
          properties: {
            command: { type: 'string' }
          },
          required: ['command']
        }
      },
      {
        name: 'search_knowledge_base',
        description: 'Semantic search over internal documents.',
        inputSchema: {
          type: 'object',
          properties: {
            query: { type: 'string' },
            limit: { type: 'number' }
          },
          required: ['query']
        }
      }
    ];
  }

  /**
   * List Available Resources
   * 列出可用资源
   */
  public async listResources(): Promise<McpResource[]> {
    this.ensureConnection();
    // Mock Resources
    return [
      { uri: 'file:///workspace/readme.md', name: 'Project Readme', mimeType: 'text/markdown' },
      { uri: 'postgres://db/users/schema', name: 'User Database Schema', mimeType: 'application/sql' }
    ];
  }

  /**
   * Call a Tool
   * 调用工具
   */
  public async callTool(request: McpToolCallRequest): Promise<McpToolCallResponse> {
    this.ensureConnection();
    console.log(`[MCP] 🛠️ Calling tool: ${request.name}`, request.arguments);

    await new Promise(resolve => setTimeout(resolve, 1500)); // Simulate execution time

    // Mock Responses based on tool name
    switch (request.name) {
      case 'read_file':
        return {
          content: [{ 
            type: 'text', 
            text: '# Project Config\nport=8080\nenv=production' 
          }]
        };
      case 'execute_command':
        return {
          content: [{
            type: 'text',
            text: 'stdout: Package installed successfully.\nstderr: 0 vulnerabilities found.'
          }]
        };
      case 'search_knowledge_base':
        return {
          content: [{
            type: 'text',
            text: 'Found 2 relevant docs:\n1. Deployment Guide (Score: 0.92)\n2. API Spec (Score: 0.88)'
          }]
        };
      default:
        return {
          isError: true,
          content: [{ type: 'text', text: `Tool ${request.name} not found.` }]
        };
    }
  }

  private ensureConnection() {
    if (!this.isConnected) {
      throw new Error("MCP Client not connected. Call connect() first.");
    }
  }
}

export const mcpClient = McpClient.getInstance();

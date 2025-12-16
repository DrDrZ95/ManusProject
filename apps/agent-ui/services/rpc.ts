
import { httpClient } from './http';

/**
 * JSON-RPC 2.0 Request Structure
 * JSON-RPC 请求结构
 */
interface JsonRpcRequest {
  jsonrpc: '2.0';
  method: string;
  params: any;
  id: string | number;
}

/**
 * JSON-RPC 2.0 Response Structure
 * JSON-RPC 响应结构
 */
interface JsonRpcResponse<T> {
  jsonrpc: '2.0';
  result?: T;
  error?: {
    code: number;
    message: string;
    data?: any;
  };
  id: string | number;
}

/**
 * System RPC Client
 * 系统级远程过程调用客户端
 * 
 * Design Purpose:
 * Used for "heavy" or "system-level" operations that are distinct from RESTful resources.
 * Examples: Executing shell commands, File system operations, Complex calculations.
 * 
 * 设计目的：
 * 用于“重型”或“系统级”操作，与 RESTful 资源区分开。
 * 例如：执行 Shell 命令、文件系统操作、复杂计算。
 */
class RpcClient {
  private static instance: RpcClient;
  private endpoint = '/system/rpc';

  private constructor() {}

  public static getInstance(): RpcClient {
    if (!RpcClient.instance) {
      RpcClient.instance = new RpcClient();
    }
    return RpcClient.instance;
  }

  /**
   * Execute a remote command
   * 执行远程命令
   */
  public async call<T>(method: string, params: any = {}): Promise<T> {
    const payload: JsonRpcRequest = {
      jsonrpc: '2.0',
      method,
      params,
      id: Date.now() // Simple ID generation
    };

    console.log(`[RPC] Invoking ${method}`, params);

    // Reuse HTTP Client for transport
    // 复用 HTTP 客户端作为传输层
    const response = await httpClient.post<JsonRpcResponse<T>>(this.endpoint, payload, {
      // In a real mock, we would intercept this specific endpoint in http.ts,
      // but for this demo, we simulate the result directly here to save complexity.
    });

    // Mocking the result for the demo since http.ts returns empty object by default
    const mockResult = this.getMockRpcResult<T>(method, params);
    return mockResult;
  }

  private getMockRpcResult<T>(method: string, params: any): T {
    // Simulate processing time
    // 模拟处理时间
    switch (method) {
      case 'sys.exec':
        return `[System] Executing: ${params.cmd}\nResult: Done.` as unknown as T;
      case 'sys.disk_usage':
        return { total: '512GB', used: '120GB', free: '392GB' } as unknown as T;
      default:
        throw new Error(`Method ${method} not found`);
    }
  }
}

export const rpcClient = RpcClient.getInstance();

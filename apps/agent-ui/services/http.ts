
import { tokenManager } from './tokenManager';

/**
 * HTTP Client Configuration
 * HTTP 客户端配置接口
 */
export interface HttpRequestConfig extends RequestInit {
  params?: Record<string, string | number | boolean>; // Query parameters / 查询参数
  requiresAuth?: boolean; // Whether JWT is required / 是否需要鉴权
}

/**
 * Standard API Response Structure
 * 标准 API 响应结构
 */
export interface ApiResponse<T = any> {
  code: number; // 200, 400, 401, 500
  message: string;
  data: T;
  timestamp: number;
}

/**
 * Core HTTP Client (Axios Simulation)
 * 核心 HTTP 客户端 (模拟 Axios)
 * 
 * Design Pattern: Singleton & Adapter
 * Description: 
 * This class wraps the native `fetch` API to provide a unified request interface.
 * It implements Interceptors to handle Token injection (Request) and global error handling (Response).
 * 
 * 设计描述：
 * 该类封装了原生 `fetch` API，提供统一的请求接口。
 * 实现了拦截器模式，用于处理 Token 注入（请求拦截）和全局错误处理（响应拦截）。
 */
class HttpClient {
  private static instance: HttpClient;
  private baseUrl: string = '/api/v1'; // Virtual Base URL / 虚拟根路径

  private constructor() {}

  public static getInstance(): HttpClient {
    if (!HttpClient.instance) {
      HttpClient.instance = new HttpClient();
    }
    return HttpClient.instance;
  }

  /**
   * Main Request Method
   * 主请求方法
   */
  public async request<T>(endpoint: string, config: HttpRequestConfig = {}): Promise<T> {
    const { params, requiresAuth = true, headers, ...customConfig } = config;

    // --- 1. Request Interceptor (请求拦截) ---
    const finalHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
      'X-Client-Version': '1.0.0',
      ...(headers as Record<string, string>),
    };

    // Inject Token / 注入 Token
    if (requiresAuth) {
      const token = tokenManager.getAccessToken();
      if (token) {
        finalHeaders['Authorization'] = `Bearer ${token}`;
      } else {
        console.warn(`[HttpClient] Warning: Request to ${endpoint} missing token.`);
      }
    }

    // Build Query String / 构建查询参数
    let url = `${this.baseUrl}${endpoint}`;
    if (params) {
      const queryString = Object.keys(params)
        .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(String(params[key]))}`)
        .join('&');
      url += `?${queryString}`;
    }

    // --- 2. Network Simulation (网络层模拟) ---
    console.groupCollapsed(`[HTTP] ${config.method || 'GET'} ${url}`);
    console.log('Headers:', finalHeaders);
    console.log('Params:', params);
    if (config.body) console.log('Body:', JSON.parse(config.body as string));
    console.groupEnd();

    // Simulate Network Latency / 模拟网络延迟 (300ms - 800ms)
    await new Promise(resolve => setTimeout(resolve, 300 + Math.random() * 500));

    // --- 3. Response Interceptor (响应拦截) ---
    try {
      // In a real app, this would be: const response = await fetch(url, { ... });
      // Here we delegate to the Mock Handler.
      // 真实应用中调用 fetch，此处委托给 Mock 处理器。
      const mockResponse = await this.mockBackendHandler<T>(endpoint, config);
      
      return mockResponse.data; // Return pure data / 返回纯数据
    } catch (error: any) {
      this.handleError(error);
      throw error;
    }
  }

  // --- Convenience Methods / 便捷方法 ---

  public get<T>(url: string, config?: HttpRequestConfig) {
    return this.request<T>(url, { ...config, method: 'GET' });
  }

  public post<T>(url: string, data?: any, config?: HttpRequestConfig) {
    return this.request<T>(url, { ...config, method: 'POST', body: JSON.stringify(data) });
  }

  public put<T>(url: string, data?: any, config?: HttpRequestConfig) {
    return this.request<T>(url, { ...config, method: 'PUT', body: JSON.stringify(data) });
  }

  public delete<T>(url: string, config?: HttpRequestConfig) {
    return this.request<T>(url, { ...config, method: 'DELETE' });
  }

  // --- Internals ---

  private handleError(error: any) {
    console.error('[HttpClient] Error:', error);
    if (error.message === '401 Unauthorized') {
      // Trigger global logout or refresh token logic
      // 触发全局登出或 Token 刷新逻辑
      console.warn('Redirecting to login due to 401...');
      // window.location.href = '/login'; 
    }
  }

  /**
   * Mock Backend Router
   * 模拟后端路由分发器
   */
  private async mockBackendHandler<T>(endpoint: string, config: HttpRequestConfig): Promise<ApiResponse<T>> {
    // Simulate Auth Check
    if (config.requiresAuth !== false && !tokenManager.hasToken()) {
      throw new Error('401 Unauthorized');
    }

    // This would be replaced by actual server responses
    return {
      code: 200,
      message: 'Success',
      timestamp: Date.now(),
      data: {} as T // The actual data is mocked in the specific services (api.ts)
    };
  }
}

export const httpClient = HttpClient.getInstance();

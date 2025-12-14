
import { LoginRequest, User, AuthResponse } from '../types';
import { tokenManager } from './tokenManager';

/**
 * Generic API Response Interface
 * 通用 API 响应接口
 */
interface ApiResponse<T> {
  code: number;
  message: string;
  data: T;
}

/**
 * API Client Configuration
 * API 客户端配置
 */
interface RequestConfig extends RequestInit {
  requiresAuth?: boolean; // Default true
}

/**
 * API Service Class
 * API 服务类
 * 
 * Design Pattern: Singleton & Facade
 * Features:
 * 1. Automatic JWT injection (Interceptor simulation).
 * 2. Centralized error handling.
 * 3. Mock data handling for demonstration.
 * 
 * 特性：
 * 1. 自动注入 JWT (模拟拦截器)。
 * 2. 集中式错误处理。
 * 3. 用于演示的 Mock 数据处理。
 */
class ApiClient {
  private static instance: ApiClient;
  private baseUrl: string = '/api/v1'; // Virtual base URL

  private constructor() {}

  public static getInstance(): ApiClient {
    if (!ApiClient.instance) {
      ApiClient.instance = new ApiClient();
    }
    return ApiClient.instance;
  }

  /**
   * Core Request Method (Simulates Fetch/Axios)
   * 核心请求方法 (模拟 Fetch/Axios)
   */
  private async request<T>(endpoint: string, config: RequestConfig = {}, mockData?: T): Promise<T> {
    const { requiresAuth = true, headers, ...rest } = config;

    // 1. Request Interceptor: Inject Token
    // 请求拦截器：注入 Token
    const authHeaders: Record<string, string> = {};
    if (requiresAuth) {
      const token = tokenManager.getAccessToken();
      if (token) {
        authHeaders['Authorization'] = `Bearer ${token}`;
      } else {
        console.warn(`[ApiClient] Request to ${endpoint} requires auth but no token found.`);
        // In a real app, you might throw unauthorized error here immediately
      }
    }

    // Combine headers
    const finalHeaders = {
      'Content-Type': 'application/json',
      ...authHeaders,
      ...headers,
    };

    console.log(`[ApiClient] Requesting: ${config.method || 'GET'} ${this.baseUrl}${endpoint}`, {
      headers: finalHeaders,
      body: rest.body ? JSON.parse(rest.body as string) : undefined
    });

    // 2. Simulate Network Latency
    // 模拟网络延迟
    await new Promise(resolve => setTimeout(resolve, 600));

    // 3. Response Interceptor: Error Handling & Mocking
    // 响应拦截器：错误处理与 Mock
    
    // Simulate 401 if requires auth but no token (Demo logic)
    if (requiresAuth && !tokenManager.getAccessToken()) {
      throw new Error('401 Unauthorized');
    }

    // Return mock data if provided
    if (mockData) {
      console.log(`[ApiClient] Response from ${endpoint}:`, mockData);
      return mockData;
    }

    throw new Error(`Endpoint ${endpoint} not mocked.`);
  }

  // --- Auth Endpoints ---

  public async login(credentials: LoginRequest): Promise<User> {
    // Mock Validation
    if (credentials.email?.includes('error')) {
      throw new Error('Invalid credentials simulation');
    }

    const mockResponse: AuthResponse = {
      user: {
        id: 'user-123',
        name: 'Agent User',
        email: credentials.email || 'user@example.com',
        avatar: 'https://api.dicebear.com/9.x/micah/svg?seed=Felix',
        role: 'admin',
        bio: 'Full Stack Engineer @ Agent Corp'
      },
      token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock-token-payload',
      refreshToken: 'mock-refresh-token',
      expiresIn: 3600
    };

    const data = await this.request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
      requiresAuth: false
    }, mockResponse);

    // Side Effect: Save Tokens
    tokenManager.setTokens(data.token, data.refreshToken);

    return data.user;
  }

  public async logout(): Promise<void> {
    await this.request<void>('/auth/logout', { method: 'POST' }, undefined);
    tokenManager.clearTokens();
  }

  public async getProfile(): Promise<User> {
    const mockUser = {
      id: 'user-123',
      name: 'Agent User',
      email: 'user@example.com',
      avatar: 'https://api.dicebear.com/9.x/micah/svg?seed=Felix',
      role: 'admin'
    };
    return this.request<User>('/users/me', { method: 'GET' }, mockUser);
  }
}

export const authApi = ApiClient.getInstance();

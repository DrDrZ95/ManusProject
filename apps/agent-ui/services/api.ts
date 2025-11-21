
import { LoginRequest, User } from '../types';

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
 * Base HTTP Request Method
 * 基础 HTTP 请求方法
 * 
 * @param endpoint API Endpoint / API 端点
 * @param options Fetch Options / Fetch 配置项
 * @returns Promise<T>
 */
async function request<T>(endpoint: string, options: RequestInit = {}, mockData?: T): Promise<T> {
  // Simulate network delay
  // 模拟网络延迟
  await new Promise(resolve => setTimeout(resolve, 800));

  console.log(`[API] Request to ${endpoint}`, options);

  // Return mock data if provided, otherwise return a safe empty object cast as T
  // 如果提供了模拟数据则返回，否则返回类型断言的安全空对象
  return mockData as T;
}

export const authApi = {
  /**
   * User Login Interface
   * 用户登录接口
   * 
   * Method: POST
   * Path: /auth/login
   * 
   * @param credentials LoginRequest { email, password }
   * @returns Promise<User>
   */
  login: async (credentials: LoginRequest): Promise<User> => {
    // Simulate validation
    if (credentials.email && credentials.email.includes('error')) {
      throw new Error('Invalid credentials');
    }

    const mockUser: User = {
      id: 'user-123',
      name: 'Agent User',
      email: credentials.email || 'user@example.com',
      avatar: 'https://ui-avatars.com/api/?name=Agent+User',
      role: 'admin'
    };

    return request<User>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials)
    }, mockUser);
  },

  /**
   * User Logout Interface
   * 用户登出接口
   * 
   * Method: POST
   * Path: /auth/logout
   */
  logout: async (): Promise<void> => {
    return request<void>('/auth/logout', {
      method: 'POST'
    }, undefined);
  },

  /**
   * Get Current User Profile
   * 获取当前用户信息
   * 
   * Method: GET
   * Path: /users/me
   */
  getProfile: async (): Promise<User> => {
    const mockProfile: User = {
      id: 'user-123',
      name: 'Agent User',
      email: 'user@example.com',
      avatar: 'https://ui-avatars.com/api/?name=Agent+User',
      role: 'admin'
    };

    return request<User>('/users/me', {
      method: 'GET'
    }, mockProfile);
  }
};

export const chatApi = {
  /**
   * Create New Conversation
   * 创建新对话
   * 
   * Method: POST
   * Path: /chats
   */
  createSession: async (title: string) => {
    return request('/chats', {
      method: 'POST',
      body: JSON.stringify({ title })
    }, { id: 'new-session-id', title });
  },

  /**
   * Get Conversation History
   * 获取对话历史
   * 
   * Method: GET
   * Path: /chats/{id}/messages
   */
  getMessages: async (sessionId: string) => {
    return request(`/chats/${sessionId}/messages`, {}, []);
  }
};

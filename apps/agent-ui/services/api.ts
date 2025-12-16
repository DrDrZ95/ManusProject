
import { httpClient } from './http';
import { tokenManager } from './tokenManager';
import { securityService } from './security';
import { LoginRequest, User, AuthResponse, ChatSession, NewsItem, Attachment } from '../types';
import { v4 as uuidv4 } from 'uuid';
import { generateNews } from './news'; 
import { MOCK_SESSIONS } from '../data/mockData';

// =============================================================================
// Auth Service (认证服务)
// Handles login, logout, and token refresh.
// 处理登录、登出和 Token 刷新。
// =============================================================================
export const authService = {
  /**
   * User Login
   * 用户登录
   * 
   * Process:
   * 1. Encrypt password using RSA Public Key (via SecurityService).
   * 2. Send encrypted credentials to backend.
   * 3. Store returned JWT tokens.
   * 
   * 流程：
   * 1. 使用 RSA 公钥加密密码（通过 SecurityService）。
   * 2. 发送加密凭证至后端。
   * 3. 存储返回的 JWT 令牌。
   * 
   * @param data Login credentials
   */
  login: async (data: LoginRequest): Promise<User> => {
    console.group('[AuthService] Login Process');
    
    // 1. Encryption Step (RSA)
    // 加密步骤
    if (data.password && data.provider === 'credentials') {
       console.log('🔒 Encrypting sensitive credentials...');
       const encryptedPassword = securityService.encrypt(data.password);
       // Replace plain password with encrypted one for the payload
       data = { ...data, password: encryptedPassword };
       console.log('📦 Payload prepared with encrypted data.');
    }

    // Mock Response
    const mockUser: User = {
      id: 'user-123',
      name: 'Agent User',
      email: data.email || 'user@example.com',
      avatar: 'https://api.dicebear.com/9.x/micah/svg?seed=Felix',
      role: 'admin',
      bio: 'Full Stack Engineer @ Agent Corp'
    };

    const mockAuthResponse: AuthResponse = {
      user: mockUser,
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      expiresIn: 3600
    };

    // 2. Network Request
    // 网络请求 (Simulated latency in httpClient)
    console.log('📡 Sending request to /auth/login...');
    await httpClient.post('/auth/login', data, { requiresAuth: false });
    
    // 3. Token Storage
    // 令牌存储
    tokenManager.setTokens(mockAuthResponse.token, mockAuthResponse.refreshToken);
    
    console.log('✅ Login successful. Session established.');
    console.groupEnd();
    
    return mockUser;
  },

  /**
   * User Logout
   * 用户登出
   */
  logout: async (): Promise<void> => {
    await httpClient.post('/auth/logout');
    tokenManager.clearTokens();
  },

  /**
   * Get Current User Profile
   * 获取当前用户信息
   */
  getProfile: async (): Promise<User> => {
    const mockUser: User = {
      id: 'user-123',
      name: 'Agent User',
      email: 'user@example.com',
      avatar: 'https://api.dicebear.com/9.x/micah/svg?seed=Felix',
      role: 'admin'
    };
    await httpClient.get('/users/me');
    return mockUser;
  }
};

// =============================================================================
// Chat Service (聊天服务)
// Manages chat sessions, history, and group organization.
// 管理聊天会话、历史记录和分组。
// =============================================================================
export const chatService = {
  /**
   * Fetch all chat sessions
   * 获取所有会话列表
   */
  getSessions: async (): Promise<ChatSession[]> => {
    // In a real app, this returns the list from DB
    await httpClient.get('/chats'); 
    return MOCK_SESSIONS;
  },

  /**
   * Create a new session
   * 创建新会话
   */
  createSession: async (title: string): Promise<ChatSession> => {
    const newSession: ChatSession = {
      id: uuidv4(),
      title,
      messages: [],
      updatedAt: Date.now()
    };
    await httpClient.post('/chats', { title });
    return newSession;
  },

  /**
   * Delete a session
   * 删除会话
   */
  deleteSession: async (id: string): Promise<void> => {
    await httpClient.delete(`/chats/${id}`);
  }
};

// =============================================================================
// File Service (文件服务)
// Handles file uploads, downloads, and "My Space" management.
// 处理文件上传、下载和“我的空间”管理。
// =============================================================================
export const fileService = {
  /**
   * Upload an attachment
   * 上传附件
   */
  upload: async (file: File): Promise<Attachment> => {
    const formData = new FormData();
    formData.append('file', file);
    
    // Simulate upload delay
    await httpClient.post('/storage/upload', formData, { 
      headers: { 'Content-Type': 'multipart/form-data' } // Browser sets boundary automatically
    });

    // Mock return
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = () => {
        const base64String = reader.result as string;
        resolve({
          id: uuidv4(),
          name: file.name,
          type: 'file',
          mimeType: file.type,
          data: base64String.split(',')[1]
        });
      };
      reader.readAsDataURL(file);
    });
  },

  /**
   * List files in My Space
   * 列出我的空间文件
   */
  listFiles: async (): Promise<any[]> => {
    await httpClient.get('/storage/files');
    return []; // Mock data handled in UI component for now
  }
};

// =============================================================================
// News Service (新闻服务)
// Fetches external news data.
// 获取外部新闻数据。
// =============================================================================
export const newsService = {
  /**
   * Fetch latest news
   * 获取最新新闻
   */
  fetchLatest: async (): Promise<NewsItem[]> => {
    await httpClient.get('/news/latest');
    // Using the deterministic generator from previous implementation
    return generateNews();
  }
};

// Export a unified API object (Facade)
// 导出统一 API 对象 (外观模式)
export const api = {
  auth: authService,
  chat: chatService,
  file: fileService,
  news: newsService
};

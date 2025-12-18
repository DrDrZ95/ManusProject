
import { httpClient } from './http';
import { tokenManager } from './tokenManager';
import { securityService } from './security';
import { LoginRequest, User, AuthResponse, ChatSession, NewsItem, Attachment } from '../types';
import { v4 as uuidv4 } from 'uuid';
import { generateNews } from './news'; 
import { MOCK_SESSIONS } from '../data/mockData';

export const authService = {
  login: async (data: LoginRequest): Promise<User> => {
    console.group('[AuthService] Login Process');
    
    if (data.password && data.provider === 'credentials') {
       console.log('🔒 Encrypting sensitive credentials...');
       const encryptedPassword = securityService.encrypt(data.password);
       data = { ...data, password: encryptedPassword };
       console.log('📦 Payload prepared with encrypted data.');
    }

    const mockUser: User = {
      id: 'user-123',
      name: 'Agent User',
      email: data.email || 'user@example.com',
      avatar: 'https://api.dicebear.com/9.x/lorelei/svg?seed=Jasper',
      role: 'admin',
      bio: 'Full Stack Engineer @ Agent Corp'
    };

    const mockAuthResponse: AuthResponse = {
      user: mockUser,
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      expiresIn: 3600
    };

    console.log('📡 Sending request to /auth/login...');
    await httpClient.post('/auth/login', data, { requiresAuth: false });
    
    tokenManager.setTokens(mockAuthResponse.token, mockAuthResponse.refreshToken);
    
    console.log('✅ Login successful. Session established.');
    console.groupEnd();
    
    return mockUser;
  },

  logout: async (): Promise<void> => {
    await httpClient.post('/auth/logout');
    tokenManager.clearTokens();
  },

  getProfile: async (): Promise<User> => {
    const mockUser: User = {
      id: 'user-123',
      name: 'Agent User',
      email: 'user@example.com',
      avatar: 'https://api.dicebear.com/9.x/lorelei/svg?seed=Jasper',
      role: 'admin'
    };
    await httpClient.get('/users/me');
    return mockUser;
  }
};

export const chatService = {
  getSessions: async (): Promise<ChatSession[]> => {
    await httpClient.get('/chats'); 
    return MOCK_SESSIONS;
  },

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

  deleteSession: async (id: string): Promise<void> => {
    await httpClient.delete(`/chats/${id}`);
  }
};

export const fileService = {
  upload: async (file: File): Promise<Attachment> => {
    const formData = new FormData();
    formData.append('file', file);
    
    await httpClient.post('/storage/upload', formData, { 
      headers: { 'Content-Type': 'multipart/form-data' } 
    });

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

  listFiles: async (): Promise<any[]> => {
    await httpClient.get('/storage/files');
    return []; 
  }
};

export const newsService = {
  fetchLatest: async (): Promise<NewsItem[]> => {
    await httpClient.get('/news/latest');
    return generateNews();
  }
};

export const api = {
  auth: authService,
  chat: chatService,
  file: fileService,
  news: newsService
};

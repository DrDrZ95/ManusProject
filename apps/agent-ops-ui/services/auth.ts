/**
 * Auth Service - 身份认证服务
 * 
 * 采用单例模式 (Singleton Pattern) 确保应用中只有一个 Auth 实例管理用户状态。
 * 负责 JWT Token 的存储、检索、过期检查以及模拟登录/登出流程。
 */

export interface UserProfile {
  id: string;
  username: string;
  role: 'admin' | 'editor' | 'viewer';
  avatar?: string;
}

interface AuthResponse {
  token: string;
  refreshToken: string;
  user: UserProfile;
  expiresIn: number; // seconds
}

class AuthService {
  private static instance: AuthService;
  private tokenKey = 'opsnexus_access_token';
  private userKey = 'opsnexus_user_profile';

  private constructor() {}

  // 获取单例实例
  public static getInstance(): AuthService {
    if (!AuthService.instance) {
      AuthService.instance = new AuthService();
    }
    return AuthService.instance;
  }

  /**
   * 模拟登录接口
   * 实际项目中这里会发送 POST 请求到后端 /api/auth/login
   */
  public async login(username: string, password: string): Promise<UserProfile> {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        // 模拟简单的验证逻辑
        if (username === 'admin' && password === 'password') {
          const mockResponse: AuthResponse = {
            token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock.signature',
            refreshToken: 'mock_refresh_token_12345',
            user: {
              id: 'u_001',
              username: 'Admin User',
              role: 'admin',
              avatar: 'https://github.com/shadcn.png'
            },
            expiresIn: 3600
          };
          
          this.setSession(mockResponse);
          resolve(mockResponse.user);
        } else {
          reject(new Error('Invalid credentials'));
        }
      }, 800); // 模拟网络延迟
    });
  }

  /**
   * 登出逻辑
   * 清除本地存储的 Token 和用户信息
   */
  public logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    // 可以在这里触发全局事件通知 UI 更新
    window.location.reload();
  }

  /**
   * 保存会话信息
   */
  private setSession(authResult: AuthResponse): void {
    localStorage.setItem(this.tokenKey, authResult.token);
    localStorage.setItem(this.userKey, JSON.stringify(authResult.user));
    // 实际项目中可以设置自动刷新 Token 的定时器
  }

  /**
   * 获取当前 Access Token
   * 用于 API 请求拦截器注入 Header
   */
  public getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /**
   * 获取当前用户信息
   */
  public getUser(): UserProfile | null {
    const userStr = localStorage.getItem(this.userKey);
    return userStr ? JSON.parse(userStr) : null;
  }

  /**
   * 检查是否已认证
   */
  public isAuthenticated(): boolean {
    const token = this.getToken();
    // 实际项目中应解析 JWT 检查 exp (过期时间)
    return !!token;
  }
}

export const authService = AuthService.getInstance();
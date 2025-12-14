
/**
 * Token Manager Service
 * JWT 令牌管理服务
 * 
 * Design Pattern: Singleton (单例模式)
 * Purpose: Centralizes the storage, retrieval, and clearing of auth tokens.
 * 目的：集中管理认证令牌的存储、获取和清除，防止本地存储逻辑分散。
 */

const ACCESS_TOKEN_KEY = 'agent_access_token';
const REFRESH_TOKEN_KEY = 'agent_refresh_token';

class TokenManager {
  private static instance: TokenManager;

  private constructor() {}

  /**
   * Get the singleton instance
   * 获取单例实例
   */
  public static getInstance(): TokenManager {
    if (!TokenManager.instance) {
      TokenManager.instance = new TokenManager();
    }
    return TokenManager.instance;
  }

  /**
   * Save tokens to storage
   * 保存令牌
   */
  public setTokens(accessToken: string, refreshToken?: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    if (refreshToken) {
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    }
    console.log('[TokenManager] Tokens saved successfully.');
  }

  /**
   * Get Access Token
   * 获取访问令牌
   */
  public getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  /**
   * Get Refresh Token
   * 获取刷新令牌
   */
  public getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  /**
   * Clear all tokens (Logout)
   * 清除所有令牌 (退出登录)
   */
  public clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    console.log('[TokenManager] Tokens cleared.');
  }

  /**
   * Check if user is logged in (Naive check)
   * 检查是否登录 (简单检查)
   */
  public hasToken(): boolean {
    return !!this.getAccessToken();
  }
}

export const tokenManager = TokenManager.getInstance();

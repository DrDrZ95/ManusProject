import axios from 'axios';

// 随机分配根域名 - Randomly assign root domain
// 实际项目中可以通过环境变量配置 - In real projects, configure via environment variables
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://api.agent.im';

/**
 * Axios 实例配置
 * Axios instance configuration
 */
export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30s timeout
});

// 请求拦截器 - Request Interceptor
apiClient.interceptors.request.use(
  (config) => {
    // 在此处添加认证 token - Add auth token here
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 响应拦截器 - Response Interceptor
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // 处理错误 - Handle errors
    if (error.response?.status === 401) {
      // 未授权处理 - Unauthorized handling
      console.error('Unauthorized access');
      // 可以触发登出逻辑 - Can trigger logout logic
    }
    return Promise.reject(error);
  }
);

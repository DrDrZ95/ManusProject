import { apiClient } from '../client';
import { UserInputRequest, ApiResponse } from '../types';

/**
 * 用户输入 (User Input) 相关 API
 * User Input related APIs
 */

/**
 * 发送用户输入
 * Send user input
 * @param request 输入请求
 * @returns 处理结果
 */
export const sendUserInput = async (request: UserInputRequest): Promise<ApiResponse<string>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/user/input', request);
  console.log('API Call: sendUserInput', request);
  return { data: 'Input processed', status: 200 };
};

/**
 * 获取用户会话历史
 * Get user session history
 * @param sessionId 会话 ID
 * @returns 历史记录
 */
export const getUserSessionHistory = async (sessionId: string): Promise<ApiResponse<any[]>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.get(`/user/sessions/${sessionId}/history`);
  console.log('API Call: getUserSessionHistory', sessionId);
  return { 
    data: [
      { role: 'user', content: 'Hello' },
      { role: 'assistant', content: 'Hi there!' }
    ], 
    status: 200 
  };
};

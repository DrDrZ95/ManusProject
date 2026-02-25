import { apiClient } from '../client';
import { ExecuteCommandRequest, CommandResult, ApiResponse } from '../types';

/**
 * 沙箱终端 (Sandbox Terminal) 相关 API
 * Sandbox Terminal related APIs
 */

/**
 * 在沙箱中执行命令
 * Execute command in sandbox
 * @param request 命令请求
 * @returns 执行结果
 */
export const executeSandboxCommand = async (request: ExecuteCommandRequest): Promise<ApiResponse<CommandResult>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/sandbox/execute', request);
  console.log('API Call: executeSandboxCommand', request);
  return { 
    data: { 
      stdout: 'Command executed successfully\nOutput line 1\nOutput line 2', 
      stderr: '', 
      exitCode: 0 
    }, 
    status: 200 
  };
};

/**
 * 获取沙箱状态
 * Get sandbox status
 * @returns 沙箱状态信息
 */
export const getSandboxStatus = async (): Promise<ApiResponse<any>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.get('/sandbox/status');
  console.log('API Call: getSandboxStatus');
  return { 
    data: { status: 'active', uptime: 3600, memory: '512MB' }, 
    status: 200 
  };
};

/**
 * 重置沙箱环境
 * Reset sandbox environment
 * @returns 重置结果
 */
export const resetSandbox = async (): Promise<ApiResponse<void>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/sandbox/reset');
  console.log('API Call: resetSandbox');
  return { data: undefined, status: 200, message: 'Sandbox reset successfully' };
};

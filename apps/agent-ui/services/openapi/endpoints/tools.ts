import { apiClient } from '../client';
import { ToolMetadata, ApiResponse } from '../types';

/**
 * 工具 (Tools) 相关 API
 * Tools related APIs
 */

/**
 * 获取可用工具列表
 * Get available tools list
 * @returns 工具列表
 */
export const getToolsList = async (): Promise<ApiResponse<ToolMetadata[]>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.get('/tools');
  console.log('API Call: getToolsList');
  return { 
    data: [
      { name: 'Calculator', description: 'Basic calculator', version: '1.0.0', parameters: { expression: 'string' } },
      { name: 'Weather', description: 'Get weather info', version: '1.0.0', parameters: { location: 'string' } }
    ], 
    status: 200 
  };
};

/**
 * 调用工具
 * Invoke tool
 * @param toolName 工具名称
 * @param parameters 参数
 * @returns 执行结果
 */
export const invokeTool = async (toolName: string, parameters: Record<string, any>): Promise<ApiResponse<any>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post(`/tools/${toolName}/invoke`, parameters);
  console.log('API Call: invokeTool', toolName, parameters);
  return { 
    data: { result: 'Tool execution result' }, 
    status: 200 
  };
};

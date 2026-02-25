import { apiClient } from '../client';
import { PromptTemplate, ApiResponse } from '../types';

/**
 * 提示词 (Prompts) 相关 API
 * Prompts related APIs
 */

/**
 * 获取提示词模板列表
 * Get prompt template list
 * @returns 模板列表
 */
export const getPromptTemplates = async (): Promise<ApiResponse<PromptTemplate[]>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.get('/prompts/templates');
  console.log('API Call: getPromptTemplates');
  return { 
    data: [
      { name: 'Summarize', category: 'Text', template: 'Summarize the following text: {{text}}', variables: ['text'] },
      { name: 'Translate', category: 'Text', template: 'Translate to {{language}}: {{text}}', variables: ['language', 'text'] }
    ], 
    status: 200 
  };
};

/**
 * 创建新的提示词模板
 * Create new prompt template
 * @param template 模板数据
 * @returns 创建结果
 */
export const createPromptTemplate = async (template: PromptTemplate): Promise<ApiResponse<string>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/prompts/templates', template);
  console.log('API Call: createPromptTemplate', template);
  return { data: 'template_id_789', status: 201, message: 'Template created' };
};

/**
 * 更新提示词模板
 * Update prompt template
 * @param name 模板名称
 * @param template 新的模板数据
 * @returns 更新结果
 */
export const updatePromptTemplate = async (name: string, template: Partial<PromptTemplate>): Promise<ApiResponse<void>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.put(`/prompts/templates/${name}`, template);
  console.log('API Call: updatePromptTemplate', name, template);
  return { data: undefined, status: 200, message: 'Template updated' };
};

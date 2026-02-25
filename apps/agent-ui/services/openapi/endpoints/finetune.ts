import { apiClient } from '../client';
import { FinetuneRequest, FinetuneJobStatus, ApiResponse } from '../types';

/**
 * 微调 (Fine-tuning) 相关 API
 * Fine-tuning related APIs
 */

/**
 * 启动微调任务
 * Start fine-tuning job
 * @param request 微调请求参数
 * @returns 任务 ID
 */
export const startFinetuneJob = async (request: FinetuneRequest): Promise<ApiResponse<string>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post('/finetune/start', request);
  console.log('API Call: startFinetuneJob', request);
  return { data: 'job_id_456', status: 200, message: 'Fine-tuning job started' };
};

/**
 * 获取微调任务状态
 * Get fine-tuning job status
 * @param jobId 任务 ID
 * @returns 任务状态
 */
export const getFinetuneJobStatus = async (jobId: string): Promise<ApiResponse<FinetuneJobStatus>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.get(`/finetune/status/${jobId}`);
  console.log('API Call: getFinetuneJobStatus', jobId);
  return { 
    data: { 
      jobId, 
      status: 'running', 
      progress: 45,
      logs: ['Epoch 1 completed', 'Epoch 2 running...'] 
    }, 
    status: 200 
  };
};

/**
 * 取消微调任务
 * Cancel fine-tuning job
 * @param jobId 任务 ID
 * @returns 取消结果
 */
export const cancelFinetuneJob = async (jobId: string): Promise<ApiResponse<void>> => {
  // 模拟 API 调用 - Simulate API call
  // return apiClient.post(`/finetune/cancel/${jobId}`);
  console.log('API Call: cancelFinetuneJob', jobId);
  return { data: undefined, status: 200, message: 'Job cancelled' };
};

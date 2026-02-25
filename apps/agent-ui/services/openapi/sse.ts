/**
 * SSE (Server-Sent Events) 处理工具
 * SSE Handling Utility
 */

export type SSECallback = (data: string) => void;
export type SSEErrorCallback = (error: Event) => void;
export type SSECompleteCallback = () => void;

interface SSEOptions {
  onMessage: SSECallback;
  onError?: SSEErrorCallback;
  onComplete?: SSECompleteCallback;
  headers?: Record<string, string>;
}

/**
 * 处理 SSE 连接
 * Handle SSE Connection
 * @param url 请求 URL
 * @param options 配置选项
 * @returns 关闭连接的函数
 */
export const connectSSE = (url: string, options: SSEOptions) => {
  const { onMessage, onError, onComplete, headers } = options;
  
  // 如果需要自定义 headers，可能需要使用 fetch + ReadableStream 而不是 EventSource
  // 这里为了简单演示，使用 EventSource，但在实际项目中，如果需要 Auth Header，通常使用 fetch
  // If custom headers are needed, fetch + ReadableStream might be required instead of EventSource
  // Using EventSource for simplicity here, but in real projects with Auth Header, fetch is usually used
  
  // 使用 fetch 实现带 header 的 SSE
  const controller = new AbortController();
  
  fetch(url, {
    method: 'POST', // 或 GET，取决于 API 定义 - Or GET, depending on API definition
    headers: {
      'Content-Type': 'application/json',
      ...headers,
    },
    signal: controller.signal,
  }).then(async (response) => {
    if (!response.ok) {
      throw new Error(`SSE Error: ${response.statusText}`);
    }
    
    const reader = response.body?.getReader();
    if (!reader) return;
    
    const decoder = new TextDecoder();
    
    while (true) {
      const { done, value } = await reader.read();
      if (done) {
        if (onComplete) onComplete();
        break;
      }
      
      const chunk = decoder.decode(value, { stream: true });
      // 解析 SSE 格式数据 - Parse SSE format data
      // 简单实现，实际可能需要更复杂的解析器处理多行数据
      // Simple implementation, real world might need complex parser for multi-line data
      const lines = chunk.split('\n');
      for (const line of lines) {
        if (line.startsWith('data: ')) {
          const data = line.slice(6);
          if (data === '[DONE]') {
            if (onComplete) onComplete();
            return;
          }
          try {
            onMessage(data);
          } catch (e) {
            console.error('Error parsing SSE data:', e);
          }
        }
      }
    }
  }).catch((err) => {
    if (onError) onError(err);
  });

  return () => controller.abort();
};

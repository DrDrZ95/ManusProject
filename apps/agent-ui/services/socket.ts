
/**
 * WebSocket Service
 * WebSocket 服务类
 * 
 * Handles real-time communication for terminal and chat events.
 * 处理终端和聊天事件的实时通信。
 */
class WebSocketService {
  private static instance: WebSocketService;
  private socket: WebSocket | null = null;
  private url: string = 'ws://localhost:8080/ws';
  private listeners: Map<string, ((data: any) => void)[]> = new Map();
  private connected: boolean = false;

  private constructor() {}

  /**
   * Get Singleton Instance
   * 获取单例实例
   */
  public static getInstance(): WebSocketService {
    if (!WebSocketService.instance) {
      WebSocketService.instance = new WebSocketService();
    }
    return WebSocketService.instance;
  }

  /**
   * Connect to WebSocket Server
   * 连接到 WebSocket 服务器
   * 
   * @param url Optional URL override / 可选的 URL 覆盖
   */
  public connect(url?: string): void {
    if (url) this.url = url;
    
    if (this.connected) {
      console.warn('[WS] Already connected');
      return;
    }

    console.log(`[WS] Connecting to ${this.url}...`);
    
    // Simulation of connection for demo purposes
    // 演示目的的连接模拟
    setTimeout(() => {
      this.connected = true;
      this.onOpen(new Event('open'));
    }, 500);

    // Real implementation would be:
    // 真实实现如下:
    // this.socket = new WebSocket(this.url);
    // this.socket.onopen = this.onOpen.bind(this);
    // this.socket.onmessage = this.onMessage.bind(this);
    // this.socket.onclose = this.onClose.bind(this);
    // this.socket.onerror = this.onError.bind(this);
  }

  /**
   * Disconnect from WebSocket Server
   * 断开 WebSocket 连接
   */
  public disconnect(): void {
    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
    this.connected = false;
    console.log('[WS] Disconnected manually');
  }

  /**
   * Send Message
   * 发送消息
   * 
   * @param event Event Name / 事件名称
   * @param payload Data Payload / 数据载荷
   */
  public send(event: string, payload: any): void {
    if (this.connected) {
      console.log(`[WS] Sending: ${event}`, payload);
      // if (this.socket) this.socket.send(JSON.stringify({ event, payload }));
    } else {
      console.warn('[WS] Socket not connected, message queued or ignored:', event);
    }
  }

  /**
   * Subscribe to Event
   * 订阅事件
   * 
   * @param event Event Name / 事件名称
   * @param callback Callback Function / 回调函数
   */
  public on(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)?.push(callback);
  }

  /**
   * Unsubscribe from Event
   * 取消订阅事件
   * 
   * @param event Event Name / 事件名称
   * @param callback Callback Function / 回调函数
   */
  public off(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) return;
    const filtered = this.listeners.get(event)?.filter(cb => cb !== callback) || [];
    this.listeners.set(event, filtered);
  }

  /**
   * Check Connection Status
   * 检查连接状态
   */
  public isConnected(): boolean {
    return this.connected;
  }

  // Private Handlers

  private onOpen(event: Event) {
    console.log('[WS] Connection Established');
  }

  private onMessage(event: MessageEvent) {
    try {
      const data = JSON.parse(event.data);
      const { event: eventName, payload } = data;
      
      if (this.listeners.has(eventName)) {
        this.listeners.get(eventName)?.forEach(cb => cb(payload));
      }
    } catch (e) {
      console.error('[WS] Failed to parse message', e);
    }
  }

  private onClose(event: CloseEvent) {
    console.log('[WS] Disconnected', event.code);
    this.connected = false;
  }

  private onError(event: Event) {
    console.error('[WS] Error', event);
  }
}

export const socketService = WebSocketService.getInstance();

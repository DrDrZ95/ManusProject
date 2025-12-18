
/**
 * WebSocket Event Types
 * WebSocket 事件类型定义
 */
export type WSEvent = 'open' | 'close' | 'error' | 'message' | 'notification' | 'typing';

type WSEventHandler<T = any> = (payload: T) => void;

/**
 * Enterprise WebSocket Service
 * 企业级 WebSocket 服务封装
 * 
 * Features:
 * 1. Automatic Reconnection (Exponential Backoff) / 自动重连（指数退避）
 * 2. Heartbeat (Ping/Pong) / 心跳保活
 * 3. Type-safe Event Subscription / 类型安全的事件订阅
 */
class WebSocketService {
  private static instance: WebSocketService;
  private url: string = 'ws://localhost:8080/ws'; // Mock URL
  
  // Connection State
  private isConnected: boolean = false;
  private reconnectAttempts: number = 0;
  private readonly MAX_RECONNECT_ATTEMPTS = 5;
  private reconnectTimer: any = null;
  
  // Heartbeat
  private heartbeatInterval: any = null;
  private readonly HEARTBEAT_RATE = 30000; // 30s

  // Event Listeners
  private listeners: Map<string, WSEventHandler[]> = new Map();

  private constructor() {}

  public static getInstance(): WebSocketService {
    if (!WebSocketService.instance) {
      WebSocketService.instance = new WebSocketService();
    }
    return WebSocketService.instance;
  }

  /**
   * Connect to WebSocket Server
   * 连接服务器
   */
  public connect(url?: string): void {
    if (this.isConnected) return;
    if (url) this.url = url;

    console.log(`[WS] Connecting to ${this.url}...`);
    this.mockConnectionProcess();
  }

  /**
   * Disconnect
   * 断开连接
   */
  public disconnect(): void {
    if (this.reconnectTimer) clearTimeout(this.reconnectTimer);
    this.stopHeartbeat();
    this.isConnected = false;
    this.dispatch('close', {});
    console.log('[WS] Disconnected.');
  }

  /**
   * Send Message
   * 发送消息
   */
  public send(event: string, payload: any): void {
    if (!this.isConnected) {
      console.warn('[WS] Send failed: Disconnected.');
      return;
    }
    console.log(`[WS] >>> ${event}`, payload);
    // this.socket.send(JSON.stringify({ event, payload }));
  }

  /**
   * Subscribe to Event
   * 订阅事件
   */
  public on<T = any>(event: WSEvent, callback: WSEventHandler<T>): void {
    if (!this.listeners.get(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)?.push(callback);
  }

  /**
   * Unsubscribe
   * 取消订阅
   */
  public off<T = any>(event: WSEvent, callback: WSEventHandler<T>): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      this.listeners.set(event, callbacks.filter(cb => cb !== callback));
    }
  }

  // --- Internals ---

  private dispatch(event: WSEvent, payload: any) {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(cb => cb(payload));
    }
  }

  private mockConnectionProcess() {
    // Ensuring reliable connection for the UI demo
    setTimeout(() => {
      this.isConnected = true;
      this.reconnectAttempts = 0;
      console.log('[WS] Connected successfully.');
      this.startHeartbeat();
      this.dispatch('open', {});
      
      // Send a system greeting
      setTimeout(() => {
          this.dispatch('notification', { title: 'System', message: 'Connected to Agent realtime gateway.' });
      }, 300);
    }, 500);
  }

  private handleReconnect() {
    if (this.reconnectAttempts >= this.MAX_RECONNECT_ATTEMPTS) {
      console.error('[WS] Max reconnect attempts reached.');
      this.dispatch('error', { message: 'Connection failed' });
      return;
    }

    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 10000);
    this.reconnectAttempts++;
    
    console.log(`[WS] Reconnecting in ${delay}ms...`);
    this.reconnectTimer = setTimeout(() => this.mockConnectionProcess(), delay);
  }

  private startHeartbeat() {
    this.stopHeartbeat();
    this.heartbeatInterval = setInterval(() => {
      if (this.isConnected) {
        console.log('[WS] ❤️ Ping');
        // this.socket.send('ping');
      }
    }, this.HEARTBEAT_RATE);
  }

  private stopHeartbeat() {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval);
      this.heartbeatInterval = null;
    }
  }
}

export const socketService = WebSocketService.getInstance();

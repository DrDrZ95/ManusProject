
/**
 * Enterprise WebSocket Service
 * 企业级 WebSocket 服务
 * 
 * Features:
 * 1. Singleton Pattern for global access.
 * 2. Automatic Reconnection (Exponential Backoff).
 * 3. Heartbeat Mechanism (Ping/Pong) to detect dead connections.
 * 4. Event-based subscription system.
 * 
 * 特性：
 * 1. 全局访问的单例模式。
 * 2. 自动重连（指数退避算法）。
 * 3. 心跳机制（Ping/Pong）检测死链接。
 * 4. 基于事件的订阅系统。
 */

type WebSocketEventHandler = (payload: any) => void;

class WebSocketService {
  private static instance: WebSocketService;
  private socket: WebSocket | null = null;
  private url: string = 'ws://localhost:8080/ws';
  
  // State
  private isConnected: boolean = false;
  private reconnectAttempts: number = 0;
  private maxReconnectAttempts: number = 5;
  private reconnectTimeoutId: any = null;
  
  // Heartbeat
  private heartbeatInterval: any = null;
  private readonly HEARTBEAT_RATE = 30000; // 30s

  // Event Listeners
  private listeners: Map<string, WebSocketEventHandler[]> = new Map();

  private constructor() {}

  public static getInstance(): WebSocketService {
    if (!WebSocketService.instance) {
      WebSocketService.instance = new WebSocketService();
    }
    return WebSocketService.instance;
  }

  /**
   * Initiate Connection
   * 初始化连接
   */
  public connect(url?: string): void {
    if (this.isConnected) return;
    if (url) this.url = url;

    console.log(`[WS] Connecting to ${this.url}...`);

    // In a real environment, use: this.socket = new WebSocket(this.url);
    // For this simulation, we mock the object.
    // 真实环境中应使用 new WebSocket(this.url)。此处为模拟。
    this.mockConnectionProcess();
  }

  /**
   * Disconnect manually
   * 手动断开
   */
  public disconnect(): void {
    if (this.reconnectTimeoutId) clearTimeout(this.reconnectTimeoutId);
    this.stopHeartbeat();
    this.isConnected = false;
    this.socket = null;
    console.log('[WS] Disconnected by user.');
  }

  /**
   * Send Message to Server
   * 发送消息
   */
  public send(event: string, payload: any): void {
    if (!this.isConnected) {
      console.warn('[WS] Cannot send message: disconnected.', event);
      return;
    }
    const message = JSON.stringify({ event, payload });
    console.log(`[WS] >>> Sending: ${message}`);
    // this.socket.send(message);
  }

  /**
   * Subscribe to event
   * 订阅事件
   */
  public on(event: string, callback: WebSocketEventHandler): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)?.push(callback);
  }

  /**
   * Unsubscribe
   * 取消订阅
   */
  public off(event: string, callback: WebSocketEventHandler): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      this.listeners.set(event, callbacks.filter(cb => cb !== callback));
    }
  }

  // --- Private Implementation Details ---

  private mockConnectionProcess() {
    // Simulate async connection time
    setTimeout(() => {
      // Random connection success/failure for realism
      const success = Math.random() > 0.1; 
      
      if (success) {
        this.isConnected = true;
        this.reconnectAttempts = 0;
        console.log('[WS] Connection Established successfully.');
        this.startHeartbeat();
        this.dispatch('open', {});
      } else {
        console.error('[WS] Connection Failed.');
        this.handleReconnect();
      }
    }, 1000);
  }

  private handleReconnect() {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('[WS] Max reconnect attempts reached. Giving up.');
      return;
    }

    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 10000);
    this.reconnectAttempts++;
    
    console.log(`[WS] Attempting reconnect in ${delay}ms (Attempt ${this.reconnectAttempts})...`);
    
    this.reconnectTimeoutId = setTimeout(() => {
      this.mockConnectionProcess();
    }, delay);
  }

  private startHeartbeat() {
    this.stopHeartbeat();
    this.heartbeatInterval = setInterval(() => {
      if (this.isConnected) {
        // Send Ping
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

  private dispatch(event: string, payload: any) {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(cb => cb(payload));
    }
  }
}

export const socketService = WebSocketService.getInstance();

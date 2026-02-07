
/**
 * WebSocket Service - 实时通信服务
 * 
 * 设计模式: Observer Pattern (观察者模式) & Singleton (单例模式)
 * 
 * 作用:
 * 处理双向实时通信。
 * 1. 维护长连接 (Connection Keep-alive)。
 * 2. 自动重连机制 (Reconnection Strategy)。
 * 3. 消息订阅与分发 (Pub/Sub)。
 */

type EventHandler = (data: any) => void;

class WebSocketService {
  private static instance: WebSocketService;
  private socket: WebSocket | null = null;
  private url: string = 'wss://api.agentproject.io/ws'; // 网关 WS 地址
  private listeners: Map<string, Set<EventHandler>> = new Map();
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private heartbeatTimer: any = null;
  
  // 模拟模式标志: 允许在无后端情况下演示实时效果
  private isMockMode = true;
  private mockInterval: any = null;

  private constructor() {}

  // 获取全局单例
  public static getInstance(): WebSocketService {
    if (!WebSocketService.instance) {
      WebSocketService.instance = new WebSocketService();
    }
    return WebSocketService.instance;
  }

  /**
   * 建立连接
   * @param url 可选的连接地址
   */
  public connect(url?: string): void {
    if (url) this.url = url;

    // 如果开启 Mock 模式，不建立真实连接，而是启动模拟数据生成器
    if (this.isMockMode) {
      this.startMockSimulation();
      return;
    }

    this.socket = new WebSocket(this.url);

    this.socket.onopen = () => {
      console.log('[WS] Connected');
      this.reconnectAttempts = 0;
      this.startHeartbeat();
      this.emit('connection', { status: 'connected' });
    };

    this.socket.onmessage = (event) => {
      try {
        const message = JSON.parse(event.data);
        // 协议约定: 消息结构 { type: 'event_name', payload: {...} }
        if (message.type) {
          this.emit(message.type, message.payload);
        }
      } catch (e) {
        console.error('[WS] Parse error', e);
      }
    };

    this.socket.onclose = () => {
      console.log('[WS] Disconnected');
      this.stopHeartbeat();
      this.handleReconnect();
    };

    this.socket.onerror = (error) => {
      console.error('[WS] Error', error);
    };
  }

  /**
   * 模拟数据流 (用于演示环境)
   * 定期发送 "system_health" 和 "log_stream" 事件
   */
  private startMockSimulation() {
    console.log('[WS] Starting Mock Simulation Mode...');
    if (this.mockInterval) clearInterval(this.mockInterval);

    this.mockInterval = setInterval(() => {
      // 1. 模拟系统健康度推送
      this.emit('system_health', {
        cpu: Math.floor(Math.random() * 30) + 20, // 波动范围 20-50%
        memory: Math.floor(Math.random() * 40) + 40, // 波动范围 40-80%
        activePods: 240 + Math.floor(Math.random() * 10),
        status: Math.random() > 0.95 ? 'warning' : 'healthy'
      });

      // 2. 模拟随机日志推送
      if (Math.random() > 0.7) {
        this.emit('log_stream', {
          timestamp: new Date().toISOString(),
          level: Math.random() > 0.9 ? 'ERROR' : 'INFO',
          service: 'inference-engine',
          message: Math.random() > 0.9 ? 'Connection timeout to Redis' : 'Processed batch #4921 successfully'
        });
      }
    }, 2000);
  }

  /**
   * 订阅消息事件
   * @param event 事件名称
   * @param handler 回调函数
   * @returns 取消订阅的函数 (Unsubscribe function)
   */
  public subscribe(event: string, handler: EventHandler): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)?.add(handler);

    // 返回 clean-up 函数，方便 useEffect 使用
    return () => {
      this.listeners.get(event)?.delete(handler);
    };
  }

  /**
   * 内部事件分发器
   */
  private emit(event: string, data: any) {
    const handlers = this.listeners.get(event);
    if (handlers) {
      handlers.forEach(handler => handler(data));
    }
  }

  /**
   * 自动重连策略 (指数退避)
   */
  private handleReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000); 
      console.log(`[WS] Reconnecting in ${delay}ms...`);
      setTimeout(() => this.connect(), delay);
    }
  }

  /**
   * 心跳保活 (Ping/Pong)
   */
  private startHeartbeat() {
    this.heartbeatTimer = setInterval(() => {
      if (this.socket?.readyState === WebSocket.OPEN) {
        this.socket.send(JSON.stringify({ type: 'ping' }));
      }
    }, 30000);
  }

  private stopHeartbeat() {
    if (this.heartbeatTimer) clearInterval(this.heartbeatTimer);
  }

  public disconnect() {
    if (this.isMockMode) {
      clearInterval(this.mockInterval);
    } else {
      this.socket?.close();
    }
  }
}

export const wsService = WebSocketService.getInstance();

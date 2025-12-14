/**
 * WebSocket Service - 实时通信服务
 * 
 * 采用观察者模式 (Observer Pattern) 处理消息订阅。
 * 功能包括：
 * 1. 自动重连 (Exponential Backoff)
 * 2. 心跳保活 (Heartbeat)
 * 3. 消息分发 (Event Dispatching)
 * 4. 模拟模式 (Mock Mode) - 用于演示环境
 */

type EventHandler = (data: any) => void;

class WebSocketService {
  private static instance: WebSocketService;
  private socket: WebSocket | null = null;
  private url: string = 'wss://api.opsnexus.io/ws'; // 默认地址
  private listeners: Map<string, Set<EventHandler>> = new Map();
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private heartbeatTimer: any = null;
  
  // 模拟模式标志
  private isMockMode = true;
  private mockInterval: any = null;

  private constructor() {}

  public static getInstance(): WebSocketService {
    if (!WebSocketService.instance) {
      WebSocketService.instance = new WebSocketService();
    }
    return WebSocketService.instance;
  }

  /**
   * 连接 WebSocket
   */
  public connect(url?: string): void {
    if (url) this.url = url;

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
        // 假设消息结构 { type: 'metrics', payload: {...} }
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
   * 模拟数据流 (演示用)
   * 定期发送系统健康度、日志等数据
   */
  private startMockSimulation() {
    console.log('[WS] Starting Mock Simulation Mode...');
    if (this.mockInterval) clearInterval(this.mockInterval);

    this.mockInterval = setInterval(() => {
      // 模拟系统健康度更新
      this.emit('system_health', {
        cpu: Math.floor(Math.random() * 30) + 20, // 20-50%
        memory: Math.floor(Math.random() * 40) + 40, // 40-80%
        activePods: 240 + Math.floor(Math.random() * 10),
        status: Math.random() > 0.95 ? 'warning' : 'healthy'
      });

      // 模拟实时日志
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
   */
  public subscribe(event: string, handler: EventHandler): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)?.add(handler);

    // 返回取消订阅函数
    return () => {
      this.listeners.get(event)?.delete(handler);
    };
  }

  /**
   * 内部事件分发
   */
  private emit(event: string, data: any) {
    const handlers = this.listeners.get(event);
    if (handlers) {
      handlers.forEach(handler => handler(data));
    }
  }

  /**
   * 自动重连逻辑
   */
  private handleReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000); // Exponential backoff
      console.log(`[WS] Reconnecting in ${delay}ms...`);
      setTimeout(() => this.connect(), delay);
    }
  }

  /**
   * 心跳检测
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
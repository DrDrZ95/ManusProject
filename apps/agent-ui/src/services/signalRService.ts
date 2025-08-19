import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface SignalRMessage {
  user: string;
  message: string;
  timestamp: Date;
  messageId: string;
  jwtId?: string;
  userId?: string;
}

export interface SignalRConnectionStatus {
  isConnected: boolean;
  connectionState: string;
  lastError?: string;
  authenticationMethod?: string;
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private connectionUrl: string;
  private accessToken?: string;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private reconnectDelays = [0, 2000, 10000, 30000, 60000]; // Automatic reconnection intervals

  // Event handlers
  public onMessageReceived: ((message: SignalRMessage) => void) | null = null;
  public onConnectionStatusChanged: ((status: SignalRConnectionStatus) => void) | null = null;
  public onUserJoined: ((user: string) => void) | null = null;
  public onUserLeft: ((user: string) => void) | null = null;
  public onAuthorizationError: ((error: any) => void) | null = null;
  public onChatResponse: ((data: any) => void) | null = null;
  public onStreamChunk: ((data: any) => void) | null = null;
  public onWelcome: ((data: any) => void) | null = null;

  constructor(hubUrl: string = 'http://localhost:5000/chathub', accessToken?: string) {
    this.connectionUrl = hubUrl;
    this.accessToken = accessToken;
  }

  /**
   * Initialize and start the SignalR connection with JWT authentication and automatic reconnection
   * 初始化并启动SignalR连接，包含JWT认证和自动重连
   */
  public async startConnection(): Promise<void> {
    try {
      // Build the connection with JWT authentication and automatic reconnection
      // 构建连接，包含JWT认证和自动重连
      this.connection = new HubConnectionBuilder()
        .withUrl(this.connectionUrl, {
          accessTokenFactory: () => this.accessToken || '',
          transport: 1 | 2, // WebSockets | LongPolling
        })
        .withAutomaticReconnect(this.reconnectDelays) // Automatic reconnection with custom delays
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      // 设置事件处理器
      this.setupEventHandlers();

      // Set up reconnection event handlers
      // 设置重连事件处理器
      this.setupReconnectionHandlers();

      // Start the connection
      // 启动连接
      await this.connection.start();
      
      this.reconnectAttempts = 0;
      this.notifyConnectionStatus();
      
      console.log('SignalR connection established successfully with JWT authentication and automatic reconnection');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.notifyConnectionStatus(error as Error);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection
   * 停止SignalR连接
   */
  public async stopConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      } finally {
        this.connection = null;
        this.notifyConnectionStatus();
      }
    }
  }

  /**
   * Send a message through SignalR (requires JWT authentication)
   * 通过SignalR发送消息（需要JWT认证）
   */
  public async sendMessage(user: string, message: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('SendMessage', user, message);
    } catch (error) {
      console.error('Error sending message:', error);
      throw error;
    }
  }

  /**
   * Send chat message with JWT authentication
   * 发送聊天消息（需要JWT认证）
   */
  public async sendChatMessage(message: string, conversationId?: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('SendChatMessage', message, conversationId);
    } catch (error) {
      console.error('Error sending chat message:', error);
      throw error;
    }
  }

  /**
   * Send streaming chat message with JWT authentication
   * 发送流式聊天消息（需要JWT认证）
   */
  public async sendStreamingChatMessage(message: string, conversationId?: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('SendStreamingChatMessage', message, conversationId);
    } catch (error) {
      console.error('Error sending streaming chat message:', error);
      throw error;
    }
  }

  /**
   * Join a specific room (requires JWT authentication)
   * 加入特定房间（需要JWT认证）
   */
  public async joinRoom(roomName: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('JoinRoom', roomName);
    } catch (error) {
      console.error('Error joining room:', error);
      throw error;
    }
  }

  /**
   * Leave a specific room (requires JWT authentication)
   * 离开特定房间（需要JWT认证）
   */
  public async leaveRoom(roomName: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('LeaveRoom', roomName);
    } catch (error) {
      console.error('Error leaving room:', error);
      throw error;
    }
  }

  /**
   * Update JWT access token
   * 更新JWT访问令牌
   */
  public updateAccessToken(token: string): void {
    this.accessToken = token;
  }

  /**
   * Manually reconnect with new JWT token
   * 使用新JWT令牌手动重连
   */
  public async reconnectWithToken(token?: string): Promise<void> {
    if (token) {
      this.updateAccessToken(token);
    }
    
    if (this.connection) {
      try {
        await this.connection.stop();
        await this.startConnection();
      } catch (error) {
        console.error('Error during manual reconnection:', error);
        throw error;
      }
    }
  }

  /**
   * Get current connection status
   * 获取当前连接状态
   */
  public getConnectionStatus(): SignalRConnectionStatus {
    return {
      isConnected: this.connection?.state === 'Connected',
      connectionState: this.connection?.state || 'Disconnected',
      authenticationMethod: this.accessToken ? 'JWT' : 'None',
      lastError: undefined
    };
  }

  /**
   * Setup event handlers for SignalR connection
   * 设置SignalR连接的事件处理器
   */
  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Handle incoming messages
    // 处理传入消息
    this.connection.on('ReceiveMessage', (user: string, message: string, metadata?: any) => {
      const signalRMessage: SignalRMessage = {
        user,
        message,
        timestamp: new Date(),
        messageId: this.generateMessageId(),
        jwtId: metadata?.jwtId,
        userId: metadata?.userId
      };
      
      if (this.onMessageReceived) {
        this.onMessageReceived(signalRMessage);
      }
    });

    // Handle welcome messages
    // 处理欢迎消息
    this.connection.on('Welcome', (data: any) => {
      console.log('Welcome message received:', data);
      if (this.onWelcome) {
        this.onWelcome(data);
      }
    });

    // Handle chat responses
    // 处理聊天响应
    this.connection.on('ChatResponse', (data: any) => {
      console.log('Chat response received:', data);
      if (this.onChatResponse) {
        this.onChatResponse(data);
      }
    });

    // Handle streaming chat chunks
    // 处理流式聊天数据块
    this.connection.on('StreamChunk', (data: any) => {
      if (this.onStreamChunk) {
        this.onStreamChunk(data);
      }
    });

    // Handle authorization errors
    // 处理授权错误
    this.connection.on('AuthorizationError', (data: any) => {
      console.error('Authorization error:', data);
      if (this.onAuthorizationError) {
        this.onAuthorizationError(data);
      }
    });

    // Handle user joined events
    // 处理用户加入事件
    this.connection.on('UserJoinedRoom', (data: any) => {
      if (this.onUserJoined) {
        this.onUserJoined(data.userName || data.user);
      }
    });

    // Handle user left events
    // 处理用户离开事件
    this.connection.on('UserLeftRoom', (data: any) => {
      if (this.onUserLeft) {
        this.onUserLeft(data.userName || data.user);
      }
    });

    // Handle general errors
    // 处理一般错误
    this.connection.on('Error', (data: any) => {
      console.error('SignalR error:', data);
    });
  }

  /**
   * Setup reconnection event handlers
   * 设置重连事件处理器
   */
  private setupReconnectionHandlers(): void {
    if (!this.connection) return;

    // Handle connection close
    // 处理连接关闭
    this.connection.onclose((error) => {
      console.log('SignalR connection closed', error);
      this.notifyConnectionStatus(error);
    });

    // Handle reconnecting
    // 处理重连中
    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
      this.reconnectAttempts++;
      this.notifyConnectionStatus(error);
    });

    // Handle reconnected
    // 处理重连成功
    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
      this.reconnectAttempts = 0;
      this.notifyConnectionStatus();
    });
  }

  /**
   * Notify connection status change
   * 通知连接状态变化
   */
  private notifyConnectionStatus(error?: Error): void {
    if (this.onConnectionStatusChanged) {
      const status: SignalRConnectionStatus = {
        isConnected: this.connection?.state === 'Connected',
        connectionState: this.connection?.state || 'Disconnected',
        authenticationMethod: this.accessToken ? 'JWT' : 'None',
        lastError: error?.message
      };
      this.onConnectionStatusChanged(status);
    }
  }

  /**
   * Generate a unique message ID
   * 生成唯一消息ID
   */
  private generateMessageId(): string {
    return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Simulate connection for development/testing (with JWT simulation)
   * 模拟连接用于开发/测试（包含JWT模拟）
   */
  public async simulateConnection(): Promise<void> {
    console.log('🔄 Simulating SignalR connection with JWT authentication...');
    
    // Simulate connection delay
    // 模拟连接延迟
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Create a mock connection object
    // 创建模拟连接对象
    this.connection = {
      state: 'Connected',
      start: async () => Promise.resolve(),
      stop: async () => Promise.resolve(),
      invoke: async (methodName: string, ...args: any[]) => {
        console.log(`📤 Simulated SignalR invoke: ${methodName}`, args);
        
        // Simulate echo response for SendMessage
        // 模拟SendMessage的回声响应
        if (methodName === 'SendMessage' && args.length >= 2) {
          setTimeout(() => {
            const echoMessage: SignalRMessage = {
              user: 'AgentUI Bot',
              message: `Echo: ${args[1]}`,
              timestamp: new Date(),
              messageId: this.generateMessageId(),
              jwtId: 'simulated_jwt_id',
              userId: 'simulated_user_id'
            };
            
            if (this.onMessageReceived) {
              this.onMessageReceived(echoMessage);
            }
          }, 500);
        }

        // Simulate chat response for SendChatMessage
        // 模拟SendChatMessage的聊天响应
        if (methodName === 'SendChatMessage' && args.length >= 1) {
          setTimeout(() => {
            const chatResponse = {
              conversationId: args[1] || 'simulated_conversation',
              response: `AI Response to: ${args[0]}`,
              userId: 'simulated_user_id',
              jwtId: 'simulated_jwt_id',
              timestamp: new Date(),
              isComplete: true
            };
            
            if (this.onChatResponse) {
              this.onChatResponse(chatResponse);
            }
          }, 800);
        }
        
        return Promise.resolve();
      },
      on: (eventName: string, callback: (...args: any[]) => void) => {
        console.log(`📡 Registered handler for: ${eventName}`);
      },
      onclose: (callback: (error?: Error) => void) => {
        console.log('📡 Registered onclose handler');
      },
      onreconnecting: (callback: (error?: Error) => void) => {
        console.log('📡 Registered onreconnecting handler');
      },
      onreconnected: (callback: (connectionId?: string) => void) => {
        console.log('📡 Registered onreconnected handler');
      }
    } as any;
    
    // Simulate welcome message
    // 模拟欢迎消息
    setTimeout(() => {
      if (this.onWelcome) {
        this.onWelcome({
          message: 'Welcome to AI-Agent with JWT authentication!',
          userId: 'simulated_user_id',
          jwtId: 'simulated_jwt_id',
          authenticationMethod: 'JWT',
          connectedAt: new Date()
        });
      }
    }, 1500);
    
    this.notifyConnectionStatus();
    console.log('✅ SignalR simulation connection established with JWT authentication');
  }
}

// Export a singleton instance
// 导出单例实例
export const signalRService = new SignalRService();


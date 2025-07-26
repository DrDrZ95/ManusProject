import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface SignalRMessage {
  user: string;
  message: string;
  timestamp: Date;
  messageId: string;
}

export interface SignalRConnectionStatus {
  isConnected: boolean;
  connectionState: string;
  lastError?: string;
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private connectionUrl: string;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 3000; // 3 seconds

  // Event handlers
  public onMessageReceived: ((message: SignalRMessage) => void) | null = null;
  public onConnectionStatusChanged: ((status: SignalRConnectionStatus) => void) | null = null;
  public onUserJoined: ((user: string) => void) | null = null;
  public onUserLeft: ((user: string) => void) | null = null;

  constructor(hubUrl: string = 'http://localhost:5000/chathub') {
    this.connectionUrl = hubUrl;
  }

  /**
   * Initialize and start the SignalR connection
   */
  public async startConnection(): Promise<void> {
    try {
      // Build the connection
      this.connection = new HubConnectionBuilder()
        .withUrl(this.connectionUrl)
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectDelay;
            }
            return null; // Stop retrying
          }
        })
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      this.setupEventHandlers();

      // Start the connection
      await this.connection.start();
      
      this.reconnectAttempts = 0;
      this.notifyConnectionStatus();
      
      console.log('SignalR connection established successfully');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.notifyConnectionStatus(error as Error);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection
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
   * Send a message through SignalR
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
   * Join a specific chat group/room
   */
  public async joinGroup(groupName: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('JoinGroup', groupName);
    } catch (error) {
      console.error('Error joining group:', error);
      throw error;
    }
  }

  /**
   * Leave a specific chat group/room
   */
  public async leaveGroup(groupName: string): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    try {
      await this.connection.invoke('LeaveGroup', groupName);
    } catch (error) {
      console.error('Error leaving group:', error);
      throw error;
    }
  }

  /**
   * Get current connection status
   */
  public getConnectionStatus(): SignalRConnectionStatus {
    return {
      isConnected: this.connection?.state === 'Connected',
      connectionState: this.connection?.state || 'Disconnected',
      lastError: undefined
    };
  }

  /**
   * Setup event handlers for SignalR connection
   */
  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Handle incoming messages
    this.connection.on('ReceiveMessage', (user: string, message: string) => {
      const signalRMessage: SignalRMessage = {
        user,
        message,
        timestamp: new Date(),
        messageId: this.generateMessageId()
      };
      
      if (this.onMessageReceived) {
        this.onMessageReceived(signalRMessage);
      }
    });

    // Handle user joined events
    this.connection.on('UserJoined', (user: string) => {
      if (this.onUserJoined) {
        this.onUserJoined(user);
      }
    });

    // Handle user left events
    this.connection.on('UserLeft', (user: string) => {
      if (this.onUserLeft) {
        this.onUserLeft(user);
      }
    });

    // Handle connection state changes
    this.connection.onclose((error) => {
      console.log('SignalR connection closed', error);
      this.notifyConnectionStatus(error);
    });

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
      this.reconnectAttempts++;
      this.notifyConnectionStatus(error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
      this.reconnectAttempts = 0;
      this.notifyConnectionStatus();
    });
  }

  /**
   * Notify connection status change
   */
  private notifyConnectionStatus(error?: Error): void {
    if (this.onConnectionStatusChanged) {
      const status: SignalRConnectionStatus = {
        isConnected: this.connection?.state === 'Connected',
        connectionState: this.connection?.state || 'Disconnected',
        lastError: error?.message
      };
      this.onConnectionStatusChanged(status);
    }
  }

  /**
   * Generate a unique message ID
   */
  private generateMessageId(): string {
    return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Simulate connection for development/testing
   */
  public async simulateConnection(): Promise<void> {
    console.log('🔄 Simulating SignalR connection...');
    
    // Simulate connection delay
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Create a mock connection object
    this.connection = {
      state: 'Connected',
      start: async () => Promise.resolve(),
      stop: async () => Promise.resolve(),
      invoke: async (methodName: string, ...args: any[]) => {
        console.log(`📤 Simulated SignalR invoke: ${methodName}`, args);
        
        // Simulate echo response for SendMessage
        if (methodName === 'SendMessage' && args.length >= 2) {
          setTimeout(() => {
            const echoMessage: SignalRMessage = {
              user: 'AgentUI Bot',
              message: `Echo: ${args[1]}`,
              timestamp: new Date(),
              messageId: this.generateMessageId()
            };
            
            if (this.onMessageReceived) {
              this.onMessageReceived(echoMessage);
            }
          }, 500);
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
    
    this.notifyConnectionStatus();
    console.log('✅ SignalR simulation connection established');
  }
}

// Export a singleton instance
export const signalRService = new SignalRService();


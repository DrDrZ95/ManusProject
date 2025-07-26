import { useState, useEffect, useCallback, useRef } from 'react';
import { signalRService, SignalRMessage, SignalRConnectionStatus } from '../services/signalRService';

export interface UseSignalROptions {
  autoConnect?: boolean;
  hubUrl?: string;
  simulateConnection?: boolean;
}

export interface UseSignalRReturn {
  // Connection state
  connectionStatus: SignalRConnectionStatus;
  isConnected: boolean;
  isConnecting: boolean;
  
  // Messages
  messages: SignalRMessage[];
  
  // Actions
  sendMessage: (user: string, message: string) => Promise<void>;
  connect: () => Promise<void>;
  disconnect: () => Promise<void>;
  joinGroup: (groupName: string) => Promise<void>;
  leaveGroup: (groupName: string) => Promise<void>;
  clearMessages: () => void;
  
  // Events
  onlineUsers: string[];
}

export const useSignalR = (options: UseSignalROptions = {}): UseSignalRReturn => {
  const {
    autoConnect = true,
    hubUrl,
    simulateConnection = true // Default to simulation for development
  } = options;

  // State
  const [connectionStatus, setConnectionStatus] = useState<SignalRConnectionStatus>({
    isConnected: false,
    connectionState: 'Disconnected'
  });
  const [isConnecting, setIsConnecting] = useState(false);
  const [messages, setMessages] = useState<SignalRMessage[]>([]);
  const [onlineUsers, setOnlineUsers] = useState<string[]>([]);
  
  // Refs to prevent stale closures
  const messagesRef = useRef<SignalRMessage[]>([]);
  const onlineUsersRef = useRef<string[]>([]);

  // Update refs when state changes
  useEffect(() => {
    messagesRef.current = messages;
  }, [messages]);

  useEffect(() => {
    onlineUsersRef.current = onlineUsers;
  }, [onlineUsers]);

  // Setup SignalR event handlers
  useEffect(() => {
    // Message received handler
    signalRService.onMessageReceived = (message: SignalRMessage) => {
      setMessages(prev => [...prev, message]);
    };

    // Connection status changed handler
    signalRService.onConnectionStatusChanged = (status: SignalRConnectionStatus) => {
      setConnectionStatus(status);
      setIsConnecting(false);
    };

    // User joined handler
    signalRService.onUserJoined = (user: string) => {
      setOnlineUsers(prev => {
        if (!prev.includes(user)) {
          return [...prev, user];
        }
        return prev;
      });
    };

    // User left handler
    signalRService.onUserLeft = (user: string) => {
      setOnlineUsers(prev => prev.filter(u => u !== user));
    };

    return () => {
      // Cleanup event handlers
      signalRService.onMessageReceived = null;
      signalRService.onConnectionStatusChanged = null;
      signalRService.onUserJoined = null;
      signalRService.onUserLeft = null;
    };
  }, []);

  // Auto-connect on mount
  useEffect(() => {
    if (autoConnect) {
      connect();
    }

    return () => {
      // Cleanup on unmount
      disconnect();
    };
  }, [autoConnect]);

  // Connection functions
  const connect = useCallback(async () => {
    if (connectionStatus.isConnected || isConnecting) {
      return;
    }

    setIsConnecting(true);
    
    try {
      if (simulateConnection) {
        await signalRService.simulateConnection();
      } else {
        if (hubUrl) {
          // Create new service instance with custom URL if provided
          const customService = new (signalRService.constructor as any)(hubUrl);
          await customService.startConnection();
        } else {
          await signalRService.startConnection();
        }
      }
    } catch (error) {
      console.error('Failed to connect to SignalR:', error);
      setIsConnecting(false);
    }
  }, [connectionStatus.isConnected, isConnecting, simulateConnection, hubUrl]);

  const disconnect = useCallback(async () => {
    try {
      await signalRService.stopConnection();
    } catch (error) {
      console.error('Failed to disconnect from SignalR:', error);
    }
  }, []);

  // Message functions
  const sendMessage = useCallback(async (user: string, message: string) => {
    try {
      await signalRService.sendMessage(user, message);
    } catch (error) {
      console.error('Failed to send message:', error);
      throw error;
    }
  }, []);

  // Group functions
  const joinGroup = useCallback(async (groupName: string) => {
    try {
      await signalRService.joinGroup(groupName);
    } catch (error) {
      console.error('Failed to join group:', error);
      throw error;
    }
  }, []);

  const leaveGroup = useCallback(async (groupName: string) => {
    try {
      await signalRService.leaveGroup(groupName);
    } catch (error) {
      console.error('Failed to leave group:', error);
      throw error;
    }
  }, []);

  // Utility functions
  const clearMessages = useCallback(() => {
    setMessages([]);
  }, []);

  return {
    // Connection state
    connectionStatus,
    isConnected: connectionStatus.isConnected,
    isConnecting,
    
    // Messages
    messages,
    
    // Actions
    sendMessage,
    connect,
    disconnect,
    joinGroup,
    leaveGroup,
    clearMessages,
    
    // Events
    onlineUsers
  };
};


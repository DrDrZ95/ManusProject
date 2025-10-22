import { useContext, useRef, useEffect, useState } from 'react';
import { Send, Plus, Upload, X, Wifi, WifiOff } from 'lucide-react';
import { ChatContext, Message } from '../contexts/ChatContext';
import { useChatActions } from '../hooks/useChatActions';
import { useSignalR } from '../hooks/useSignalR';

// File upload component
const FileUploadArea = ({ onFileUpload }: { onFileUpload: (files: File[]) => void }) => {
  const [isDragOver, setIsDragOver] = useState(false);
  const [isUploadDialogOpen, setIsUploadDialogOpen] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    const files = Array.from(e.dataTransfer.files);
    if (files.length > 0) {
      onFileUpload(files);
      setIsUploadDialogOpen(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length > 0) {
      onFileUpload(files);
      setIsUploadDialogOpen(false);
    }
  };

  return (
    <>
      {/* Upload button */}
      <button
        type="button"
        onClick={() => setIsUploadDialogOpen(true)}
        className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-md transition-colors duration-150"
        title="Upload files"
      >
        <Plus size={20} />
      </button>

      {/* Upload dialog */}
      {isUploadDialogOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-30 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4 shadow-lg border border-gray-200">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-base font-semibold text-gray-900">Upload Files</h3>
              <button
                onClick={() => setIsUploadDialogOpen(false)}
                className="text-gray-500 hover:text-gray-700 transition-colors"
              >
                <X size={20} />
              </button>
            </div>
            
            {/* Drag and drop area */}
            <div
              className={`border-2 border-dashed rounded-lg p-8 text-center transition-all duration-200 ${
                isDragOver
                  ? 'border-gray-400 bg-gray-50'
                  : 'border-gray-300 hover:border-gray-400'
              }`}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
            >
              <Upload size={40} className="mx-auto mb-3 text-gray-400" />
              <p className="text-gray-700 mb-2 text-sm font-medium">
                Drag and drop files here, or{' '}
                <button
                  onClick={() => fileInputRef.current?.click()}
                  className="text-blue-600 underline hover:text-blue-700"
                >
                  browse
                </button>
              </p>
              <p className="text-xs text-gray-500">
                Supports images, documents, and text files
              </p>
            </div>

            <input
              ref={fileInputRef}
              type="file"
              multiple
              className="hidden"
              onChange={handleFileSelect}
              accept="image/*,.pdf,.doc,.docx,.txt,.md"
            />
          </div>
        </div>
      )}
    </>
  );
};

// SignalR Connection Status Component
const SignalRStatus = ({ isConnected, connectionState }: { isConnected: boolean; connectionState: string }) => {
  return (
    <div className="flex items-center space-x-2 text-xs text-gray-600">
      {isConnected ? (
        <>
          <Wifi size={14} className="text-green-500" />
          <span className="text-green-700">Connected</span>
        </>
      ) : (
        <>
          <WifiOff size={14} className="text-red-500" />
          <span className="text-red-700">{connectionState}</span>
        </>
      )}
    </div>
  );
};

// Message bubble component - Notion-style
const MessageBubble = ({ message }: { message: Message }) => {
  const isUser = message.role === 'user';
  
  return (
    <div className={`flex ${isUser ? 'justify-end' : 'justify-start'} mb-3`}>
      <div 
        className={`max-w-[70%] rounded-lg px-4 py-3 ${
          isUser 
            ? 'bg-blue-600 text-white rounded-br-none shadow-sm' 
            : 'bg-gray-100 text-gray-900 rounded-bl-none'
        }`}
      >
        <div className="text-sm leading-relaxed whitespace-pre-wrap break-words">{message.content}</div>
        <div className={`text-xs mt-2 opacity-70 ${isUser ? 'text-blue-100' : 'text-gray-600'}`}>
          {new Date(message.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
        </div>
      </div>
    </div>
  );
};

export const ChatArea = () => {
  const { currentConversationId } = useContext(ChatContext);
  const { sendMessage, isLoading, getCurrentConversation } = useChatActions();
  const [inputValue, setInputValue] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  // SignalR integration
  const {
    isConnected: signalRConnected,
    connectionStatus,
    messages: signalRMessages,
    sendMessage: sendSignalRMessage,
    connect: connectSignalR,
    disconnect: disconnectSignalR,
    clearMessages: clearSignalRMessages
  } = useSignalR({
    autoConnect: true,
    simulateConnection: true,
    hubUrl: 'http://localhost:5000/chathub'
  });
  
  const currentConversation = getCurrentConversation();
  
  // Auto-scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [currentConversation?.messages, signalRMessages]);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (inputValue.trim() && !isLoading) {
      sendMessage(inputValue);
      
      if (signalRConnected) {
        try {
          await sendSignalRMessage('User', inputValue);
        } catch (error) {
          console.error('Failed to send SignalR message:', error);
        }
      }
      
      setInputValue('');
    }
  };

  const handleFileUpload = (files: File[]) => {
    const fileNames = files.map(file => file.name).join(', ');
    const uploadMessage = `📎 Files uploaded: ${fileNames}\n\nSimulated upload successful! Files are ready for processing.`;
    
    sendMessage(uploadMessage);
    
    if (signalRConnected) {
      sendSignalRMessage('User', uploadMessage).catch(console.error);
    }
  };

  const handleSignalRReconnect = async () => {
    try {
      await connectSignalR();
    } catch (error) {
      console.error('Failed to reconnect SignalR:', error);
    }
  };
  
  if (!currentConversationId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center bg-white text-gray-600 h-full">
        <div className="text-center max-w-md p-6">
          <div className="mb-6">
            <div className="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center">
              <span className="text-3xl">🤖</span>
            </div>
          </div>
          <h2 className="text-2xl font-semibold mb-3 text-gray-900">How can I help you today?</h2>
          <p className="mb-6 text-gray-600 text-sm leading-relaxed">
            I'm AgentUI, your AI assistant. I can help with a wide range of tasks including analysis, writing, coding, and much more.
          </p>
          
          {/* SignalR Status in welcome screen */}
          <div className="mb-6">
            <SignalRStatus isConnected={signalRConnected} connectionState={connectionStatus.connectionState} />
          </div>
          
          <button
            onClick={() => sendMessage("Hello, I'm a new user. What can you help me with?")}
            className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-md transition-colors duration-200 text-sm font-medium"
          >
            Start conversation
          </button>
        </div>
      </div>
    );
  }
  
  return (
    <div className="flex-1 flex flex-col h-full bg-white">
      {/* Messages area */}
      <div className="flex-1 overflow-y-auto px-6 py-6">
        {currentConversation?.messages.length === 0 && signalRMessages.length === 0 ? (
          <div className="h-full flex items-center justify-center text-gray-500">
            <p className="text-center">
              <span className="block text-5xl mb-3">💬</span>
              <span className="text-sm">Send a message to start the conversation</span>
            </p>
          </div>
        ) : (
          <>
            {/* Regular chat messages */}
            {currentConversation?.messages.map((message) => (
              <MessageBubble key={message.id} message={message} />
            ))}
            
            {/* SignalR messages */}
            {signalRMessages.map((signalRMessage) => (
              <div key={signalRMessage.messageId} className="flex justify-start mb-3">
                <div className="max-w-[70%] rounded-lg px-4 py-3 bg-blue-50 text-gray-900 rounded-bl-none border border-blue-200">
                  <div className="text-xs font-semibold text-blue-700 mb-1">
                    📡 {signalRMessage.user}
                  </div>
                  <div className="text-sm whitespace-pre-wrap break-words">{signalRMessage.message}</div>
                  <div className="text-xs mt-2 text-blue-600">
                    {new Date(signalRMessage.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
              </div>
            ))}
          </>
        )}
        <div ref={messagesEndRef} />
      </div>
      
      {/* Input area */}
      <div className="px-6 py-4 border-t border-gray-200 bg-white">
        {/* SignalR Status Bar */}
        <div className="flex justify-between items-center mb-3">
          <SignalRStatus isConnected={signalRConnected} connectionState={connectionStatus.connectionState} />
          {!signalRConnected && (
            <button
              onClick={handleSignalRReconnect}
              className="text-xs text-blue-600 hover:text-blue-700 underline transition-colors"
            >
              Reconnect
            </button>
          )}
        </div>
        
        <form onSubmit={handleSubmit} className="flex items-end gap-2">
          <FileUploadArea onFileUpload={handleFileUpload} />
          <div className="flex-1">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="Message AgentUI..."
              className="w-full py-2 px-4 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-transparent bg-white text-sm resize-none"
              disabled={isLoading}
            />
          </div>
          <button
            type="submit"
            disabled={!inputValue.trim() || isLoading}
            className={`p-2 rounded-md transition-all duration-200 ${
              !inputValue.trim() || isLoading
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-blue-600 text-white hover:bg-blue-700'
            }`}
          >
            <Send size={18} />
          </button>
        </form>
        {isLoading && (
          <div className="text-xs text-gray-600 mt-2 flex items-center justify-center">
            <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-blue-600 mr-2"></div>
            AI is thinking...
          </div>
        )}
      </div>
    </div>
  );
};


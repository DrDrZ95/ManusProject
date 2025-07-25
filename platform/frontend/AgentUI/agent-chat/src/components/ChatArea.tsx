import { useContext, useRef, useEffect, useState } from 'react';
import { Send, Plus, Upload, X } from 'lucide-react';
import { ChatContext, Message } from '../contexts/ChatContext';
import { useChatActions } from '../hooks/useChatActions';

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
        className="p-3 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-all duration-200"
        title="Upload files"
      >
        <Plus size={20} />
      </button>

      {/* Upload dialog */}
      {isUploadDialogOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-semibold text-gray-800">Upload Files</h3>
              <button
                onClick={() => setIsUploadDialogOpen(false)}
                className="text-gray-500 hover:text-gray-700"
              >
                <X size={20} />
              </button>
            </div>
            
            {/* Drag and drop area */}
            <div
              className={`border-2 border-dashed rounded-lg p-8 text-center transition-all duration-200 ${
                isDragOver
                  ? 'border-gray-500 bg-gray-50'
                  : 'border-gray-300 hover:border-gray-400'
              }`}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
            >
              <Upload size={48} className="mx-auto mb-4 text-gray-400" />
              <p className="text-gray-600 mb-2">
                Drag and drop files here, or{' '}
                <button
                  onClick={() => fileInputRef.current?.click()}
                  className="text-gray-500 underline hover:text-gray-700"
                >
                  browse
                </button>
              </p>
              <p className="text-sm text-gray-500">
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

// Message bubble component
const MessageBubble = ({ message }: { message: Message }) => {
  const isUser = message.role === 'user';
  
  return (
    <div className={`flex ${isUser ? 'justify-end' : 'justify-start'} mb-4`}>
      <div 
        className={`max-w-[80%] rounded-lg px-4 py-3 ${
          isUser 
            ? 'bg-gradient-to-r from-gray-500 to-gray-600 text-white rounded-tr-none shadow-md' 
            : 'bg-white text-gray-800 rounded-tl-none border border-gray-300 shadow-sm'
        }`}
      >
        <div className="whitespace-pre-wrap">{message.content}</div>
        <div className={`text-xs mt-2 ${isUser ? 'text-gray-200' : 'text-gray-500'}`}>
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
  
  const currentConversation = getCurrentConversation();
  
  // Auto-scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [currentConversation?.messages]);
  
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (inputValue.trim() && !isLoading) {
      sendMessage(inputValue);
      setInputValue('');
    }
  };

  const handleFileUpload = (files: File[]) => {
    // Simulate file upload success
    const fileNames = files.map(file => file.name).join(', ');
    sendMessage(`📎 Files uploaded: ${fileNames}\n\nSimulated upload successful! Files are ready for processing.`);
  };
  
  if (!currentConversationId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center bg-white text-gray-600 h-full">
        <div className="text-center max-w-md p-6">
          <div className="mb-6">
            <div className="w-16 h-16 mx-auto mb-4 bg-gray-900 rounded-xl flex items-center justify-center">
              <span className="text-2xl text-white">🤖</span>
            </div>
          </div>
          <h2 className="text-xl font-medium mb-3 text-gray-800">How can I help you today?</h2>
          <p className="mb-6 text-gray-500 text-sm">I'm AgentUI, your AI assistant. I can help with a wide range of tasks.</p>
          <button
            onClick={() => sendMessage("Hello, I'm a new user. What can you help me with?")}
            className="bg-gray-900 hover:bg-gray-800 text-white py-2 px-4 rounded-lg transition-all duration-200 text-sm font-medium"
          >
            Start conversation
          </button>
        </div>
      </div>
    );
  }
  
  return (
    <div className="flex-1 flex flex-col h-full bg-white">
      {/* Messages area - Full height with proper scrolling */}
      <div className="flex-1 overflow-y-auto px-6 py-4">
        {currentConversation?.messages.length === 0 ? (
          <div className="h-full flex items-center justify-center text-gray-500">
            <p className="text-center">
              <span className="block text-4xl mb-2">💬</span>
              Send a message to start the conversation
            </p>
          </div>
        ) : (
          <>
            {currentConversation?.messages.map((message) => (
              <MessageBubble key={message.id} message={message} />
            ))}
          </>
        )}
        <div ref={messagesEndRef} />
      </div>
      
      {/* Input area - Fixed at bottom with clean design */}
      <div className="px-6 py-4 border-t border-gray-200 bg-white">
        <form onSubmit={handleSubmit} className="flex items-end gap-3 max-w-4xl mx-auto">
          <FileUploadArea onFileUpload={handleFileUpload} />
          <div className="flex-1">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="Message AgentUI..."
              className="w-full py-3 px-4 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-gray-500 focus:border-transparent bg-white shadow-sm resize-none"
              disabled={isLoading}
            />
          </div>
          <button
            type="submit"
            disabled={!inputValue.trim() || isLoading}
            className={`p-3 rounded-xl transition-all duration-200 ${
              !inputValue.trim() || isLoading
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-gray-900 text-white hover:bg-gray-800 shadow-sm'
            }`}
          >
            <Send size={20} />
          </button>
        </form>
        {isLoading && (
          <div className="text-xs text-gray-600 mt-2 flex items-center justify-center">
            <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-gray-600 mr-2"></div>
            AI is thinking...
          </div>
        )}
      </div>
    </div>
  );
};


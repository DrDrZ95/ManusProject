import { useContext, useRef, useEffect, useState } from 'react';
import { Send } from 'lucide-react';
import { ChatContext, Message } from '../contexts/ChatContext';
import { useChatActions } from '../hooks/useChatActions';

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
  
  if (!currentConversationId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 text-gray-600 h-full">
        <div className="text-center max-w-md p-6">
          <div className="mb-6">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-r from-gray-500 to-gray-600 rounded-full flex items-center justify-center">
              <span className="text-3xl text-white">🤖</span>
            </div>
          </div>
          <h2 className="text-2xl font-semibold mb-3 text-gray-700">Welcome to AI Chat Assistant</h2>
          <p className="mb-6 text-gray-600">Start a new conversation or select an existing one from the sidebar.</p>
          <button
            onClick={() => sendMessage("Hello, I'm a new user. What can you help me with?")}
            className="bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white py-3 px-6 rounded-lg transition-all duration-200 shadow-md hover:shadow-lg transform hover:scale-105 font-medium"
          >
            Start New Conversation
          </button>
        </div>
      </div>
    );
  }
  
  return (
    <div className="flex-1 flex flex-col h-full bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Chat header */}
      <div className="py-4 px-6 border-b border-gray-300 bg-white bg-opacity-80 backdrop-blur-sm">
        <h2 className="font-semibold text-gray-800 truncate text-lg">
          {currentConversation?.title || 'New Conversation'}
        </h2>
      </div>
      
      {/* Messages area - Fixed height to ensure input stays at bottom */}
      <div className="flex-1 overflow-y-auto p-6" style={{ height: 'calc(100vh - 200px)' }}>
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
      
      {/* Input area - Fixed at bottom */}
      <div className="p-6 border-t border-gray-300 bg-white bg-opacity-90 backdrop-blur-sm">
        <form onSubmit={handleSubmit} className="flex items-center gap-3">
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="Type your message..."
            className="flex-1 py-3 px-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-gray-500 focus:border-transparent bg-white shadow-sm"
            disabled={isLoading}
          />
          <button
            type="submit"
            disabled={!inputValue.trim() || isLoading}
            className={`p-3 rounded-lg transition-all duration-200 ${
              !inputValue.trim() || isLoading
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-gradient-to-r from-gray-500 to-gray-600 text-white hover:from-gray-600 hover:to-gray-700 shadow-md hover:shadow-lg transform hover:scale-105'
            }`}
          >
            <Send size={20} />
          </button>
        </form>
        {isLoading && (
          <div className="text-xs text-gray-600 mt-2 flex items-center">
            <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-gray-600 mr-2"></div>
            AI is thinking...
          </div>
        )}
      </div>
    </div>
  );
};


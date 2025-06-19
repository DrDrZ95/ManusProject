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
            ? 'bg-gradient-to-r from-blue-500 to-indigo-600 text-white rounded-tr-none shadow-md' 
            : 'bg-white text-gray-800 rounded-tl-none border border-gray-200 shadow-sm'
        }`}
      >
        <div className="whitespace-pre-wrap">{message.content}</div>
        <div className={`text-xs mt-2 ${isUser ? 'text-blue-100' : 'text-gray-500'}`}>
          {new Date(message.timestamp).toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })}
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
      <div className="flex-1 flex flex-col items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 text-gray-600">
        <div className="text-center max-w-md p-6">
          <div className="mb-6">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-r from-blue-500 to-indigo-600 rounded-full flex items-center justify-center">
              <span className="text-3xl text-white">🤖</span>
            </div>
          </div>
          <h2 className="text-2xl font-semibold mb-3 text-indigo-700">欢迎使用 AI 智能助手</h2>
          <p className="mb-6 text-indigo-600">开始新对话或从侧边栏选择现有对话。</p>
          <button
            onClick={() => sendMessage("你好，我是新用户。你能帮我做什么？")}
            className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white py-3 px-6 rounded-lg transition-all duration-200 shadow-md hover:shadow-lg transform hover:scale-105 font-medium"
          >
            开始新对话
          </button>
        </div>
      </div>
    );
  }
  
  return (
    <div className="flex-1 flex flex-col h-full bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Chat header */}
      <div className="py-4 px-6 border-b border-indigo-200 bg-white bg-opacity-80 backdrop-blur-sm">
        <h2 className="font-semibold text-indigo-800 truncate text-lg">
          {currentConversation?.title || '新对话'}
        </h2>
      </div>
      
      {/* Messages area */}
      <div className="flex-1 overflow-y-auto p-6">
        {currentConversation?.messages.length === 0 ? (
          <div className="h-full flex items-center justify-center text-indigo-500">
            <p className="text-center">
              <span className="block text-4xl mb-2">💬</span>
              发送消息开始对话
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
      
      {/* Input area */}
      <div className="p-6 border-t border-indigo-200 bg-white bg-opacity-80 backdrop-blur-sm">
        <form onSubmit={handleSubmit} className="flex items-center gap-3">
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="输入您的消息..."
            className="flex-1 py-3 px-4 border border-indigo-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white shadow-sm"
            disabled={isLoading}
          />
          <button
            type="submit"
            disabled={!inputValue.trim() || isLoading}
            className={`p-3 rounded-lg transition-all duration-200 ${
              !inputValue.trim() || isLoading
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-gradient-to-r from-blue-500 to-indigo-600 text-white hover:from-blue-600 hover:to-indigo-700 shadow-md hover:shadow-lg transform hover:scale-105'
            }`}
          >
            <Send size={20} />
          </button>
        </form>
        {isLoading && (
          <div className="text-xs text-indigo-600 mt-2 flex items-center">
            <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-indigo-600 mr-2"></div>
            AI 正在思考中...
          </div>
        )}
      </div>
    </div>
  );
};


import { useContext, useRef, useEffect } from 'react';
import { Send } from 'lucide-react';
import { ChatContext, Message } from '../contexts/ChatContext';
import { useChatActions } from '../hooks/useChatActions';

// Message bubble component
const MessageBubble = ({ message }: { message: Message }) => {
  const isUser = message.role === 'user';
  
  return (
    <div className={`flex ${isUser ? 'justify-end' : 'justify-start'} mb-4`}>
      <div 
        className={`max-w-[80%] rounded-lg px-4 py-2 ${
          isUser 
            ? 'bg-silver-500 text-white rounded-tr-none' 
            : 'bg-gray-100 text-gray-800 rounded-tl-none border border-gray-200'
        }`}
      >
        <div className="whitespace-pre-wrap">{message.content}</div>
        <div className={`text-xs mt-1 ${isUser ? 'text-silver-200' : 'text-gray-500'}`}>
          {new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
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
      <div className="flex-1 flex flex-col items-center justify-center bg-gray-50 text-gray-500">
        <div className="text-center max-w-md p-6">
          <h2 className="text-2xl font-semibold mb-2 text-silver-700">Welcome to AI Agent Chat</h2>
          <p className="mb-6">Start a new conversation or select an existing one from the sidebar.</p>
          <button
            onClick={() => sendMessage("Hello, I'm new here. What can you help me with?")}
            className="bg-silver-500 hover:bg-silver-600 text-white py-2 px-4 rounded-md transition-colors"
          >
            Start a new conversation
          </button>
        </div>
      </div>
    );
  }
  
  return (
    <div className="flex-1 flex flex-col h-full bg-white">
      {/* Chat header */}
      <div className="py-3 px-4 border-b border-gray-200">
        <h2 className="font-medium text-silver-800 truncate">
          {currentConversation?.title || 'New Conversation'}
        </h2>
      </div>
      
      {/* Messages area */}
      <div className="flex-1 overflow-y-auto p-4 bg-gray-50">
        {currentConversation?.messages.length === 0 ? (
          <div className="h-full flex items-center justify-center text-gray-500">
            <p>Send a message to start the conversation</p>
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
      <div className="p-4 border-t border-gray-200 bg-white">
        <form onSubmit={handleSubmit} className="flex items-center gap-2">
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="Type your message..."
            className="flex-1 py-2 px-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-silver-500 focus:border-transparent"
            disabled={isLoading}
          />
          <button
            type="submit"
            disabled={!inputValue.trim() || isLoading}
            className={`p-2 rounded-md ${
              !inputValue.trim() || isLoading
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-silver-500 text-white hover:bg-silver-600'
            } transition-colors`}
          >
            <Send size={20} />
          </button>
        </form>
        {isLoading && (
          <div className="text-xs text-gray-500 mt-2">
            AI is thinking...
          </div>
        )}
      </div>
    </div>
  );
};

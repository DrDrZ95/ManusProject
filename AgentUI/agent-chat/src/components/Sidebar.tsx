import { MessageSquare, Plus, Trash2 } from 'lucide-react';
import { useContext } from 'react';
import { ChatContext } from '../contexts/ChatContext';

export const Sidebar = () => {
  const { 
    conversations, 
    currentConversationId, 
    setCurrentConversationId, 
    createNewConversation,
    deleteConversation
  } = useContext(ChatContext);

  return (
    <aside className="h-screen w-64 bg-gray-100 border-r border-gray-200 flex flex-col">
      <div className="p-4 border-b border-gray-200">
        <button 
          onClick={createNewConversation}
          className="w-full flex items-center justify-center gap-2 bg-silver-500 hover:bg-silver-600 text-white py-2 px-4 rounded-md transition-colors"
        >
          <Plus size={18} />
          <span>New Chat</span>
        </button>
      </div>
      
      <div className="flex-1 overflow-y-auto p-2">
        <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2 px-2">
          Conversations
        </h2>
        
        {conversations.length === 0 ? (
          <div className="text-center py-6 text-gray-500 text-sm">
            No conversations yet
          </div>
        ) : (
          <ul className="space-y-1">
            {conversations.map((conversation) => (
              <li key={conversation.id}>
                <button
                  onClick={() => setCurrentConversationId(conversation.id)}
                  className={`w-full flex items-center gap-2 py-2 px-3 rounded-md text-left text-sm transition-colors ${
                    currentConversationId === conversation.id
                      ? 'bg-silver-200 text-silver-800'
                      : 'hover:bg-gray-200 text-gray-700'
                  }`}
                >
                  <MessageSquare size={16} className="flex-shrink-0" />
                  <span className="truncate flex-1">{conversation.title}</span>
                  {currentConversationId === conversation.id && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteConversation(conversation.id);
                      }}
                      className="opacity-0 group-hover:opacity-100 hover:text-red-500 transition-opacity"
                      aria-label="Delete conversation"
                    >
                      <Trash2 size={16} />
                    </button>
                  )}
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
      
      <div className="p-4 border-t border-gray-200">
        <div className="text-xs text-gray-500 text-center">
          AI Agent Chat Interface
        </div>
      </div>
    </aside>
  );
};

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
    <aside className="h-screen w-64 bg-sidebar-background text-sidebar-foreground border-r border-sidebar-border flex flex-col shadow-lg">
      <div className="p-4 border-b border-gray-300">
        <button 
          onClick={createNewConversation}
          className="w-full flex items-center justify-center gap-2 bg-sidebar-primary text-sidebar-primary-foreground py-3 px-4 rounded-lg transition-all duration-200 shadow-md hover:shadow-lg transform hover:scale-105"
        >
          <Plus size={18} />
          <span className="font-medium">New Chat</span>
        </button>
      </div>
      
      <div className="flex-1 overflow-y-auto p-2">
        <h2 className="text-xs font-semibold text-gray-600 uppercase tracking-wider mb-2 px-2">
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
                  className={`w-full flex items-center gap-2 py-3 px-3 rounded-lg text-left text-sm transition-all duration-200 group ${
                    currentConversationId === conversation.id
                      ? 'bg-sidebar-primary text-sidebar-primary-foreground shadow-md'
                      : 'hover:bg-sidebar-accent hover:text-sidebar-accent-foreground text-sidebar-foreground'
                  }`
                >
                  <MessageSquare size={16} className="flex-shrink-0" />
                  <span className="truncate flex-1 font-medium">{conversation.title}</span>
                  {currentConversationId === conversation.id && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteConversation(conversation.id);
                      }}
                      className="opacity-0 group-hover:opacity-100 hover:text-red-300 transition-all duration-200"
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
      
      <div className="p-4 border-t border-gray-300 bg-white bg-opacity-50">
        <div className="text-xs text-gray-600 text-center font-medium">
          AI Chat Assistant
        </div>
      </div>
    </aside>
  );
};


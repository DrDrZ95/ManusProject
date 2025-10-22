import { MessageSquare, Plus, Trash2, ChevronDown, Settings } from 'lucide-react';
import { useContext, useState } from 'react';
import { ChatContext } from '../contexts/ChatContext';

export const Sidebar = () => {
  const { 
    conversations, 
    currentConversationId, 
    setCurrentConversationId, 
    createNewConversation,
    deleteConversation
  } = useContext(ChatContext);

  const [isCollapsed, setIsCollapsed] = useState(false);

  return (
    <aside className="h-screen w-64 bg-white text-gray-900 border-r border-gray-200 flex flex-col transition-all duration-300">
      {/* Header */}
      <div className="p-4 border-b border-gray-100">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-md bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center">
              <span className="text-white font-bold text-sm">A</span>
            </div>
            <span className="font-semibold text-gray-900">Agent</span>
          </div>
        </div>
        
        {/* New Chat Button */}
        <button 
          onClick={createNewConversation}
          className="w-full flex items-center justify-center gap-2 bg-gray-100 hover:bg-gray-200 text-gray-900 py-2 px-3 rounded-md transition-colors duration-200 text-sm font-medium"
        >
          <Plus size={16} />
          <span>New Chat</span>
        </button>
      </div>
      
      {/* Conversations Section */}
      <div className="flex-1 overflow-y-auto p-3">
        <div className="mb-3">
          <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider px-2 py-1">
            Conversations
          </h2>
        </div>
        
        {conversations.length === 0 ? (
          <div className="text-center py-8 text-gray-400 text-sm">
            <MessageSquare size={24} className="mx-auto mb-2 opacity-50" />
            <p>No conversations yet</p>
          </div>
        ) : (
          <ul className="space-y-1">
            {conversations.map((conversation) => (
              <li key={conversation.id}>
                <button
                  onClick={() => setCurrentConversationId(conversation.id)}
                  className={`w-full flex items-center gap-2 py-2 px-3 rounded-md text-left text-sm transition-all duration-150 group ${
                    currentConversationId === conversation.id
                      ? 'bg-gray-100 text-gray-900 font-medium'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`
                }
                >
                  <MessageSquare size={14} className="flex-shrink-0 opacity-60" />
                  <span className="truncate flex-1">{conversation.title}</span>
                  {currentConversationId === conversation.id && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteConversation(conversation.id);
                      }}
                      className="opacity-0 group-hover:opacity-100 hover:text-red-500 transition-all duration-150 p-1"
                      aria-label="Delete conversation"
                    >
                      <Trash2 size={14} />
                    </button>
                  )}
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
      
      {/* Footer */}
      <div className="p-3 border-t border-gray-100 bg-gray-50">
        <button className="w-full flex items-center justify-center gap-2 text-gray-600 hover:text-gray-900 py-2 px-3 rounded-md hover:bg-gray-100 transition-colors text-sm">
          <Settings size={14} />
          <span>Settings</span>
        </button>
      </div>
    </aside>
  );
};


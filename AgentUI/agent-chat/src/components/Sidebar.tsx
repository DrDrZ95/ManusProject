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
    <aside className="h-screen w-64 bg-gradient-to-b from-blue-50 to-indigo-100 border-r border-indigo-200 flex flex-col shadow-lg">
      <div className="p-4 border-b border-indigo-200">
        <button 
          onClick={createNewConversation}
          className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white py-3 px-4 rounded-lg transition-all duration-200 shadow-md hover:shadow-lg transform hover:scale-105"
        >
          <Plus size={18} />
          <span className="font-medium">新建对话</span>
        </button>
      </div>
      
      <div className="flex-1 overflow-y-auto p-2">
        <h2 className="text-xs font-semibold text-indigo-600 uppercase tracking-wider mb-2 px-2">
          对话列表
        </h2>
        
        {conversations.length === 0 ? (
          <div className="text-center py-6 text-indigo-500 text-sm">
            暂无对话记录
          </div>
        ) : (
          <ul className="space-y-1">
            {conversations.map((conversation) => (
              <li key={conversation.id}>
                <button
                  onClick={() => setCurrentConversationId(conversation.id)}
                  className={`w-full flex items-center gap-2 py-3 px-3 rounded-lg text-left text-sm transition-all duration-200 group ${
                    currentConversationId === conversation.id
                      ? 'bg-gradient-to-r from-indigo-500 to-blue-600 text-white shadow-md'
                      : 'hover:bg-white hover:shadow-md text-indigo-700'
                  }`}
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
                      aria-label="删除对话"
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
      
      <div className="p-4 border-t border-indigo-200 bg-white bg-opacity-50">
        <div className="text-xs text-indigo-600 text-center font-medium">
          AI 智能对话助手
        </div>
      </div>
    </aside>
  );
};


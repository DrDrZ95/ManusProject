import { createContext, useState, ReactNode } from 'react';

export interface Message {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

export interface Conversation {
  id: string;
  title: string;
  messages: Message[];
  createdAt: Date;
  updatedAt: Date;
}

interface ChatContextType {
  conversations: Conversation[];
  currentConversationId: string | null;
  setCurrentConversationId: (id: string | null) => void;
  addMessage: (conversationId: string, role: 'user' | 'assistant', content: string) => void;
  createNewConversation: () => string;
  deleteConversation: (id: string) => void;
  renameConversation: (id: string, newTitle: string) => void;
}

export const ChatContext = createContext<ChatContextType>({
  conversations: [],
  currentConversationId: null,
  setCurrentConversationId: () => {},
  addMessage: () => {},
  createNewConversation: () => '',
  deleteConversation: () => {},
  renameConversation: () => {},
});

interface ChatProviderProps {
  children: ReactNode;
}

export const ChatProvider = ({ children }: ChatProviderProps) => {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [currentConversationId, setCurrentConversationId] = useState<string | null>(null);

  const createNewConversation = () => {
    const id = `conv-${Date.now()}`;
    const newConversation: Conversation = {
      id,
      title: 'New Conversation',
      messages: [],
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    setConversations((prev) => [...prev, newConversation]);
    setCurrentConversationId(id);
    return id;
  };

  const addMessage = (conversationId: string, role: 'user' | 'assistant', content: string) => {
    const message: Message = {
      id: `msg-${Date.now()}`,
      role,
      content,
      timestamp: new Date(),
    };

    setConversations((prev) =>
      prev.map((conv) => {
        if (conv.id === conversationId) {
          return {
            ...conv,
            messages: [...conv.messages, message],
            updatedAt: new Date(),
            // Update title based on first user message if it's still the default
            title: conv.title === 'New Conversation' && role === 'user' && conv.messages.length === 0
              ? content.substring(0, 30) + (content.length > 30 ? '...' : '')
              : conv.title,
          };
        }
        return conv;
      })
    );
  };

  const deleteConversation = (id: string) => {
    setConversations((prev) => prev.filter((conv) => conv.id !== id));
    if (currentConversationId === id) {
      const remaining = conversations.filter((conv) => conv.id !== id);
      setCurrentConversationId(remaining.length > 0 ? remaining[0].id : null);
    }
  };

  const renameConversation = (id: string, newTitle: string) => {
    setConversations((prev) =>
      prev.map((conv) => {
        if (conv.id === id) {
          return { ...conv, title: newTitle };
        }
        return conv;
      })
    );
  };

  return (
    <ChatContext.Provider
      value={{
        conversations,
        currentConversationId,
        setCurrentConversationId,
        addMessage,
        createNewConversation,
        deleteConversation,
        renameConversation,
      }}
    >
      {children}
    </ChatContext.Provider>
  );
};

import { useContext, useState } from 'react';
import { ChatContext } from '../contexts/ChatContext';

export const useChatActions = () => {
  const { 
    conversations, 
    currentConversationId, 
    addMessage, 
    createNewConversation 
  } = useContext(ChatContext);
  
  const [isLoading, setIsLoading] = useState(false);

  const sendMessage = async (message: string) => {
    if (!message.trim()) return;
    
    let conversationId = currentConversationId;
    
    // Create a new conversation if none exists
    if (!conversationId) {
      conversationId = createNewConversation();
    }
    
    // Add user message
    addMessage(conversationId, 'user', message);
    
    // Simulate API call to get assistant response
    setIsLoading(true);
    
    try {
      // In a real app, this would be an API call to your backend
      // For now, we'll simulate a delay and a mock response
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // Mock response - in a real app, this would come from your API
      const mockResponses = [
        "I'm an AI assistant. How can I help you today?",
        "That's an interesting question. Let me think about that...",
        "I understand your request. Here's what I can tell you about that topic.",
        "Thanks for sharing. Would you like to know more about this subject?",
        "I'm processing your request. Is there anything specific you'd like to focus on?"
      ];
      
      const randomResponse = mockResponses[Math.floor(Math.random() * mockResponses.length)];
      addMessage(conversationId, 'assistant', randomResponse);
    } catch (error) {
      console.error('Error getting response:', error);
      addMessage(conversationId, 'assistant', 'Sorry, I encountered an error processing your request.');
    } finally {
      setIsLoading(false);
    }
  };

  const getCurrentConversation = () => {
    if (!currentConversationId) return null;
    return conversations.find(conv => conv.id === currentConversationId) || null;
  };

  return {
    sendMessage,
    isLoading,
    getCurrentConversation
  };
};


import { create } from 'zustand';
import { AppState, Message, Role, ChatSession, Attachment, LoginRequest } from './types';
import { v4 as uuidv4 } from 'uuid';
import { generateNews, shouldFetchNews } from './services/news';
import { MOCK_SESSIONS } from './data/mockData';
import { authApi } from './services/api';

// Removed STORAGE_KEY to prevent persistence

const createInitialSession = (): ChatSession => ({
  id: uuidv4(),
  title: 'New Chat',
  messages: [],
  updatedAt: Date.now(),
});

// Default initial state without loading from localStorage
const initialState = {
  sessions: MOCK_SESSIONS,
  currentSessionId: MOCK_SESSIONS[0].id,
  isAuthenticated: true // Default to true for simulation
};

export const useStore = create<AppState>((set, get) => {
  return {
    isAuthenticated: initialState.isAuthenticated,
    user: {
      id: 'user-123',
      name: 'Agent User',
      email: 'user@example.com',
      avatar: 'https://api.dicebear.com/9.x/adventurer/svg?seed=Felix',
      role: 'admin',
      bio: ''
    },
    settings: {
      streamResponses: true,
      soundEffects: true,
      allowTraining: false
    },
    sessions: initialState.sessions,
    groups: [],
    currentSessionId: initialState.currentSessionId,
    input: '',
    attachments: [],
    isLoading: false,
    selectedModel: 'kimi',
    isSidebarOpen: true,
    isTerminalOpen: true,
    terminalWidth: 380,
    language: 'en',
    activeModal: null,
    inputMode: 'general',
    isAgentMode: false,
    
    news: [],
    lastNewsFetch: 0,

    login: async (credentials: LoginRequest) => {
      try {
        const user = await authApi.login(credentials);
        // Preserve existing user settings/avatar if re-logging in within session
        const existingUser = get().user;
        set({ 
          isAuthenticated: true, 
          user: { ...user, ...existingUser, ...user } // merge but prioritize API response initially, realistically
        });
      } catch (error) {
        console.error("Login failed", error);
        throw error; 
      }
    },
    
    logout: async () => {
      await authApi.logout();
      set({ isAuthenticated: false, user: null });
    },

    updateUser: (updates) => set(state => ({
      user: state.user ? { ...state.user, ...updates } : null
    })),

    updateSettings: (updates) => set(state => ({
      settings: { ...state.settings, ...updates }
    })),

    setInput: (input) => set({ input }),

    addAttachment: async (file: File) => {
      return new Promise<void>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
          const base64String = reader.result as string;
          const base64Data = base64String.split(',')[1];
          
          const newAttachment: Attachment = {
            id: uuidv4(),
            name: file.name,
            type: 'file',
            mimeType: file.type,
            data: base64Data
          };
          
          set(state => ({ attachments: [...state.attachments, newAttachment] }));
          resolve();
        };
        reader.onerror = reject;
        reader.readAsDataURL(file);
      });
    },

    removeAttachment: (id) => set(state => ({
      attachments: state.attachments.filter(a => a.id !== id)
    })),

    clearAttachments: () => set({ attachments: [] }),

    addMessage: (sessionId, message) => set((state) => {
      const sessionIndex = state.sessions.findIndex(s => s.id === sessionId);
      if (sessionIndex === -1) return state;

      const updatedSessions = [...state.sessions];
      const currentSession = updatedSessions[sessionIndex];
      const newMessages = [...currentSession.messages, message];

      updatedSessions[sessionIndex] = {
        ...currentSession,
        messages: newMessages,
        updatedAt: Date.now(),
        title: currentSession.messages.length === 0 && message.role === Role.USER 
          ? message.content.slice(0, 30) + (message.content.length > 30 ? '...' : '')
          : currentSession.title
      };

      return { sessions: updatedSessions };
    }),

    updateLastMessage: (sessionId, content) => set((state) => {
      const sessionIndex = state.sessions.findIndex(s => s.id === sessionId);
      if (sessionIndex === -1) return state;

      const updatedSessions = [...state.sessions];
      const messages = [...updatedSessions[sessionIndex].messages];
      const lastMsgIndex = messages.length - 1;

      if (lastMsgIndex >= 0) {
        messages[lastMsgIndex] = {
          ...messages[lastMsgIndex],
          content: content,
          isStreaming: true 
        };
      }

      updatedSessions[sessionIndex] = {
        ...updatedSessions[sessionIndex],
        messages,
        updatedAt: Date.now()
      };

      return { sessions: updatedSessions };
    }),

    createNewSession: () => set((state) => {
      const newSession = createInitialSession();
      return {
        sessions: [newSession, ...state.sessions],
        currentSessionId: newSession.id,
        input: '', 
        attachments: [],
        isAgentMode: false // Reset Agent mode on new session
      };
    }),

    selectSession: (id) => set({ currentSessionId: id, attachments: [] }),

    navigateToHome: () => set((state) => {
      const firstSession = state.sessions[0];
      if (firstSession && firstSession.messages.length === 0) {
        return { currentSessionId: firstSession.id, input: '', attachments: [] };
      }

      const newSession = createInitialSession();
      return {
        sessions: [newSession, ...state.sessions],
        currentSessionId: newSession.id,
        input: '', 
        attachments: [],
        isAgentMode: false
      };
    }),

    createGroup: (id, title) => set((state) => {
      if (state.groups.length >= 10) return state;
      return { 
        groups: [...state.groups, { id, title, collapsed: false }] 
      };
    }),

    updateGroup: (id, updates) => set((state) => ({
      groups: state.groups.map(g => g.id === id ? { ...g, ...updates } : g)
    })),

    deleteGroup: (id) => set((state) => ({
      groups: state.groups.filter(g => g.id !== id),
      sessions: state.sessions.map(s => s.groupId === id ? { ...s, groupId: undefined } : s)
    })),

    moveSession: (sessionId, groupId) => set((state) => ({
      sessions: state.sessions.map(s => s.id === sessionId ? { ...s, groupId } : s)
    })),

    renameSession: (sessionId, newTitle) => set((state) => ({
      sessions: state.sessions.map(s => s.id === sessionId ? { ...s, title: newTitle } : s)
    })),

    deleteSession: (sessionId) => set((state) => {
      const newSessions = state.sessions.filter(s => s.id !== sessionId);
      let nextSessionId = state.currentSessionId;
      if (state.currentSessionId === sessionId) {
        nextSessionId = newSessions.length > 0 ? newSessions[0].id : null;
      }
      
      if (newSessions.length === 0) {
         const freshSession = createInitialSession();
         return { sessions: [freshSession], currentSessionId: freshSession.id };
      }

      return { sessions: newSessions, currentSessionId: nextSessionId };
    }),

    fetchNews: () => set((state) => {
      if (shouldFetchNews(state.lastNewsFetch) || state.news.length === 0) {
        return {
          news: generateNews(),
          lastNewsFetch: Date.now()
        };
      }
      return {};
    }),

    toggleSidebar: () => set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),
    toggleTerminal: () => set((state) => ({ isTerminalOpen: !state.isTerminalOpen })),
    setTerminalWidth: (width) => set({ terminalWidth: width }),
    setModel: (model) => set({ selectedModel: model }),
    setLoading: (loading) => set({ isLoading: loading }),
    setLanguage: (lang) => set({ language: lang }),
    setActiveModal: (modal) => set({ activeModal: modal }),
    setInputMode: (mode) => set({ inputMode: mode }),
    setAgentMode: (enabled) => set({ isAgentMode: enabled }),
  };
});

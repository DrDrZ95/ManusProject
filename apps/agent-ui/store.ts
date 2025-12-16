
import { create } from 'zustand';
import { AppState, Message, Role, ChatSession, Attachment, LoginRequest } from './types';
import { v4 as uuidv4 } from 'uuid';
import { api } from './services/api'; // Use the new API Facade
import { tokenManager } from './services/tokenManager';

// Mock initial data still used for bootstrapping UI state if API returns empty in demo
import { MOCK_SESSIONS } from './data/mockData';

const createInitialSession = (): ChatSession => ({
  id: uuidv4(),
  title: 'New Chat',
  messages: [],
  updatedAt: Date.now(),
});

// Initial auth check
const hasExistingToken = tokenManager.hasToken();

export const useStore = create<AppState>((set, get) => {
  return {
    isAuthenticated: hasExistingToken,
    user: null, // User is fetched after login/hydration
    settings: {
      streamResponses: true,
      soundEffects: true,
      allowTraining: false
    },
    sessions: MOCK_SESSIONS,
    groups: [],
    currentSessionId: MOCK_SESSIONS[0].id,
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

    // --- Actions using Service Layer ---

    login: async (credentials: LoginRequest) => {
      try {
        // Call Auth Service
        const user = await api.auth.login(credentials);
        
        set({ 
          isAuthenticated: true, 
          user: user
        });

        // Fetch User Profile & Initial Data
        // get().fetchNews(); // Triggered in App.tsx effects usually, but can be here
      } catch (error) {
        console.error("Login failed", error);
        throw error; 
      }
    },
    
    logout: async () => {
      await api.auth.logout();
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
      try {
        // Use File Service
        const attachment = await api.file.upload(file);
        set(state => ({ attachments: [...state.attachments, attachment] }));
      } catch (e) {
        console.error("Upload failed", e);
        alert("Upload failed");
      }
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

    createNewSession: async () => {
      // Use Chat Service
      // const newSession = await api.chat.createSession('New Chat'); 
      // For instant UI feedback we usually do optimistic updates or synchronous mock creation
      const newSession = createInitialSession();
      set((state) => ({
        sessions: [newSession, ...state.sessions],
        currentSessionId: newSession.id,
        input: '', 
        attachments: [],
        isAgentMode: false 
      }));
    },

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

    deleteSession: async (sessionId) => {
      // await api.chat.deleteSession(sessionId); // Async call to server
      set((state) => {
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
      });
    },

    fetchNews: async () => {
      const state = get();
      // Simple cache check
      if (Date.now() - state.lastNewsFetch > 3600 * 1000 * 6 || state.news.length === 0) {
        try {
            const news = await api.news.fetchLatest();
            set({ news, lastNewsFetch: Date.now() });
        } catch (e) {
            console.warn("Failed to fetch news", e);
        }
      }
    },

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

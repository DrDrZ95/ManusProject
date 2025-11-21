
import { create } from 'zustand';
import { AppState, Message, Role, ChatSession, Attachment, LoginRequest } from './types';
import { v4 as uuidv4 } from 'uuid';
import { generateNews, shouldFetchNews } from './services/news';
import { MOCK_SESSIONS } from './data/mockData';
import { authApi } from './services/api';

const STORAGE_KEY = 'grok-app-storage-v1';

const createInitialSession = (): ChatSession => ({
  id: uuidv4(),
  title: 'New Chat',
  messages: [],
  updatedAt: Date.now(),
});

// Load state from local storage
const loadState = (): Partial<AppState> => {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      const parsed = JSON.parse(saved);
      // Hydrate with mock sessions if the user has no real sessions saved (simulating DB connection)
      if (!parsed.sessions || parsed.sessions.length <= 1 && parsed.sessions[0].messages.length === 0) {
          return { ...parsed, sessions: MOCK_SESSIONS, currentSessionId: MOCK_SESSIONS[0].id };
      }
      // Default isAuthenticated to true if missing (backward compatibility)
      if (parsed.isAuthenticated === undefined) {
        parsed.isAuthenticated = true;
      }
      return parsed;
    }
  } catch (e) {
    console.error("Failed to load state", e);
  }
  // Default Fallback: Load Mock Data
  return {
      sessions: MOCK_SESSIONS,
      currentSessionId: MOCK_SESSIONS[0].id,
      isAuthenticated: true // Default to true as per requirements
  };
};

const initialState = loadState();

export const useStore = create<AppState>((set, get) => {
  return {
    isAuthenticated: initialState.isAuthenticated ?? true,
    user: initialState.user || null,
    sessions: initialState.sessions || MOCK_SESSIONS,
    groups: initialState.groups || [],
    currentSessionId: initialState.currentSessionId || MOCK_SESSIONS[0].id,
    input: '',
    attachments: [],
    isLoading: false,
    selectedModel: (initialState.selectedModel as any) || 'kimi',
    isSidebarOpen: initialState.isSidebarOpen ?? true,
    isTerminalOpen: initialState.isTerminalOpen ?? true,
    terminalWidth: initialState.terminalWidth || 380,
    language: initialState.language || 'en',
    activeModal: null,
    inputMode: (initialState.inputMode as any) || 'general',
    
    news: initialState.news || [],
    lastNewsFetch: initialState.lastNewsFetch || 0,

    login: async (credentials: LoginRequest) => {
      try {
        const user = await authApi.login(credentials);
        set({ isAuthenticated: true, user });
      } catch (error) {
        console.error("Login failed", error);
        throw error; // Re-throw for UI to handle
      }
    },
    
    logout: async () => {
      await authApi.logout();
      set({ isAuthenticated: false, user: null });
    },

    setInput: (input) => set({ input }),

    addAttachment: async (file: File) => {
      return new Promise<void>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
          const base64String = reader.result as string;
          // Extract base64 data part
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
        attachments: []
      };
    }),

    selectSession: (id) => set({ currentSessionId: id, attachments: [] }),

    navigateToHome: () => set((state) => {
      // If the first session is empty (no messages), just go there.
      // This prevents spamming "New Chat" every time user clicks logo.
      const firstSession = state.sessions[0];
      if (firstSession && firstSession.messages.length === 0) {
        return { currentSessionId: firstSession.id, input: '', attachments: [] };
      }

      // Otherwise create a new one
      const newSession = createInitialSession();
      return {
        sessions: [newSession, ...state.sessions],
        currentSessionId: newSession.id,
        input: '', 
        attachments: []
      };
    }),

    // Group & Session Management
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

    // News Logic
    fetchNews: () => set((state) => {
      if (shouldFetchNews(state.lastNewsFetch)) {
        return {
          news: generateNews(),
          lastNewsFetch: Date.now()
        };
      }
      // If existing news is empty for some reason, fetch anyway
      if (state.news.length === 0) {
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
  };
});

// Subscribe to store changes and persist to localStorage
useStore.subscribe((state) => {
  const stateToSave: Partial<AppState> = {
    isAuthenticated: state.isAuthenticated, // Persist Auth State
    user: state.user,
    sessions: state.sessions,
    groups: state.groups,
    currentSessionId: state.currentSessionId,
    terminalWidth: state.terminalWidth,
    language: state.language,
    selectedModel: state.selectedModel,
    isSidebarOpen: state.isSidebarOpen,
    isTerminalOpen: state.isTerminalOpen,
    news: state.news,
    lastNewsFetch: state.lastNewsFetch,
    inputMode: state.inputMode
  };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(stateToSave));
});

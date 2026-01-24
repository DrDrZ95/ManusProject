
import { create } from 'zustand';
import { AppState, Message, Role, ChatSession, Attachment, LoginRequest, Group } from './types';
import { v4 as uuidv4 } from 'uuid';
import { api } from './services/api';
import { tokenManager } from './services/tokenManager';
import { MOCK_SESSIONS } from './data/mockData';

const DEFAULT_AVATAR = 'https://api.dicebear.com/9.x/lorelei/svg?seed=Jasper';

const createInitialSession = (): ChatSession => ({
  id: uuidv4(),
  title: 'New Chat',
  messages: [],
  updatedAt: Date.now(),
});

const PROJECT_COLORS = [
  'bg-blue-500', 'bg-emerald-500', 'bg-violet-500', 'bg-amber-500',
  'bg-rose-500', 'bg-cyan-500', 'bg-indigo-500', 'bg-teal-500',
  'bg-orange-500', 'bg-fuchsia-500', 'bg-slate-500'
];

const MARKER_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';

const hasExistingToken = tokenManager.hasToken();

// Extend AppState with sidebar width for resizability
interface ExtendedAppState extends AppState {
  sidebarWidth: number;
  setSidebarWidth: (width: number) => void;
}

export const useStore = create<ExtendedAppState>((set, get) => {
  return {
    isAuthenticated: hasExistingToken,
    user: hasExistingToken ? {
      id: 'user-123',
      name: 'Agent User',
      email: 'user@example.com',
      avatar: DEFAULT_AVATAR,
      role: 'admin',
      bio: 'Full Stack Engineer @ Agent Corp'
    } : null,
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
    sidebarWidth: 280,
    isTerminalOpen: true,
    terminalWidth: 380,
    language: 'zh', 
    activeModal: null,
    editingProjectId: null,
    inputMode: 'general',
    isAgentMode: false,
    news: [],
    lastNewsFetch: 0,
    toast: null,
    isSearchOpen: false,

    login: async (credentials: LoginRequest) => {
      try {
        const user = await api.auth.login(credentials);
        set({ isAuthenticated: true, user: user });
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
        messages[lastMsgIndex] = { ...messages[lastMsgIndex], content: content, isStreaming: true };
      }
      updatedSessions[sessionIndex] = { ...updatedSessions[sessionIndex], messages, updatedAt: Date.now() };
      return { sessions: updatedSessions };
    }),

    createNewSession: async () => {
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
      const firstSession = state.sessions.find(s => !s.isDeleted);
      if (firstSession && firstSession.messages.length === 0) {
        return { currentSessionId: firstSession.id, input: '', attachments: [] };
      }
      const newSession = createInitialSession();
      return { sessions: [newSession, ...state.sessions], currentSessionId: newSession.id, input: '', attachments: [], isAgentMode: false };
    }),

    createGroup: (id, title) => set((state) => {
      if (state.groups.filter(g => !g.isDeleted).length >= 10) {
        return { activeModal: 'quota_limit' };
      }
      const randomColor = PROJECT_COLORS[Math.floor(Math.random() * PROJECT_COLORS.length)];
      const randomMarker = MARKER_CHARS[Math.floor(Math.random() * MARKER_CHARS.length)];
      
      // Changed collapsed to true by default
      return { groups: [...state.groups, { id, title, marker: randomMarker, color: randomColor, collapsed: true }] };
    }),

    updateGroup: (id, updates) => set((state) => ({
      groups: state.groups.map(g => g.id === id ? { ...g, ...updates } : g)
    })),

    deleteGroup: (id) => set((state) => {
      const groupName = state.groups.find(g => g.id === id)?.title || 'Project';
      return {
        groups: state.groups.map(g => g.id === id ? { ...g, isDeleted: true } : g),
        sessions: state.sessions.map(s => s.groupId === id ? { ...s, groupId: undefined } : s),
        toast: { message: `"${groupName}" soft deleted. Action is recoverable in simulation.`, type: 'info' }
      };
    }),

    moveSession: (sessionId, groupId) => set((state) => ({
      sessions: state.sessions.map(s => s.id === sessionId ? { ...s, groupId } : s)
    })),

    renameSession: (sessionId, newTitle) => set((state) => ({
      sessions: state.sessions.map(s => s.id === sessionId ? { ...s, title: newTitle } : s)
    })),

    deleteSession: async (sessionId) => {
      set((state) => {
        const sessionTitle = state.sessions.find(s => s.id === sessionId)?.title || 'Chat';
        const updatedSessions = state.sessions.map(s => s.id === sessionId ? { ...s, isDeleted: true } : s);
        
        let nextSessionId = state.currentSessionId;
        if (state.currentSessionId === sessionId) {
          const visible = updatedSessions.filter(s => !s.isDeleted);
          nextSessionId = visible.length > 0 ? visible[0].id : null;
        }

        if (updatedSessions.filter(s => !s.isDeleted).length === 0) {
           const freshSession = createInitialSession();
           return { 
             sessions: [freshSession, ...updatedSessions], 
             currentSessionId: freshSession.id,
             toast: { message: `"${sessionTitle}" soft deleted. Action is recoverable.`, type: 'info' }
           };
        }

        return { 
          sessions: updatedSessions, 
          currentSessionId: nextSessionId,
          toast: { message: `"${sessionTitle}" soft deleted. Action is recoverable.`, type: 'info' }
        };
      });
    },

    fetchNews: async () => {
      const state = get();
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
    setSidebarWidth: (width) => set({ sidebarWidth: width }),
    toggleTerminal: () => set((state) => ({ isTerminalOpen: !state.isTerminalOpen })),
    setTerminalWidth: (width) => set({ terminalWidth: width }),
    setModel: (model) => set({ selectedModel: model }),
    setLoading: (loading) => set({ isLoading: loading }),
    setLanguage: (lang) => set({ language: lang }),
    setActiveModal: (modal) => set({ activeModal: modal }),
    setEditingProject: (id) => set({ editingProjectId: id }),
    setInputMode: (mode) => set({ inputMode: mode }),
    setAgentMode: (enabled) => set({ isAgentMode: enabled }),
    setToast: (toast) => set({ toast }),
    toggleSearch: () => set((state) => ({ isSearchOpen: !state.isSearchOpen })),
  };
});

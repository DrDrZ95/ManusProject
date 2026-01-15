
import { Language } from './locales';

export enum Role {
  USER = 'user',
  MODEL = 'model'
}

export interface Message {
  id: string;
  role: Role;
  content: string;
  timestamp: number;
  isStreaming?: boolean;
  attachments?: Attachment[];
  mode?: InputMode;
}

export interface ChatSession {
  id: string;
  title: string;
  messages: Message[];
  updatedAt: number;
  groupId?: string;
  isDeleted?: boolean; // Added for Soft Delete
}

export interface Group {
  id: string;
  title: string;
  collapsed: boolean;
  marker: string; // One character or icon name
  color: string;  // Tailwind color class (e.g., 'bg-blue-500')
  isDeleted?: boolean; // Added for Soft Delete
}

export interface Attachment {
  id: string;
  name: string;
  type: 'image' | 'file';
  mimeType: string;
  data: string; // base64
}

export interface NewsItem {
  id: string;
  title: string;
  summary: string;
  thumbnailUrl: string;
  url: string;
  category: 'IT' | 'Finance' | 'AI';
  timestamp: number;
}

export type ModelType = 'kimi' | 'deepseek' | 'gpt-oss';
export type ModalType = 'upgrade' | 'account' | 'help' | 'settings' | 'project_edit' | 'quota_limit' | null;
export type InputMode = 'general' | 'brainstorm' | 'oa_work' | 'company' | 'agent';

export interface User {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  role?: string;
  bio?: string;
}

export interface LoginRequest {
  email?: string;
  phone?: string;
  password?: string;
  provider?: 'google' | 'outlook' | 'credentials';
}

export interface AuthResponse {
  user: User;
  token: string;
  refreshToken: string;
  expiresIn: number;
}

export interface Settings {
  streamResponses: boolean;
  soundEffects: boolean;
  allowTraining: boolean;
}

/**
 * Model Context Protocol (MCP) Interfaces
 * These interfaces were missing and causing compilation errors in services/mcp.ts
 */
export interface McpTool {
  name: string;
  description: string;
  inputSchema: {
    type: string;
    properties: Record<string, any>;
    required?: string[];
  };
}

export interface McpResource {
  uri: string;
  name: string;
  mimeType: string;
}

export interface McpToolCallRequest {
  name: string;
  arguments: Record<string, any>;
}

export interface McpToolCallResponse {
  content: Array<{
    type: string;
    text: string;
  }>;
  isError?: boolean;
}

export interface AppState {
  isAuthenticated: boolean;
  user: User | null;
  settings: Settings;
  sessions: ChatSession[];
  groups: Group[];
  currentSessionId: string | null;
  input: string;
  attachments: Attachment[];
  isLoading: boolean;
  selectedModel: ModelType;
  language: Language;
  inputMode: InputMode;
  isAgentMode: boolean;
  news: NewsItem[];
  lastNewsFetch: number;
  isSidebarOpen: boolean;
  isTerminalOpen: boolean;
  terminalWidth: number;
  activeModal: ModalType;
  editingProjectId: string | null;
  toast: { message: string; type: 'info' | 'success' | 'warning' } | null; // Toast state
  isSearchOpen: boolean; // Search Modal State
  
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  updateUser: (updates: Partial<User>) => void;
  updateSettings: (updates: Partial<Settings>) => void;
  setInput: (input: string) => void;
  addAttachment: (file: File) => Promise<void>;
  removeAttachment: (id: string) => void;
  clearAttachments: () => void;
  addMessage: (sessionId: string, message: Message) => void;
  updateLastMessage: (sessionId: string, content: string) => void;
  createNewSession: () => void;
  selectSession: (id: string) => void;
  navigateToHome: () => void;
  createGroup: (id: string, title: string, marker?: string, color?: string) => void;
  updateGroup: (id: string, updates: Partial<Group>) => void;
  deleteGroup: (id: string) => void;
  moveSession: (sessionId: string, groupId: string | undefined) => void;
  renameSession: (sessionId: string, newTitle: string) => void;
  deleteSession: (sessionId: string) => void;
  fetchNews: () => void;
  toggleSidebar: () => void;
  toggleTerminal: () => void;
  setTerminalWidth: (width: number) => void;
  setModel: (model: ModelType) => void;
  setLoading: (loading: boolean) => void;
  setLanguage: (lang: Language) => void;
  setActiveModal: (modal: ModalType) => void;
  setEditingProject: (id: string | null) => void;
  setInputMode: (mode: InputMode) => void;
  setAgentMode: (enabled: boolean) => void;
  setToast: (toast: { message: string; type: 'info' | 'success' | 'warning' } | null) => void;
  toggleSearch: () => void;
}
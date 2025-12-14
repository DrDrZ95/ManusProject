
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
}

export interface Group {
  id: string;
  title: string;
  collapsed: boolean;
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
export type ModalType = 'upgrade' | 'account' | 'help' | 'settings' | null;
export type InputMode = 'general' | 'brainstorm' | 'oa_work' | 'company' | 'agent';

// --- Auth & API Types ---

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

// --- MCP (Model Context Protocol) Types ---

export interface McpTool {
  name: string;
  description: string;
  inputSchema: Record<string, any>;
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
    type: 'text' | 'image' | 'resource';
    text?: string;
    data?: string;
    mimeType?: string;
  }>;
  isError?: boolean;
}

// --- State Types ---

export interface Settings {
  streamResponses: boolean;
  soundEffects: boolean;
  allowTraining: boolean;
}

export interface AppState {
  // Auth State
  isAuthenticated: boolean;
  user: User | null;
  settings: Settings;

  // Chat State
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
  
  // News State
  news: NewsItem[];
  lastNewsFetch: number;

  // UI State
  isSidebarOpen: boolean;
  isTerminalOpen: boolean;
  terminalWidth: number;
  activeModal: ModalType;
  
  // Actions
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
  
  // Group & Session Management Actions
  createGroup: (id: string, title: string) => void;
  updateGroup: (id: string, updates: Partial<Group>) => void;
  deleteGroup: (id: string) => void;
  moveSession: (sessionId: string, groupId: string | undefined) => void;
  renameSession: (sessionId: string, newTitle: string) => void;
  deleteSession: (sessionId: string) => void;

  // News Actions
  fetchNews: () => void;

  toggleSidebar: () => void;
  toggleTerminal: () => void;
  setTerminalWidth: (width: number) => void;
  setModel: (model: ModelType) => void;
  setLoading: (loading: boolean) => void;
  setLanguage: (lang: Language) => void;
  setActiveModal: (modal: ModalType) => void;
  setInputMode: (mode: InputMode) => void;
  setAgentMode: (enabled: boolean) => void;
}

export type Language = 'en' | 'zh';
export type ThemeMode = 'light' | 'dark';

export interface ProjectStatus {
  id: string;
  name: string;
  status: 'healthy' | 'warning' | 'critical' | 'deploying';
  uptime: string;
  lastDeployment: string;
  repo: string;
  address?: string; // Physical address
  coordinates?: [number, number]; // Lat/Long for map
  branch: string;
}

export interface MetricPoint {
  time: string;
  value: number;
  status?: 'success' | 'failure';
  details?: string;
}

export interface ClusterNode {
  id: string;
  name: string;
  role: 'master' | 'worker';
  cpu: number; // percentage
  memory: number; // percentage
  status: 'ready' | 'not-ready';
  pods: number;
}

export interface EbpfMetrics {
  httpThroughput: number; // requests per second
  tcpLatency: number; // ms
  tcpRetransmits: number;
  pid: number;
  lastSyscall: string;
  networkIn: number; // MB/s
  networkOut: number; // MB/s
}

export interface K8sPod {
  id: string;
  name: string;
  namespace: string;
  status: 'Running' | 'Pending' | 'Error';
  restarts: number;
  age: string;
  nodeId: string; // Linked Node ID
  logs: string[];
  connections: string[]; // IDs of pods this pod talks to
  ebpf?: EbpfMetrics; // Advanced observability data
}

export interface DataToolNode {
  id: string;
  name: string; // e.g., 'Elasticsearch-01'
  type: 'Elasticsearch' | 'ClickHouse' | 'Redis' | 'HBase';
  shards: number;
  status: 'green' | 'yellow' | 'red';
  diskUsage: number;
  qps: number;
  connections: string[]; // IDs of connected nodes
}

export interface TerminalLine {
  id: string;
  type: 'input' | 'output' | 'error' | 'system';
  content: string;
  timestamp: number;
}

export type ServerContext = 'local' | 'remote-aws' | 'remote-aliyun';

export enum ViewState {
  DASHBOARD = 'DASHBOARD',
  MLOPS = 'MLOPS',
  LOCAL_MODEL = 'LOCAL_MODEL',
  KUBERNETES = 'KUBERNETES',
  DATA_TOOLS = 'DATA_TOOLS',
  PROJECTS = 'PROJECTS',
  FILESYSTEM = 'FILESYSTEM',
  SETTINGS = 'SETTINGS'
}

// Flowchart Types
export interface FlowNode {
  id: string;
  type: 'ingestion' | 'process' | 'training' | 'eval' | 'deploy';
  label: string;
  x: number;
  y: number;
  status: 'pending' | 'running' | 'completed' | 'error';
}

export interface PipelineLog {
  id: string;
  timestamp: string;
  level: 'INFO' | 'WARN' | 'ERROR';
  message: string;
  stage: string;
}

// Model Automation Types
export interface LocalModel {
  id: string;
  name: string;
  baseModel: string;
  status: 'Idle' | 'Fine-Tuning' | 'Deployed';
  accuracy: number;
  precision: number;
  recall: number;
  epoch: number;
  totalEpochs: number;
}

// Git Types
export interface GitCommit {
  id: string;
  message: string;
  author: string;
  date: string;
  hash: string;
}

// Filesystem Types
export interface FileSystemNode {
  id: string;
  name: string;
  type: 'folder' | 'file' | 'drive';
  size: string;
  rawSize: number; // For treemap
  permissions: string;
  mountPoint?: string;
  usage?: number; // percentage for drives
  children?: FileSystemNode[];
  content?: string; // For file preview
}
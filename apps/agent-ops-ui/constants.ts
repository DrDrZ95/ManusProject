
import { ProjectStatus, ClusterNode, DataToolNode, MetricPoint, FlowNode, PipelineLog, LocalModel, K8sPod, GitCommit, FileSystemNode } from './types';

export const TRANSLATIONS = {
  en: {
    dashboard: "Operations Overview",
    mlops: "AI Pipelines (MLOps)",
    localModel: "LLM Fine-tuning",
    kubernetes: "Kubernetes Clusters",
    dataTools: "Data Infrastructure",
    projects: "Project Catalog",
    filesystem: "Unified Filesystem",
    settings: "Control Plane Settings",
    quickActions: "Quick Actions",
    systemHealth: "System Health",
    activeClusters: "Active Clusters",
    opsTimeline: "Operations Timeline",
    deployAll: "Deploy All Models",
    retrain: "Retrain Pipeline",
    flushCache: "Flush Redis Cache",
    emergencyStop: "Emergency Stop",
    uptime: "Uptime",
    lastDeploy: "Last Deploy",
    injectParams: "Inject Parameters",
    executePipeline: "Execute Pipeline",
    logs: "System Logs",
    export: "Export",
    fineTuning: "Fine-Tuning",
    dataCollection: "Data Collection",
    deployment: "Deployment",
    publish: "Publish",
    trainLoss: "Train Loss",
    valLoss: "Validation Loss",
    searchPlaceholder: "Search AI assets, logs, or clusters...",
    remoteServer: "Remote Server",
    localContext: "Local Context",
    switchContext: "Switch Context",
    cpuUsage: "CPU Usage",
    memUsage: "Memory Usage",
    diskUsage: "Disk Usage",
    topology: "Topology Map",
    restartNode: "Restart Node",
    queryStatus: "Query Status",
    loginTitle: "ManusProject Ops",
    loginSubtitle: "Enterprise AI & Infrastructure Control Plane",
    username: "Username",
    password: "Password",
    login: "Sign In",
    projectCatalog: "Project Catalog",
    deploy: "Deploy",
    gitHistory: "Git History",
    physicalAddress: "Physical Address",
    fileExplorer: "File Explorer",
    mountPoint: "Mount Point",
    permissions: "Permissions",
    syncFiles: "Sync Files",
    backup: "Backup",
    logout: "Logout",
    profile: "Profile",
    accountSettings: "Account Settings",
    createProject: "Create New Project",
    projectName: "Project Name",
    repositoryUrl: "Repository URL",
    create: "Create",
    creating: "Creating...",
    namespace: "Namespace",
    namespaces: "Namespaces",
    pods: "Pods",
    serviceMesh: "Service Mesh View",
    ebpfMetrics: "eBPF Observability",
    backToMap: "Back to Map",
    indent: "Indent Menu",
    // Categories
    catMachineLearning: "Machine Learning",
    catProjectInfo: "Project Information",
    catStorage: "Storage",
    catSystem: "System Management",
    // Settings specific
    general: "General",
    security: "Security",
    notifications: "Notifications",
    saveChanges: "Save Changes",
    saved: "Saved Successfully",
    fullName: "Full Name",
    email: "Email Address",
    role: "Role",
    bio: "Bio",
    changePassword: "Change Password",
    currentPassword: "Current Password",
    newPassword: "New Password",
    confirmPassword: "Confirm Password",
    apiKeys: "API Keys",
    createKey: "Create New Key",
    twoFactor: "Two-Factor Authentication",
    enable2FA: "Enable 2FA",
    appearance: "Appearance",
    theme: "Theme",
    language: "Language",
    logRetention: "Log Retention (Days)",
    enableNotifications: "Enable Notifications",
    activeSessions: "Active Sessions"
  },
  zh: {
    dashboard: "运维概览",
    mlops: "AI 调度工作流",
    localModel: "LLM 模型微调",
    kubernetes: "K8s 集群管理",
    dataTools: "分布式数据工具",
    projects: "项目资产目录",
    filesystem: "统一存储",
    settings: "系统配置",
    quickActions: "快捷运维",
    systemHealth: "系统健康度",
    activeClusters: "活跃集群",
    opsTimeline: "运维时间轴",
    deployAll: "部署所有模型",
    retrain: "重训 Pipeline",
    flushCache: "清空 Redis 缓存",
    emergencyStop: "紧急停止",
    uptime: "运行时间",
    lastDeploy: "上次部署",
    injectParams: "注入参数",
    executePipeline: "执行流水线",
    logs: "系统日志",
    export: "导出日志",
    fineTuning: "模型微调",
    dataCollection: "数据采集",
    deployment: "部署上线",
    publish: "发布服务",
    trainLoss: "训练损失",
    valLoss: "验证损失",
    searchPlaceholder: "搜索 AI 资产、日志或集群...",
    remoteServer: "远程服务器",
    localContext: "本地环境",
    switchContext: "切换环境",
    cpuUsage: "CPU 使用率",
    memUsage: "内存使用率",
    diskUsage: "磁盘使用率",
    topology: "拓扑结构图",
    restartNode: "重启节点",
    queryStatus: "查询状态",
    loginTitle: "ManusProject Ops",
    loginSubtitle: "企业级 AI 与基础设施管理平台",
    username: "用户名",
    password: "密码",
    login: "登录系统",
    projectCatalog: "项目资产目录",
    deploy: "一键部署",
    gitHistory: "Git 提交记录",
    physicalAddress: "物理部署地址",
    fileExplorer: "文件资源管理器",
    mountPoint: "挂载点",
    permissions: "权限",
    syncFiles: "文件同步",
    backup: "数据备份",
    logout: "退出登录",
    profile: "个人资料",
    accountSettings: "账户设置",
    createProject: "创建新项目",
    projectName: "项目名称",
    repositoryUrl: "仓库地址",
    create: "立即创建",
    creating: "创建中...",
    namespace: "命名空间",
    namespaces: "命名空间视图",
    pods: "Pods 视图",
    serviceMesh: "服务网格视图",
    ebpfMetrics: "eBPF 可观测性指标",
    backToMap: "返回拓扑图",
    indent: "缩进菜单",
    // Categories
    catMachineLearning: "机器学习",
    catProjectInfo: "项目信息",
    catStorage: "存储",
    catSystem: "系统管理",
    // Settings specific
    general: "通用设置",
    security: "安全设置",
    notifications: "通知设置",
    saveChanges: "保存更改",
    saved: "保存成功",
    fullName: "全名",
    email: "电子邮箱",
    role: "角色",
    bio: "个人简介",
    changePassword: "修改密码",
    currentPassword: "当前密码",
    newPassword: "新密码",
    confirmPassword: "确认新密码",
    apiKeys: "API 密钥",
    createKey: "创建新密钥",
    twoFactor: "双重认证",
    enable2FA: "启用 2FA",
    appearance: "外观设置",
    theme: "主题",
    language: "语言",
    logRetention: "日志保留 (天)",
    enableNotifications: "启用通知",
    activeSessions: "活跃会话"
  }
};

export const MOCK_PROJECTS: ProjectStatus[] = [
  { id: '1', name: 'Alpha-Model-Service', status: 'healthy', uptime: '99.9%', lastDeployment: '2h ago', repo: 'git/alpha-model', address: 'San Francisco, DC-01', branch: 'main' },
  { id: '2', name: 'Beta-Data-Pipeline', status: 'warning', uptime: '98.5%', lastDeployment: '1d ago', repo: 'git/beta-pipe', address: 'Shanghai, CN-East-2', branch: 'dev' },
  { id: '3', name: 'Gamma-Inference-Engine', status: 'deploying', uptime: '99.0%', lastDeployment: 'Just now', repo: 'git/gamma-engine', address: 'Tokyo, JP-North', branch: 'release/v2' },
  { id: '4', name: 'Delta-Training-Job', status: 'critical', uptime: 'N/A', lastDeployment: 'FAILED', repo: 'git/delta-train', address: 'London, UK-South', branch: 'experiment-x' },
];

export const MOCK_CLUSTER_NODES: ClusterNode[] = [
  { id: 'n1', name: 'k8s-master-01', role: 'master', cpu: 15, memory: 40, status: 'ready', pods: 12 },
  { id: 'n2', name: 'k8s-worker-01', role: 'worker', cpu: 78, memory: 82, status: 'ready', pods: 45 },
  { id: 'n3', name: 'k8s-worker-02', role: 'worker', cpu: 45, memory: 60, status: 'ready', pods: 38 },
  { id: 'n4', name: 'k8s-worker-03', role: 'worker', cpu: 92, memory: 90, status: 'not-ready', pods: 0 },
];

export const MOCK_PODS: K8sPod[] = [
  { 
    id: 'p1', nodeId: 'n1', name: 'nginx-ingress-controller-8d2f', namespace: 'kube-system', status: 'Running', restarts: 0, age: '12d', logs: ['Starting Nginx...', 'Ready to handle requests'],
    connections: ['p2'],
    ebpf: { httpThroughput: 1240, tcpLatency: 2.1, tcpRetransmits: 0, pid: 481, lastSyscall: 'epoll_wait', networkIn: 4.5, networkOut: 4.2 }
  },
  { 
    id: 'p2', nodeId: 'n1', name: 'core-dns-autoscaler-7b9c', namespace: 'kube-system', status: 'Running', restarts: 1, age: '5d', logs: ['Scale up detected', 'Replicas updated to 3'],
    connections: [],
    ebpf: { httpThroughput: 50, tcpLatency: 0.8, tcpRetransmits: 0, pid: 892, lastSyscall: 'futex', networkIn: 0.1, networkOut: 0.1 }
  },
  { 
    id: 'p3', nodeId: 'n2', name: 'model-inference-api-v2-x9s2', namespace: 'default', status: 'Error', restarts: 5, age: '2h', logs: ['Error: OOMKilled', 'Restarting container...'],
    connections: ['p4', 'p5'],
    ebpf: { httpThroughput: 0, tcpLatency: 0, tcpRetransmits: 15, pid: 1024, lastSyscall: 'exit_group', networkIn: 0, networkOut: 0 }
  },
  { 
    id: 'p4', nodeId: 'n2', name: 'redis-cart-cache-0', namespace: 'default', status: 'Running', restarts: 0, age: '24d', logs: ['Ready to accept connections'],
    connections: [],
    ebpf: { httpThroughput: 8500, tcpLatency: 0.4, tcpRetransmits: 0, pid: 1102, lastSyscall: 'read', networkIn: 12.0, networkOut: 35.5 }
  },
  { 
    id: 'p5', nodeId: 'n2', name: 'payment-service-v3', namespace: 'default', status: 'Running', restarts: 0, age: '5d', logs: ['Payment gateway connected', 'Listening on port 8080'],
    connections: ['p4'],
    ebpf: { httpThroughput: 450, tcpLatency: 15.2, tcpRetransmits: 2, pid: 2201, lastSyscall: 'write', networkIn: 1.2, networkOut: 1.8 }
  },
  { 
    id: 'p6', nodeId: 'n3', name: 'analytics-worker-01', namespace: 'processing', status: 'Running', restarts: 2, age: '1d', logs: ['Batch processing started', 'Processing 10k records'],
    connections: ['p7'],
    ebpf: { httpThroughput: 5, tcpLatency: 45.0, tcpRetransmits: 5, pid: 3001, lastSyscall: 'recvfrom', networkIn: 55.0, networkOut: 2.0 }
  },
  { 
    id: 'p7', nodeId: 'n3', name: 'analytics-worker-02', namespace: 'processing', status: 'Running', restarts: 0, age: '1d', logs: ['Worker ready'],
    connections: ['p6'],
    ebpf: { httpThroughput: 8, tcpLatency: 42.0, tcpRetransmits: 1, pid: 3005, lastSyscall: 'recvfrom', networkIn: 48.0, networkOut: 1.5 }
  },
];

export const MOCK_DATA_NODES: DataToolNode[] = [
  { id: 'd1', name: 'ES-Cluster-Main', type: 'Elasticsearch', shards: 24, status: 'green', diskUsage: 45, qps: 2400, connections: ['d2', 'd3'] },
  { id: 'd2', name: 'ClickHouse-Analytics', type: 'ClickHouse', shards: 8, status: 'yellow', diskUsage: 88, qps: 150, connections: [] },
  { id: 'd3', name: 'Redis-Hot-Cache', type: 'Redis', shards: 1, status: 'green', diskUsage: 20, qps: 12500, connections: ['d4'] },
  { id: 'd4', name: 'HBase-User-Profile', type: 'HBase', shards: 48, status: 'green', diskUsage: 55, qps: 890, connections: [] },
];

export const MOCK_TIMELINE_DATA: MetricPoint[] = Array.from({ length: 24 }).map((_, i) => ({
  time: `${i}:00`,
  value: Math.floor(Math.random() * 100),
  status: Math.random() > 0.9 ? 'failure' : 'success',
  details: Math.random() > 0.9 ? 'Pipeline failure detected' : 'Routine check pass'
}));

export const MOCK_FLOW_NODES: FlowNode[] = [
  { id: '1', type: 'ingestion', label: 'Data Ingestion', x: 50, y: 100, status: 'completed' },
  { id: '2', type: 'process', label: 'Preprocessing', x: 250, y: 100, status: 'completed' },
  { id: '3', type: 'training', label: 'Model Training', x: 450, y: 100, status: 'running' },
  { id: '4', type: 'eval', label: 'Evaluation', x: 650, y: 100, status: 'pending' },
  { id: '5', type: 'deploy', label: 'Deployment', x: 850, y: 100, status: 'pending' },
];

export const MOCK_LOGS: PipelineLog[] = [
  { id: '1', timestamp: '2023-10-27 10:00:01', level: 'INFO', message: 'Pipeline started via One-Click Execution', stage: 'System' },
  { id: '2', timestamp: '2023-10-27 10:00:05', level: 'INFO', message: 'Data ingestion complete. 45GB loaded.', stage: 'Ingestion' },
  { id: '3', timestamp: '2023-10-27 10:01:20', level: 'WARN', message: 'Missing values detected in col "feature_x". Imputing...', stage: 'Preprocessing' },
  { id: '4', timestamp: '2023-10-27 10:02:15', level: 'INFO', message: 'Training started on GPU-01', stage: 'Training' },
  { id: '5', timestamp: '2023-10-27 10:05:00', level: 'ERROR', message: 'Gradient explosion detected in layer 4.', stage: 'Training' },
];

export const MOCK_LOCAL_MODELS: LocalModel[] = [
  { id: 'm1', name: 'LLaMA-3-8B-Finance', baseModel: 'llama-3-8b', status: 'Fine-Tuning', accuracy: 88, precision: 85, recall: 90, epoch: 14, totalEpochs: 20 },
  { id: 'm2', name: 'Qwen-72B-Sentiment', baseModel: 'qwen-72b-chat', status: 'Deployed', accuracy: 94, precision: 92, recall: 93, epoch: 10, totalEpochs: 10 },
  { id: 'm3', name: 'Mistral-7B-v0.3', baseModel: 'mistral-7b', status: 'Idle', accuracy: 76, precision: 70, recall: 78, epoch: 0, totalEpochs: 50 },
];

export const MOCK_COMMITS: GitCommit[] = [
  { id: 'c1', message: 'fix: updated model hyperparameters', author: 'DevOps-Lead', date: '2h ago', hash: '8f2a9c' },
  { id: 'c2', message: 'feat: added new data validation step', author: 'DataScientist-01', date: '5h ago', hash: '7b3d1e' },
  { id: 'c3', message: 'chore: updated dependencies', author: 'Bot', date: '1d ago', hash: '9c4f2a' },
];

export const MOCK_FILESYSTEM: FileSystemNode[] = [
  {
    id: 'root',
    name: '/',
    type: 'drive',
    size: '1.2 TB',
    rawSize: 1200,
    permissions: 'drwxr-xr-x',
    mountPoint: '/mnt/data',
    usage: 65,
    children: [
      {
        id: 'f1',
        name: 'models',
        type: 'folder',
        size: '450 GB',
        rawSize: 450,
        permissions: 'drwxrwxr--',
        children: [
           { id: 'f1-1', name: 'llama-7b.bin', type: 'file', size: '13 GB', rawSize: 13, permissions: '-rw-r--r--', content: 'BINARY DATA' },
           { id: 'f1-2', name: 'config.json', type: 'file', size: '2 KB', rawSize: 0.002, permissions: '-rw-r--r--', content: '{\n  "layers": 32,\n  "dim": 4096\n}' },
        ]
      },
      {
        id: 'f2',
        name: 'datasets',
        type: 'folder',
        size: '600 GB',
        rawSize: 600,
        permissions: 'drwxr-xr-x',
        children: []
      },
      {
        id: 'f3',
        name: 'logs',
        type: 'folder',
        size: '150 GB',
        rawSize: 150,
        permissions: 'drwxr-xr-x',
        children: []
      }
    ]
  }
];

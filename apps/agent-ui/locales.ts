
export const translations = {
  en: {
    newChat: "New chat",
    newGroup: "New Project",
    recent: "Chats",
    groups: "Projects",
    user: "User",
    proPlan: "Pro Plan",
    grokIntroTitle: "Agent",
    grokIntroDesc: "I am Agent, your intelligent assistant for coding, work automation, and terminal operations.",
    askAnything: "Ask anything...",
    disclaimer: "Agent may make mistakes. Please verify sensitive information.",
    copy: "Copy",
    copied: "Copied",
    terminalTitle: "Linux Terminal • Remote",
    openTerminal: "Open Terminal",
    modelKimi: "MoonShot",
    modelDeepseek: "HangZhou DeepSeek",
    modelGptOss: "OpenAI",
    languageName: "English",
    systemInstruction: "You are Agent, a highly advanced AI assistant. You are helpful, precise, and can see the user's terminal environment. When showing code, use markdown code blocks. Keep responses concise but informative.",
    
    // Voice
    listening: "Listening...",
    tapToSpeak: "Tap to speak",

    // Context Menu & Groups
    rename: "Rename",
    delete: "Delete",
    emailChat: "Email Chat",
    moveToGroup: "Move to Project",
    removeFromGroup: "Remove from Project",
    createGroup: "Create Project",
    groupNamePlaceholder: "Project Name",
    untitledGroup: "Untitled Project",
    groupLimitReached: "Project limit reached (max 10)",
    confirmDelete: "Are you sure you want to delete this?",
    deleteGroupWarning: "Deleting a project will move its chats to the main list.",
    ungrouped: "No Project",

    // User Menu
    upgradeSubscription: "Upgrade Subscription",
    account: "Account",
    getHelp: "Get Help",
    settings: "Settings",
    signOut: "Sign Out",

    // Input Modes
    modeGeneral: "General",
    modeBrainstorm: "Brainstorming",
    modeOAWork: "OA / Work Assistant",
    modeCompany: "Company Info",
    modeAgent: "Agent Mode", // Added

    // Agent Mode Confirm
    agentModeConfirmTitle: "Enter Agent Mode?",
    agentModeConfirmDesc: "You are about to enter Agent mode. You cannot choose the model independently. Do you confirm to continue?",
    confirm: "Confirm",
    cancel: "Cancel",

    // Placeholders
    placeholderGeneral: "Ask anything, or start coding...",
    placeholderBrainstorm: "Enter topic to generate ideas...",
    placeholderOAWork: "Type your request (e.g., 'Draft email to team')...",
    placeholderCompany: "Ask about policies or structure...",
    placeholderAgent: "Agent Mode Active. Describing task...",

    // Login
    welcomeBack: "Welcome back",
    loginSubtitle: "Sign in to your internal workspace",
    emailOrId: "Email or Employee ID",
    password: "Password",
    phoneNumber: "Phone Number",
    continue: "Continue",
    orContinueWith: "Or continue with",
    signInWithGoogle: "Sign in with Google",
    signInWithOutlook: "Sign in with Outlook",
    usePhone: "Use Phone Number",
    useEmail: "Use Email / ID",
    forgotPassword: "Forgot password?",
    rememberMe: "Remember me",
    loginMethodPassword: "Password",
    loginMethodSMS: "SMS Code",
    loginMethodWeChat: "WeChat",
    wechatScanTitle: "Scan to Login",
    wechatScanDesc: "Open WeChat and scan the QR code",
    sendCode: "Get Code",
    resendCode: "Resend",
    enterCode: "Verification Code",
    codeSent: "Code sent!",

    // My Space
    mySpace: "My Space",
    remoteLinux: "Remote Linux",
    functionUnderDev: "Function under development",
    previewFile: "Preview File",
    download: "Download",
    file: "File",
    closePreview: "Close Preview",
    openMySpace: "Open My Space",
    itemsSelected: "Selected",
    deleteSelected: "Delete Selected",
    confirmBatchDelete: "Are you sure you want to delete these files? This action cannot be undone.",
    mySpaceFiles: "Files",
    mySpaceSolutions: "Solutions",

    // Terminal
    terminalWelcomeMsg: `\x1b[38;2;224;224;224mAgent-OS Terminal\x1b[0m v1.0.0
Linux kernel 6.8.0-generic x86_64

Since this module is an independent space within a Container, and due to limited resources, you can perform simple operations.

Type "help" for available commands.

`,

    // User Modals
    // Upgrade
    unleashPotential: "Unleash Full Potential",
    getAccessPro: "Get access to Agent Pro with advanced reasoning and faster speeds.",
    freePlan: "Agent Free",
    proPlanCard: "Agent Pro",
    recommended: "RECOMMENDED",
    perMonth: "/mo",
    standardSpeed: "Standard Response Speed",
    dailyLimits: "Daily Conversation Limits",
    accessKimi: "Access to MoonShot Basic",
    fastSpeed: "Fast Response Speed",
    unlimitedChats: "Unlimited Conversations",
    accessDeepseek: "Access to Deepseek & OpenAI",
    prioritySupport: "Priority Support",
    upgradeBtn: "Upgrade to Pro",
    securePayment: "Secure payment processed by Stripe. Cancel anytime.",
    
    // Account
    changeAvatar: "Change Avatar",
    avatarSize: "JPG, GIF or PNG. Max size of 800K",
    styleAnime: "Anime Style",
    styleSimple: "Simple Style",
    displayName: "Display Name",
    emailLabel: "Email Address",
    bio: "Bio",
    bioPlaceholder: "Tell us about yourself...",
    saveChanges: "Save Changes",

    // Help
    needHelpTitle: "Need immediate assistance?",
    needHelpDesc: "Our support team is available 24/7 to help you with any issues.",
    faq1Title: "How do I use the Linux Terminal?",
    faq1Desc: "The terminal is a simulated environment connected to the backend. You can run standard Linux commands like `ls`, `cd`, and `cat`. Currently, it operates in a restricted sandbox mode.",
    faq2Title: "Is my data private?",
    faq2Desc: "Yes, all conversation data is stored locally in your browser (Local Storage) and is not sent to any server other than the AI inference provider for generation.",
    faq3Title: "Can I export my chats?",
    faq3Desc: "Currently, you can email chats to yourself using the context menu on the sidebar. Full export features are coming soon.",
    contactSupport: "Contact Support",
    describeIssue: "Describe your issue...",
    sendMessage: "Send Message",

    // Settings
    general: "General",
    streamResponses: "Stream Responses",
    streamDesc: "Show text as it is being generated",
    soundEffects: "Sound Effects",
    soundDesc: "Play subtle sounds for messages",
    dataPrivacy: "Data & Privacy",
    trainingData: "Training Data",
    trainingDesc: "Allow conversations to be used for training",
    clearData: "Clear All Data",
    done: "Done"
  },
  zh: {
    newChat: "新对话",
    newGroup: "新建项目",
    recent: "对话列表",
    groups: "项目",
    user: "用户",
    proPlan: "专业版",
    grokIntroTitle: "Agent",
    grokIntroDesc: "我是 Agent，您的智能助手，致力于代码编写、工作自动化和终端操作。",
    askAnything: "问点什么...",
    disclaimer: "Agent 可能会犯错。请核实敏感信息。",
    copy: "复制",
    copied: "已复制",
    terminalTitle: "Linux 终端 • 远程",
    openTerminal: "打开终端",
    modelKimi: "MoonShot (月之暗面)",
    modelDeepseek: "HangZhou DeepSeek (深度求索)",
    modelGptOss: "OpenAI",
    languageName: "中文",
    systemInstruction: "你是 Agent，一个高度先进的人工智能助手。你乐于助人、精准，并且可以看到用户的终端环境。在显示代码时，使用 markdown 代码块。保持回答简洁但信息丰富。",

    // Voice
    listening: "正在聆听...",
    tapToSpeak: "点击麦克风说话",

    // Context Menu & Groups
    rename: "重命名",
    delete: "删除",
    emailChat: "邮件发送",
    moveToGroup: "移动到项目",
    removeFromGroup: "移出项目",
    createGroup: "创建项目",
    groupNamePlaceholder: "项目名称",
    untitledGroup: "未命名项目",
    groupLimitReached: "项目数量已达上限 (10)",
    confirmDelete: "确定要删除吗？",
    deleteGroupWarning: "删除项目后，对话将移动到主列表。",
    ungrouped: "未分类",

    // User Menu
    upgradeSubscription: "升级订阅",
    account: "账户",
    getHelp: "获取帮助",
    settings: "设置",
    signOut: "退出登录",

    // Input Modes
    modeGeneral: "通用模式",
    modeBrainstorm: "头脑风暴",
    modeOAWork: "OA / 工作助理",
    modeCompany: "企业信息",
    modeAgent: "Agent 模式", // Added

    // Agent Mode Confirm
    agentModeConfirmTitle: "进入 Agent 模式？",
    agentModeConfirmDesc: "您即将进入 Agent 模式。您将无法自主选择模型。确认继续吗？",
    confirm: "确认",
    cancel: "取消",

    // Placeholders
    placeholderGeneral: "随便问点什么，或者开始写代码...",
    placeholderBrainstorm: "输入主题以生成创意...",
    placeholderOAWork: "输入您的请求（例如：“起草发给团队的邮件”）...",
    placeholderCompany: "询问关于政策或架构...",
    placeholderAgent: "Agent 模式已激活。请描述任务...",

    // Login
    welcomeBack: "欢迎回来",
    loginSubtitle: "登录您的内部工作区",
    emailOrId: "邮箱或员工 ID",
    password: "密码",
    phoneNumber: "手机号码",
    continue: "继续",
    orContinueWith: "或其他方式",
    signInWithGoogle: "使用 Google 登录",
    signInWithOutlook: "使用 Outlook 登录",
    usePhone: "使用手机号登录",
    useEmail: "使用邮箱/ID登录",
    forgotPassword: "忘记密码？",
    rememberMe: "记住我",
    loginMethodPassword: "密码登录",
    loginMethodSMS: "验证码登录",
    loginMethodWeChat: "微信登录",
    wechatScanTitle: "扫码登录",
    wechatScanDesc: "请使用微信扫描二维码",
    sendCode: "获取验证码",
    resendCode: "重新发送",
    enterCode: "输入验证码",
    codeSent: "验证码已发送！",

    // My Space
    mySpace: "我的空间",
    remoteLinux: "远程 Linux",
    functionUnderDev: "功能开发中",
    previewFile: "文件预览",
    download: "下载",
    file: "文件",
    closePreview: "关闭预览",
    openMySpace: "打开我的空间",
    itemsSelected: "项已选择",
    deleteSelected: "删除所选",
    confirmBatchDelete: "确定要删除这些文件吗？此操作无法撤销。",
    mySpaceFiles: "文件",
    mySpaceSolutions: "解决方案",

    // Terminal
    terminalWelcomeMsg: `\x1b[38;2;224;224;224mAgent-OS 终端\x1b[0m v1.0.0
Linux 内核 6.8.0-generic x86_64

由于此模块是容器内的独立空间，且资源有限，您仅可执行简单的操作。

输入 "help" 查看可用命令。

`,

    // User Modals
    // Upgrade
    unleashPotential: "释放全部潜能",
    getAccessPro: "获取 Agent Pro 权限，享受高级推理和更快的速度。",
    freePlan: "Agent 免费版",
    proPlanCard: "Agent 专业版",
    recommended: "推荐",
    perMonth: "/月",
    standardSpeed: "标准响应速度",
    dailyLimits: "每日对话限制",
    accessKimi: "使用 MoonShot 基础模型",
    fastSpeed: "极速响应",
    unlimitedChats: "无限对话",
    accessDeepseek: "使用 Deepseek & OpenAI",
    prioritySupport: "优先支持",
    upgradeBtn: "升级到专业版",
    securePayment: "由 Stripe 提供安全支付。随时取消。",

    // Account
    changeAvatar: "更换头像",
    avatarSize: "JPG, GIF 或 PNG。最大 800K",
    styleAnime: "动漫风格",
    styleSimple: "简约风格",
    displayName: "显示名称",
    emailLabel: "邮箱地址",
    bio: "个人简介",
    bioPlaceholder: "介绍一下你自己...",
    saveChanges: "保存更改",

    // Help
    needHelpTitle: "需要立即协助？",
    needHelpDesc: "我们的支持团队 24/7 全天候为您解决任何问题。",
    faq1Title: "如何使用 Linux 终端？",
    faq1Desc: "终端是一个连接到后端的模拟环境。您可以运行标准的 Linux 命令，如 ls、cd 和 cat。目前，它在受限的沙盒模式下运行。",
    faq2Title: "我的数据隐私吗？",
    faq2Desc: "是的，所有对话数据都存储在您的浏览器本地（LocalStorage），除了用于生成的 AI 推理提供商外，不会发送到任何服务器。",
    faq3Title: "我可以导出聊天记录吗？",
    faq3Desc: "目前，您可以使用侧边栏上的上下文菜单将聊天记录通过邮件发送给自己。完整的导出功能即将推出。",
    contactSupport: "联系支持",
    describeIssue: "描述您的问题...",
    sendMessage: "发送消息",

    // Settings
    general: "通用",
    streamResponses: "流式响应",
    streamDesc: "生成时显示文本",
    soundEffects: "音效",
    soundDesc: "播放微妙的消息提示音",
    dataPrivacy: "数据与隐私",
    trainingData: "训练数据",
    trainingDesc: "允许使用对话进行训练",
    clearData: "清除所有数据",
    done: "完成"
  }
};

export type Language = keyof typeof translations;


export const translations = {
  en: {
    newChat: "New chat",
    newGroup: "New Group",
    recent: "Recent",
    groups: "Groups",
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
    
    // Context Menu & Groups
    rename: "Rename",
    delete: "Delete",
    emailChat: "Email Chat",
    moveToGroup: "Move to Group",
    removeFromGroup: "Remove from Group",
    createGroup: "Create Group",
    groupNamePlaceholder: "Group Name",
    untitledGroup: "Untitled Group",
    groupLimitReached: "Group limit reached (max 10)",
    confirmDelete: "Are you sure you want to delete this?",
    deleteGroupWarning: "Deleting a group will move its chats to Recent.",
    ungrouped: "Ungrouped",

    // User Menu
    upgradeSubscription: "Upgrade Subscription",
    account: "Account",
    getHelp: "Get Help",
    settings: "Settings",
    signOut: "Sign Out",

    // Input Modes
    modeGeneral: "General",
    modeWorkReport: "Work Progress Report",
    modeOA: "OA Workflow",
    modeProject: "Project Assistance",
    modeCompany: "About the Company",

    // Placeholders - Optimized & Detailed
    placeholderGeneral: "Ask anything...",
    placeholderWorkReport: "Draft a structured daily report summarizing key achievements, code commits, and blockers for the engineering team sync...",
    placeholderOA: "Guide me through the internal reimbursement process for travel expenses, including category selection and approval chain...",
    placeholderProject: "Generate a Gantt chart timeline for the Q3 migration project, outlining milestones, dependencies, and resource allocation...",
    placeholderCompany: "Retrieve the latest organizational structure for the Product Design department and summarize the key leadership hierarchy...",

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
    rememberMe: "Remember me"
  },
  zh: {
    newChat: "新对话",
    newGroup: "新建分组",
    recent: "最近",
    groups: "分组",
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

    // Context Menu & Groups
    rename: "重命名",
    delete: "删除",
    emailChat: "邮件发送",
    moveToGroup: "移动到分组",
    removeFromGroup: "移出分组",
    createGroup: "创建分组",
    groupNamePlaceholder: "分组名称",
    untitledGroup: "未命名分组",
    groupLimitReached: "分组数量已达上限 (10)",
    confirmDelete: "确定要删除吗？",
    deleteGroupWarning: "删除分组后，对话将移动到“最近”列表。",
    ungrouped: "未分组",

    // User Menu
    upgradeSubscription: "升级订阅",
    account: "账户",
    getHelp: "获取帮助",
    settings: "设置",
    signOut: "退出登录",

    // Input Modes
    modeGeneral: "通用模式",
    modeWorkReport: "工作进度汇报",
    modeOA: "OA 工作流",
    modeProject: "项目协助",
    modeCompany: "关于公司",

    // Placeholders - Optimized & Detailed
    placeholderGeneral: "问点什么...",
    placeholderWorkReport: "起草一份结构化的日报，总结关键成就、代码提交记录以及遇到的阻碍，用于工程团队同步...",
    placeholderOA: "引导我完成差旅费用的内部报销流程，包括类别选择和审批链条说明...",
    placeholderProject: "为Q3迁移项目生成甘特图时间表，概述里程碑、依赖关系和资源分配...",
    placeholderCompany: "检索产品设计部门的最新组织架构，并总结关键的领导层级体系...",

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
    rememberMe: "记住我"
  }
};

export type Language = keyof typeof translations;

## Agent-UI Analysis and Enhancement Plan

This document provides a comprehensive analysis of the `agent-ui` project, focusing on its API access patterns, component structure, and a plan for enhancing the `InputArea.tsx` component.

### 1. API Access Patterns and Services

The `agent-ui` project utilizes a modular service architecture to interact with backend APIs. The core services are:

*   **`api.ts`**: This is the main service that consolidates all other services into a single `api` object. It handles authentication, chat sessions, file uploads, and news fetching.
*   **`http.ts`**: A wrapper around `axios` that provides a centralized HTTP client for all API requests. It automatically handles authentication tokens and error handling.
*   **`ai.ts`**: This service is responsible for interacting with the AI models. It currently simulates streaming responses locally but is designed to be easily extended to support real AI providers.
*   **`tokenManager.ts`**: Manages JWT tokens for authentication.
*   **`security.ts`**: Handles encryption and other security-related tasks.
*   **`socket.ts`**: Manages WebSocket connections for real-time communication.
*   **`mcp.ts`**: Interacts with the Model Context Protocol (MCP) for advanced agent capabilities.

The API access pattern is consistent across all services: each service encapsulates a specific domain of functionality and uses the `httpClient` to make API requests. This modular approach makes the codebase easy to maintain and extend.

### 2. Component Structure and API Reference Plan

The `agent-ui` project is built with React and uses a component-based architecture. Below is a reference plan mapping core components to their primary API interactions.

#### Component API Reference Plan

```markdown
# App.tsx
GET - /api/v1/user/profile (User authentication and profile loading)
GET - /api/v1/config (Application configuration)

# Sidebar.tsx
GET - /api/v1/chats (List of chat sessions)
POST - /api/v1/chats (Create new chat session)
DELETE - /api/v1/chats/{id} (Delete chat session)

# MessageBubble.tsx
GET - /api/v1/messages/{id}/status (Message status polling/update)
POST - /api/v1/messages/{id}/feedback (User feedback on message)

# InputArea.tsx
POST - /api/v1/messages (User Input Module - Send new message/prompt)
GET - /api/v1/messages/staging (User Input Content Staging Module - Retrieve draft content)
POST - /api/v1/files/upload (File upload for attachments)

# LoginPage.tsx
POST - /api/v1/auth/login (User login and token retrieval)
```

### 3. `InputArea.tsx` Enhancement Plan

The `InputArea.tsx` component is a critical part of the user experience. The following enhancements are proposed:

#### 3.1. Prompts Module

A new "Prompts" module will be added to the `InputArea.tsx` component. This module will allow users to select from a list of predefined prompts, which can be used to quickly start a conversation with the AI.

The prompts will be categorized by topic (e.g., "Brainstorming", "OA Work", "Company") and will be displayed in a dropdown menu. When a user selects a prompt, the prompt text will be inserted into the input area.

#### 3.2. Change Mode Module

The existing "Change Mode" module will be enhanced to provide a more intuitive user experience. The current implementation uses a simple dropdown menu to switch between different input modes. The new implementation will use a more visual approach, with icons and descriptions for each mode.

#### 3.3. 增强实时监控与调试 (Terminal & RPC)

利用现有的 `TerminalPanel.tsx` 和 `rpc.ts` 服务，增强前端对后端 Agent 执行状态的实时可视化。
- **功能点**：显示 LLM 思考过程 (Chain of Thought)、工具调用入参/出参、eBPF 捕获的底层系统调用。
- **预留接口**：
  ```typescript
  // services/rpc.ts
  /**
   * 订阅 Agent 执行轨迹流
   * @param sessionId 会话ID
   * @param callback 处理轨迹数据的回调
   */
  export const subscribeAgentTrace = (sessionId: string, callback: (trace: AgentTrace) => void) => {
    // TODO: 实现基于 WebSocket 或 SSE 的实时轨迹订阅
  };
  ```

#### 3.4. 知识库与 RAG 管理 (MySpacePanel)

完善 `MySpacePanel.tsx`，支持用户上传本地文档并实时查看向量化进度。
- **功能点**：文档分片预览、向量数据库状态监控、检索效果测试 (RAG Testing)。
- **预留接口**：
  ```typescript
  // services/api.ts
  /**
   * 获取向量化进度状态
   * @param fileId 文件ID
   */
  export const getVectorizationStatus = async (fileId: string): Promise<VectorStatus> => {
    // TODO: GET /api/v1/files/{fileId}/vector-status
    return { progress: 0, status: 'pending' };
  };
  ```

### 4. 模块化分析与接口预留

为了方便后期功能扩展，以下是核心服务的接口预留计划：

#### 4.1. 插件/工具市场 (Tool Marketplace)
- **目标**：允许用户动态加载 OpenAPI 插件或 Mcp 工具。
- **接口预留**：
  ```typescript
  // services/mcp.ts
  /**
   * 发现并连接新的 MCP 服务器
   * @param config MCP服务器连接配置
   */
  export const connectMcpServer = async (config: McpConfig): Promise<McpServerInfo> => {
    // TODO: POST /api/v1/mcp/connect
  };
  ```

#### 4.2. 异常处理与重试机制 (Frontend Resiliency)
- **目标**：结合 `ErrorBoundary.tsx`，实现智能重试和降级显示。
- **接口预留**：
  ```typescript
  // services/http.ts
  // 已经在 http.ts 中集成了基础拦截器，后期可引入针对特定 ErrorCode 的自动重试策略
  ```

### 5. 下一步行动计划

1.  **UI 优化**：在 `InputArea.tsx` 中集成 `Prompts` 下拉模块，提升输入效率。
2.  **状态同步**：利用 `socket.ts` 实现前端与后端 `ShortTermMemoryService` 的实时同步，特别是压缩后的历史摘要展示。
3.  **调试增强**：在 `TerminalPanel` 中接入后端流式日志，实现真正的“透明 Agent”。


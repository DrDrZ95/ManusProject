
import { ModelType, Attachment, InputMode } from "../types";

/**
 * AI Service Configuration
 * AI 服务配置
 */
interface AIStreamConfig {
  model: ModelType;
  history: { role: string; parts: { text: string }[] }[];
  prompt: string;
  attachments: Attachment[];
  inputMode: InputMode;
}

/**
 * AI Generation Service
 * AI 生成服务
 * 
 * Purpose: Abstract the complexity of connecting to different LLM providers.
 * Currently simulates streaming responses locally.
 * 
 * 目的：抽象连接不同 LLM 提供商的复杂性。目前在本地模拟流式响应。
 */
class AIService {
  private static instance: AIService;

  private constructor() {}

  public static getInstance(): AIService {
    if (!AIService.instance) {
      AIService.instance = new AIService();
    }
    return AIService.instance;
  }

  /**
   * Stream a response from the AI
   * 获取 AI 流式响应
   * 
   * @param config Configuration object
   * @param callbacks Callbacks for stream events
   */
  public async streamResponse(
    config: AIStreamConfig,
    callbacks: {
      onChunk: (text: string) => void;
      onFinish: () => void;
      onError: (error: Error) => void;
    }
  ): Promise<void> {
    
    // Simulate API Latency / 模拟 API 延迟
    await new Promise(resolve => setTimeout(resolve, 600));

    try {
      const responseTemplate = this.selectSimulationTemplate(config.inputMode);
      
      let fullText = "";
      if (config.attachments.length > 0) {
        fullText += `[System: Received ${config.attachments.length} attachment(s): ${config.attachments.map(a => a.name).join(', ')}]\n\n`;
      }
      fullText += responseTemplate;

      let currentText = "";
      const chunkSize = 4; // Characters per chunk
      const delay = 10;    // ms per chunk (Typing effect)

      // Stream Simulation Loop
      for (let i = 0; i < fullText.length; i += chunkSize) {
        const chunk = fullText.slice(i, i + chunkSize);
        currentText += chunk;
        callbacks.onChunk(currentText);
        await new Promise(resolve => setTimeout(resolve, delay));
      }

      callbacks.onFinish();
    } catch (error: any) {
      console.error("[AIService] Generation Error:", error);
      callbacks.onError(error);
    }
  }

  // --- Private Helpers for Mock Content ---

  private selectSimulationTemplate(mode: InputMode): string {
    const templates = this.getTemplates(mode);
    return templates[Math.floor(Math.random() * templates.length)];
  }

  private getTemplates(mode: InputMode): string[] {
    switch (mode) {
      case 'agent':
        return [
          `> **Agent Workflow Active**\n\n**Step 1: Analysis**\nAnalyzing request parameters...\n\n**Step 2: Execution**\nExecuting system command \`./deploy.sh --env=staging\`...\n\n\`\`\`bash\n[INFO] Starting deployment...\n[INFO] Build successful.\n[SUCCESS] Deployed to 192.168.1.10\n\`\`\`\n\n**Step 3: Verification**\nHealth check passed.`,
          `> **Agent Reasoning**\n\nI have scanned the internal knowledge base. Based on "Doc_v2.pdf", the answer involves three steps:\n1. Configure the VPC peering.\n2. Update the security group rules.\n3. Restart the RDS instance.`
        ];
      case 'oa_work':
        return [
          `### 📧 Email Draft\n\n**Subject:** Meeting Follow-up\n\nHi Team,\n\nHere is a summary of today's discussion regarding the Q3 roadmap...\n\nBest,\n[Name]`,
          `### 📊 Weekly Summary\n\n*   **Completed:** API Migration\n*   **In Progress:** UI Refactor\n*   **Blocked:** Waiting for AWS credentials`
        ];
      case 'brainstorm':
        return [
          `### 💡 Idea 1: Gamification\nAdd a points system to encourage daily logins.\n\n### 💡 Idea 2: Dark Mode\nImplement a system-aware dark theme.\n\n### 💡 Idea 3: Voice Control\nAllow users to navigate using voice commands.`
        ];
      case 'company':
        return [
          `### 🏢 Company Policy\n\nAccording to the employee handbook, remote work requests must be submitted 48 hours in advance via the HR portal.`,
          `### 👥 Org Chart\n\nThe Engineering team reports to the CTO, while Product reports to the CPO. Cross-functional squads are formed for specific initiatives.`
        ];
      default: // General
        return [
          `Here is a Python example:\n\`\`\`python\nprint("Hello World")\n\`\`\``,
          `The capital of France is Paris. It is known for the Eiffel Tower.`,
          `Could you please clarify what specific data you need from the database?`
        ];
    }
  }
}

export const aiService = AIService.getInstance();

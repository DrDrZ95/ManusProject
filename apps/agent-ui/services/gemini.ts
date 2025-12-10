
import { ModelType, Attachment, InputMode } from "../types";

// Simulated response sets
const GENERAL_RESPONSES = [
  // --- English Simulations ---
  `Here is a simulated Python script that demonstrates a simple REST API using Flask:

\`\`\`python
from flask import Flask, jsonify, request

app = Flask(__name__)

# Sample data
tasks = [
    {'id': 1, 'title': 'Buy groceries', 'done': False},
    {'id': 2, 'title': 'Learn React', 'done': False}
]

@app.route('/tasks', methods=['GET'])
def get_tasks():
    return jsonify({'tasks': tasks})

@app.route('/tasks', methods=['POST'])
def create_task():
    if not request.json or not 'title' in request.json:
        return jsonify({'error': 'Bad Request'}), 400
    task = {
        'id': tasks[-1]['id'] + 1,
        'title': request.json['title'],
        'done': False
    }
    tasks.append(task)
    return jsonify({'task': task}), 201

if __name__ == '__main__':
    app.run(debug=True)
\`\`\`

This is a mock response generated locally. It simulates code generation capabilities.`,

  `**Analysis of Current Context:**

Based on the inputs provided, here is a summary of the simulated workflow:

1.  **Efficiency**: The new algorithm improves processing speed by approximately **32%**.
2.  **Scalability**: The horizontal scaling architecture allows for handling up to 10k concurrent requests.
3.  **Risks**: There is a potential bottleneck in the database connection pool during peak loads.

*Recommendation*: Consider implementing Redis caching for frequently accessed data to mitigate database load.`,

  `I am Agent, your intelligent assistant. Since this is a **simulated environment**, I am generating this text stream locally to demonstrate the UI capabilities.

I can help you with:
*   Drafting technical documentation
*   Simulating terminal commands
*   Brainstorming project ideas

Please let me know what you would like to simulate next!`,

  // --- Chinese Simulations ---
  `好的，这里为您生成一个基于 **Vue 3 + Composition API** 的简单计数器组件示例：

\`\`\`vue
<script setup>
import { ref } from 'vue'

const count = ref(0)

function increment() {
  count.value++
}

function decrement() {
  if (count.value > 0) {
    count.value--
  }
}
</script>

<template>
  <div class="card">
    <h2>当前计数: {{ count }}</h2>
    <div class="button-group">
      <button @click="decrement" class="btn secondary">-</button>
      <button @click="increment" class="btn primary">+</button>
    </div>
  </div>
</template>

<style scoped>
.card {
  padding: 20px;
  border: 1px solid #eee;
  border-radius: 8px;
  text-align: center;
}
.button-group {
  gap: 10px;
  display: flex;
  justify-content: center;
}
.btn {
  padding: 8px 16px;
  cursor: pointer;
}
</style>
\`\`\`

这段代码使用了 \`<script setup>\` 语法糖，更加简洁。您可以直接将其复制到您的 Vue 项目中使用。需要我为您解释一下 \`ref\` 的原理吗？`,
];

const OA_WORK_RESPONSES = [
  `### 📧 Draft Email: Project Status Update

**Subject:** Update on Q3 Development Milestones

Hi Team,

I wanted to share a quick update on our progress for the Q3 deliverables.

**Highlights:**
*   ✅ Authentication module completed.
*   ✅ API integration with third-party vendors finalized.
*   🔄 UI/UX testing is currently 80% complete.

**Next Steps:**
We are aiming to deploy the staging environment by Friday. Please review the attached documentation before our sync meeting.

Best regards,
[Your Name]`,

  `### 📊 Weekly Report Summary

**Key Achievements:**
1.  **Resolved Critical Bug #402**: Fixed the memory leak issue in the main dashboard.
2.  **Client Onboarding**: Successfully onboarded 3 new enterprise clients this week.

**Blockers:**
*   Waiting for the design assets for the new landing page.

**Plan for Next Week:**
*   Focus on performance optimization for the mobile app.`,
  
  `### 📋 Reimbursement Process Guide

To submit your reimbursement request for the recent business trip:

1.  Log in to the **OA Portal**.
2.  Navigate to **My Requests** > **Expenses**.
3.  Click **New Claim**.
4.  Upload your receipts (PDF or JPG).
5.  Select "Travel" as the category.
6.  Submit for approval.

*Note: Requests over $500 require VP approval.*`
];

const BRAINSTORM_RESPONSES = [
  `### 💡 Brainstorming: Marketing Campaign Ideas

Here are 5 creative concepts for the upcoming product launch:

1.  **"Day in the Life" Series**: Short formatted videos featuring real users solving problems with our app.
2.  **Interactive Webinar**: A live coding session showing how to build a plugin in 10 minutes.
3.  **Community Challenge**: A "Hackathon" with prizes for the best open-source contribution.
4.  **Influencer Takeover**: Partner with key tech influencers to manage our Twitter account for a day.
5.  **Mystery Box**: Send physical "deployment kits" to our top 50 users.`,

  `### 🧠 Mind Map: Feature Expansion

**Core Core**
*   **Security** -> 2FA, SSO, Audit Logs
*   **Performance** -> CDN Edge, Caching, Lazy Loading

**User Experience**
*   **Mobile** -> Native App, Offline Mode
*   **Accessibility** -> Screen Reader Support, High Contrast Mode

**Integrations**
*   **Slack** -> Notifications
*   **GitHub** -> PR Sync
*   **Jira** -> Ticket Creation`
];

const COMPANY_RESPONSES = [
  `### 🏢 Company Policy: Remote Work

**Overview:**
Our company supports a "Hybrid First" approach.

**Guidelines:**
*   Employees are expected to be in the office **2 days a week** (typically Tuesday/Thursday).
*   Remote days can be taken from any location with a stable internet connection.
*   Core collaboration hours are **10:00 AM - 3:00 PM** regardless of location.

**Equipment:**
The company provides a stipend of $1,000 for home office setup every 2 years.`,

  `### 👥 Organizational Structure

**Executive Team**
*   CEO: Jane Doe
*   CTO: John Smith
*   CPO: Sarah Johnson

**Engineering (Reporting to CTO)**
*   **Platform Team**: Infrastructure & DevOps
*   **Product Team A**: User Facing Features
*   **Product Team B**: Enterprise Solutions
*   **Data Team**: Analytics & AI

**Sales (Reporting to CRO)**
*   Enterprise Sales
*   Mid-Market Sales
*   SDR Team`
];

// Richer Agent Mode Simulations with "Steps"
const AGENT_RESPONSES = [
  `> **Initializing Agent Workflow...**

**Step 1: Information Retrieval**
*   📡 Searching web for "latest frontend frameworks 2025"...
*   🔍 Found 3 relevant sources.
*   *Source A: TechCrunch - "The Rise of Qwik and SolidJS"*
*   *Source B: GitHub Trends - "React 19 Adoption Rates"*

**Step 2: Planning & Construction**
*   🧠 Analyzing requirements...
*   📐 Drafting architecture diagram...
*   🔨 Generating code modules for *Dashboard Component*...

\`\`\`typescript
interface DashboardProps {
  user: User;
  stats: Statistic[];
}

const Dashboard: React.FC<DashboardProps> = ({ user, stats }) => {
  return (
    <div className="grid grid-cols-3 gap-4">
       {stats.map(s => <StatCard key={s.id} data={s} />)}
    </div>
  )
}
\`\`\`

**Step 3: Execution**
*   🧪 Running unit tests... [PASS]
*   🛡️ Verifying integrity... [PASS]

**Final Summary**
Task completed successfully. The dashboard architecture has been updated to support the new metrics. I have also cached the results in Redis to improve load times by 40%.`,

  `> **Initializing Agent Workflow...**

**Step 1: Deep Search**
*   🗄️ Querying internal knowledge base...
*   📄 *Found document: "Q3_Financial_Report.pdf"*
*   📄 *Found document: "Project_Titan_Specs.docx"*

**Step 2: Data Synthesis**
*   📊 Extracting key metrics...
*   📉 Comparing Q2 vs Q3 growth...
*   ⚠️ Identifying discrepancies in budget allocation...

**Step 3: Report Generation**
*   📝 Compiling tables...
*   ✍️ Drafting executive summary...

**Final Output**
Based on the analysis, the project is currently **under budget** by 15%. However, the timeline is at risk due to supply chain delays identified in the Q3 report.

*Recommendation*: Reallocate surplus budget to expedite shipping logistics.`,

  `> **Initializing Agent Workflow...**

**Step 1: Environment Setup**
*   ⚙️ Checking system dependencies...
*   ✅ *Detected Node.js v20.1.0*
*   ✅ *Detected Docker v24.0.5*

**Step 2: Task Execution**
*   🚀 Running build script \`npm run build\`...
*   > [Webpack] Compiling...
*   > [Webpack] 98% emitted
*   > [Webpack] Compiled successfully.

**Step 3: Deployment**
*   🐳 Pushing image to registry...
*   ☸️ Updating Kubernetes manifest...
*   🔄 Rolling restart of pods triggered.

**Final Summary**
Deployment to **staging** is complete. You can verify the changes at \`staging.internal.app\`. No regression issues were detected during the smoke test.`
];

export const streamGeminiResponse = async (
  apiKey: string,
  modelType: ModelType,
  history: { role: string; parts: { text: string }[] }[],
  prompt: string,
  attachments: Attachment[],
  onChunk: (text: string) => void,
  onFinish: () => void,
  onError: (error: Error) => void,
  inputMode: InputMode = 'general' // Added inputMode parameter
) => {
  // Simulate network latency
  await new Promise(resolve => setTimeout(resolve, 600));

  try {
    // Pick response based on input mode
    let responseSet = GENERAL_RESPONSES;
    if (inputMode === 'agent') responseSet = AGENT_RESPONSES;
    else if (inputMode === 'oa_work') responseSet = OA_WORK_RESPONSES;
    else if (inputMode === 'brainstorm') responseSet = BRAINSTORM_RESPONSES;
    else if (inputMode === 'company') responseSet = COMPANY_RESPONSES;

    const responseTemplate = responseSet[Math.floor(Math.random() * responseSet.length)];
    
    let fullText = "";
    // If there are attachments, acknowledge them
    if (attachments.length > 0) {
      fullText += `[Received ${attachments.length} attachment(s): ${attachments.map(a => a.name).join(', ')}]\n\n`;
    }
    fullText += responseTemplate;

    let currentText = "";
    const chunkSize = 4; // Characters per chunk
    const delay = 10;    // Milliseconds per chunk (typing speed)

    // Stream the response
    for (let i = 0; i < fullText.length; i += chunkSize) {
      const chunk = fullText.slice(i, i + chunkSize);
      currentText += chunk;
      onChunk(currentText);
      await new Promise(resolve => setTimeout(resolve, delay));
    }

    onFinish();
  } catch (error: any) {
    console.error("Simulation Error:", error);
    onError(error);
  }
};

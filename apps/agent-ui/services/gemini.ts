
import { ModelType, Attachment } from "../types";

// Simulated response sets
const MOCK_RESPONSES = [
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

  `**本周工作总结草案**

根据您的要求，我为您整理了一份简洁的周报模板：

### 📅 本周工作重点
1.  **核心功能开发**：完成了用户登录模块的 OAuth 2.0 对接，支持 Google 和 Outlook 第三方登录。
2.  **性能优化**：重构了前端长列表渲染逻辑，首屏加载速度提升了 **40%**。
3.  **Bug 修复**：解决了移动端侧边栏偶尔无法收起的问题 (Ticket #402)。

### 🚀 下周计划
*   启动支付网关（Stripe）的集成调研。
*   配合设计团队完成“深色模式”的 UI 走查。

### ⚠️ 需要支持
*   需要后端团队提供最新的 API 接口文档，以便进行联调。

您看这个格式是否符合您的需求？我可以帮您进一步润色语言，使其听起来更正式。`,

  `关于 **Transformer 架构**，让我用通俗易懂的方式为您解释：

想象您在翻译一句话。传统的模型（如 RNN）像是一个逐字阅读的学生，读到后面可能忘了前面。

而 **Transformer** 引入了一个核心概念：**注意力机制 (Attention Mechanism)**。

1.  **全局视野**：它不再是一个字一个字读，而是一眼看到整句话。
2.  **关注重点**：当它处理“苹果”这个词时，它会根据上下文判断这是“水果”还是“手机品牌”。如果句子里有“吃”，它会把更多的**注意力**分配给“水果”这个语义。
3.  **并行计算**：因为不需要按顺序读，它可以同时处理所有单词，这使得它的训练速度比以前的模型快得多。

正是因为这种架构，才诞生了现在的 GPT、Claude 和 DeepSeek 等强大的大模型。`
];

export const streamGeminiResponse = async (
  apiKey: string,
  modelType: ModelType,
  history: { role: string; parts: { text: string }[] }[],
  prompt: string,
  attachments: Attachment[],
  onChunk: (text: string) => void,
  onFinish: () => void,
  onError: (error: Error) => void
) => {
  // Simulate network latency
  await new Promise(resolve => setTimeout(resolve, 600));

  try {
    // Pick a random response from the mock sets
    const responseTemplate = MOCK_RESPONSES[Math.floor(Math.random() * MOCK_RESPONSES.length)];
    
    let fullText = "";
    // If there are attachments, acknowledge them
    if (attachments.length > 0) {
      fullText += `[Received ${attachments.length} attachment(s): ${attachments.map(a => a.name).join(', ')}]\n\n`;
    }
    fullText += responseTemplate;

    let currentText = "";
    const chunkSize = 2; // Characters per chunk
    const delay = 15;    // Milliseconds per chunk (typing speed)

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

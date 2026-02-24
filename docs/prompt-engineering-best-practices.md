# Prompt Engineering Best Practices for AgentProject

本指南基于当前项目中的 RAG、工作流与工具调用场景，总结了一套可复用的 Prompt 模板库与工程化实践，尤其面向：
- RAG 查询类：文档检索、摘要生成、问答
- 工作流类：任务分解、状态判断、人机协同
- 工具调用类：参数提取、错误恢复、结果验证

模板在代码中通过 `PromptsService` 和组合模板引擎进行管理，并可与 Prompt 性能分析与 MLflow 实验追踪联动。  

---

## 1. RAG 查询类模板

### 1.1 文档检索与问答（RAG Query + QA）

- 模板 ID：`rag_query_best_practice`
- 分类：`rag`
- 典型用法：
  - 企业知识库问答：合规、SOP、安全基线
  - 架构设计与运维手册查询
  - 事故复盘文档问答

#### 使用场景说明

当用户在自然语言中提出“为什么/如何/是什么/有哪些”等问题，并希望答案严格基于企业知识库时，推荐使用该模板。它会：
- 对用户问题进行重述与结构化
- 结合已检索到的文档上下文构造系统指令
- 要求模型显式区分“有依据”和“无依据”的内容
- 输出答案 + 引用源信息 + 置信度

#### Few-shot 示例（3 个）

1. 合规与安全基线
   - 输入：
     - `query`：公司最新的安全基线有哪些要求？
     - `context`：来自安全基线文档的多个章节内容
   - 模型行为期望：
     - 列出关键要求（密码策略、访问控制、日志留存周期等）
     - 指明对应文档章节/标题
     - 当某项内容不在文档中时直接说明“未在当前知识库中发现”

2. 架构设计
   - 输入：
     - `query`：订单服务的限流策略是如何配置的？
     - `context`：架构设计说明 + 部署文档片段
   - 模型行为期望：
     - 描述限流算法（如 Token Bucket、Leaky Bucket）
     - 说明限流阈值、降级策略以及监控告警
     - 引用涉及的组件名称与配置文件位置

3. 故障复盘
   - 输入：
     - `query`：最近一次重大故障的原因和改进措施是什么？
     - `context`：事后复盘文档、故障时间线、影响评估
   - 模型行为期望：
     - 概括根因（Root Cause）和影响范围
     - 分条列出改进措施和负责团队
     - 说明哪些结论来自复盘文档，哪些是合理推断（并标记）

#### 参数说明与类型约束

- `query`：string，必填  
  - 用户问题的原文，建议保持自然语言且包含业务上下文。  
- `context`：string，必填  
  - RAG 检索出来的文档片段拼接文本，应由服务侧控制长度和顺序。  
- `language`：string，可选，默认 `"zh-CN"`  
  - 回答语言，常见值：`zh-CN`、`en`、`ja`。  
- `include_sources`：boolean，可选，默认 `true`  
  - 是否在回答中带上引用来源列表。  

#### 预期输出格式（JSON Schema）

```json
{
  "type": "object",
  "properties": {
    "answer": { "type": "string" },
    "sources": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "document_id": { "type": "string" },
          "title": { "type": "string" },
          "snippet": { "type": "string" }
        },
        "required": [ "document_id", "snippet" ]
      }
    },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 }
  },
  "required": [ "answer" ]
}
```

#### 成本估算（token 数）

- 查询本身：50–200 tokens  
- 文档上下文：1000–2000 tokens（取决于 chunk 拼接策略）  
- 指令 + 模板结构：200–300 tokens  
- 回答：300–800 tokens  

综合估算：**1500–3000 tokens / 次**，在代码中模板估算值为 `EstimatedTokenCost = 1500`，实际由上下文长度与回答长度决定。  

---

### 1.2 文档分析与摘要（Document Analysis & Summarization）

- 模板 ID：`rag_document_analysis`
- 分类：`rag`

#### 使用场景说明

在将文档纳入知识库前，需要生成结构化摘要、关键词、可回答问题列表，用于：
- 提升检索质量（关键词、主题）
- 让运营或业务快速了解文档内容
- 支撑后续的 RAG 问答与多文档分析

#### Few-shot 示例

1. 产品需求文档摘要  
2. 安全策略文档摘要  
3. 架构评审文档摘要  

（在代码中由 `Examples` 字段维护，建议将真实内部文档的脱敏示例补齐。）  

#### 参数说明与类型约束

- `title`：string，必填  
- `document_type`：string，默认 `"general"`，可根据内部约定扩展：`"spec"`, `"design"`, `"postmortem"` 等。  
- `content`：string，必填，建议控制在 10k tokens 内。  

#### 预期输出格式（推荐）

可以约定输出为 Markdown + 结构化段落，或扩展为 JSON Schema：

```json
{
  "type": "object",
  "properties": {
    "summary": { "type": "string" },
    "topics": { "type": "array", "items": { "type": "string" } },
    "keywords": { "type": "array", "items": { "type": "string" } },
    "potential_questions": { "type": "array", "items": { "type": "string" } }
  },
  "required": [ "summary" ]
}
```

#### 成本估算

- 长文档内容：2000–8000 tokens  
- 模板与指令：200–300 tokens  
- 输出摘要与结构化内容：300–800 tokens  

---

## 2. 工作流类模板

### 2.1 任务分解与计划（Workflow Task Decomposition）

- 组合模板 ID：`workflow_task_decomposition`
- 基础模板 ID：`workflow_task_breakdown`
- 分类：`workflow`

#### 使用场景说明

针对中长期、跨多步骤的任务（例如“为现有应用接入 RAG”、“构建监控体系”、“对接第三方支付”），需要：
- 将目标拆成多步、每步有明确的类型和输入输出
+- 建立步骤间依赖关系
 - 为状态机和工作流引擎提供结构化输入

#### Few-shot 示例

1. 接入 RAG 知识库  
2. 搭建 Prometheus+Grafana 监控  
3. 对接第三方支付网关  

（具体文本对应到组合模板的 `Examples` 字段，可在代码中继续充实。）  

#### 参数说明与类型约束

- `project_name`：string，必填  
- `objective`：string，必填，建议包含业务目标而不是仅技术动作。  
- `timeline`：string，可选，如“2 周内完成”、“本季度”。  
- `resources`：string，可选，可包括人员、环境、预算等信息。  

#### 预期输出格式（JSON Schema）

参考组合模板中的 Schema：

```json
{
  "type": "object",
  "properties": {
    "steps": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "index": { "type": "integer" },
          "text": { "type": "string" },
          "type": { "type": "string" },
          "state": {
            "type": "string",
            "enum": ["NotStarted", "InProgress", "Completed", "Blocked"]
          }
        },
        "required": ["index", "text", "type"]
      }
    },
    "dependencies": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "from": { "type": "integer" },
          "to": { "type": "integer" }
        },
        "required": ["from", "to"]
      }
    }
  },
  "required": ["steps"]
}
```

该结构可直接映射到 `WorkflowPlan` 和 `WorkflowStep`，以及工作流引擎的状态和依赖关系。  

#### 成本估算

- 输入描述：200–500 tokens  
- 输出步骤与依赖：300–1000 tokens  

---

## 3. 工具调用类模板

### 3.1 工具参数提取与调用（Tool Invocation）

- 组合模板 ID：`tool_invocation_best_practice`
- 基础模板 ID：`code_api_documentation`（作为指令部分）
- 分类：`tools`

#### 使用场景说明

在需要从自然语言请求中提取结构化参数、决定是否调用某个工具/接口，并对结果进行校验时使用，例如：
- SQL 查询构造与参数校验
- Kubernetes 部署参数提取（镜像、replicas、ports 等）
- 压测工具参数提取与边界检查

#### Few-shot 示例

1. SQL 聚合查询参数提取  
2. Kubernetes Deployment 参数提取  
3. 压测任务参数提取与安全检查  

#### 参数说明与类型约束

- `tool_name`：string，必填  
- `natural_language_request`：string，必填，用户原始请求。  
- `schema`：string，必填，对应工具参数的 JSON Schema，服务侧可自动生成。  

#### 预期输出格式（JSON Schema）

```json
{
  "type": "object",
  "properties": {
    "arguments": {
      "type": "object"
    },
    "validation_errors": {
      "type": "array",
      "items": { "type": "string" }
    },
    "should_invoke": { "type": "boolean" }
  },
  "required": ["arguments", "should_invoke"]
}
```

#### 成本估算

- 指令模板 + Schema：200–400 tokens  
- 用户请求：50–200 tokens  
- 输出参数对象和校验结果：200–500 tokens  

---

## 4. 动态模板组合与渲染

项目中通过 `CompositePromptTemplate` + `PromptComposer` 实现了：
- 模板继承和组合
- 变量插值与条件渲染
- 上下文长度自适应裁剪

### 4.1 模板定义（CompositePromptTemplate）

位置：`Agent.Application/Services/Prompts/PromptCompositionModels.cs`  

关键字段：
- `BaseTemplateId`：基础模板（通常是系统 Prompt）
- `IncludeTemplateIds`：可选的附加模板（如摘要、注意事项等）
- `Variables`：参数列表，附带名称、类型与必填属性
- `Examples`：Few-shot 示例文本列表
- `OutputSchemaJson`：预期输出 JSON Schema 字符串
- `EstimatedTokenCost`：预估 token 成本

### 4.2 渲染流程（PromptComposer）

位置：`Agent.Application/Services/Prompts/PromptComposer.cs`  

流程：
1. 根据 `BaseTemplateId` 和 `IncludeTemplateIds` 依次拼接模板内容。  
2. 使用 `{variable}` 占位符进行变量替换（区分大小写）。  
3. 支持简单条件渲染：
   - 语法：`[[if:variable_name]]...[[endif]]`
   - 当变量被视为“真值”时才输出包裹内容。  
4. 对上下文进行自适应裁剪：
   - 通过字符长度近似估算 token 数（约 4 字符 ≈ 1 token）。  
   - 如果预计总 token 超过 `MaxTokens`，优先裁剪 `Context` 字段。  

该设计参考了 LangChain/Anthropic 等生态中对 Prompt 模板化的实践，但实现保持轻量级，避免引入额外依赖。  

### 4.3 使用方式示例

以 RAG 查询模板为例，在业务代码中可以：

1. 通过 `IPromptsService.GetCompositeTemplateAsync("rag_query_best_practice")` 获取模板定义。  
2. 组装 `CompositePromptRequest`：
   - `Template`：上述模板  
   - `Variables`：`query`、`language`、`include_sources` 等  
   - `Context`：RagService 返回的文档上下文  
   - `MaxTokens`：根据模型上下文长度设置（例如 4096 或 8192）  
3. 调用 `IPromptsService.RenderCompositePrompt(request)` 获取最终 Prompt 字符串，传给 LLM。  

---

## 5. 与性能分析和 A/B 测试联动

借助前面实现的 Prompt 性能分析和 A/B 测试能力，可以对模板库进行持续优化：

- 为不同的 `CompositePromptTemplate` 设置不同的 `experiment_id` 和 `variant_name`。  
- 使用 `PromptAnalyticsService`：
  - 记录执行日志：`LogExecutionAsync`（包含 `quality_score` 与 `cost_usd`）  
  - 聚合指标：`GetVariantStatisticsAsync`（成功率、平均延迟、平均成本）  
  - 做 A/B 对比：`RunAbTestAsync`（质量分 / 成功率 / 成本）  
- 将结果同步到 MLflow：
  - 使用 `MlflowTrackingService.TrackPromptExperimentAsync` 把各变体指标写入实验系统。  

这使得 Prompt 模板库不仅是“静态最佳实践集合”，还能在真实流量下通过实验不断迭代。  

---

## 6. 与社区最佳实践的对齐

本模板库设计参考了：

- OpenAI Prompt Engineering Guide：
  - 将复杂任务拆解为“说明 + 示例 + 约束 + 输出结构”几个部分。  
  - 对回答结构和格式进行显式约束，提高解析稳定性。  

- Anthropic Prompt Engineering：
  - 强调系统指令与用户指令分层，避免指令冲突。  
  - 鼓励使用 few-shot 示例覆盖边界情况和常见错误。  

- LangChain Prompt Templates：
  - 鼓励模板复用与组合，通过“基础模板 + 变体”方式支持不同场景。  
  - 将参数、上下文、few-shot 示例都视为可组合片段。  

在 AgentProject 中，这些理念通过以下方式落地：

- 使用 `PromptTemplate` + `CompositePromptTemplate` 将 Prompt 变成一等公民，具备版本、实验与组合能力。  
- 在 RAG 和工作流场景中统一通过模板驱动 LLM 交互，减少散落在代码中的“魔法字符串”。  
- 将 Prompt 表现指标与 A/B 测试结果回流到 MLflow，支持数据驱动的 Prompt 优化闭环。  


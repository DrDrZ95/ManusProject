# Workflow Management Integration Documentation
# 工作流管理集成文档

## Overview - 概述

This document describes the integration of workflow management functionality from the OpenManus project into the AgentWebApi. The implementation provides comprehensive workflow planning, task tracking, and todo list file interaction capabilities.

本文档描述了将OpenManus项目的工作流管理功能集成到AgentWebApi中的实现。该实现提供了全面的工作流规划、任务跟踪和待办事项列表文件交互功能。

## Architecture - 架构

### Core Components - 核心组件

1. **IWorkflowService** - Service interface - 服务接口
2. **WorkflowService** - Service implementation - 服务实现
3. **WorkflowController** - REST API controller - REST API控制器
4. **WorkflowExtensions** - Dependency injection extensions - 依赖注入扩展

### Key Features from OpenManus - 来自OpenManus的关键功能

| Feature | OpenManus | AgentWebApi Implementation |
|---------|-----------|---------------------------|
| Plan Creation | Python planning tool | C# workflow service - C#工作流服务 |
| Step Status Tracking | Enum-based status | Enhanced status with timestamps - 带时间戳的增强状态 |
| Todo List Generation | Markdown output | Markdown with Chinese support - 支持中文的Markdown |
| File Interaction | File-based persistence | File I/O with parsing - 带解析的文件I/O |
| Progress Tracking | Basic completion | Detailed progress metrics - 详细进度指标 |

## Data Models - 数据模型

### PlanStepStatus Enumeration - 计划步骤状态枚举

```csharp
public enum PlanStepStatus
{
    NotStarted,    // 未开始 - [ ]
    InProgress,    // 进行中 - [→]
    Completed,     // 已完成 - [✓]
    Blocked        // 已阻塞 - [!]
}
```

### WorkflowPlan Model - 工作流计划模型

```csharp
public class WorkflowPlan
{
    public string Id { get; set; }                           // 计划ID
    public string Title { get; set; }                        // 计划标题
    public string Description { get; set; }                  // 计划描述
    public List<WorkflowStep> Steps { get; set; }           // 计划步骤
    public List<PlanStepStatus> StepStatuses { get; set; }  // 步骤状态
    public DateTime CreatedAt { get; set; }                 // 创建时间
    public DateTime UpdatedAt { get; set; }                 // 更新时间
    public Dictionary<string, object> Metadata { get; set; } // 元数据
    public int? CurrentStepIndex { get; set; }              // 当前步骤索引
    public List<string> ExecutorKeys { get; set; }          // 执行器键
}
```

### WorkflowStep Model - 工作流步骤模型

```csharp
public class WorkflowStep
{
    public int Index { get; set; }                          // 步骤索引
    public string Text { get; set; }                        // 步骤文本
    public string? Type { get; set; }                       // 步骤类型
    public PlanStepStatus Status { get; set; }              // 步骤状态
    public string? Result { get; set; }                     // 执行结果
    public DateTime? StartedAt { get; set; }                // 开始时间
    public DateTime? CompletedAt { get; set; }              // 完成时间
    public Dictionary<string, object> Metadata { get; set; } // 元数据
}
```

## API Endpoints - API端点

### 1. Create Workflow Plan - 创建工作流计划

```http
POST /api/workflow
Content-Type: application/json

{
  "title": "项目开发计划",
  "description": "完整的项目开发流程",
  "steps": [
    "需求分析",
    "[DESIGN] 系统设计",
    "[CODE] 编码实现",
    "[TEST] 测试验证",
    "部署上线"
  ],
  "executorKeys": ["developer", "tester"],
  "metadata": {
    "priority": "high",
    "deadline": "2024-01-31"
  }
}
```

**Response - 响应:**
```json
{
  "id": "plan_1703123456",
  "title": "项目开发计划",
  "description": "完整的项目开发流程",
  "steps": [
    {
      "index": 0,
      "text": "需求分析",
      "type": null,
      "status": "NotStarted",
      "result": null,
      "startedAt": null,
      "completedAt": null,
      "metadata": {}
    }
  ],
  "stepStatuses": ["NotStarted", "NotStarted", "NotStarted", "NotStarted", "NotStarted"],
  "createdAt": "2023-12-21T10:30:56Z",
  "updatedAt": "2023-12-21T10:30:56Z",
  "metadata": {
    "priority": "high",
    "deadline": "2024-01-31"
  },
  "currentStepIndex": 0,
  "executorKeys": ["developer", "tester"]
}
```

### 2. Get Workflow Plan - 获取工作流计划

```http
GET /api/workflow/{planId}
```

### 3. Update Step Status - 更新步骤状态

```http
PUT /api/workflow/{planId}/steps/{stepIndex}/status
Content-Type: application/json

{
  "status": "InProgress"
}
```

### 4. Get Current Step - 获取当前步骤

```http
GET /api/workflow/{planId}/current-step
```

### 5. Complete Step - 完成步骤

```http
POST /api/workflow/{planId}/steps/{stepIndex}/complete
Content-Type: application/json

{
  "result": "需求分析已完成，生成了详细的需求文档"
}
```

### 6. Generate Todo List - 生成待办事项列表

```http
GET /api/workflow/{planId}/todo
Accept: text/markdown
```

**Response - 响应:**
```markdown
# 项目开发计划

**计划ID**: plan_1703123456
**创建时间**: 2023-12-21 10:30:56
**最后更新**: 2023-12-21 11:15:30

**描述**: 完整的项目开发流程

## 进度概览 (Progress Overview)

- **总步骤数**: 5
- **已完成**: 1
- **进行中**: 1
- **已阻塞**: 0
- **完成度**: 20.0%

## 任务列表 (Task List)

[✓] **步骤 1**: 需求分析
  - *完成时间*: 2023-12-21 11:00:00
  - *执行结果*: 需求分析已完成，生成了详细的需求文档

[→] **步骤 2**: 系统设计 `[DESIGN]`
  - *开始时间*: 2023-12-21 11:15:30

[ ] **步骤 3**: 编码实现 `[CODE]`

[ ] **步骤 4**: 测试验证 `[TEST]`

[ ] **步骤 5**: 部署上线

## 状态说明 (Status Legend)

- `[ ]` 未开始 (Not Started)
- `[→]` 进行中 (In Progress)
- `[✓]` 已完成 (Completed)
- `[!]` 已阻塞 (Blocked)

## 元数据 (Metadata)

- **priority**: high
- **deadline**: 2024-01-31

---
*生成时间: 2023-12-21 11:15:30 UTC*
```

### 7. Save Todo List to File - 保存待办事项列表到文件

```http
POST /api/workflow/{planId}/todo/save
Content-Type: application/json

{
  "filePath": "/path/to/project_plan.md"
}
```

### 8. Load Todo List from File - 从文件加载待办事项列表

```http
POST /api/workflow/todo/load
Content-Type: application/json

{
  "filePath": "/path/to/project_plan.md"
}
```

### 9. Get Progress - 获取进度

```http
GET /api/workflow/{planId}/progress
```

**Response - 响应:**
```json
{
  "planId": "plan_1703123456",
  "totalSteps": 5,
  "completedSteps": 1,
  "inProgressSteps": 1,
  "blockedSteps": 0,
  "progressPercentage": 20.0,
  "currentStepIndex": 1,
  "isCompleted": false,
  "hasBlockedSteps": false
}
```

### 10. Delete Plan - 删除计划

```http
DELETE /api/workflow/{planId}
```

## Configuration - 配置

### appsettings.json Configuration - 配置文件设置

```json
{
  "Workflow": {
    "DefaultToDoDirectory": "todo_lists",
    "EnableAutoSave": true,
    "AutoSaveIntervalMinutes": 5,
    "MaxPlansInMemory": 100,
    "EnableDetailedLogging": true
  }
}
```

### Service Registration - 服务注册

```csharp
// In Program.cs
builder.Services.AddWorkflowServices(builder.Configuration);

// Or with custom options - 或使用自定义选项
builder.Services.AddWorkflowServices(options =>
{
    options.DefaultToDoDirectory = "custom_todo_dir";
    options.EnableAutoSave = false;
    options.MaxPlansInMemory = 50;
});
```

## Todo List File Format - 待办事项列表文件格式

### File Structure - 文件结构

The todo list files are generated in Markdown format with specific patterns that can be parsed back:
待办事项列表文件以Markdown格式生成，具有可以解析回来的特定模式：

1. **Plan Metadata Section - 计划元数据部分**
   ```markdown
   # 计划标题
   **计划ID**: plan_1703123456
   **创建时间**: 2023-12-21 10:30:56
   **最后更新**: 2023-12-21 11:15:30
   **描述**: 计划描述
   ```

2. **Progress Overview - 进度概览**
   ```markdown
   ## 进度概览 (Progress Overview)
   - **总步骤数**: 5
   - **已完成**: 1
   - **进行中**: 1
   - **已阻塞**: 0
   - **完成度**: 20.0%
   ```

3. **Task List with Status Markers - 带状态标记的任务列表**
   ```markdown
   ## 任务列表 (Task List)
   [✓] **步骤 1**: 需求分析
   [→] **步骤 2**: 系统设计 `[DESIGN]`
   [ ] **步骤 3**: 编码实现 `[CODE]`
   [!] **步骤 4**: 测试验证 `[TEST]`
   ```

### Status Markers - 状态标记

| Marker | Status | Description |
|--------|--------|-------------|
| `[ ]` | NotStarted | 未开始 |
| `[→]` | InProgress | 进行中 |
| `[✓]` | Completed | 已完成 |
| `[!]` | Blocked | 已阻塞 |

### Step Type Extraction - 步骤类型提取

Step types are extracted from text patterns like `[TYPE]`:
步骤类型从类似`[TYPE]`的文本模式中提取：

- `[DESIGN]` → type: "design"
- `[CODE]` → type: "code"
- `[TEST]` → type: "test"
- `[DEPLOY]` → type: "deploy"

## Usage Examples - 使用示例

### Basic Workflow Creation - 基本工作流创建

```csharp
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    
    public ProjectController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }
    
    [HttpPost("create-project-plan")]
    public async Task<IActionResult> CreateProjectPlan([FromBody] CreateProjectRequest request)
    {
        // 创建工作流计划 - Create workflow plan
        var planRequest = new CreatePlanRequest
        {
            Title = $"项目计划: {request.ProjectName}",
            Description = request.Description,
            Steps = new List<string>
            {
                "需求分析和规划",
                "[DESIGN] 系统架构设计",
                "[CODE] 核心功能开发",
                "[TEST] 功能测试",
                "[DEPLOY] 部署上线"
            },
            ExecutorKeys = new List<string> { "developer", "tester", "devops" },
            Metadata = new Dictionary<string, object>
            {
                { "projectType", request.ProjectType },
                { "priority", request.Priority },
                { "estimatedDays", request.EstimatedDays }
            }
        };
        
        var plan = await _workflowService.CreatePlanAsync(planRequest);
        
        // 自动保存待办事项列表 - Auto-save todo list
        var todoPath = $"projects/{request.ProjectName}/todo.md";
        await _workflowService.SaveToDoListToFileAsync(plan.Id, todoPath);
        
        return Ok(new { planId = plan.Id, todoPath });
    }
}
```

### Step Execution with Progress Tracking - 带进度跟踪的步骤执行

```csharp
[HttpPost("execute-step")]
public async Task<IActionResult> ExecuteStep([FromBody] ExecuteStepRequest request)
{
    // 获取当前步骤 - Get current step
    var currentStep = await _workflowService.GetCurrentStepAsync(request.PlanId);
    if (currentStep == null)
    {
        return BadRequest("No active step found - 没有找到活动步骤");
    }
    
    try
    {
        // 执行步骤逻辑 - Execute step logic
        var result = await ExecuteStepLogic(currentStep);
        
        // 标记步骤完成 - Mark step as completed
        await _workflowService.CompleteStepAsync(
            request.PlanId, 
            currentStep.Index, 
            result);
        
        // 获取更新后的进度 - Get updated progress
        var progress = await _workflowService.GetProgressAsync(request.PlanId);
        
        // 更新待办事项文件 - Update todo file
        var todoPath = $"projects/{request.ProjectName}/todo.md";
        await _workflowService.SaveToDoListToFileAsync(request.PlanId, todoPath);
        
        return Ok(new { 
            stepCompleted = true, 
            result, 
            progress,
            nextStep = await _workflowService.GetCurrentStepAsync(request.PlanId)
        });
    }
    catch (Exception ex)
    {
        // 标记步骤为阻塞 - Mark step as blocked
        await _workflowService.UpdateStepStatusAsync(
            request.PlanId, 
            currentStep.Index, 
            PlanStepStatus.Blocked);
        
        return StatusCode(500, $"Step execution failed: {ex.Message}");
    }
}
```

### File-based Plan Synchronization - 基于文件的计划同步

```csharp
[HttpPost("sync-from-file")]
public async Task<IActionResult> SyncFromFile([FromBody] SyncRequest request)
{
    try
    {
        // 从文件加载计划状态 - Load plan status from file
        var planId = await _workflowService.LoadToDoListFromFileAsync(request.FilePath);
        
        if (planId == null)
        {
            return NotFound("Could not load plan from file - 无法从文件加载计划");
        }
        
        // 获取同步后的计划 - Get synchronized plan
        var plan = await _workflowService.GetPlanAsync(planId);
        var progress = await _workflowService.GetProgressAsync(planId);
        
        return Ok(new { 
            message = "Plan synchronized from file - 计划已从文件同步",
            planId,
            plan,
            progress
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Sync failed: {ex.Message}");
    }
}
```

## Migration from OpenManus - 从OpenManus迁移

### What Was Adapted - 已适配的内容

1. **Core Planning Logic - 核心规划逻辑**
   - Plan creation and management - 计划创建和管理
   - Step status tracking - 步骤状态跟踪
   - Progress calculation - 进度计算

2. **Status Management - 状态管理**
   - Enum-based status system - 基于枚举的状态系统
   - Status marker symbols - 状态标记符号
   - Active status detection - 活动状态检测

3. **File Interaction - 文件交互**
   - Markdown generation - Markdown生成
   - File parsing and loading - 文件解析和加载
   - Status synchronization - 状态同步

### What Was Enhanced - 已增强的内容

1. **Data Models - 数据模型**
   - Comprehensive metadata support - 全面的元数据支持
   - Timestamp tracking - 时间戳跟踪
   - Detailed progress metrics - 详细进度指标

2. **API Design - API设计**
   - RESTful endpoints - RESTful端点
   - Comprehensive error handling - 全面错误处理
   - Multiple response formats - 多种响应格式

3. **Configuration - 配置**
   - Flexible options system - 灵活选项系统
   - Environment-specific settings - 环境特定设置

### What Was Simplified - 已简化的内容

1. **Agent Integration - 代理集成**
   - Removed complex agent orchestration - 移除复杂的代理编排
   - Simplified executor selection - 简化执行器选择

2. **LLM Integration - LLM集成**
   - Manual plan creation instead of AI-generated - 手动计划创建而非AI生成
   - Direct step management - 直接步骤管理

## Performance Considerations - 性能考虑

### 1. Memory Management - 内存管理
- In-memory plan storage with configurable limits - 带可配置限制的内存计划存储
- Automatic cleanup of old plans - 自动清理旧计划
- Efficient data structures - 高效数据结构

### 2. File I/O Optimization - 文件I/O优化
- Asynchronous file operations - 异步文件操作
- Batch file updates - 批量文件更新
- Directory management - 目录管理

### 3. Concurrent Access - 并发访问
- Thread-safe operations - 线程安全操作
- Lock-based synchronization - 基于锁的同步
- Atomic status updates - 原子状态更新

## Monitoring and Logging - 监控和日志

### Log Levels - 日志级别

```json
{
  "Logging": {
    "LogLevel": {
      "AgentWebApi.Services.Workflow": "Debug"
    }
  }
}
```

### Key Log Events - 关键日志事件

- Plan creation and deletion - 计划创建和删除
- Step status changes - 步骤状态变更
- File operations - 文件操作
- Error conditions - 错误条件

## Future Enhancements - 未来增强

### Planned Features - 计划功能

1. **Database Persistence - 数据库持久化**
   - Replace in-memory storage - 替换内存存储
   - Entity Framework integration - Entity Framework集成

2. **AI Integration - AI集成**
   - Automatic plan generation - 自动计划生成
   - Smart step recommendations - 智能步骤推荐

3. **Real-time Updates - 实时更新**
   - SignalR integration - SignalR集成
   - Live progress tracking - 实时进度跟踪

4. **Advanced File Formats - 高级文件格式**
   - JSON export/import - JSON导出/导入
   - Excel integration - Excel集成

## Troubleshooting - 故障排除

### Common Issues - 常见问题

1. **File Access Errors - 文件访问错误**
   - Check file permissions - 检查文件权限
   - Verify directory existence - 验证目录存在

2. **Plan Not Found - 计划未找到**
   - Verify plan ID format - 验证计划ID格式
   - Check memory limits - 检查内存限制

3. **Status Parsing Errors - 状态解析错误**
   - Validate markdown format - 验证markdown格式
   - Check status markers - 检查状态标记

### Debug Mode - 调试模式

Enable debug logging for detailed workflow information:
启用调试日志以获取详细工作流信息：

```json
{
  "Logging": {
    "LogLevel": {
      "AgentWebApi.Services.Workflow": "Debug"
    }
  }
}
```

## Conclusion - 结论

The Workflow Management integration provides a comprehensive solution for managing business processes and task tracking. Based on the OpenManus planning.py functionality, it offers enhanced features while maintaining the core workflow concepts. The todo list file interaction capability enables seamless integration with external tools and manual workflow management.

工作流管理集成为管理业务流程和任务跟踪提供了全面的解决方案。基于OpenManus planning.py功能，它提供了增强功能，同时保持了核心工作流概念。待办事项列表文件交互功能使与外部工具和手动工作流管理的无缝集成成为可能。


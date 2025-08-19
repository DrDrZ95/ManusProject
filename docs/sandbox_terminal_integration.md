# Sandbox Terminal Integration Documentation
# 沙盒终端集成文档

## Overview - 概述

This document describes the integration of sandbox terminal functionality from the AI-Agent project into the Agent.Api. The implementation provides a secure, local sandbox environment for command execution.

本文档描述了将AI-Agent项目的沙盒终端功能集成到Agent.Api中的实现。该实现提供了一个安全的本地沙盒环境用于命令执行。

## Architecture - 架构

### Core Components - 核心组件

1. **ISandboxTerminalService** - Service interface - 服务接口
2. **SandboxTerminalService** - Service implementation - 服务实现
3. **SandboxTerminalController** - REST API controller - REST API控制器
4. **SandboxTerminalExtensions** - Dependency injection extensions - 依赖注入扩展

### Key Differences from AI-Agent - 与AI-Agent的主要差异

| Feature | AI-Agent | Agent.Api Implementation |
|---------|-----------|---------------------------|
| Environment | Docker containers | Local processes - 本地进程 |
| Language | Python | C# .NET |
| Session Management | Complex socket-based | Simplified process-based - 简化的基于进程的 |
| Security | Container isolation | Command filtering + sanitization - 命令过滤和清理 |

## API Endpoints - API端点

### 1. Execute Command - 执行命令
```http
POST /api/sandboxterminal/execute
Content-Type: application/json

{
  "command": "ls -la",
  "workingDirectory": "/tmp",
  "timeout": 30
}
```

**Response - 响应:**
```json
{
  "exitCode": 0,
  "standardOutput": "total 8\ndrwxrwxrwt 2 root root 4096 ...",
  "standardError": "",
  "executionTimeMs": 125,
  "isSuccess": true,
  "combinedOutput": "total 8\ndrwxrwxrwt 2 root root 4096 ..."
}
```

### 2. Execute Command with Streaming - 流式执行命令
```http
POST /api/sandboxterminal/execute/stream
Content-Type: application/json

{
  "command": "ping -c 5 google.com",
  "timeout": 30
}
```

**Response - 响应:** (Streaming text/plain)
```
[STDOUT] PING google.com (142.250.191.14) 56(84) bytes of data.
[STDOUT] 64 bytes from lga25s62-in-f14.1e100.net (142.250.191.14): icmp_seq=1 ttl=118 time=12.3 ms
...
```

### 3. Working Directory Management - 工作目录管理

**Get Working Directory - 获取工作目录:**
```http
GET /api/sandboxterminal/workdir
```

**Set Working Directory - 设置工作目录:**
```http
POST /api/sandboxterminal/workdir
Content-Type: application/json

{
  "path": "/home/user/projects"
}
```

### 4. Command Safety Check - 命令安全检查
```http
POST /api/sandboxterminal/check-command
Content-Type: application/json

{
  "command": "rm -rf /"
}
```

**Response - 响应:**
```json
{
  "isSafe": false,
  "command": "rm -rf /",
  "message": "Command contains potentially dangerous operations"
}
```

### 5. System Information - 系统信息
```http
GET /api/sandboxterminal/system-info
```

### 6. Health Check - 健康检查
```http
GET /api/sandboxterminal/health
```

## Configuration - 配置

### appsettings.json Configuration - 配置文件设置

```json
{
  "SandboxTerminal": {
    "DefaultWorkingDirectory": "/tmp/sandbox",
    "DefaultTimeout": 30,
    "MaxTimeout": 300,
    "EnableLogging": true,
    "EnableSanitization": true,
    "AllowedCommands": [],
    "BlockedCommands": [
      "rm -rf /",
      "rm -rf /*",
      "mkfs",
      "dd if=/dev/zero",
      ":(){ :|:& };:",
      "shutdown",
      "reboot"
    ],
    "EnvironmentVariables": {
      "TERM": "xterm-256color",
      "LANG": "en_US.UTF-8",
      "PATH": "/usr/local/bin:/usr/bin:/bin"
    }
  }
}
```

### Service Registration - 服务注册

```csharp
// In Program.cs
builder.Services.AddSandboxTerminal(builder.Configuration);

// Or with custom options - 或使用自定义选项
builder.Services.AddSandboxTerminal(options =>
{
    options.DefaultTimeout = 60;
    options.EnableSanitization = true;
    options.BlockedCommands.Add("custom-dangerous-command");
});
```

## Security Features - 安全特性

### 1. Command Filtering - 命令过滤

The service implements multiple layers of security:
服务实现了多层安全机制：

- **Blocked Commands List** - 阻止命令列表
- **Allowed Commands List** - 允许命令列表（可选）
- **Dangerous Pattern Detection** - 危险模式检测
- **Path Traversal Prevention** - 路径遍历防护

### 2. Command Sanitization - 命令清理

```csharp
// Example of command sanitization - 命令清理示例
var sanitized = command
    .Replace("$(", "\\$(")  // Escape command substitution - 转义命令替换
    .Replace("`", "\\`")    // Escape backticks - 转义反引号
    .Replace("|", "\\|");   // Escape pipes - 转义管道
```

### 3. Timeout Protection - 超时保护

All commands are executed with configurable timeouts to prevent:
所有命令都使用可配置的超时执行，以防止：

- Infinite loops - 无限循环
- Long-running processes - 长时间运行的进程
- Resource exhaustion - 资源耗尽

## Usage Examples - 使用示例

### Basic Command Execution - 基本命令执行

```csharp
[ApiController]
public class MyController : ControllerBase
{
    private readonly ISandboxTerminalService _terminal;
    
    public MyController(ISandboxTerminalService terminal)
    {
        _terminal = terminal;
    }
    
    [HttpPost("run-script")]
    public async Task<IActionResult> RunScript([FromBody] string script)
    {
        // 检查命令安全性 - Check command safety
        if (!_terminal.IsCommandSafe(script))
        {
            return BadRequest("Unsafe command detected");
        }
        
        // 执行命令 - Execute command
        var result = await _terminal.ExecuteCommandAsync(script);
        
        if (result.IsSuccess)
        {
            return Ok(result.StandardOutput);
        }
        else
        {
            return BadRequest(result.StandardError);
        }
    }
}
```

### Streaming Command Execution - 流式命令执行

```csharp
[HttpGet("stream-logs")]
public async Task StreamLogs()
{
    Response.ContentType = "text/plain";
    
    await foreach (var line in _terminal.ExecuteCommandStreamAsync("tail -f /var/log/app.log"))
    {
        await Response.WriteAsync($"{line}\n");
        await Response.Body.FlushAsync();
    }
}
```

## Error Handling - 错误处理

### Common Error Scenarios - 常见错误场景

1. **Command Timeout - 命令超时**
   ```json
   {
     "exitCode": -1,
     "standardError": "Command timed out after 30 seconds",
     "isSuccess": false
   }
   ```

2. **Unsafe Command - 不安全命令**
   ```json
   {
     "exitCode": -1,
     "standardError": "Command blocked for security reasons",
     "isSuccess": false
   }
   ```

3. **Working Directory Error - 工作目录错误**
   ```json
   {
     "success": false,
     "message": "The specified path is invalid or inaccessible"
   }
   ```

## Performance Considerations - 性能考虑

### 1. Process Management - 进程管理
- Processes are properly disposed after execution - 进程在执行后正确释放
- Process trees are killed on timeout - 超时时终止进程树
- Memory usage is monitored - 监控内存使用

### 2. Concurrent Execution - 并发执行
- Multiple commands can run simultaneously - 多个命令可以同时运行
- Each request gets its own process - 每个请求获得自己的进程
- No shared state between executions - 执行之间无共享状态

### 3. Resource Limits - 资源限制
- Configurable timeouts - 可配置超时
- Working directory isolation - 工作目录隔离
- Environment variable control - 环境变量控制

## Monitoring and Logging - 监控和日志

### Log Levels - 日志级别

```json
{
  "Logging": {
    "LogLevel": {
      "Agent.Core.Services.Sandbox": "Debug"
    }
  }
}
```

### Key Log Events - 关键日志事件

- Command execution start/completion - 命令执行开始/完成
- Security violations - 安全违规
- Timeout events - 超时事件
- Error conditions - 错误条件

## Migration from AI-Agent - 从AI-Agent迁移

### What Was Adapted - 已适配的内容

1. **Core Terminal Logic** - 核心终端逻辑
   - Command execution - 命令执行
   - Output handling - 输出处理
   - Error management - 错误管理

2. **Security Features** - 安全特性
   - Command sanitization - 命令清理
   - Dangerous command detection - 危险命令检测
   - Path safety checks - 路径安全检查

### What Was Simplified - 已简化的内容

1. **Session Management** - 会话管理
   - No persistent sessions - 无持久会话
   - Stateless execution - 无状态执行

2. **Container Integration** - 容器集成
   - Local process execution instead of Docker - 本地进程执行而非Docker
   - Direct OS interaction - 直接操作系统交互

### What Was Enhanced - 已增强的内容

1. **API Design** - API设计
   - RESTful endpoints - RESTful端点
   - Comprehensive error handling - 全面错误处理
   - Streaming support - 流式支持

2. **Configuration** - 配置
   - Flexible options system - 灵活选项系统
   - Environment-specific settings - 环境特定设置

## Future Enhancements - 未来增强

### Planned Features - 计划功能

1. **Docker Integration** - Docker集成
   - Optional container execution - 可选容器执行
   - Resource limits - 资源限制

2. **Session Management** - 会话管理
   - Persistent terminal sessions - 持久终端会话
   - Interactive command execution - 交互式命令执行

3. **Advanced Security** - 高级安全
   - Sandboxing with chroot - 使用chroot的沙盒
   - User privilege management - 用户权限管理

## Troubleshooting - 故障排除

### Common Issues - 常见问题

1. **Permission Denied - 权限拒绝**
   - Check working directory permissions - 检查工作目录权限
   - Verify user context - 验证用户上下文

2. **Command Not Found - 命令未找到**
   - Check PATH environment variable - 检查PATH环境变量
   - Verify command installation - 验证命令安装

3. **Timeout Issues - 超时问题**
   - Increase timeout values - 增加超时值
   - Check for infinite loops - 检查无限循环

### Debug Mode - 调试模式

Enable debug logging for detailed execution information:
启用调试日志以获取详细执行信息：

```json
{
  "Logging": {
    "LogLevel": {
      "Agent.Core.Services.Sandbox": "Debug"
    }
  }
}
```

## Conclusion - 结论

The Sandbox Terminal integration provides a secure and efficient way to execute commands in a controlled environment. While simplified compared to the original AI-Agent Docker-based approach, it maintains the core functionality while being more suitable for local development and testing scenarios.

沙盒终端集成提供了在受控环境中执行命令的安全高效方式。虽然相比原始的AI-Agent基于Docker的方法有所简化，但它保持了核心功能，同时更适合本地开发和测试场景。


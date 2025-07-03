# AI-Agent eBPF Detective Module Integration

This document details the integration of an eBPF-based detective module into the `AgentWebApi` project, enabling low-level system monitoring and analysis.

## 1. Overview

The eBPF (extended Berkeley Packet Filter) detective module leverages the power of eBPF to provide deep insights into system activities such as CPU usage, memory consumption, network traffic, and process behavior. It uses `bpftrace` scripts to collect real-time data from the Linux kernel, offering unparalleled visibility for performance monitoring, troubleshooting, and security analysis.

## 2. Key Features

- **Real-time System Metrics**: Monitor CPU and memory usage directly from the kernel.
- **Network Activity Monitoring**: Trace TCP connections, data send/receive, and packet transmission.
- **Process Behavior Analysis**: Observe process execution, file operations, and memory allocations.
- **Low Overhead**: eBPF operates efficiently within the kernel, minimizing performance impact.
- **Extensible Scripting**: Easily add new monitoring capabilities by writing `bpftrace` scripts.
- **C# Integration**: Seamlessly interact with eBPF scripts and data from the .NET application.

## 3. File Hierarchy Design

The eBPF detective module is structured as an independent component within the `AgentWebApi` project:

```
AgentWebApi/
├── eBPF/
│   ├── Detective/
│   │   ├── IeBPFDetectiveService.cs   # eBPF 服务接口 - eBPF Service Interface
│   │   └── eBPFDetectiveService.cs    # eBPF 服务实现 - eBPF Service Implementation
│   ├── Controllers/
│   │   └── eBPFController.cs          # REST API 控制器 - REST API Controller
│   └── Scripts/                       # bpftrace 脚本目录 - bpftrace Scripts Directory
│       ├── cpu_usage.bt               # CPU 使用率脚本 - CPU Usage Script
│       ├── memory_usage.bt            # 内存使用率脚本 - Memory Usage Script
│       ├── network_monitor.bt         # 网络监控脚本 - Network Monitoring Script
│       └── process_monitor.bt         # 进程监控脚本 - Process Monitoring Script
└── Extensions/
    └── eBPFExtensions.cs              # 扩展方法，用于DI配置 - Extension Methods for DI Configuration
```

## 4. `bpftrace` Installation

`bpftrace` is a prerequisite for the eBPF detective module. It can be installed on Linux systems using the following commands:

```bash
sudo apt-get update
sudo apt-get install -y bpftrace
```

Verify the installation:

```bash
bpftrace --version
```

## 5. eBPF Case Grammar and Scripts

The `Scripts` directory contains various `bpftrace` scripts for different monitoring purposes. These scripts are written in the `bpftrace` language, which is a high-level tracing language for Linux eBPF.

### 5.1. CPU Usage (`cpu_usage.bt`)

This script calculates and reports the system's CPU usage percentage every second.

```bpftrace
#!/usr/local/bin/bpftrace

/*
 * cpu_usage.bt
 * 计算系统CPU使用率的bpftrace脚本
 * This bpftrace script calculates system CPU usage.
 */

BEGIN
{
    printf("Tracing CPU usage... Hit Ctrl-C to end.\n");
    @start_time = nsecs;
    @prev_idle = 0;
    @prev_total = 0;
}

interval:s:1
{
    $cpu_stats = ksym("kstat_cpu");
    $idle = $cpu_stats->cpus[0]->cp_idle;
    $total = $cpu_stats->cpus[0]->cp_user + $cpu_stats->cpus[0]->cp_nice + \
             $cpu_stats->cpus[0]->cp_system + $cpu_stats->cpus[0]->cp_idle + \
             $cpu_stats->cpus[0]->cp_iowait + $cpu_stats->cpus[0]->cp_irq + \
             $cpu_stats->cpus[0]->cp_softirq + $cpu_stats->cpus[0]->cp_steal;

    $idle_delta = $idle - @prev_idle;
    $total_delta = $total - @prev_total;

    if ($total_delta > 0) {
        $cpu_usage = (100.0 - ($idle_delta * 100.0 / $total_delta));
        printf("%.2f\n", $cpu_usage);
    }

    @prev_idle = $idle;
    @prev_total = $total;
}

END
{
    printf("CPU usage tracing ended.\n");
}
```

### 5.2. Memory Usage (`memory_usage.bt`)

This script monitors and reports the system's memory usage percentage every second.

```bpftrace
#!/usr/local/bin/bpftrace

/*
 * memory_usage.bt
 * 监控系统内存使用率的bpftrace脚本
 * This bpftrace script monitors system memory usage.
 */

BEGIN
{
    printf("Tracing memory usage... Hit Ctrl-C to end.\n");
}

interval:s:1
{
    $mem_total = ksym("MemTotal");
    $mem_free = ksym("MemFree");
    $buffers = ksym("Buffers");
    $cached = ksym("Cached");

    $mem_used = $mem_total - $mem_free - $buffers - $cached;

    if ($mem_total > 0) {
        $memory_usage_percent = ($mem_used * 100.0) / $mem_total;
        printf("%.2f\n", $memory_usage_percent);
    }
}

END
{
    printf("Memory usage tracing ended.\n");
}
```

### 5.3. Network Monitor (`network_monitor.bt`)

This script traces various network activities, including TCP connection establishment, data send/receive, and packet transmission at the device level.

```bpftrace
#!/usr/local/bin/bpftrace

/*
 * network_monitor.bt
 * 监控网络活动（TCP连接、数据包发送/接收）的bpftrace脚本
 * This bpftrace script monitors network activity (TCP connections, packet send/receive).
 */

BEGIN
{
    printf("Monitoring network activity... Hit Ctrl-C to end.\n");
}

kprobe:tcp_connect
{
    printf("TCP Connect: PID %d, Comm %s\n", pid, comm);
}

kprobe:tcp_sendmsg
{
    printf("TCP Send: PID %d, Comm %s, Size %d\n", pid, comm, arg2);
}

kprobe:tcp_recvmsg
{
    printf("TCP Recv: PID %d, Comm %s, Size %d\n", pid, comm, arg2);
}

kprobe:dev_queue_xmit
{
    printf("Net Xmit: PID %d, Comm %s, Len %d\n", pid, comm, arg1->len);
}

kprobe:netif_receive_skb
{
    printf("Net Recv: PID %d, Comm %s, Len %d\n", pid, comm, arg0->len);
}

END
{
    printf("Network monitoring ended.\n");
}
```

### 5.4. Process Monitor (`process_monitor.bt`)

This script monitors the activities of a specific process, including its execution, exit, file open/write operations, and memory allocations.

```bpftrace
#!/usr/local/bin/bpftrace

/*
 * process_monitor.bt
 * 监控特定进程活动的bpftrace脚本
 * This bpftrace script monitors activity of a specific process.
 */

BEGIN
{
    printf("Monitoring process ", comm, "... Hit Ctrl-C to end.\n");
}

tracepoint:sched:sched_process_exec
/comm == comm/
{
    printf("Process Exec: PID %d, Comm %s, Filename %s\n", pid, comm, args->filename);
}

tracepoint:sched:sched_process_exit
/comm == comm/
{
    printf("Process Exit: PID %d, Comm %s, Exit Code %d\n", pid, comm, args->exit_code);
}

kprobe:do_sys_openat2
/comm == comm/
{
    printf("File Open: PID %d, Comm %s, Filename %s\n", pid, comm, str(arg1));
}

kprobe:vfs_write
/comm == comm/
{
    printf("File Write: PID %d, Comm %s, Size %d\n", pid, comm, arg2);
}

kprobe:__kmalloc
/comm == comm/
{
    printf("Memory Alloc: PID %d, Comm %s, Size %d\n", pid, comm, arg0);
}

END
{
    printf("Process monitoring ended.\n");
}
```

## 6. Deployment Plan

### 6.1. Prerequisites

- **Linux Environment**: The eBPF module requires a Linux operating system with kernel version 4.9 or higher (for `bpftrace` compatibility).
- **`bpftrace` Installation**: Ensure `bpftrace` is installed and accessible on the system where `AgentWebApi` will run.
- **Root Privileges**: `bpftrace` typically requires root privileges to run. The `eBPFDetectiveService` uses `sudo` to execute `bpftrace` commands.

### 6.2. Integration Steps

1. **Copy eBPF Files**: Ensure the `eBPF` directory (containing `Detective`, `Controllers`, and `Scripts` subdirectories) is copied to the `AgentWebApi` project root.
2. **Add Dependencies**: Add necessary NuGet packages (e.g., `Microsoft.AspNetCore.Mvc.Core`, `Microsoft.Extensions.Logging.Abstractions`) to `AgentWebApi.csproj` if not already present.
3. **Register Services**: Register `IeBPFDetectiveService` and `eBPFDetectiveService` in the `Program.cs` using extension methods (e.g., `AddeBPFDetectiveServices()`).
4. **Map Controllers**: Ensure the eBPF controllers are mapped in `Program.cs`.

### 6.3. Example `Program.cs` Integration

```csharp
// In Program.cs

// Add eBPF Detective services
builder.Services.AddeBPFDetectiveServices(); // This extension method needs to be created

// ... other service configurations

var app = builder.Build();

// Map eBPF controllers
app.MapControllers();

// ... other pipeline configurations
```

### 6.4. Security Considerations

- **`sudo` Usage**: Running `bpftrace` with `sudo` grants root privileges. Ensure that the `AgentWebApi` application is properly secured and that only authorized users can trigger eBPF scripts.
- **Script Validation**: Implement robust validation for `bpftrace` script names and arguments to prevent malicious injection.
- **Least Privilege**: Consider running `AgentWebApi` with the minimum necessary privileges. For production environments, explore alternatives to `sudo` for eBPF execution, such as capabilities or `setuid` wrappers, if applicable and secure.

## 7. Usage Examples (API Endpoints)

Once deployed, you can interact with the eBPF detective module via its REST API endpoints:

- **Run a specific script**:
  `GET /api/ebpf/script/{scriptName}?args={scriptArguments}`
  Example: `GET /api/ebpf/script/cpu_usage.bt`

- **Get CPU usage**:
  `GET /api/ebpf/cpu-usage`

- **Get Memory usage**:
  `GET /api/ebpf/memory-usage`

- **Monitor network activity (streaming)**:
  `GET /api/ebpf/monitor/network?durationSeconds=60`

- **Monitor process activity (streaming)**:
  `GET /api/ebpf/monitor/process/{processName}?durationSeconds=60`
  Example: `GET /api/ebpf/monitor/process/nginx?durationSeconds=30`

This integration provides a powerful tool for system observability and security within your AI-Agent application.


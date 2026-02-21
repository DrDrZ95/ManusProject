# Agent Ops 严格无侵入（Strictly Non-Intrusive）采集约束说明

本文件用于在探测/观测板块中，建立一套**严格无侵入（Strictly Non-Intrusive）**的硬约束与可验证验收标准，指导后续 eBPF / EventPipe / OTEL Collector 等能力的设计与实现。

目标：在**不破坏业务进程行为**的前提下，获得足够的运行时可观测性，并为“严格模式 vs 可选增强模式”的能力分级提供统一基线。

参考资料（权威来源）：
- Microsoft .NET Profiling API & EventPipe  
  - Profiling API（不允许在严格模式中使用）  
    - https://learn.microsoft.com/dotnet/framework/unmanaged-api/profiling/  
  - EventPipe / Diagnostic IPC（允许作为“外部订阅者”使用）  
    - https://learn.microsoft.com/dotnet/core/diagnostics/eventpipe  
- Datadog Agent 架构与权限模型  
  - https://docs.datadoghq.com/agent/  
  - https://docs.datadoghq.com/agent/cluster_agent/  
- OpenTelemetry Collector 采集与处理流水线  
  - https://opentelemetry.io/docs/collector/  

---

## 1. Strictly Non-Intrusive 硬约束定义

在“严格模式（Strict Mode）”下，Agent 必须满足以下**硬约束**：

1. 不进入目标进程地址空间
   - 禁止 DLL 注入、`LD_PRELOAD`、`DYLD_INSERT_LIBRARIES`、`ptrace` 附加等技术。
   - 禁止使用 CLR Profiling API (`ICorProfilerCallback`, `COR_ENABLE_PROFILING`, `COR_PROFILER` 等) 将探针加载到 .NET 业务进程内部。

2. 不使用 Profiler/Instrumentation 类接口
   - 禁止使用任何会修改 JIT 行为或 IL 代码的 Profiling 接口（参见 Microsoft Profiling API 文档）。
   - 禁止在运行时启用 `--profiler`、`-agentlib` 等 JVM / CLR / native profiler 选项。

3. 不修改业务进程启动参数
   - 不增加、删除或修改业务进程启动的命令行参数。
   - 不强制要求注入调试标志、诊断参数（例如 `COMPlus_*`、`DOTNET_DiagnosticPorts` 等），除非业务团队主动配置并认可为非侵入。

4. 不修改业务进程环境变量
   - Agent 自身可以使用独立进程/容器内的环境变量，但**不得要求业务进程**增加特定诊断环境变量（如 Profiling / Tracing 相关标志）。

5. 不要求修改业务代码或重建镜像
   - 不要求在业务代码中显式引入 SDK（例如 APM SDK）作为 Strict 模式的前提，仅可作为“可选增强模式”。
   - 不要求业务镜像重新构建以加入特定探针、sidecar 等组件。

6. 仅通过操作系统或平台提供的**外部可观测接口**采集
   - 允许：eBPF、perf_event、/proc 文件系统、ETW、EventPipe、OpenTelemetry Collector 接收的 OTLP 数据等。
   - 禁止：对业务进程代码、JIT、字节码、IL 的动态改写。

7. 明确的资源占用与性能预算
   - Agent 必须在约定的 CPU / 内存 / IO 预算内运行，并具备限流与自我降级机制。
   - 采集逻辑应可配置采样率，避免对业务产生明显抖动。

---

## 2. 可验证验收测试（自动化为主）

本节为每条硬约束定义对应的**验收测试思路**，优先通过自动化脚本和 CI 任务执行。

### 2.1 进程模块检查（Module / Library Scan）

目标：验证 Agent 未注入到目标业务进程内部。

- 检查内容：
  - Windows：通过 `EnumProcessModules` 或 WMI/PowerShell，检查业务进程的模块列表中**不存在** Agent DLL、Profiling 相关 DLL。
  - Linux：检查 `/proc/<pid>/maps`，确认不存在 Agent SO、`libprofiler`、`ld_preload` 相关库。
- 自动化测试示例（伪代码）：
  - 启动业务基准进程（无 Agent），记录模块列表 baseline。
  - 启动 Agent（Strict 模式），再次获取模块列表。
  - 断言：两次结果的差异中**不包含 Agent 相关模块**。

### 2.2 采集前后线程 / 句柄差异

目标：确保 Agent 不在业务进程内部创建额外线程或打开额外句柄。

- 检查内容：
  - Windows：使用 `GetProcessHandleCount`、`GetProcessInformation`、性能计数器检查线程数、句柄数。
  - Linux：比较 `/proc/<pid>/task` 数量和 `/proc/<pid>/fd` 数量。
- 自动化流程：
  1. 在无 Agent 环境下运行业务 N 分钟，记录线程/句柄数分布（作为 baseline）。
  2. 启用 Agent 严格模式，再次记录同样指标。
  3. 断言：线程/句柄数变化在预设阈值内（例如 `< 5%`），且**无新增线程属于 Agent 模块栈帧**。

### 2.3 性能预算与限流策略

目标：保证 Agent 的 CPU / 内存 / 网络开销在可接受范围内。

- 参考基准：
  - CPU：额外占用长期平均 < 3%，短时峰值 < 10%。
  - 内存：常驻内存 < 200 MB（可按场景调整），无渐进性泄漏。
  - 网络：每分钟导出数据上限（如 < 5 MB/min），并可通过配置收紧。
- 自动化流程：
  1. 使用标准压测（如 k6 / JMeter）对业务施加固定流量。
  2. 记录未开启 Agent 时的资源曲线（baseline）。
  3. 开启 Agent 严格模式，重复压测，记录差值。
  4. 若差值超出预算，则测试失败，阻断合并。

### 2.4 权限矩阵验证

目标：确认 Agent 所需权限最小化，且不会提升业务进程权限。

- 权限矩阵示例：
  - Linux：
    - 需要：对 `/proc` 的只读访问权限、BPF/PerfEvent 能力（如 `CAP_BPF`, `CAP_PERFMON`，依据内核版本）。
    - 不需要：`CAP_SYS_ADMIN`、`CAP_SYS_PTRACE`（如无特殊说明）。
  - Kubernetes：
    - Agent Pod 以非 root 身份运行；尽量避免 `privileged: true`。
    - 使用 DaemonSet / Sidecar 时，确保业务容器权限不被提升。
- 自动化流程：
  - 使用 `kubectl auth can-i` / `kubectl exec` 验证 Pod 的 ServiceAccount 权限。
  - 使用 `docker inspect` / `kubectl get pod -o yaml` 检查 `securityContext`。
  - 若发现超出矩阵定义的高危权限，则测试失败。

---

## 3. 严格模式 vs 可选增强模式 对照表

下表描述在不同模式下允许/禁止的采集技术，以指导后续能力规划。

| 能力/技术                                 | 严格模式（Strict）      | 可选增强模式（Enhanced）                          | 说明 |
|------------------------------------------|-------------------------|----------------------------------------------------|------|
| eBPF 内核探针（网络、系统调用、进程）    | ✅ 允许                 | ✅ 允许                                            | 无需修改业务进程，可通过 BPF 程序在内核态采集 |
| perf_event / perf_counter                | ✅ 允许                 | ✅ 允许                                            | 对 CPU 性能有轻微开销，需要采样率控制 |
| /proc 文件系统采集                       | ✅ 允许                 | ✅ 允许                                            | 只读访问，不影响业务执行 |
| .NET EventPipe / Diagnostic IPC          | ✅ 允许（外部订阅）     | ✅ 允许                                            | 通过外部工具（如 dotnet-trace）订阅，不注入进程 |
| ETW Session（Windows）                   | ✅ 允许                 | ✅ 允许                                            | 类似 EventPipe 的外部订阅 |
| OpenTelemetry Collector（独立进程）      | ✅ 允许                 | ✅ 允许                                            | Collector 作为独立进程 / Sidecar 运行 |
| OpenTelemetry SDK 植入业务代码           | ❌ 不要求               | ✅ 推荐                                            | 作为可选增强，需要业务团队改代码 |
| CLR Profiling API（ICorProfilerCallback）| ❌ 禁止                 | ⚠️ 谨慎启用，需专门评审                           | 会进入业务进程，修改 JIT 行为 |
| JVM Agent (`-javaagent`)                 | ❌ 禁止                 | ⚠️ 可选，需业务团队配合并评估风险                 | 改变启动参数，属于侵入式 |
| `LD_PRELOAD` / `DYLD_INSERT_LIBRARIES`   | ❌ 禁止                 | ⚠️ 极不推荐                                       | 拦截系统调用，高风险 |
| ptrace 动态调试/注入                     | ❌ 禁止                 | ❌ 禁止                                           | 极高风险，可能触发安全策略 |
| 在业务镜像内预装探针                     | ❌ 不要求               | ✅ 可选                                           | 作为增强方案，但不属于 Strict 范畴 |

---

## 4. 可执行测试清单（Checklists）与 CI 占位

本节将上述验收项整理为测试清单，便于自动化集成到 CI/CD 流水线中。当前仅定义测试条目与预期，CI 配置将在后续仓库中单独补充。

### 4.1 测试清单（Checklists）

#### Checklist: 进程无注入验证

- [ ] 在测试环境启动业务进程（无 Agent），采集模块列表快照 `modules_baseline.json`。
- [ ] 启动 Agent（Strict 模式），重复采集模块列表 `modules_with_agent.json`。
- [ ] 自动 diff 两个列表，验证：
  - [ ] 未发现新注入的 Agent 模块（DLL/SO）。
  - [ ] 未发现 profiler / instrumentation 相关模块。

#### Checklist: 线程/句柄差异验证

- [ ] 在无 Agent 场景下，采集目标进程线程/句柄的时间序列（如 5 分钟）。
- [ ] 在启用 Agent 后，重复采集同样时长的数据。
- [ ] 通过脚本计算差值，验证：
  - [ ] 线程数波动 < 5%。
  - [ ] 句柄数波动 < 5%。
  - [ ] 无属于 Agent 模块栈帧的新线程。

#### Checklist: 性能预算与限流

- [ ] 定义标准压测场景（QPS、并发数、持续时间等）。
- [ ] 记录无 Agent 时的 CPU/内存/网络指标基线。
- [ ] 在 Agent 严格模式下重复压测。
- [ ] 验证：
  - [ ] CPU 额外占用 < 3%（长期平均）。
  - [ ] 内存额外占用 < 预设门限（如 200 MB）。
  - [ ] 网络出口带宽不超过设定阈值。
  - [ ] 触发限流/采样时，Agent 能正常降级而不影响业务。

#### Checklist: 权限矩阵验证

- [ ] 在 Kubernetes / 容器环境中，检查 Agent Pod 的 `securityContext`：
  - [ ] 非 root 运行。
  - [ ] 未启用 `privileged: true`（除非经过特别批准）。
  - [ ] 仅授予 BPF/Perf 所必需的 capabilities。
- [ ] 使用 `kubectl auth can-i` 检查 ServiceAccount 权限，不超过矩阵定义。
- [ ] 在权限收紧场景下（移除高危权限）重复运行 Agent，确认：
  - [ ] Agent 行为符合预期（必要功能正常，非必要功能显式降级）。

### 4.2 CI 流水线占位设计

在 CI/CD 流水线中预留以下阶段入口（本仓库暂不创建具体 CI 配置文件，仅在此文档中记录约定）：

- Job: `non_intrusive_contract_checks`
  - Stage: `post-merge / pre-release`
  - 主要步骤：
    1. 部署业务示例应用与 Agent（Strict 模式）到测试环境。
    2. 运行 `scripts/non_intrusive/verify_modules.sh`  
       - 负责“进程无注入验证”检查。
    3. 运行 `scripts/non_intrusive/verify_resources.sh`  
       - 负责线程/句柄差异与性能预算验证。
    4. 运行 `scripts/non_intrusive/verify_permissions.sh`  
       - 负责权限矩阵验证。
    5. 汇总报告并产出 `non_intrusive_report.json` 作为 CI 工件。

- Job 失败条件：
  - 任一脚本返回非零退出码。
  - 报告中标记为 `severity = "high"` 的问题数量 > 0。

上述 Job 名称与脚本路径仅为**占位约定**，后续实现时应在对应仓库中创建实际脚本与 CI 配置（如 GitHub Actions / GitLab CI / Azure DevOps Pipeline 等），并与本文保持同步更新。

---

## 5. 容器与 Kubernetes 采集适配（Strict Mode Extras）

本节描述在符合 Strictly Non-Intrusive 约束前提下，如何为 Docker / Kubernetes 环境适配日志与元数据采集。设计参考 Datadog 关于容器日志 file-based vs socket-based、Kubernetes Cluster Agent / RBAC 以及 Admission Controller 注入 env/UDS 的官方文档。

> 参考资料：
> - Datadog 容器日志采集与 Docker 集成  
>   - https://docs.datadoghq.com/containers/logs/  
>   - https://docs.datadoghq.com/agent/docker/log/  
> - Datadog Kubernetes Cluster Agent 与 RBAC  
>   - https://docs.datadoghq.com/agent/cluster_agent/  
> - Datadog Admission Controller 与 env / UDS / CSI 注入  
>   - https://docs.datadoghq.com/agent/cluster_agent/admission_controller/

### 5.1 Docker 日志采集：file-based 优先，socket-based 作为增强

在 Strict 模式下，容器日志采集应优先采用**文件方式**，即在宿主机挂载容器运行时生成的日志目录，避免通过 Docker Engine Socket 获取具备强控制能力的管理员权限。

- file-based 模式（推荐）：
  - Linux：
    - 对于 Docker：挂载 `/var/lib/docker/containers` 只读目录，读取 `*-json.log` 文件。
    - 对于 containerd / CRI-O：挂载 `/var/log/pods` 或 `/var/log/containers`。
  - Windows：
    - 挂载 `C:\ProgramData\docker\containers` 对应目录（只读）。
  - Agent 内部通过 File tail collector（参见 Agent.Metering 的 `FileTailReceiver`）按行读取日志，并依赖上层 pipeline 完成多行聚合与脱敏。
  - 权限约束：
    - 宿主目录以只读方式挂载到 Agent Pod / 容器。
    - 不需要访问 Docker Engine Socket。

- socket-based 模式（可选增强）：
  - 当宿主日志目录不可用时，可通过 `unix:///var/run/docker.sock`（Linux）或 `npipe:////./pipe/docker_engine`（Windows）调用 Docker Engine API。
  - 用途：
    - 作为 file-based 模式的 fallback。
    - 获取容器标签 / 元数据，用于增强日志富化。
  - 风险与约束：
    - 访问 Docker Socket 赋予较高的宿主控制能力，必须标记为**可选增强层**，默认关闭，仅在严格评估后启用。
  - RBAC 与安全建议：
    - 当启用 socket-based 模式时，应通过独立的 ServiceAccount 或宿主级别 ACL 严格限制可执行操作（仅允许 `logs`/`inspect`）。

在本 Agent 中，推荐在 DaemonSet/Service 容器中通过配置决定：

- 默认模式：仅 file-based，挂载日志目录，禁用 Docker Socket。
- 增强模式：显式开启 Docker Socket 访问并标注为 Enhanced，不属于 Strict 基线能力。

### 5.2 Kubernetes Metadata Enricher：标签与标准资源属性映射

为避免在日志中丢失拓扑信息，需要将 Pod / Node / Namespace 的标签与常见环境信息映射为标准资源属性。设计原则：

- 首选 Downward API 与环境变量：
  - 通过以下方式注入到 Agent Pod：
    - 环境变量：
      - `POD_NAME` ← `metadata.name`
      - `POD_NAMESPACE` ← `metadata.namespace`
      - `NODE_NAME` ← `spec.nodeName`
    - Downward API volume：
      - `metadata.labels` 写入文件（例如 `/var/run/agent/pod_labels`）。
      - `metadata.annotations` 写入文件（例如 `/var/run/agent/pod_annotations`）。
  - Agent 内部的 `KubernetesMetadataProcessor` 读取上述 env 与文件，将信息映射为标准标签：
    - `k8s.pod.name`
    - `k8s.namespace.name`
    - `k8s.node.name`
    - `k8s.pod.label.<key>`
    - `k8s.pod.annotation.<key>`

- 可选 Cluster Agent / 集中式元数据：
  - 参考 Datadog Cluster Agent 设计，可选部署一个 Cluster Agent 进程，聚合与缓存集群级元数据（如 Node/Service/Deployment 标签）。
  - Strict 模式下的采集 Agent 仅通过本地 HTTP/UDS 访问 Cluster Agent，避免直接与 Kubernetes API Server 频繁交互。
  - RBAC 最小化：
    - 采集 Agent 的 ServiceAccount 仅需要访问自身所在 Node 的基本信息（若通过 Downward API 获取，则甚至不需要额外 RBAC）。
    - Cluster Agent 的 ServiceAccount 负责更高权限的集群级读权限，并单独进行安全评估。

- 错误降级：
  - 若 Downward API 文件或环境变量缺失，`KubernetesMetadataProcessor` 仅提供有限 metadata，不影响日志采集与导出。
  - 若 Cluster Agent 不可用或返回错误，Agent 应记录告警日志并自动降级，而不是影响业务流量。

### 5.3 DaemonSet 部署与配置（Helm/Kustomize 模板约定）

为便于在每个 Node 上部署 Strict Mode Agent，可通过 DaemonSet + Helm/Kustomize 生成部署配置。以下为关键约定：

- 容器与挂载：
  - 以 DaemonSet 的形式在每个 Node 上运行一个 Agent Pod。
  - 宿主目录挂载：
    - `hostPath: /var/lib/docker/containers` → 容器内 `/var/lib/docker/containers`（只读）。
    - 或 `hostPath: /var/log/pods` / `/var/log/containers` 视运行时而定。
  - Downward API 挂载：
    - Volume `podinfo`，分别挂载 `metadata.labels` 和 `metadata.annotations` 到容器路径，如 `/var/run/agent/pod_labels` 与 `/var/run/agent/pod_annotations`。

- ServiceAccount / RBAC：
  - 默认 Strict 模式下：
    - ServiceAccount 仅具备读取自身 Pod 元数据所需的最小权限，优先通过 Downward API 实现，从而避免直接访问 API Server。
    - 禁止授予 `cluster-admin`、`list/watch nodes` 等高权限，除非通过 cluster-agent 分层方式由单独组件承担。
  - 可选增强：
    - 单独创建 Cluster Agent（Deployment）及其 ServiceAccount，用于集中拉取 Pod/Node/Service 标签并以只读 API 提供给 DaemonSet Agent。

- Helm/Kustomize 模板建议：
  - Helm Chart 中：
    - `values.yaml` 中暴露：
      - `logs.containerRuntimePath`（如 `/var/lib/docker/containers`）。
      - `logs.useDockerSocket`（默认 false）。
      - `kubernetes.metadata.enabled`、`kubernetes.clusterAgent.enabled` 等开关。
    - `daemonset.yaml` 模板中使用上述 values 配置 hostPath 与 env。
  - Kustomize 中：
    - 通过 `patchesStrategicMerge` 向 DaemonSet 注入 hostPath 与 Downward API 配置。

### 5.4 Datadog 兼容与 Admission Controller 增强层

为与 Datadog 生态兼容，可选提供一层“增强模式”，通过 Admission Controller 在业务 Pod 中注入与 Agent 通信所需的 env/UDS 配置，同时保持 Strict 模式的核心原则不变。

- 兼容目标：
  - 支持 Datadog 风格的 Cluster Agent / Node Agent 拓扑，以便在同一集群中共存或迁移。
  - 支持通过 Admission Controller 为业务 Pod 注入：
    - 指向 Agent 的 UDS/socket 地址（如 `/var/run/agent/agent.sock`）。
    - 与环境相关的配置 env（如 service/environment/version 标签）。

- 实现思路：
  - 定义一组标准环境变量与 Volume 挂载约定，例如：
    - `AGENT_UDS_PATH=/var/run/agent/agent.sock`
    - `AGENT_ENV`, `AGENT_SERVICE`, `AGENT_VERSION` 等。
  - 在 Admission Controller 中，根据命名空间/标签策略为目标 Pod 注入：
    - `env`：上述环境变量。
    - `volumeMounts`：挂载由 DaemonSet 提供的 UDS/CSI Volume（类似 Datadog 使用 `csi/uds` 为应用暴露 Unix Domain Socket 的模式）。
  - Agent 端仅监听 UDS 或文件系统，不进入业务进程，符合 Strict 模式。

- 安全与定位：
  - Admission Controller 注入能力被视为**增强层**：
    - 默认关闭，仅在需要与业务进程深度集成且经过安全评审后启用。
    - 不属于 Strict 模式合规所必须的能力。
  - 引用文档：
    - Datadog Admission Controller 关于 env/UDS/CSI 注入的设计，详见其官方文档链接。

综上，容器与 Kubernetes 采集适配通过“file-based 日志 + Downward API/Cluster Agent 元数据 + DaemonSet 部署 + 可选 Admission Controller 增强层”的组合实现，在保持 Strictly Non-Intrusive 约束的同时，提供与 Datadog 等主流方案兼容的部署与集成路径。

---

## 6. Dapr 集成与 OTEL / Datadog 适配

本节描述在 Strict 模式下对 Dapr sidecar 的可观测性适配，重点关注 metrics 与 tracing 的标准化接入路径。

> 参考资料：
> - Dapr Metrics  
>   - https://docs.dapr.io/operations/monitoring/metrics/  
> - Dapr Tracing / OpenTelemetry 集成  
>   - https://docs.dapr.io/operations/monitoring/tracing/  
> - Datadog OTLP ingest 与本地主机限制  
>   - https://docs.datadoghq.com/opentelemetry/otlp_ingest/

### 6.1 Dapr metrics：Prometheus 端点自动发现与抓取

Dapr 在默认配置下会暴露 Prometheus metrics 端点，默认端口 9090，路径 `/metrics`，并可通过配置参数或环境变量进行修改：

- 默认行为：
  - Dapr sidecar 默认启用 metrics。
  - 默认监听在 `http://127.0.0.1:9090/metrics`。
- 可配置方式（示意，具体以官方文档为准）：
  - CLI 参数：`--metrics-port`。
  - 环境变量：`DAPR_METRICS_PORT` 或 `DAPR_METRICS_HTTP_PORT`。

Agent.Metering 中的 `DaprPrometheusReceiver` 作为 Strict Mode Extras，对 Dapr metrics 进行 file-less 抓取：

- 端点自动发现策略：
  - 优先读取环境变量 `DAPR_METRICS_HTTP_PORT` / `DAPR_METRICS_PORT`，构造 `http://127.0.0.1:<port>/metrics`。
  - 若未设置，则回退到默认 `http://127.0.0.1:9090/metrics`。
- 抓取与解析：
  - 使用 HTTP GET 周期性抓取 Prometheus 文本格式。
  - 解析 `name{label="value"} 1.23` 等样本，将其映射为内部 `MeterRecord`：
    - `Name` ← metric name。
    - `Value` ← 样本值。
    - `Tags` ← Prometheus labels + `metric.source = "dapr"`。
- 映射到 OTLP metrics / Prometheus remote-write：
  - 在 Agent 内部，Dapr metrics 被标准化为独立的 metric 记录流，可通过：
    - OTEL Collector 的 OTLP metrics pipeline 转发到后端（推荐）。
    - 或与 Prometheus remote-write 兼容的 Collector/exporter 进行转储。
  - Agent.Metering 本身专注于采集与规范化，不内嵌特定厂商的 exporter 实现。

### 6.2 Dapr tracing：OTEL Collector 与 Datadog OTLP ingest

Dapr 官方推荐使用 OpenTelemetry 协议对 tracing 进行导出，支持将 traces 发送到 OpenTelemetry Collector 或其他兼容后端。

- 推荐路径 A：接入 OpenTelemetry Collector
  - Dapr sidecar 配置：
    - 将 tracing exporter 设置为 OTLP。
    - 指向集群内/本机的 OTEL Collector（如 `otel-collector:4317`）。
  - OTEL Collector：
    - 负责接收 Dapr sidecar 的 OTLP traces。
    - 在 Collector 内部配置 exporter（如 Jaeger、Tempo、APM 平台等）。
  - Agent.Metering：
    - 作为 metrics/logs pipeline 的一部分，与 tracing 管道通过资源属性（service/pod/node 等）关联。
    - 不直接参与 traces 的协议转换，遵循 “Collector first” 的设计。

- 可选路径 B：Datadog Agent OTLP ingest（增强层）
  - Datadog 提供 OTLP ingest 能力，通常要求将 OTLP traces 发送到**本机 Datadog Agent**（即每台主机固定 Agent，监听 `localhost:4317/4318`）。
  - 在这种模式下：
    - Dapr sidecar 将 OTLP traces 指向本机 Datadog Agent（如 `http://127.0.0.1:4317`）。
    - Datadog Agent 再将数据汇聚到 Datadog 后端。
  - 限制与提示：
    - 必须遵守 “每主机本地 Agent” 的限制，不应跨 Node 发送到远程 Agent 端点，以避免额外网络 hop 与可靠性问题。
    - 此模式属于**可选增强层**，需要对 Datadog Agent 的权限与资源消耗进行单独评估。

### 6.3 Dapr 与 Strict Mode 的关系

在 Strict 模式下，Dapr 适配需要满足以下原则：

- 不注入到 Dapr 进程内部，不使用 Dapr 或业务进程的调试/Profiling API。
- Metrics：
  - 仅通过 Dapr sidecar 暴露的外部 Prometheus 端点进行 HTTP 拉取。
  - 不修改 Dapr sidecar 的启动参数，仅在用户显式配置时读取端口相关环境变量。
- Tracing：
  - 推荐使用 OTEL Collector 作为独立进程或 Sidecar，Dapr 仅向其发送 OTLP traces。
  - Datadog OTLP ingest 集成视为增强层，通过本机 Agent 进行协议转换与上传。

通过上述设计，Agent 在 Dapr 场景下依然遵循 Strictly Non-Intrusive 的核心约束，只利用 Dapr 官方公开的指标与 tracing 输出能力，不改变或注入 Dapr/业务进程。

---

## 7. OTLP Exporter 与 Datadog 集成及对比验证

本节描述在 Strict 模式下，如何为 Agent.Metering 实现通用 OTLP 导出能力，并与 Datadog 进行集成与行为对比。

> 参考资料：
> - OpenTelemetry OTLP 协议与 Collector  
>   - https://opentelemetry.io/docs/specs/otlp/  
>   - https://opentelemetry.io/docs/collector/  
> - Datadog OTLP ingest（通过本机 Agent 接收 OTLP 数据）  
>   - https://docs.datadoghq.com/opentelemetry/otlp_ingest/  
> - Datadog OpenTelemetry Collector exporter（通过 OTel Collector 推送到 Datadog）  
>   - https://docs.datadoghq.com/opentelemetry/collector_exporter/

### 7.1 OTLP Exporter：OTLP/HTTP 与 OTLP/gRPC 支持

Agent.Metering 内部提供 `OtlpHttpExporter` 与配置对象 `OtlpExporterOptions`，用于将标准化的 `MeterRecord` 批量导出到 OTLP 兼容的 Collector 或网关。

配置要点：

- 协议类型：
  - `Protocol = Http`：面向 OTLP/HTTP 端点，如 `http://localhost:4318/v1/metrics`、`/v1/logs`。
  - `Protocol = Grpc`：用于标记目标为 OTLP/gRPC 网关或 Collector 端点，当前实现仍通过 HTTP JSON 发送，建议在前置 Collector/Agent 中完成正式 OTLP/gRPC 映射。
- Endpoint：
  - 默认：`http://127.0.0.1:4318/v1/metrics`（本机 OTLP/HTTP Collector）。
  - 可根据场景切换到 `v1/logs` 或 `v1/traces`。
- TLS：
  - 通过 Endpoint 的 `https://` scheme 启用 TLS。
  - `UseTls` 字段用于配置与策略检查，实际握手由底层 HTTPS 完成。
- Headers：
  - `Headers` 支持注入 `Authorization`、`x-datadog-*` 等自定义头部。
  - 可用于路由、租户标记或安全认证。

在 Strict 模式下，OTLP Exporter 遵守以下约束：

- 不在业务进程内部执行，仅作为独立 Agent.Metering 进程的一部分运行。
- 仅通过出站 HTTP(S) 与 Collector / Agent 通信。
- 所有导出失败通过日志记录并受 backpressure/限流约束，不影响业务进程。

### 7.2 Datadog 集成路径：Agent OTLP ingest 与 OTel Collector exporter

Datadog 官方提供两种常见接入形态：

- 路径 A：Datadog Agent OTLP ingest（本机 Agent）
  - 部署方式：
    - 每台主机运行一个 Datadog Agent。
    - Agent 暴露 OTLP/HTTP 或 OTLP/gRPC 端点（通常为 `localhost:4317` / `localhost:4318`）。
  - 配置方式：
    - 将 `OtlpExporterOptions.Endpoint` 指向本机 Datadog Agent 的 OTLP ingest 端点，例如：
      - `http://127.0.0.1:4318/v1/metrics`
      - `http://127.0.0.1:4318/v1/logs`
    - 通过 `Headers` 注入必要的 Datadog API key / site 信息（如按官方建议走 Agent side 配置，避免在 Agent.Metering 中携带敏感信息）。
  - 限制与约束：
    - Datadog OTLP ingest 能力主要面向 APM/metrics/logs 等核心产品，部分专有产品或旧版本可能不支持直接 OTLP 接入，需要通过 Datadog Agent 本地采集方式或专有协议。
    - 官方文档中明确：OTLP ingest 并非所有场景的统一入口，具体功能矩阵需参考对应版本文档。
    - 应遵守“每主机本地 Agent”的模式，不建议跨 Node 向远程 Agent 发送 OTLP，以避免网络与 HA 问题。

- 路径 B：OpenTelemetry Collector + Datadog exporter
  - 部署方式：
    - 在集群中部署独立的 OpenTelemetry Collector。
    - Collector 接收来自 Agent.Metering 与 Dapr 等组件的 OTLP 数据。
    - Collector 内部通过 Datadog exporter 将数据推送到 Datadog。
  - 配置方式：
    - Agent.Metering 的 `OtlpHttpExporter` 指向 Collector 的 OTLP 入口：
      - 例如 `http://otel-collector:4318/v1/metrics`。
    - Datadog exporter 在 Collector 内部配置（API key、site、其他映射规则），与 Agent.Metering 解耦。
  - 优点：
    - Collector 层可进行采样、重标签、脱敏、路由等复杂处理。
    - 便于同时对接多家后端，不锁定在单一厂商。

### 7.3 对比验证套件：OTLP 与 Datadog 管道一致性

为验证同一 workload 在两条管道下的行为一致性，建议构建以下“对比验证套件”：

- 目标：
  - 验证 host identity（host.name / host.id）在 OTLP 与 Datadog 路径上一致。
  - 验证 service tags（service.name / service.env / service.version 等）一致。
  - 验证 logs 与 traces 的关联字段（trace_id / span_id 等）在两条路径中保持可关联。
  - 验证数据丢弃策略（采样/背压）是否一致或在预期差异范围内。

- 验证设计（建议作为脚本与 CI Job 实现）：
  1. 部署同一版本的业务示例应用与 Agent.Metering。
  2. 配置两套独立 pipeline：
     - Pipeline A：使用 `OtlpHttpExporter` 直接发送到 OTLP Collector（或 Datadog OTLP ingest）。
     - Pipeline B：通过 OTel Collector + Datadog exporter，将相同来源数据送入 Datadog。
  3. 使用固定 workload（如压测脚本）触发一定时长的稳定流量。
  4. 在 Collector / Datadog 中导出原始数据快照（可通过 API 或导出功能），生成：
     - `otlp_path_snapshot.json`
     - `datadog_path_snapshot.json`
  5. 编写对比脚本（如 `scripts/compare/compare_otlp_datadog.py`）：
     - 依据 host/service/tag/trace_id 等关键字段进行匹配与统计。
     - 输出：
       - 一致性率（如某字段 99% 以上记录一致）。
       - 丢弃/缺失记录比率。
       - 样例 diff（方便人工审查）。

- CI 集成占位：
  - Job: `otlp_datadog_pipeline_comparison`
    - Stage: `post-merge / nightly`
    - 主要步骤：
      1. 部署两条 pipeline 并运行标准 workload。
      2. 收集快照并运行对比脚本。
      3. 若关键 identity/tag 差异超过阈值，或丢弃比率不在预期范围内，则 Job 失败。

### 7.4 Datadog OTLP ingest 版本能力与限制说明

根据 Datadog 官方 OTLP ingest 与 OTel Collector exporter 文档，需在本项目中明确以下约束：

- OTLP ingest 能力与 Datadog 产品版本相关：
  - 新版 Datadog Agent 对 OTLP/HTTP 与 OTLP/gRPC 的支持更完备，旧版本可能仅支持部分信号类型（如只支持 traces，不支持 metrics/logs）。
  - 某些专有或历史产品不直接支持 OTLP ingest，需要通过 Datadog Agent 的传统采集方式或专用集成。
- 能力差异的处理建议：
  - 在部署前，应根据目标 Datadog 版本核对官方文档中的信号支持矩阵（traces / metrics / logs）。
  - 对于不支持 OTLP ingest 的场景：
    - 优先通过 OTel Collector + Datadog exporter 路径接入。
    - 或退回到 Datadog 原生 Agent 配置（file-based logs、APM 自动注入等），并在本项目中标记为“增强模式”能力。
- 在 Strict 模式下的约束：
  - 无论使用哪种路径，Agent.Metering 仅负责发送标准化数据，不在业务进程中注入任何 Datadog SDK 或自动注入探针。
  - 与 Datadog 的集成通过本机 Agent 或 Collector 完成，符合“独立进程 + 外部接口”的非侵入原则。



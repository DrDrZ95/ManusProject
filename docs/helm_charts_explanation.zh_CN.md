# Helm Charts 使用示例与说明

Helm 是 Kubernetes 的包管理器，它使用 Charts 来定义、安装和升级 Kubernetes 应用程序。Charts 是一系列文件，描述了 Kubernetes 应用程序的资源集合。

## Helm Chart 结构

一个典型的 Helm Chart 目录结构如下：

```
mychart/
  Chart.yaml          # Chart 的基本信息，如名称、版本、描述等
  values.yaml         # Chart 的默认配置值，可以在安装时覆盖
  charts/             # 依赖的子 Chart
  templates/          # 包含 Kubernetes 资源定义模板的目录
  LICENSE             # 许可证文件
  README.md           # Chart 的说明文档
  ...                 # 其他可选文件
```

## 核心文件说明

### `Chart.yaml`

`Chart.yaml` 文件包含了 Chart 的元数据，例如：

```yaml
apiVersion: v2
name: mychart
description: A Helm chart for Kubernetes

type: application # 或 library

version: 0.1.0 # Chart 的版本
appVersion: "1.16.0" # 应用程序的版本
```

*   `apiVersion`: Chart API 版本。
*   `name`: Chart 的名称。
*   `description`: Chart 的简要描述。
*   `type`: Chart 的类型，可以是 `application`（应用程序）或 `library`（库）。
*   `version`: Chart 的版本号，遵循 SemVer 2.0.0 规范。
*   `appVersion`: 应用程序的版本号。

### `values.yaml`

`values.yaml` 文件定义了 Chart 模板中使用的默认配置值。这些值可以在安装或升级 Chart 时通过命令行参数或另一个 `values.yaml` 文件进行覆盖。

```yaml
replicaCount: 1

image:
  repository: nginx
  pullPolicy: IfNotPresent
  tag: "latest"

service:
  type: ClusterIP
  port: 80
```

### `templates/` 目录

`templates/` 目录包含了 Kubernetes 资源定义模板。Helm 使用 Go 模板语言和 Sprig 模板函数来渲染这些模板，生成最终的 Kubernetes YAML 文件。

#### 示例：`templates/deployment.yaml`

这是一个 Deployment 资源的模板示例，它使用了 `values.yaml` 中定义的值：

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "mychart.fullname" . }}
  labels:
    {{- include "mychart.labels" . | nindent 4 }}
spec:
  replicas: {{ .Values.replicaCount }}
  selector:
    matchLabels:
      {{- include "mychart.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      labels:
        {{- include "mychart.selectorLabels" . | nindent 8 }}
    spec:
      containers:
        - name: {{ .Chart.Name }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          imagePullPolicy: {{ .Values.image.pullPolicy }}
          ports:
            - name: http
              containerPort: 80
              protocol: TCP
```

*   `{{ include 


"mychart.fullname" . }}`: 这是一个 Helm 模板函数，用于生成部署的完整名称。
*   `{{ .Values.replicaCount }}`: 从 `values.yaml` 中获取 `replicaCount` 的值。
*   `{{ .Values.image.repository }}`: 从 `values.yaml` 中获取镜像仓库的名称。
*   `{{ .Values.image.tag | default .Chart.AppVersion }}`: 从 `values.yaml` 中获取镜像标签，如果未设置，则使用 `Chart.yaml` 中定义的 `appVersion`。

#### 示例：`templates/service.yaml`

这是一个 Service 资源的模板示例：

```yaml
apiVersion: v1
kind: Service
metadata:
  name: {{ include "mychart.fullname" . }}
  labels:
    {{- include "mychart.labels" . | nindent 4 }}
spec:
  type: {{ .Values.service.type }}
  ports:
    - port: {{ .Values.service.port }}
      targetPort: http
      protocol: TCP
      name: http
  selector:
    {{- include "mychart.selectorLabels" . | nindent 4 }}
```

### `templates/_helpers.tpl`

`_helpers.tpl` 文件通常用于定义可重用的模板片段（命名模板），这些片段可以在其他模板文件中通过 `include` 或 `required` 函数调用。这有助于保持模板的 DRY（Don't Repeat Yourself）原则。

```yaml
{{/*
Expand the name of the chart.
*/}}
{{- define "mychart.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Create a default fully qualified app name. We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "mychart.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{/*
Create chart name and version as a label.
*/}}
{{- define "mychart.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Common labels
*/}}
{{- define "mychart.labels" -}}
helm.sh/chart: {{ include "mychart.chart" . }}
{{- include "mychart.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{/*
Selector labels
*/}}
{{- define "mychart.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mychart.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{/*
Create the name of the service account to use
*/}}
{{- define "mychart.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
    {{- default (include "mychart.fullname" .) .Values.serviceAccount.name -}}
{{- else -}}
    {{- default "default" .Values.serviceAccount.name -}}
{{- end -}}
{{- end -}}
```

## 如何使用 Helm Chart

1.  **安装 Chart**：

    ```bash
    helm install my-release ./mychart
    ```

2.  **升级 Chart**：

    ```bash
    helm upgrade my-release ./mychart
    ```

3.  **卸载 Chart**：

    ```bash
    helm uninstall my-release
    ```

通过使用 Helm Charts，您可以更轻松地管理和部署复杂的 Kubernetes 应用程序，实现版本控制、配置管理和生命周期管理。


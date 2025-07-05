# Kubernetes、Istio 与灰度发布指南

本文档详细介绍了如何设置 Kubernetes 集群、安装 Istio 服务网格以及如何利用 Istio 实现应用程序的灰度发布。

## 1. Kubernetes 集群设置

在开始之前，您需要一个可用的 Kubernetes 集群。您可以选择以下方式之一进行设置：

### 1.1. 使用 Minikube (本地开发环境)

Minikube 是一个轻量级的 Kubernetes 实现，用于本地开发和测试。

1.  **安装 Minikube 和 kubectl**：
    请根据您的操作系统，参考 Minikube 官方文档进行安装：[https://minikube.sigs.k8s.io/docs/start/](https://minikube.sigs.k8s.io/docs/start/)

2.  **启动 Minikube 集群**：
    ```bash
    minikube start
    ```

3.  **检查集群状态**：
    ```bash
    kubectl cluster-info
    kubectl get nodes
    ```

### 1.2. 使用 kubeadm (生产环境)

kubeadm 是一个用于快速部署生产级 Kubernetes 集群的工具。

1.  **准备服务器**：
    *   至少两台运行兼容 Linux 操作系统的服务器 (例如 Ubuntu 20.04+)。
    *   每台服务器至少 2GB RAM，2 CPU。
    *   禁用 Swap：`sudo swapoff -a && sudo sed -i '/ swap / s/^/#/' /etc/fstab`
    *   配置网络 (例如，设置静态 IP，确保节点间可达)。

2.  **安装容器运行时 (例如 containerd)**：
    ```bash
    sudo apt update
    sudo apt install -y containerd.io
    sudo mkdir -p /etc/containerd
    sudo containerd config default | sudo tee /etc/containerd/config.toml
    sudo systemctl restart containerd
    ```

3.  **安装 kubeadm, kubelet, kubectl**：
    ```bash
    sudo apt-get update
    sudo apt-get install -y apt-transport-https ca-certificates curl
    sudo curl -fsSLo /usr/share/keyrings/kubernetes-archive-keyring.gpg https://packages.cloud.google.com/apt/doc/apt-key.gpg
    echo "deb [signed-by=/usr/share/keyrings/kubernetes-archive-keyring.gpg] https://apt.kubernetes.io/ kubernetes-xenial main" | sudo tee /etc/apt/sources.list.d/kubernetes.list
    sudo apt-get update
    sudo apt-get install -y kubelet kubeadm kubectl
    sudo apt-mark hold kubelet kubeadm kubectl
    ```

4.  **初始化 Master 节点**：
    在 Master 节点上运行 (替换 `<pod-network-cidr>` 为您的 Pod 网络 CIDR，例如 `10.244.0.0/16` 用于 Flannel)：
    ```bash
    sudo kubeadm init --pod-network-cidr=<pod-network-cidr>
    ```
    根据输出，配置 `kubectl` 环境：
    ```bash
    mkdir -p $HOME/.kube
    sudo cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
    sudo chown $(id -u):$(id -g) $HOME/.kube/config
    ```

5.  **安装 Pod 网络插件 (例如 Flannel)**：
    ```bash
    kubectl apply -f https://raw.githubusercontent.com/flannel-io/flannel/master/Documentation/kube-flannel.yml
    ```

6.  **加入 Worker 节点**：
    在 Worker 节点上运行 `kubeadm init` 命令输出的 `kubeadm join` 命令。

## 2. Istio 服务网格安装

Istio 是一个功能强大的服务网格，用于连接、保护、控制和观察微服务。

1.  **下载 Istio**：
    ```bash
    curl -L https://istio.io/downloadIstio | sh -
    cd istio-<version>
    export PATH=$PWD/bin:$PATH
    ```

2.  **安装 Istio (选择配置配置)**：
    您可以选择不同的配置，例如 `demo`、`default`、`minimal`、`sds`、`remote` 或 `multicluster`。这里以 `demo` 为例：
    ```bash
    istioctl install --set profile=demo -y
    ```

3.  **验证安装**：
    ```bash
    kubectl get ns
    kubectl get pods -n istio-system
    ```

4.  **为命名空间启用 Istio 自动注入**：
    将您的应用程序部署到的命名空间标记为自动注入 Istio sidecar 代理：
    ```bash
    kubectl label namespace default istio-injection=enabled --overwrite
    ```
    (将 `default` 替换为您的应用命名空间)

## 3. 应用程序部署与灰度发布

灰度发布 (Canary Release) 是一种部署策略，通过逐步将流量从旧版本路由到新版本，从而降低新版本引入风险。

### 3.1. 部署应用程序

假设您有两个版本的 `AgentWebApi` 和 `AgentUI` 应用程序，版本格式为 `A.XX.YY` (例如 `A.01.00`, `A.01.01`)。

1.  **部署旧版本 (V1)**：
    首先部署您的应用程序的稳定版本 (例如 `A.01.00`)。确保您的 Kubernetes Deployment 和 Service YAML 文件中包含 `app` 和 `version` 标签。

    例如，`AgentWebApi` 的 `deployment.A.01.00.yml` 和 `service.yml`：
    ```yaml
    # agentwebapi/deployment.A.01.00.yml
    apiVersion: apps/v1
    kind: Deployment
    metadata:
      name: agentwebapi-deployment-A.01.00
      labels:
        app: agentwebapi
        version: A.01.00
    spec:
      replicas: 2
      selector:
        matchLabels:
          app: agentwebapi
          version: A.01.00
      template:
        metadata:
          labels:
            app: agentwebapi
            version: A.01.00
        spec:
          containers:
          - name: agentwebapi
            image: your-registry/agentwebapi:A.01.00
            ports:
            - containerPort: 80
    ---
    # agentwebapi/service.yml (Service 通常不带版本标签，因为它路由到所有版本)
    apiVersion: v1
    kind: Service
    metadata:
      name: agentwebapi-service
      labels:
        app: agentwebapi
    spec:
      selector:
        app: agentwebapi
      ports:
        - protocol: TCP
          port: 80
          targetPort: 80
      type: ClusterIP
    ```
    部署：
    ```bash
    kubectl apply -f kubernetes/agentwebapi/deployment.A.01.00.yml
    kubectl apply -f kubernetes/agentwebapi/service.yml
    # 对 AgentUI 也进行类似操作
    kubectl apply -f kubernetes/agentui/deployment.A.01.00.yml
    kubectl apply -f kubernetes/agentui/service.yml
    ```

2.  **创建 Istio Gateway 和 VirtualService**：
    Gateway 用于管理进入网格的外部流量。VirtualService 定义了如何将请求路由到网格内的服务。

    ```yaml
    # gateway.yml
    apiVersion: networking.istio.io/v1beta1
    kind: Gateway
    metadata:
      name: ai-agent-gateway
    spec:
      selector:
        istio: ingressgateway # 使用 Istio 默认的 ingress gateway
      servers:
      - port:
          number: 80
          name: http
          protocol: HTTP
        hosts:
        - "*"
    ---
    # virtualservice-agentui.yml
    apiVersion: networking.istio.io/v1beta1
    kind: VirtualService
    metadata:
      name: agentui-virtualservice
    spec:
      hosts:
      - "*"
      gateways:
      - ai-agent-gateway
      http:
      - route:
        - destination:
            host: agentui-service
            subset: v1 # 初始指向 V1 版本
          weight: 100
    ---
    # virtualservice-agentwebapi.yml
    apiVersion: networking.istio.io/v1beta1
    kind: VirtualService
    metadata:
      name: agentwebapi-virtualservice
    spec:
      hosts:
      - "*"
      gateways:
      - ai-agent-gateway
      http:
      - route:
        - destination:
            host: agentwebapi-service
            subset: v1 # 初始指向 V1 版本
          weight: 100
    ```
    部署：
    ```bash
    kubectl apply -f gateway.yml
    kubectl apply -f virtualservice-agentui.yml
    kubectl apply -f virtualservice-agentwebapi.yml
    ```

3.  **创建 DestinationRule**：
    DestinationRule 定义了服务的子集 (subset)，Istio 可以根据这些子集进行流量路由。

    ```yaml
    # destinationrule-agentui.yml
    apiVersion: networking.istio.io/v1beta1
    kind: DestinationRule
    metadata:
      name: agentui-destinationrule
    spec:
      host: agentui-service
      subsets:
      - name: v1
        labels:
          version: A.01.00 # 对应旧版本
      - name: v2
        labels:
          version: A.01.01 # 对应新版本
    ---
    # destinationrule-agentwebapi.yml
    apiVersion: networking.istio.io/v1beta1
    kind: DestinationRule
    metadata:
      name: agentwebapi-destinationrule
    spec:
      host: agentwebapi-service
      subsets:
      - name: v1
        labels:
          version: A.01.00 # 对应旧版本
      - name: v2
        labels:
          version: A.01.01 # 对应新版本
    ```
    部署：
    ```bash
    kubectl apply -f destinationrule-agentui.yml
    kubectl apply -f destinationrule-agentwebapi.yml
    ```

### 3.2. 执行灰度发布

现在，您可以部署新版本 (V2，例如 `A.01.01`) 并逐步将流量路由过去。

1.  **部署新版本 (V2)**：
    部署 `AgentWebApi` 和 `AgentUI` 的新版本 Deployment (例如 `A.01.01`)。Service 保持不变，因为它通过 `app` 标签选择所有版本的 Pod。

    ```bash
    kubectl apply -f kubernetes/agentwebapi/deployment.A.01.01.yml
    kubectl apply -f kubernetes/agentui/deployment.A.01.01.yml
    ```

2.  **逐步路由流量**：
    通过修改 VirtualService 的 `weight` 字段，将一小部分流量路由到新版本。

    例如，将 10% 的流量路由到 `AgentWebApi` 的 V2 版本：
    ```yaml
    # virtualservice-agentwebapi-canary-10.yml
    apiVersion: networking.istio.io/v1beta1
    kind: VirtualService
    metadata:
      name: agentwebapi-virtualservice
    spec:
      hosts:
      - "*"
      gateways:
      - ai-agent-gateway
      http:
      - route:
        - destination:
            host: agentwebapi-service
            subset: v1
          weight: 90 # 90% 流量到旧版本
        - destination:
            host: agentwebapi-service
            subset: v2
          weight: 10 # 10% 流量到新版本
    ```
    应用此配置：
    ```bash
    kubectl apply -f virtualservice-agentwebapi-canary-10.yml
    ```
    观察新版本的表现。如果一切正常，可以逐步增加 V2 的权重 (例如 50%、100%)，直到所有流量都路由到新版本。

3.  **完成灰度发布**：
    当所有流量都路由到新版本 (V2) 并且新版本稳定运行时，您可以删除旧版本 (V1) 的 Deployment。

    例如，将 100% 的流量路由到 `AgentWebApi` 的 V2 版本：
    ```yaml
    # virtualservice-agentwebapi-canary-100.yml
    apiVersion: networking.istio.io/v1beta1
    kind: VirtualService
    metadata:
      name: agentwebapi-virtualservice
    spec:
      hosts:
      - "*"
      gateways:
      - ai-agent-gateway
      http:
      - route:
        - destination:
            host: agentwebapi-service
            subset: v2
          weight: 100 # 100% 流量到新版本
    ```
    应用此配置：
    ```bash
    kubectl apply -f virtualservice-agentwebapi-canary-100.yml
    ```
    然后删除旧版本 Deployment：
    ```bash
    kubectl delete deployment agentwebapi-deployment-A.01.00
    kubectl delete deployment agentui-deployment-A.01.00
    ```

## 4. Kubernetes 部署文件版本管理规范

为了更好地管理 Kubernetes 部署，我们采用 `[A-Z].[0-99].[0-99]` 的版本格式，例如 `A.01.00`。

*   **`A`**: 主版本号，通常在重大架构变更或不兼容 API 更改时递增。
*   **`01`**: 次版本号，在添加新功能或进行较大改进时递增。
*   **`00`**: 补丁版本号，用于修复 Bug 或进行小型、向后兼容的更改时递增。

**YAML 文件命名约定**：

`deployment.<app_name>.<version>.yml`

例如：

*   `kubernetes/agentwebapi/deployment.A.01.00.yml`
*   `kubernetes/agentwebapi/deployment.A.01.01.yml`
*   `kubernetes/agentui/deployment.B.02.00.yml`

Service 和 Ingress (或 Gateway/VirtualService) 通常不包含版本号，因为它们旨在路由到不同版本的 Pod，或者在版本升级过程中保持稳定。

## 5. 总结

通过遵循本文档中的步骤，您可以成功设置 Kubernetes 集群，安装 Istio 服务网格，并利用其强大的流量管理功能实现应用程序的灰度发布，从而确保新版本的平稳上线和风险的最小化。



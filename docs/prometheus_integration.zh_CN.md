# Prometheus 集成指南

## 1. Prometheus 简介
Prometheus 是一个开源的系统监控和警报工具包，最初由 SoundCloud 构建。自 2012 年问世以来，许多公司和组织已将 Prometheus 作为独立的监控解决方案。它通过在给定时间间隔从配置的目标抓取指标、评估规则表达式、显示结果，并在观察到某些条件为真时触发警报来收集和存储时间序列数据。

**主要特点：**
*   多维数据模型（时间序列由指标名称和键/值对标识）
*   灵活的查询语言 (PromQL)
*   不依赖分布式存储；单个服务器节点是自治的
*   时间序列收集通过 HTTP 拉取模型进行
*   Pushgateway 用于从临时作业推送时间序列
*   通过服务发现或静态配置进行目标发现
*   支持多种图形和仪表板模式

## 2. Prometheus 基本安装
本指南概述了在 Linux 系统上安装 Prometheus 的基本步骤。对于其他操作系统或更高级的部署（例如 Docker、Kubernetes），请参阅 Prometheus 官方文档。

### 先决条件
*   基于 Linux 的操作系统（例如 Ubuntu、CentOS）
*   用于下载文件的 `wget` 或 `curl`
*   用于解压归档的 `tar`

### 分步安装

1.  **创建 Prometheus 用户（可选但推荐）**
    建议在专用用户帐户下运行 Prometheus。
    ```bash
    sudo useradd --no-create-home --shell /bin/false prometheus
    ```

2.  **为 Prometheus 创建目录**
    为 Prometheus 二进制文件和配置创建必要的目录。
    ```bash
    sudo mkdir /etc/prometheus
    sudo mkdir /var/lib/prometheus
    sudo chown prometheus:prometheus /var/lib/prometheus
    ```

3.  **下载 Prometheus**
    访问 [Prometheus 下载页面](https://prometheus.io/download/) 查找最新的稳定版本。将 `[VERSION]` 和 `[OS_ARCH]` 替换为适当的值（例如 `prometheus-2.x.x.linux-amd64.tar.gz`）。
    ```bash
    wget https://github.com/prometheus/prometheus/releases/download/v[VERSION]/prometheus-[VERSION].[OS_ARCH].tar.gz
    ```
    示例：
    ```bash
    wget https://github.com/prometheus/prometheus/releases/download/v2.46.0/prometheus-2.46.0.linux-amd64.tar.gz
    ```

4.  **解压并安装 Prometheus 二进制文件**
    解压下载的归档文件并将二进制文件移动到 `/usr/local/bin/`。
    ```bash
    tar xvf prometheus-[VERSION].[OS_ARCH].tar.gz
    sudo mv prometheus-[VERSION].[OS_ARCH]/prometheus /usr/local/bin/
    sudo mv prometheus-[VERSION].[OS_ARCH]/promtool /usr/local/bin/
    sudo chown prometheus:prometheus /usr/local/bin/prometheus
    sudo chown prometheus:prometheus /usr/local/bin/promtool
    ```

5.  **移动控制台库和示例**
    将 `consoles` 和 `console_libraries` 目录移动到 `/etc/prometheus/`。
    ```bash
    sudo mv prometheus-[VERSION].[OS_ARCH]/consoles /etc/prometheus
    sudo mv prometheus-[VERSION].[OS_ARCH]/console_libraries /etc/prometheus
    sudo chown -R prometheus:prometheus /etc/prometheus/consoles
    sudo chown -R prometheus:prometheus /etc/prometheus/console_libraries
    ```

6.  **配置 Prometheus**
    创建基本的 Prometheus 配置文件 `/etc/prometheus/prometheus.yml`。
    ```bash
    sudo nano /etc/prometheus/prometheus.yml
    ```
    添加以下内容：
    ```yaml
    global:
      scrape_interval: 15s

    scrape_configs:
      - job_name: 'prometheus'
        static_configs:
          - targets: ['localhost:9090']
    ```
    设置配置文件的所有权：
    ```bash
    sudo chown prometheus:prometheus /etc/prometheus/prometheus.yml
    ```

7.  **创建 Systemd 服务文件**
    创建 systemd 服务文件以管理 Prometheus 进程。
    ```bash
    sudo nano /etc/systemd/system/prometheus.service
    ```
    添加以下内容：
    ```ini
    [Unit]
    Description=Prometheus
    Wants=network-online.target
    After=network-online.target

    [Service]
    User=prometheus
    Group=prometheus
    Type=simple
    ExecStart=/usr/local/bin/prometheus \
        --config.file /etc/prometheus/prometheus.yml \
        --storage.tsdb.path /var/lib/prometheus \
        --web.console.templates=/etc/prometheus/consoles \
        --web.console.libraries=/etc/prometheus/console_libraries
    ExecReload=/bin/kill -HUP $MAINPID
    Restart=on-failure

    [Install]
    WantedBy=multi-user.target
    ```

8.  **重新加载 Systemd 并启动 Prometheus**
    重新加载 systemd 管理器配置并启动 Prometheus。
    ```bash
    sudo systemctl daemon-reload
    sudo systemctl start prometheus
    sudo systemctl enable prometheus
    ```

9.  **验证 Prometheus 状态**
    检查 Prometheus 服务的状态。
    ```bash
    sudo systemctl status prometheus
    ```
    您应该看到 `active (running)`。您还可以通过 Web 浏览器访问 Prometheus UI：`http://<您的服务器IP>:9090`。

这完成了 Prometheus 的基本安装。您现在可以配置它来抓取应用程序和基础设施的指标。


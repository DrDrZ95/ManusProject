# Grafana 集成指南

## 1. Grafana 简介
Grafana 是一个开源的数据可视化和监控工具，它允许您查询、可视化、告警和探索您的指标、日志和追踪，无论它们存储在哪里。它提供了一种强大而优雅的方式来从各种数据源创建仪表板，使其成为操作分析的流行选择。

**主要特点：**
*   **统一可视化**：连接到众多数据源（Prometheus、InfluxDB、Elasticsearch、PostgreSQL、MySQL 等），并允许您在单个界面中可视化数据。
*   **丰富的仪表板**：提供广泛的可视化选项，包括图表、热力图、表格等，以及高度可定制的面板。
*   **告警**：根据您的指标定义告警规则，并通过各种渠道（电子邮件、Slack、PagerDuty 等）接收通知。
*   **模板化**：在仪表板中使用变量创建动态和可重用的模板，减少重复。
*   **插件**：通过丰富的社区构建和官方插件生态系统扩展功能，用于数据源、面板和应用程序。

## 2. Grafana 基本安装
本指南概述了在 Linux 系统上安装 Grafana 的基本步骤。对于其他操作系统或更高级的部署（例如 Docker、Kubernetes），请参阅 Grafana 官方文档。

### 先决条件
*   基于 Linux 的操作系统（例如 Ubuntu、CentOS）
*   用于下载文件的 `wget` 或 `curl`
*   `apt` 或 `yum` 包管理器

### 分步安装 (Ubuntu/Debian)

1.  **安装所需软件包**
    ```bash
    sudo apt-get install -y apt-transport-https software-properties-common wget
    ```

2.  **导入 GPG 密钥**
    ```bash
    sudo mkdir -p /etc/apt/keyrings/
    sudo wget -q -O /etc/apt/keyrings/grafana.key https://apt.grafana.com/gpg.key
    ```

3.  **添加 Grafana 仓库**
    ```bash
    echo "deb [signed-by=/etc/apt/keyrings/grafana.key] https://apt.grafana.com stable main" | sudo tee -a /etc/apt/sources.list.d/grafana.list
    ```

4.  **更新 Apt 缓存**
    ```bash
    sudo apt-get update
    ```

5.  **安装 Grafana**
    ```bash
    sudo apt-get install grafana
    ```

6.  **启动并启用 Grafana 服务**
    ```bash
    sudo systemctl daemon-reload
    sudo systemctl start grafana-server
    sudo systemctl enable grafana-server
    ```

7.  **验证 Grafana 状态**
    检查 Grafana 服务的状态。
    ```bash
    sudo systemctl status grafana-server
    ```
    您应该看到 `active (running)`。您可以通过 Web 浏览器访问 Grafana UI：`http://<您的服务器IP>:3000`。默认登录凭据是 `admin`/`admin`。首次登录时会提示您更改密码。

这完成了 Grafana 的基本安装。您现在可以配置数据源（例如 Prometheus）并构建仪表板以可视化您的指标。


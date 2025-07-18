# Prometheus Integration Guide

## 1. Introduction to Prometheus
Prometheus is an open-source systems monitoring and alerting toolkit originally built at SoundCloud. Since its inception in 2012, many companies and organizations have adopted Prometheus as a standalone monitoring solution. It collects and stores time-series data by scraping metrics from configured targets at given intervals, evaluating rule expressions, displaying the results, and can trigger alerts if some condition is observed to be true.

**Key Features:**
*   A multi-dimensional data model (time series identified by metric name and key/value pairs)
*   Flexible query language (PromQL)
*   No reliance on distributed storage; single server nodes are autonomous
*   Time series collection happens via a pull model over HTTP
*   Pushgateway for pushing time series from ephemeral jobs
*   Target discovery via service discovery or static configuration
*   Multiple modes of graphing and dashboarding support

## 2. Basic Installation of Prometheus
This guide outlines the basic steps to install Prometheus on a Linux system. For other operating systems or more advanced deployments (e.g., Docker, Kubernetes), please refer to the official Prometheus documentation.

### Prerequisites
*   A Linux-based operating system (e.g., Ubuntu, CentOS)
*   `wget` or `curl` for downloading files
*   `tar` for extracting archives

### Step-by-Step Installation

1.  **Create a Prometheus User (Optional but Recommended)**
    It's good practice to run Prometheus under a dedicated user account.
    ```bash
    sudo useradd --no-create-home --shell /bin/false prometheus
    ```

2.  **Create Directories for Prometheus**
    Create necessary directories for Prometheus binaries and configuration.
    ```bash
    sudo mkdir /etc/prometheus
    sudo mkdir /var/lib/prometheus
    sudo chown prometheus:prometheus /var/lib/prometheus
    ```

3.  **Download Prometheus**
    Visit the [Prometheus download page](https://prometheus.io/download/) to find the latest stable version. Replace `[VERSION]` and `[OS_ARCH]` with the appropriate values (e.g., `prometheus-2.x.x.linux-amd64.tar.gz`).
    ```bash
    wget https://github.com/prometheus/prometheus/releases/download/v[VERSION]/prometheus-[VERSION].[OS_ARCH].tar.gz
    ```
    Example:
    ```bash
    wget https://github.com/prometheus/prometheus/releases/download/v2.46.0/prometheus-2.46.0.linux-amd64.tar.gz
    ```

4.  **Extract and Install Prometheus Binaries**
    Extract the downloaded archive and move the binaries to `/usr/local/bin`.
    ```bash
    tar xvf prometheus-[VERSION].[OS_ARCH].tar.gz
    sudo mv prometheus-[VERSION].[OS_ARCH]/prometheus /usr/local/bin/
    sudo mv prometheus-[VERSION].[OS_ARCH]/promtool /usr/local/bin/
    sudo chown prometheus:prometheus /usr/local/bin/prometheus
    sudo chown prometheus:prometheus /usr/local/bin/promtool
    ```

5.  **Move Console Libraries and Examples**
    Move the `consoles` and `console_libraries` directories to `/etc/prometheus`.
    ```bash
    sudo mv prometheus-[VERSION].[OS_ARCH]/consoles /etc/prometheus
    sudo mv prometheus-[VERSION].[OS_ARCH]/console_libraries /etc/prometheus
    sudo chown -R prometheus:prometheus /etc/prometheus/consoles
    sudo chown -R prometheus:prometheus /etc/prometheus/console_libraries
    ```

6.  **Configure Prometheus**
    Create a basic Prometheus configuration file `/etc/prometheus/prometheus.yml`.
    ```bash
    sudo nano /etc/prometheus/prometheus.yml
    ```
    Add the following content:
    ```yaml
    global:
      scrape_interval: 15s

    scrape_configs:
      - job_name: 'prometheus'
        static_configs:
          - targets: ['localhost:9090']
    ```
    Set ownership for the configuration file:
    ```bash
    sudo chown prometheus:prometheus /etc/prometheus/prometheus.yml
    ```

7.  **Create a Systemd Service File**
    Create a systemd service file to manage the Prometheus process.
    ```bash
    sudo nano /etc/systemd/system/prometheus.service
    ```
    Add the following content:
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

8.  **Reload Systemd and Start Prometheus**
    Reload the systemd manager configuration and start Prometheus.
    ```bash
    sudo systemctl daemon-reload
    sudo systemctl start prometheus
    sudo systemctl enable prometheus
    ```

9.  **Verify Prometheus Status**
    Check the status of the Prometheus service.
    ```bash
    sudo systemctl status prometheus
    ```
    You should see `active (running)`. You can also access the Prometheus UI in your web browser at `http://<your_server_ip>:9090`.

This completes the basic installation of Prometheus. You can now configure it to scrape metrics from your applications and infrastructure.


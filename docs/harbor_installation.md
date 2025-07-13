# Harbor Installation Guide

Harbor is an open-source trusted cloud native registry project that stores, signs, and scans content. This guide provides steps to install Harbor.

## Prerequisites

Before installing Harbor, ensure you have the following:

*   Docker Engine (version 1.13.0 or later)
*   Docker Compose (version 1.18.0 or later)
*   OpenSSL (for generating certificates, if using HTTPS)
*   A Linux machine (Ubuntu recommended) with sufficient resources (CPU, memory, disk space)

## Installation Steps

1.  **Download the Harbor Installer**

    Visit the [Harbor releases page](https://github.com/goharbor/harbor/releases) and download the latest offline installer. For example:

    ```bash
    wget https://github.com/goharbor/harbor/releases/download/v2.10.0/harbor-offline-installer-v2.10.0.tgz
    tar xvf harbor-offline-installer-v2.10.0.tgz
    cd harbor
    ```

2.  **Configure `harbor.yml`**

    Edit the `harbor.yml` file to configure your Harbor instance. Key configurations include:

    *   `hostname`: The FQDN or IP address of your Harbor instance.
    *   `http` / `https`: Enable HTTPS and provide paths to your SSL certificates (recommended for production).
    *   `harbor_admin_password`: Set the password for the default Harbor administrator.

    Example `harbor.yml` snippet for HTTPS:

    ```yaml
    hostname: your.harbor.domain

    https:
      port: 443
      certificate: /etc/harbor/ssl/your.harbor.domain.crt
      private_key: /etc/harbor/ssl/your.harbor.domain.key

    harbor_admin_password: your_admin_password
    ```

    If you don't have certificates, you can generate self-signed certificates for testing:

    ```bash
    mkdir -p /etc/harbor/ssl
    openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes \
        -keyout /etc/harbor/ssl/your.harbor.domain.key -out /etc/harbor/ssl/your.harbor.domain.crt \
        -subj "/CN=your.harbor.domain" -addext "subjectAltName=DNS:your.harbor.domain,IP:your.harbor.ip"
    ```

3.  **Run the Installer Script**

    Execute the `install.sh` script to deploy Harbor:

    ```bash
    ./install.sh
    ```

    If you configured HTTPS, you might need to run:

    ```bash
    ./install.sh --with-https
    ```

4.  **Verify Installation**

    After the installation completes, you can access the Harbor UI via your configured hostname (e.g., `https://your.harbor.domain`). Log in with the `admin` user and the password you set in `harbor.yml`.

## Post-Installation

*   **Configure DNS**: Ensure your `hostname` resolves to the IP address of your Harbor server.
*   **Firewall Rules**: Open ports 80 (HTTP) and 443 (HTTPS) on your server.
*   **Data Persistence**: Harbor stores its data in Docker volumes. Ensure these volumes are backed up regularly.

For more advanced configurations and troubleshooting, refer to the [official Harbor documentation](https://goharbor.io/docs/).


# Helm Installation Guide

Helm is the package manager for Kubernetes. It helps you manage Kubernetes applications — Helm Charts help you define, install, and upgrade even the most complex Kubernetes application.

## Prerequisites

Before installing Helm, ensure you have the following:

*   A working Kubernetes cluster.
*   `kubectl` configured to connect to your Kubernetes cluster.

## Installation Steps

There are several ways to install Helm. The most common methods are via script or a package manager.

### Method 1: Install with Script (Recommended for Linux/macOS)

This is the easiest way to get Helm up and running on Linux or macOS.

1.  **Download the installation script:**

    ```bash
    curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3
    ```

2.  **Make the script executable:**

    ```bash
    chmod 700 get_helm.sh
    ```

3.  **Run the script:**

    ```bash
    ./get_helm.sh
    ```

    This script will download the latest stable Helm version and install it in your system.

### Method 2: Install with Homebrew (macOS)

If you are on macOS, you can use Homebrew:

```bash
brew install helm
```

### Method 3: Install from Binary

1.  **Download the desired version:**

    Visit the [Helm releases page](https://github.com/helm/helm/releases) and download the binary release for your operating system and architecture. For example, for Linux AMD64:

    ```bash
    wget https://get.helm.sh/helm-v3.14.2-linux-amd64.tar.gz
    tar -zxvf helm-v3.14.2-linux-amd64.tar.gz
    ```

2.  **Move the Helm binary to your executable path:**

    ```bash
    sudo mv linux-amd64/helm /usr/local/bin/helm
    ```

### Verify Installation

After installation, verify that Helm is installed correctly by checking its version:

```bash
helm version
```

You should see output similar to:

```
version.BuildInfo{Version:"v3.14.2", GitCommit:"2a781a965037e07167037b7b546896686a775178", GitTreeState:"clean", GoVersion:"go1.21.7"}
```

## Basic Helm Commands

*   **`helm install`**: Install a chart.
*   **`helm upgrade`**: Upgrade a release.
*   **`helm uninstall`**: Uninstall a release.
*   **`helm list`**: List releases.
*   **`helm search repo`**: Search for charts in configured repositories.
*   **`helm repo add`**: Add a chart repository.
*   **`helm repo update`**: Update information of available charts from chart repositories.

For more detailed information and advanced usage, refer to the [official Helm documentation](https://helm.sh/docs/).


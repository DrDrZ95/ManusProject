# GitHub 上传指南

本文档提供了将 AI 代理代码上传到您的 GitHub 代码仓库的说明，需配合已生成的 SSH 密钥使用。

## 先决条件

- 您已按照 [SSH 设置指南](docs/ssh_setup.zh-CN.md) 中的说明将 SSH 密钥添加到了您的 GitHub 帐户。
- 您已在 GitHub 上为此项目创建了一个代码仓库 (例如 `ai-agent`)。

## 上传步骤

### 1. 配置 Git

如果您尚未配置，首先请使用您的 GitHub 用户名和电子邮件设置 Git 配置：

```bash
git config --global user.name "您的 GitHub 用户名"
git config --global user.email "your.email@example.com"
```

### 2. 初始化 Git 代码仓库

导航到项目目录并初始化 Git 代码仓库 (如果用户尚未完成此操作)：

```bash
cd /home/ubuntu/ai-agent
git init
```

### 3. 创建 .gitignore 文件

确保存在一个 `.gitignore` 文件以排除不必要的文件。一个好的起点是：

```bash
cat > .gitignore << EOF
# Python
__pycache__/
*.py[cod]
*$py.class
*.so
.Python

# 虚拟环境
venv/
ENV/

# 模型文件 (大型文件 - 如果必须提交，请考虑使用 Git LFS)
models/*
!models/.gitkeep
# 如果您将模型下载到 models 的子目录中，例如 models/Qwen3-4B-Instruct，
# 并且希望保留目录结构但不包含大文件：
# models/Qwen3-4B-Instruct/*
# !models/Qwen3-4B-Instruct/.gitkeep

# 日志
logs/
*.log
model_server.log

# 本地配置
.env
.env.local

# IDE 特定文件
.idea/
.vscode/
*.swp
*.swo

# 操作系统生成的文件
.DS_Store
Thumbs.db
EOF
```
通常建议 **不要** 将大型模型文件直接提交到 Git。上面的 `models/*` 条目将忽略它们。如果您想在 Git 中保留目录本身，请在 `models/` 内部使用一个 `.gitkeep` 文件。

### 4. 添加远程代码仓库

将您的 GitHub 代码仓库添加为远程源 (替换 `username` 和 `repository-name`)：

```bash
# 示例: git remote add origin git@github.com:your-username/ai-agent.git
git remote add origin git@github.com:username/repository-name.git
```

如果名为 `origin` 的远程已存在，您可能需要更新它：
```bash
git remote set-url origin git@github.com:username/repository-name.git
```

### 5. 配置 SSH 密钥使用

为确保 Git 使用正确的 SSH 密钥 (如果您有多个或非标准密钥)，您可以：

- 创建或编辑 `~/.ssh/config` 文件 (推荐用于持久配置)：
  ```
  Host github.com
    HostName github.com
    User git
    IdentityFile ~/.ssh/ai_agent_github_rsa  # 或您用于此项目的特定密钥
  ```

- 或者为每个 Git 命令使用 `GIT_SSH_COMMAND` 环境变量 (不常用于常规使用)：
  ```bash
  GIT_SSH_COMMAND="ssh -i ~/.ssh/ai_agent_github_rsa" git push -u origin main
  ```

### 6. 添加并提交文件

将所有相关的项目文件添加到 Git 代码仓库并创建初始提交：

```bash
cd /home/ubuntu/ai-agent
git add .
git commit -m "初始提交：AI 代理项目及 Qwen3-4B-Instruct 模型设置"
```

### 7. 推送到 GitHub

将代码推送到您的 GitHub 代码仓库 (假设您的主分支名为 `main`)：

```bash
git push -u origin main
```

如果您使用不同的分支名称 (例如 "master")，请相应调整命令。

### 8. 验证上传

在 Web 浏览器中访问您的 GitHub 代码仓库，以验证所有预期的文件是否已成功上传。

## 故障排除

如果您遇到权限问题：

1.  验证 SSH 公钥 (`~/.ssh/ai_agent_github_rsa.pub` 或您的特定密钥) 是否已添加到您的 GitHub 帐户。
2.  检查 SSH 私钥权限：
    ```bash
    chmod 600 ~/.ssh/ai_agent_github_rsa # 或您的特定私钥文件
    ```
3.  测试与 GitHub 的 SSH 连接：
    ```bash
    ssh -T git@github.com -i ~/.ssh/ai_agent_github_rsa # 或您的特定密钥
    ```
    您应该会看到类似这样的消息："Hi username! You've successfully authenticated, but GitHub does not provide shell access."

如果您遇到大文件被拒绝的问题，请确保它们已在 `.gitignore` 中正确列出。对于 *必须* 进行版本控制的非常大的文件 (不推荐用于模型)，请研究并使用 Git LFS (Large File Storage)。

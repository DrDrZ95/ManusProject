# GitHub SSH 密钥设置指南

本文档为 AI 代理项目提供了将生成的 SSH 密钥添加到您的 GitHub 帐户以启用安全代码上传的说明。

## SSH 密钥信息

如果您需要生成新密钥，可以使用以下命令 (替换 `your_email@example.com`)：

```bash
ssh-keygen -t rsa -b 4096 -C "your_email@example.com" -f ~/.ssh/ai_agent_github_rsa
```

这将创建：
- **私钥位置**: `~/.ssh/ai_agent_github_rsa`
- **公钥位置**: `~/.ssh/ai_agent_github_rsa.pub`

如果您使用的是现有密钥 (例如，先前生成的 `~/.ssh/github_rsa`)，请确保您知道其位置。

## 将 SSH 密钥添加到 GitHub

1.  **复制您的 SSH 公钥**。如果您如上所述生成了新密钥，请使用：
    ```bash
    cat ~/.ssh/ai_agent_github_rsa.pub
    ```
    如果使用现有密钥，请 `cat` 该公钥文件。
2.  登录到您的 GitHub 帐户。
3.  单击右上角的个人资料图片，然后选择 “Settings” (设置)。
4.  在左侧边栏中，单击 “SSH and GPG keys” (SSH 和 GPG 密钥)。
5.  单击 “New SSH key” (新建 SSH 密钥) 或 “Add SSH key” (添加 SSH 密钥) 按钮。
6.  为密钥输入描述性标题 (例如，“AI 代理部署密钥” 或 “我的笔记本电脑密钥”)。
7.  将复制的公钥粘贴到 “Key” (密钥) 字段中。
8.  单击 “Add SSH key” (添加 SSH 密钥) 保存。

## 测试 SSH 连接

将密钥添加到 GitHub 后，您可以测试连接 (如果不同，请将 `~/.ssh/ai_agent_github_rsa` 替换为您的实际私钥路径)：

```bash
ssh -T git@github.com -i ~/.ssh/ai_agent_github_rsa
```

您应该会看到类似这样的消息：“Hi `username`! You've successfully authenticated, but GitHub does not provide shell access.” (嗨 `username`！您已成功通过身份验证，但 GitHub 不提供 shell 访问权限。)

## 为 AI 代理项目使用 SSH 密钥进行 Git 操作

要将此特定 SSH 密钥用于 `/home/ubuntu/ai-agent` 项目的 Git 操作，您可以：

1.  **配置 SSH 代理 (推荐，易于使用)**：
    将您的私钥添加到 SSH 代理：
    ```bash
    eval "$(ssh-agent -s)"
    ssh-add ~/.ssh/ai_agent_github_rsa # 或您的特定私钥
    ```

2.  **配置 `~/.ssh/config` 文件 (推荐用于特定主机/密钥)**：
    创建或编辑 `~/.ssh/config` 并为 GitHub 添加一个条目，指定您的密钥：
    ```
    Host github.com
      HostName github.com
      User git
      IdentityFile ~/.ssh/ai_agent_github_rsa  # 您的私钥路径
      IdentitiesOnly yes
    ```
    通过此配置，Git 将自动为 `github.com` 使用指定的密钥。

3.  **将 Git 远程 URL 设置为使用 SSH**：
    导航到您的本地项目目录 `/home/ubuntu/ai-agent`。
    如果您使用 HTTPS 克隆了代码仓库，请更新远程 URL：
    ```bash
    cd /home/ubuntu/ai-agent
    git remote set-url origin git@github.com:username/repository-name.git
    ```
    将 `username/repository-name.git` 替换为您的实际 GitHub 代码仓库路径。

4.  **使用 `GIT_SSH_COMMAND` 指定密钥 (不常用于常规使用)**：
    您可以在 Git 命令前加上此环境变量：
    ```bash
    GIT_SSH_COMMAND="ssh -i ~/.ssh/ai_agent_github_rsa" git clone git@github.com:username/repository.git
    GIT_SSH_COMMAND="ssh -i ~/.ssh/ai_agent_github_rsa" git push
    ```

通过正确配置 SSH，您可以从 `/home/ubuntu/ai-agent` 项目目录安全地与您的 GitHub 代码仓库进行交互。

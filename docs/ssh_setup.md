# GitHub SSH Key Setup Instructions

This document provides instructions for adding a generated SSH key to your GitHub account to enable secure code uploads for the AI Agent project.

## SSH Key Information

If you need to generate a new key, you can use the following command (replace `your_email@example.com`):

```bash
ssh-keygen -t rsa -b 4096 -C "your_email@example.com" -f ~/.ssh/ai_agent_github_rsa
```

This will create:
- **Private Key Location**: `~/.ssh/ai_agent_github_rsa`
- **Public Key Location**: `~/.ssh/ai_agent_github_rsa.pub`

If you are using an existing key (e.g., `~/.ssh/github_rsa` as previously generated), ensure you know its location.

## Adding SSH Key to GitHub

1.  **Copy your SSH public key**. If you generated a new key as above, use:
    ```bash
    cat ~/.ssh/ai_agent_github_rsa.pub
    ```
    If using an existing key, cat that public key file.
2.  Log in to your GitHub account.
3.  Click on your profile picture in the top-right corner and select "Settings".
4.  In the left sidebar, click on "SSH and GPG keys".
5.  Click the "New SSH key" or "Add SSH key" button.
6.  Enter a descriptive title for the key (e.g., "AI Agent Deploy Key" or "My Laptop Key").
7.  Paste the copied public key into the "Key" field.
8.  Click "Add SSH key" to save.

## Testing the SSH Connection

After adding the key to GitHub, you can test the connection (replace `~/.ssh/ai_agent_github_rsa` with your actual private key path if different):

```bash
ssh -T git@github.com -i ~/.ssh/ai_agent_github_rsa
```

You should see a message like: "Hi `username`! You've successfully authenticated, but GitHub does not provide shell access."

## Using the SSH Key for Git Operations with the AI Agent Project

To use this specific SSH key for Git operations with the `/home/ubuntu/ai-agent` project, you can:

1.  **Configure SSH Agent (Recommended for ease of use)**:
    Add your private key to the SSH agent:
    ```bash
    eval "$(ssh-agent -s)"
    ssh-add ~/.ssh/ai_agent_github_rsa # Or your specific private key
    ```

2.  **Configure `~/.ssh/config` file (Recommended for specific hosts/keys)**:
    Create or edit `~/.ssh/config` and add an entry for GitHub, specifying your key:
    ```
    Host github.com
      HostName github.com
      User git
      IdentityFile ~/.ssh/ai_agent_github_rsa  # Path to your private key
      IdentitiesOnly yes
    ```
    With this configuration, Git will automatically use the specified key for `github.com`.

3.  **Set Git remote URL to use SSH**:
    Navigate to your local project directory `/home/ubuntu/ai-agent`.
    If you've cloned using HTTPS, update the remote URL:
    ```bash
    cd /home/ubuntu/ai-agent
    git remote set-url origin git@github.com:username/repository-name.git
    ```
    Replace `username/repository-name.git` with your actual GitHub repository path.

4.  **Specify key with `GIT_SSH_COMMAND` (Less common for regular use)**:
    You can prefix your Git commands with this environment variable:
    ```bash
    GIT_SSH_COMMAND="ssh -i ~/.ssh/ai_agent_github_rsa" git clone git@github.com:username/repository.git
    GIT_SSH_COMMAND="ssh -i ~/.ssh/ai_agent_github_rsa" git push
    ```

By configuring SSH correctly, you can securely interact with your GitHub repository from the `/home/ubuntu/ai-agent` project directory.

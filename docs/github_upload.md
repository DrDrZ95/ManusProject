# GitHub Upload Instructions

This document provides instructions for uploading the AI Agent code to your GitHub repository using the generated SSH key.

## Prerequisites

- You have already added the SSH key to your GitHub account as described in the [SSH Setup Guide](docs/ssh_setup.md)
- You have created a repository on GitHub for this project (e.g., `ai-agent`)

## Upload Instructions

### 1. Configure Git

First, set up your Git configuration with your GitHub username and email if you haven't already:

```bash
git config --global user.name "Your GitHub Username"
git config --global user.email "your.email@example.com"
```

### 2. Initialize Git Repository

Navigate to the project directory and initialize a Git repository (if not already done by the user):

```bash
cd /home/ubuntu/ai-agent
git init
```

### 3. Create .gitignore File

Ensure a `.gitignore` file exists to exclude unnecessary files. A good starting point would be:

```bash
cat > .gitignore << EOF
# Python
__pycache__/
*.py[cod]
*$py.class
*.so
.Python

# Virtual Environment
venv/
ENV/

# Model files (large files - consider Git LFS for these if you must commit them)
models/*
!models/.gitkeep
# If you download models into a subdirectory of models, e.g. models/Llama3.1-8B-Instruct,
# and want to keep the directory structure but not the large files:
# models/Llama3.1-8B-Instruct/*
# !models/Llama3.1-8B-Instruct/.gitkeep

# Logs
logs/
*.log
model_server.log

# Local configuration
.env
.env.local

# IDE specific files
.idea/
.vscode/
*.swp
*.swo

# OS generated files
.DS_Store
Thumbs.db
EOF
```
It's generally recommended **not** to commit large model files directly to Git. The `models/*` entry above will ignore them. Use a `.gitkeep` file inside `models/` if you want to keep the directory itself in Git.

### 4. Add Remote Repository

Add your GitHub repository as the remote origin (replace `username` and `repository-name`):

```bash
# Example: git remote add origin git@github.com:your-username/ai-agent.git
git remote add origin git@github.com:username/repository-name.git
```

If a remote named `origin` already exists, you might need to update it:
```bash
git remote set-url origin git@github.com:username/repository-name.git
```

### 5. Configure SSH Key Usage

To ensure Git uses the correct SSH key (if you have multiple or non-standard keys), you can either:

- Create or edit `~/.ssh/config` file (recommended for persistent configuration):
  ```
  Host github.com
    HostName github.com
    User git
    IdentityFile ~/.ssh/github_rsa  # Or your specific key for this project
  ```

- Or use the `GIT_SSH_COMMAND` environment variable for each Git command (less common for regular use):
  ```bash
  GIT_SSH_COMMAND="ssh -i ~/.ssh/github_rsa" git push -u origin main
  ```

### 6. Add and Commit Files

Add all relevant project files to the Git repository and create an initial commit:

```bash
cd /home/ubuntu/ai-agent
git add .
git commit -m "Initial commit: AI Agent project with Llama3.1-8B-Instruct model setup"
```

### 7. Push to GitHub

Push the code to your GitHub repository (assuming your main branch is named `main`):

```bash
git push -u origin main
```

If you are using a different branch name (e.g., "master"), adjust the command accordingly.

### 8. Verify Upload

Visit your GitHub repository in a web browser to verify that all intended files have been uploaded successfully.

## Troubleshooting

If you encounter permission issues:

1. Verify that the SSH public key (`~/.ssh/github_rsa.pub` or your specific key) has been added to your GitHub account.
2. Check SSH private key permissions:
   ```bash
   chmod 600 ~/.ssh/github_rsa # Or your specific private key file
   ```
3. Test SSH connection to GitHub:
   ```bash
   ssh -T git@github.com -i ~/.ssh/github_rsa # Or your specific key
   ```
   You should see a message like: "Hi username! You've successfully authenticated, but GitHub does not provide shell access."

If you encounter issues with large files being rejected, ensure they are correctly listed in `.gitignore`. For very large files that *must* be versioned (not recommended for models), research and use Git LFS (Large File Storage).

# GitHub Upload Instructions

This document provides instructions for uploading the DeepSeek Agent code to your GitHub repository using the generated SSH key.

## Prerequisites

- You have already added the SSH key to your GitHub account as described in the [SSH Setup Guide](/home/ubuntu/deepseek-agent/docs/ssh_setup.md)
- You have created a repository on GitHub for this project

## Upload Instructions

### 1. Configure Git

First, set up your Git configuration with your GitHub username and email:

```bash
git config --global user.name "Your GitHub Username"
git config --global user.email "your.email@example.com"
```

### 2. Initialize Git Repository

Navigate to the project directory and initialize a Git repository:

```bash
cd /home/ubuntu/deepseek-agent
git init
```

### 3. Create .gitignore File

Create a `.gitignore` file to exclude unnecessary files:

```bash
cat > .gitignore << 'EOF'
# Python
__pycache__/
*.py[cod]
*$py.class
*.so
.Python
env/
build/
develop-eggs/
dist/
downloads/
eggs/
.eggs/
lib/
lib64/
parts/
sdist/
var/
*.egg-info/
.installed.cfg
*.egg

# Virtual Environment
venv/
ENV/

# Model files (large files)
*.bin
*.pt
*.pth
*.onnx
*.safetensors

# Logs
logs/
*.log

# Local configuration
.env
.env.local

# IDE specific files
.idea/
.vscode/
*.swp
*.swo
EOF
```

### 4. Add Remote Repository

Add your GitHub repository as the remote origin:

```bash
git remote add origin git@github.com:username/repository-name.git
```

Replace `username` and `repository-name` with your GitHub username and repository name.

### 5. Configure SSH Key Usage

To ensure Git uses the correct SSH key, you can either:

- Create or edit `~/.ssh/config` file:
  ```
  Host github.com
    IdentityFile ~/.ssh/github_rsa
    User git
  ```

- Or use the GIT_SSH_COMMAND environment variable for each Git command:
  ```bash
  GIT_SSH_COMMAND="ssh -i ~/.ssh/github_rsa" git push -u origin main
  ```

### 6. Add and Commit Files

Add all files to the Git repository and create an initial commit:

```bash
git add .
git commit -m "Initial commit: DeepSeek Agent project structure and documentation"
```

### 7. Push to GitHub

Push the code to your GitHub repository:

```bash
git push -u origin main
```

If you're using a different branch name (e.g., "master" instead of "main"), adjust the command accordingly.

### 8. Verify Upload

Visit your GitHub repository in a web browser to verify that all files have been uploaded successfully.

## Troubleshooting

If you encounter permission issues:

1. Verify that the SSH key has been added to your GitHub account
2. Check SSH key permissions:
   ```bash
   chmod 600 ~/.ssh/github_rsa
   chmod 644 ~/.ssh/github_rsa.pub
   ```
3. Test SSH connection:
   ```bash
   ssh -T git@github.com -i ~/.ssh/github_rsa
   ```

If you encounter issues with large files, consider using Git LFS (Large File Storage) for model files.

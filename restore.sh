#!/bin/bash
# === AI Agent 操作系统恢复脚本 ===
# 功能：为了避免manus'computer无法恢复，集成 pnpm、.NET 8、Python 、 Unsloth 、 Docker、 btrace 相关环境，持续补充中。

set -e  # 出错时立即中止执行

# ========================================
# 0. Ollama和npm，一般都有
# ========================================
echo "🔍 检查 pnpm 是否已安装..."
pnpm --version || echo "⚠️ 未检测到 pnpm，请手动安装。"

echo "🚀 安装 Ollama..."
cd /home/ubuntu && curl -fsSL https://ollama.com/install.sh | sh

# ========================================
# 1. 安装 .NET 8 SDK
# ========================================
echo "⚙️ 安装 .NET 8 SDK..."
cd /home/ubuntu
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --version latest --channel 8.0

echo "⚙️ 设置全局dotnet"
echo 'export PATH="$PATH:/home/ubuntu/.dotnet"' >> /home/ubuntu/.bashrc
echo 'export DOTNET_ROOT=/home/ubuntu/.dotnet' >> /home/ubuntu/.bashrc

source ~/.bashrc

cd /home/ubuntu/
dotnet --version

# ========================================
# 2. 安装 Python 与系统依赖项
# ========================================
echo "🐍 安装 Python 和系统依赖项..."
sudo apt-get update
sudo apt-get install -y \
    python3-dev \
    python3-pip \
    python3-venv \
    build-essential \
    cmake \
    git \
    wget \
    curl \
    libopenblas-dev \
    libomp-dev

echo "✅ 系统依赖项安装完成。"

# ========================================
# 3. 安装 Docker 和 btrace（适用于 Ubuntu）
# ========================================
echo "🐳 安装 Docker..."

# 移除旧版本（如果存在）
sudo apt-get remove -y docker docker-engine docker.io containerd runc || true

# 安装依赖工具
sudo apt-get install -y ca-certificates curl gnupg

# 添加 Docker 官方 GPG 密钥
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

# 添加 Docker 官方仓库
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# 更新并安装 Docker 引擎
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 验证 Docker 安装
docker --version

echo "✅ Docker 安装完成。"

echo "⚙️ 安装 bpftrace(eBPF)..."
cd /home/ubuntu && sudo apt-get update && sudo apt-get install -y bpftrace
bpftrace --version
echo "✅ bpftrace(eBPF) 安装完成。"

# ========================================
# 4. 还原 .NET 与前端依赖
# ========================================
echo "📦 恢复 .NET 和 pnpm 项目依赖..."
dotnet restore /home/ubuntu/ai-agent/platform/backend/AgentWebApi/AgentWebApi.csproj
cd /home/ubuntu/ai-agent/platform/frontend/AgentUI/agent-chat
pnpm install

# ========================================
# 5. 安装 Unsloth 和 LoRA 微调依赖
# ========================================
echo "🧪 设置 Unsloth 和 LoRA 微调环境..."
if [ ! -d "finetune/venv" ]; then
    python3 -m venv finetune/venv
fi

source finetune/venv/bin/activate

echo "🔥 安装支持 CUDA 的 PyTorch..."
pip install torch==2.1.2 torchvision==0.16.2 torchaudio==2.1.2 --index-url https://download.pytorch.org/whl/cu121

echo "📥 安装 Unsloth..."
pip install unsloth

echo "🔧 安装其他微调相关依赖..."
pip install \
    transformers==4.36.2 \
    datasets==2.16.1 \
    peft==0.7.1 \
    accelerate==0.25.0 \
    bitsandbytes==0.41.3 \
    trl==0.7.4 \
    wandb==0.16.1 \
    sentencepiece==0.1.99 \
    gradio==4.12.0 \
    pandas numpy matplotlib scikit-learn

echo "📝 保存当前环境到 requirements.txt..."
pip freeze > finetune/requirements.txt

echo "✅ 安装全部完成！"
echo "💡 若需再次激活微调环境，请执行：source finetune/venv/bin/activate"


# ========================================
# 6. git部分需要手动进行
# ========================================
exit

if [ ! -d "/home/ubuntu/ai-agent" ]; then
    mkdir -p /home/ubuntu/ai-agent
fi
cd /home/ubuntu/ai-agent
echo "1. 初始化"
git init
echo "2. 添加远程remote"
git remote add origin "https://github.com/DrDrZ95/ManusProject"

echo "3. 拉取最新变更"
git fetch origin

echo "4. 切换到 main 分支"
git checkout -b main origin/main

echo "5. 确保本地 跟踪"
git branch --set-upstream-to=origin/main
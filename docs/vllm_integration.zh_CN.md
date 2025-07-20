# vLLM 集成指南

## 1. vLLM 简介
vLLM 是一个用于大型语言模型 (LLM) 推理和服务的快速易用库。vLLM 在加州大学伯克利分校的 Sky Computing 实验室开发，旨在最大限度地提高 LLM 服务的吞吐量并最大限度地降低延迟。它通过一种名为 PagedAttention 的新型注意力算法实现这一目标，该算法有效地管理键值缓存，减少内存浪费并提高批处理能力。

**主要特点：**
*   **PagedAttention**：一种优化的注意力算法，可有效管理键值缓存，减少内存开销并提高吞吐量。
*   **连续批处理**：以连续批处理方式处理请求，通过并发运行多个请求来最大限度地提高 GPU 利用率。
*   **优化的 CUDA 内核**：利用高度优化的 CUDA 内核进行各种 LLM 操作，从而加快执行速度。
*   **分布式推理**：支持跨多个 GPU 和机器的分布式推理。
*   **兼容性**：与 Hugging Face Transformers 等流行的 LLM 框架兼容。
*   **OpenAI 兼容 API 服务器**：提供一个模仿 OpenAI API 的 API 服务器，使其易于与现有应用程序集成。

## 2. vLLM 基本安装
vLLM 可以通过 pip 或从源代码安装。建议使用 pip，特别是如果您有兼容的 CUDA 环境。

### 先决条件
*   Python 3.8+
*   NVIDIA GPU，支持 CUDA 11.8 或 12.1（用于 GPU 加速）
*   `pip` 包管理器

### 分步安装

#### 选项 1：通过 pip 安装（推荐）
这是开始使用 vLLM 最简单的方法。

1.  **创建虚拟环境（可选但推荐）**
    建议在虚拟环境中安装 Python 包，以避免冲突。
    ```bash
    python3 -m venv venv_vllm
    source venv_vllm/bin/activate
    ```

2.  **安装 vLLM**
    使用 pip 安装 vLLM。确保您的系统上安装了正确的 CUDA 版本。vLLM 为常见的 CUDA 版本提供了预构建的 wheel 包。
    
    对于 CUDA 12.1：
    ```bash
    pip install vllm
    ```
    
    对于 CUDA 11.8：
    ```bash
    pip install vllm==0.3.0+cu118 -f https://storage.googleapis.com/vllm-wheels/whl/torch-2.1.0.html
    ```
    （注意：如果需要，将 `0.3.0` 替换为所需的 vLLM 版本。请查看 [vLLM GitHub 版本](https://github.com/vllm-project/vllm/releases) 以获取特定版本及其 CUDA 兼容性。）

3.  **验证安装**
    您可以通过在 Python 解释器中尝试导入 `vllm` 或运行一个快速推理示例来验证安装。
    ```bash
    python -c "from vllm import LLM, SamplingParams; print(\'vLLM installed successfully!\')"
    ```

#### 选项 2：从源代码安装
此选项适用于需要为特定环境（例如，不同的 CUDA 版本、自定义修改）构建 vLLM 的开发人员或用户。

1.  **克隆 vLLM 仓库**
    ```bash
    git clone https://github.com/vllm-project/vllm.git
    cd vllm
    ```

2.  **安装依赖项并构建**
    ```bash
    pip install -e .
    ```
    此命令安装必要的 Python 依赖项并构建 CUDA 内核。此过程可能需要一些时间，具体取决于您的系统规格。

### 基本使用示例
安装后，您可以使用 vLLM 提供 LLM 服务。这是一个简单的 Python 示例：

```python
from vllm import LLM, SamplingParams

# 示例提示
prompts = [
    "你好，我的名字是",
    "美国总统是",
    "法国首都是",
]

# 创建采样参数对象。
sampling_params = SamplingParams(temperature=0.7, top_p=0.95, max_tokens=100)

# 创建 LLM。您可以指定模型名称或路径。
# 例如，使用 Llama-2-7B-Chat：
llm = LLM(model="lmsys/vicuna-7b-v1.5") # 或 "meta-llama/Llama-2-7b-chat-hf"

# 从提示生成文本。
outputs = llm.generate(prompts, sampling_params)

# 打印输出。
for prompt, output in zip(prompts, outputs):
    print(f"提示: {prompt!r}, 生成的文本: {output.outputs[0].text!r}")
```

本指南提供了 vLLM 及其安装的基本概述。有关更高级的配置、模型服务和 API 使用，请参阅 [vLLM 官方文档](https://docs.vllm.ai/)。


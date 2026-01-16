# Llama 4 Scout-17B 模型 API 文档

本文档为 Llama 4 Scout-17B 模型 API 提供了全面的文档说明。

## API 概述

Llama 4 Scout-17B 模型通过 FastAPI 部署为一个 RESTful API 服务。该 API 在 2025 端口上运行，并提供使用 Llama 4 Scout-17B 语言模型进行文本生成的端点。相比之前的 Llama4 版本，Llama 4 Scout-17B 提供了更好的性能和功能。

## API 端点

### 根端点

- **URL**: `/`
- **方法**: GET
- **描述**: 返回有关 API 服务的基本信息
- **响应示例**:
  ```json
  {
    "name": "Llama 4 Scout-17B API",
    "version": "1.0.0",
    "status": "active",
    "model": "meta-llama/Llama-4-Scout-17B-16E-Instruct"
  }
  ```

### 健康检查

- **URL**: `/health`
- **方法**: GET
- **描述**: 检查模型和分词器是否已加载且服务是否健康
- **响应示例**:
  ```json
  {
    "status": "healthy"
  }
  ```
- **错误响应** (503 Service Unavailable 服务不可用):
  ```json
  {
    "detail": "Model or tokenizer not loaded"
  }
  ```

### 文本生成

- **URL**: `/generate`
- **方法**: POST
- **描述**: 根据提供的提示和参数生成文本
- **请求体**:
  ```json
  {
    "prompt": "用 Python 编写一个计算斐波那契数列的函数",
    "max_length": 512,
    "temperature": 0.7,
    "top_p": 0.9,
    "top_k": 50,
    "num_return_sequences": 1
  }
  ```
- **参数**:
  - `prompt` (字符串, 必需): 用于生成的输入文本提示
  - `max_length` (整数, 可选): 生成文本的最大长度，默认值: 512
  - `temperature` (浮点数, 可选): 控制生成过程中的随机性，默认值: 0.7
  - `top_p` (浮点数, 可选): 核心采样参数，默认值: 0.9
  - `top_k` (整数, 可选): Top-k 采样参数，默认值: 50
  - `num_return_sequences` (整数, 可选): 要生成的序列数量，默认值: 1

- **响应示例**:
  ```json
  {
    "generated_texts": [
      "要用 Python 计算斐波那契数列，你可以使用递归方法或迭代方法。这是一个迭代函数的示例：\n\n```python\ndef fibonacci_iterative(n):\n    if n <= 0:\n        return []\n    elif n == 1:\n        return [0]\n    else:\n        list_fib = [0, 1]\n        while len(list_fib) < n:\n            next_fib = list_fib[-1] + list_fib[-2]\n            list_fib.append(next_fib)\n        return list_fib[:n] # 如果 n 较小，确保准确返回 n 个数字\n\n# 示例用法：\nnum_terms = 10\nprint(f\"斐波那契数列前 {num_terms} 项: {fibonacci_iterative(num_terms)}\")\n```\n\n此函数将返回一个包含前 `n` 个斐波那契数字的列表，从 0 开始。"
    ],
    "parameters": {
      "prompt": "用 Python 编写一个计算斐波那契数列的函数",
      "max_length": 512,
      "temperature": 0.7,
      "top_p": 0.9,
      "top_k": 50,
      "num_return_sequences": 1
    }
  }
  ```

- **错误响应** (500 Internal Server Error 服务器内部错误):
  ```json
  {
    "detail": "Generation failed: [error message]"
  }
  ```

## Llama4 模型改进

Llama 4 Scout-17B 相比之前的 Llama 版本提供了多项改进：

1. **增强性能**：更好的推理能力和更准确的响应
2. **改进的上下文处理**：更有效地利用上下文窗口
3. **减少幻觉**：更加事实准确和可靠的输出
4. **更好的指令遵循**：更精确地遵循用户指令
5. **优化资源使用**：更高效的内存利用和推理速度

## API 使用示例

代码仓库中包含一个 Python 脚本 (`src/api_examples.py`)，演示了如何与 API 交互。以下是如何从 `/home/ubuntu/ai-agent` 目录使用它：

```bash
# 首先激活虚拟环境
source venv/bin/activate

# 使用默认参数进行基本使用
python src/api_examples.py

# 自定义提示
python src/api_examples.py --prompt "写一个递归函数来计算 Python 中的阶乘"

# 自定义生成参数
python src/api_examples.py --max-length 256 --temperature 0.8 --top-p 0.95 --num-sequences 2

# 自定义 API URL (如果不在本地运行)
python src/api_examples.py --url "http://your-server-address:2025"
```

## API 客户端实现

这是一个简单的 Python 示例，展示了如何从您自己的代码中调用 API：

```python
import requests

def generate_text_from_llama4(prompt, max_length=512, temperature=0.7):
    """使用 Llama 4 Scout-17B API 生成文本"""
    api_url = "http://localhost:2025/generate"
    
    payload = {
        "prompt": prompt,
        "max_length": max_length,
        "temperature": temperature
    }
    
    response = requests.post(api_url, json=payload)
    
    if response.status_code == 200:
        return response.json()["generated_texts"][0]
    else:
        raise Exception(f"API 请求失败: {response.text}")

# 示例用法
result = generate_text_from_llama4("编写一个 Python 函数以升序对数字列表进行排序。")
print(result)
```

## 高级使用模式

### 对话完成

Llama 4 Scout-17B 支持结构化格式的对话完成。以下是使用方法：

```python
import requests
import json

def chat_with_llama4(messages, temperature=0.7):
    """使用 Llama 4 Scout-17B API 生成对话完成"""
    api_url = "http://localhost:2025/generate"
    
    # 将消息格式化为对话提示
    formatted_prompt = ""
    for msg in messages:
        role = msg["role"]
        content = msg["content"]
        if role == "system":
            formatted_prompt += f"<|im_start|>system\n{content}<|im_end|>\n"
        elif role == "user":
            formatted_prompt += f"<|im_start|>user\n{content}<|im_end|>\n"
        elif role == "assistant":
            formatted_prompt += f"<|im_start|>assistant\n{content}<|im_end|>\n"
    
    # 添加最终的助手提示
    formatted_prompt += "<|im_start|>assistant\n"
    
    payload = {
        "prompt": formatted_prompt,
        "temperature": temperature,
        "max_length": 1024
    }
    
    response = requests.post(api_url, json=payload)
    
    if response.status_code == 200:
        return response.json()["generated_texts"][0]
    else:
        raise Exception(f"API 请求失败: {response.text}")

# 示例用法
messages = [
    {"role": "system", "content": "你是一个有帮助的AI助手。"},
    {"role": "user", "content": "Python的主要特点是什么？"}
]
response = chat_with_llama4(messages)
print(response)
```

## 错误处理

API 为各种场景提供了适当的错误处理：

1. 如果模型或分词器加载失败，API 将返回 503 Service Unavailable 错误。
2. 如果文本生成失败，API 将返回 500 Internal Server Error 并提供详细信息。
3. 如果请求格式错误，API 将返回 422 Unprocessable Entity 错误并提供验证详细信息。

## 性能注意事项

- 第一次请求可能需要更长时间，因为模型需要加载到内存中 (尤其是加载到 GPU)。
- 生成时间取决于请求的 `max_length` 参数和提示的复杂性。
- 与仅 CPU 推理相比，使用具有足够显存 (推荐 Llama 4 Scout-17B >=8GB) 的 CUDA 兼容 GPU 可显著提高性能。
- 对于生产用途，请考虑部署在具有足够 RAM (>=16GB) 和合适 GPU 的机器上。
- Llama 4 Scout-17B 在性能和资源需求之间提供了良好的平衡，使其适合在资源有限的环境中部署。

## 资源优化

要优化 Llama 4 Scout-17B 的资源使用：

1. **量化**：考虑使用 4 位或 8 位量化以减少内存占用
2. **批处理**：尽可能批量处理多个请求
3. **上下文长度**：将上下文长度限制在您的用例所需的范围内
4. **CPU 部署**：对于没有 GPU 的环境，使用 `device_map="auto"` 或 `cpu_only=True` 参数
5. **缓存**：为常见查询实现响应缓存

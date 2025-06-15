# Qwen3-4B Model API Documentation

This document provides comprehensive documentation for the Qwen3-4B model API.

## API Overview

The Qwen3-4B model is deployed as a RESTful API service using FastAPI. The API runs on port 2025 and provides endpoints for text generation using the Qwen3-4B language model, which offers improved performance and capabilities over previous Qwen versions.

## API Endpoints

### Root Endpoint

- **URL**: `/`
- **Method**: GET
- **Description**: Returns basic information about the API service
- **Response Example**:
  ```json
  {
    "name": "Qwen3-4B API",
    "version": "1.0.0",
    "status": "active",
    "model": "Qwen/Qwen3-4B-Instruct"
  }
  ```

### Health Check

- **URL**: `/health`
- **Method**: GET
- **Description**: Checks if the model and tokenizer are loaded and the service is healthy
- **Response Example**:
  ```json
  {
    "status": "healthy"
  }
  ```
- **Error Response** (503 Service Unavailable):
  ```json
  {
    "detail": "Model or tokenizer not loaded"
  }
  ```

### Text Generation

- **URL**: `/generate`
- **Method**: POST
- **Description**: Generates text based on the provided prompt and parameters
- **Request Body**:
  ```json
  {
    "prompt": "Write a function to calculate the Fibonacci sequence in Python",
    "max_length": 512,
    "temperature": 0.7,
    "top_p": 0.9,
    "top_k": 50,
    "num_return_sequences": 1
  }
  ```
- **Parameters**:
  - `prompt` (string, required): The input text prompt for generation
  - `max_length` (integer, optional): Maximum length of generated text, default: 512
  - `temperature` (float, optional): Controls randomness in generation, default: 0.7
  - `top_p` (float, optional): Nucleus sampling parameter, default: 0.9
  - `top_k` (integer, optional): Top-k sampling parameter, default: 50
  - `num_return_sequences` (integer, optional): Number of sequences to generate, default: 1

- **Response Example**:
  ```json
  {
    "generated_texts": [
      "To calculate the Fibonacci sequence in Python, you can use either a recursive approach or an iterative one. Here's an example of an iterative function:\n\n```python\ndef fibonacci_iterative(n):\n    if n <= 0:\n        return []\n    elif n == 1:\n        return [0]\n    else:\n        list_fib = [0, 1]\n        while len(list_fib) < n:\n            next_fib = list_fib[-1] + list_fib[-2]\n            list_fib.append(next_fib)\n        return list_fib[:n] # Ensure it returns exactly n numbers if n is small\n\n# Example usage:\nnum_terms = 10\nprint(f\"Fibonacci sequence up to {num_terms} terms: {fibonacci_iterative(num_terms)}\")\n```\n\nThis function will return a list containing the first `n` Fibonacci numbers, starting with 0."
    ],
    "parameters": {
      "prompt": "Write a function to calculate the Fibonacci sequence in Python",
      "max_length": 512,
      "temperature": 0.7,
      "top_p": 0.9,
      "top_k": 50,
      "num_return_sequences": 1
    }
  }
  ```

- **Error Response** (500 Internal Server Error):
  ```json
  {
    "detail": "Generation failed: [error message]"
  }
  ```

## Qwen3 Model Improvements

Qwen3-4B offers several improvements over previous Qwen versions:

1. **Enhanced Performance**: Better reasoning capabilities and more accurate responses
2. **Improved Context Handling**: More effective utilization of context window
3. **Reduced Hallucinations**: More factual and reliable outputs
4. **Better Instruction Following**: More precise adherence to user instructions
5. **Optimized Resource Usage**: More efficient memory utilization and inference speed

## API Usage Examples

The repository includes a Python script (`src/api_examples.py`) that demonstrates how to interact with the API. Here's how to use it from the `/home/ubuntu/ai-agent` directory:

```bash
# Activate virtual environment first
source venv/bin/activate

# Basic usage with default parameters
python src/api_examples.py

# Custom prompt
python src/api_examples.py --prompt "Write a recursive function to calculate factorial in Python"

# Custom generation parameters
python src/api_examples.py --max-length 256 --temperature 0.8 --top-p 0.95 --num-sequences 2

# Custom API URL (if not running locally)
python src/api_examples.py --url "http://your-server-address:2025"
```

## API Client Implementation

Here's a simple Python example of how to call the API from your own code:

```python
import requests

def generate_text_from_qwen3(prompt, max_length=512, temperature=0.7):
    """Generate text using the Qwen3-4B API"""
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
        raise Exception(f"API request failed: {response.text}")

# Example usage
result = generate_text_from_qwen3("Write a Python function to sort a list of numbers in ascending order.")
print(result)
```

## Advanced Usage Patterns

### Chat Completion

Qwen3-4B supports chat completion with a structured format. Here's how to use it:

```python
import requests
import json

def chat_with_qwen3(messages, temperature=0.7):
    """Generate chat completion using Qwen3-4B API"""
    api_url = "http://localhost:2025/generate"
    
    # Format messages into a chat prompt
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
    
    # Add final assistant prompt
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
        raise Exception(f"API request failed: {response.text}")

# Example usage
messages = [
    {"role": "system", "content": "You are a helpful AI assistant."},
    {"role": "user", "content": "What are the key features of Python?"}
]
response = chat_with_qwen3(messages)
print(response)
```

## Error Handling

The API includes proper error handling for various scenarios:

1. If the model or tokenizer fails to load, the API will return a 503 Service Unavailable error
2. If text generation fails, the API will return a 500 Internal Server Error with details
3. If the request is malformed, the API will return a 422 Unprocessable Entity error with validation details

## Performance Considerations

- The first request may take longer as the model needs to be loaded into memory (especially onto GPU).
- Generation time depends on the requested `max_length` parameter and the complexity of the prompt.
- Using a CUDA-compatible GPU with sufficient VRAM (>=8GB recommended for Qwen3-4B) significantly improves performance compared to CPU-only inference.
- For production use, consider deploying on a machine with adequate RAM (>=16GB) and a suitable GPU.
- Qwen3-4B offers a good balance between performance and resource requirements, making it suitable for deployment in environments with limited resources.

## Resource Optimization

To optimize resource usage with Qwen3-4B:

1. **Quantization**: Consider using 4-bit or 8-bit quantization to reduce memory footprint
2. **Batching**: Process multiple requests in batches when possible
3. **Context Length**: Limit context length to what's necessary for your use case
4. **CPU Deployment**: For environments without GPUs, use `device_map="auto"` or `cpu_only=True` parameters
5. **Caching**: Implement response caching for common queries

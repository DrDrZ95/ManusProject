# Qwen2-7B-Instruct Model API Documentation

This document provides comprehensive documentation for the Qwen2-7B-Instruct model API.

## API Overview

The Qwen2-7B-Instruct model is deployed as a RESTful API service using FastAPI. The API runs on port 2025 and provides endpoints for text generation using the Qwen2-7B-Instruct language model.

## API Endpoints

### Root Endpoint

- **URL**: `/`
- **Method**: GET
- **Description**: Returns basic information about the API service
- **Response Example**:
  ```json
  {
    "name": "Qwen2-7B-Instruct API",
    "version": "1.0.0",
    "status": "active",
    "model": "Qwen/Qwen2-7B-Instruct"
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

def generate_text_from_qwen(prompt, max_length=512, temperature=0.7):
    """Generate text using the Qwen2-7B-Instruct API"""
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
result = generate_text_from_qwen("Write a Python function to sort a list of numbers in ascending order.")
print(result)
```

## Error Handling

The API includes proper error handling for various scenarios:

1. If the model or tokenizer fails to load, the API will return a 503 Service Unavailable error
2. If text generation fails, the API will return a 500 Internal Server Error with details
3. If the request is malformed, the API will return a 422 Unprocessable Entity error with validation details

## Performance Considerations

- The first request may take longer as the model needs to be loaded into memory (especially onto GPU).
- Generation time depends on the requested `max_length` parameter and the complexity of the prompt.
- Using a CUDA-compatible GPU with sufficient VRAM (>=10GB recommended for Qwen2-7B) significantly improves performance compared to CPU-only inference, which will be very slow.
- For production use, consider deploying on a machine with adequate RAM (>=16GB) and a suitable GPU.

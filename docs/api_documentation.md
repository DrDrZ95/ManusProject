# DeepSeek R1 1.5B Model API Documentation

This document provides comprehensive documentation for the DeepSeek R1 1.5B model API.

## API Overview

The DeepSeek R1 1.5B model is deployed as a RESTful API service using FastAPI. The API runs on port 2025 and provides endpoints for text generation using the DeepSeek R1 1.5B language model.

## API Endpoints

### Root Endpoint

- **URL**: `/`
- **Method**: GET
- **Description**: Returns basic information about the API service
- **Response Example**:
  ```json
  {
    "name": "DeepSeek R1 1.5B API",
    "version": "1.0.0",
    "status": "active",
    "model": "deepseek-ai/deepseek-coder-1.5b-base"
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
    "max_length": 256,
    "temperature": 0.7,
    "top_p": 0.9,
    "top_k": 50,
    "num_return_sequences": 1
  }
  ```
- **Parameters**:
  - `prompt` (string, required): The input text prompt for generation
  - `max_length` (integer, optional): Maximum length of generated text, default: 256
  - `temperature` (float, optional): Controls randomness in generation, default: 0.7
  - `top_p` (float, optional): Nucleus sampling parameter, default: 0.9
  - `top_k` (integer, optional): Top-k sampling parameter, default: 50
  - `num_return_sequences` (integer, optional): Number of sequences to generate, default: 1

- **Response Example**:
  ```json
  {
    "generated_texts": [
      "Write a function to calculate the Fibonacci sequence in Python\n\ndef fibonacci(n):\n    \"\"\"Calculate the Fibonacci sequence up to the nth term.\n    \n    Args:\n        n: An integer representing the position in the Fibonacci sequence.\n        \n    Returns:\n        The nth number in the Fibonacci sequence.\n    \"\"\"\n    if n <= 0:\n        return 0\n    elif n == 1:\n        return 1\n    else:\n        return fibonacci(n-1) + fibonacci(n-2)\n\n# Example usage\nfor i in range(10):\n    print(fibonacci(i))"
    ],
    "parameters": {
      "prompt": "Write a function to calculate the Fibonacci sequence in Python",
      "max_length": 256,
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

The repository includes a Python script (`src/api_examples.py`) that demonstrates how to interact with the API. Here's how to use it:

```bash
# Basic usage with default parameters
python src/api_examples.py

# Custom prompt
python src/api_examples.py --prompt "Write a recursive function to calculate factorial in Python"

# Custom generation parameters
python src/api_examples.py --max-length 512 --temperature 0.8 --top-p 0.95 --num-sequences 2

# Custom API URL (if not running locally)
python src/api_examples.py --url "http://your-server-address:2025"
```

## API Client Implementation

Here's a simple Python example of how to call the API from your own code:

```python
import requests

def generate_text(prompt, max_length=256, temperature=0.7):
    """Generate text using the DeepSeek R1 1.5B API"""
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
result = generate_text("Write a Python function to sort a list")
print(result)
```

## Error Handling

The API includes proper error handling for various scenarios:

1. If the model or tokenizer fails to load, the API will return a 503 Service Unavailable error
2. If text generation fails, the API will return a 500 Internal Server Error with details
3. If the request is malformed, the API will return a 422 Unprocessable Entity error with validation details

## Performance Considerations

- The first request may take longer as the model needs to be loaded into memory
- Generation time depends on the requested `max_length` parameter
- Using a GPU significantly improves performance compared to CPU-only inference
- For production use, consider deploying on a machine with at least 8GB RAM and a CUDA-compatible GPU

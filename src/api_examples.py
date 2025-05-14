#!/usr/bin/env python3
# api_examples.py
# Script to demonstrate API requests to the Qwen2-7B-Instruct model server

import requests
import json
import argparse
import sys

# Default API endpoint
DEFAULT_API_URL = "http://localhost:2025"
MODEL_DISPLAY_NAME = "Qwen2-7B-Instruct" # For display purposes

def check_server_health(api_url):
    """Check if the server is running and healthy"""
    try:
        response = requests.get(f"{api_url}/health", timeout=10) # Increased timeout for initial load
        if response.status_code == 200:
            print(f"✅ Server for {MODEL_DISPLAY_NAME} is healthy and ready to accept requests")
            return True
        else:
            print(f"❌ Server health check failed with status code: {response.status_code}")
            print(f"Response: {response.text}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ Failed to connect to server: {str(e)}")
        print("Make sure the server is running (./scripts/run_model_server.sh) and the API URL is correct.")
        return False

def get_server_info(api_url):
    """Get information about the API server"""
    try:
        response = requests.get(api_url, timeout=5)
        if response.status_code == 200:
            print("\n📊 Server Information:")
            print(json.dumps(response.json(), indent=2))
            return True
        else:
            print(f"❌ Failed to get server info. Status code: {response.status_code}")
            print(f"Response: {response.text}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"❌ Failed to connect to server: {str(e)}")
        return False

def generate_text(api_url, prompt, max_length=512, temperature=0.7, top_p=0.9, top_k=50, num_sequences=1):
    """Generate text using the model API"""
    try:
        # Prepare the request payload
        payload = {
            "prompt": prompt,
            "max_length": max_length,
            "temperature": temperature,
            "top_p": top_p,
            "top_k": top_k,
            "num_return_sequences": num_sequences
        }
        
        # Print request details
        print("\n🔍 Request Details:")
        print(f"Endpoint: {api_url}/generate")
        print(f"Payload: {json.dumps(payload, indent=2)}")
        
        # Send the request
        print("\n⏳ Sending request to generate text...")
        response = requests.post(f"{api_url}/generate", json=payload, timeout=120) # Increased timeout for generation
        
        # Process the response
        if response.status_code == 200:
            result = response.json()
            print("\n✅ Text generation successful!")
            print("\n📝 Generated Text:")
            for i, text in enumerate(result["generated_texts"]):
                print(f"\n--- Sequence {i+1} ---")
                # Qwen models might include the prompt in the output, this basic example prints the full output.
                # For chat, the output structure might be different or require post-processing.
                print(text)
            return True
        else:
            print(f"\n❌ Text generation failed. Status code: {response.status_code}")
            print(f"Response: {response.text}")
            return False
    except requests.exceptions.RequestException as e:
        print(f"\n❌ Request failed: {str(e)}")
        return False

def main():
    """Main function to run API examples"""
    parser = argparse.ArgumentParser(description=f"{MODEL_DISPLAY_NAME} API Example Client")
    parser.add_argument("--url", default=DEFAULT_API_URL, help=f"API server URL (default: {DEFAULT_API_URL})")
    parser.add_argument("--prompt", default="Hello Qwen, can you write a short story about a robot learning to paint?", 
                        help="Text prompt for generation")
    parser.add_argument("--max-length", type=int, default=512, help="Maximum length of generated text")
    parser.add_argument("--temperature", type=float, default=0.7, help="Temperature for sampling")
    parser.add_argument("--top-p", type=float, default=0.9, help="Top-p sampling parameter")
    parser.add_argument("--top-k", type=int, default=50, help="Top-k sampling parameter")
    parser.add_argument("--num-sequences", type=int, default=1, help="Number of sequences to generate")
    
    args = parser.parse_args()
    
    print(f"🚀 {MODEL_DISPLAY_NAME} API Example Client")
    print(f"API URL: {args.url}")
    
    # Check server health
    if not check_server_health(args.url):
        print("\nPlease ensure the model server is running. You can start it with: ./scripts/run_model_server.sh")
        sys.exit(1)
    
    # Get server info
    get_server_info(args.url)
    
    # Generate text
    generate_text(
        args.url, 
        args.prompt,
        args.max_length,
        args.temperature,
        args.top_p,
        args.top_k,
        args.num_sequences
    )

if __name__ == "__main__":
    main()

#!/usr/bin/env python3
# model_server.py
# Script to deploy deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B model with FastAPI

import os
import torch
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from transformers import AutoModelForCausalLM, AutoTokenizer
import uvicorn
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler("model_server.log")
    ]
)
logger = logging.getLogger(__name__)

# Initialize FastAPI app
app = FastAPI(
    title="DeepSeek API",
    description="API for DeepSeek language model",
    version="1.0.0"
)

# Define request model
class GenerationRequest(BaseModel):
    prompt: str
    max_length: int = 512  # Increased default max_length for potentially longer DeepSeek responses
    temperature: float = 0.7
    top_p: float = 0.9
    top_k: int = 50
    num_return_sequences: int = 1

# Global variables for model and tokenizer
model = None
tokenizer = None
MODEL_NAME = "deepseek-ai/DeepSeek-R1-Distill-Qwen3-4B" # HF model ID for DeepSeek

@app.on_event("startup")
async def startup_event():
    """Load model and tokenizer on startup"""
    global model, tokenizer
    
    logger.info(f"Loading {MODEL_NAME} model and tokenizer...")
    
    try:
        # Load tokenizer directly from Hugging Face
        logger.info(f"Loading tokenizer from Hugging Face: {MODEL_NAME}")
        tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME, trust_remote_code=True)
        
        # Load model directly from Hugging Face
        logger.info(f"Loading model from Hugging Face: {MODEL_NAME}")
        model = AutoModelForCausalLM.from_pretrained(
            MODEL_NAME,
            torch_dtype=torch.bfloat16 if torch.cuda.is_available() and torch.cuda.is_bf16_supported() else torch.float16 if torch.cuda.is_available() else torch.float32,
            low_cpu_mem_usage=True,
            device_map="auto",
            trust_remote_code=True
        )
        
        logger.info(f"Model and tokenizer loaded successfully from Hugging Face")
    except Exception as e:
        logger.error(f"Error loading model: {str(e)}")
        raise RuntimeError(f"Failed to load model: {str(e)}")

@app.get("/")
async def root():
    """Root endpoint with API information"""
    return {
        "name": "DeepSeek API",
        "version": "1.0.0",
        "status": "active",
        "model": MODEL_NAME
    }

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    if model is None or tokenizer is None:
        raise HTTPException(status_code=503, detail="Model or tokenizer not loaded")
    return {"status": "healthy"}

@app.post("/generate")
async def generate_text(request: GenerationRequest):
    """Generate text based on the provided prompt"""
    global model, tokenizer
    
    if model is None or tokenizer is None:
        raise HTTPException(status_code=503, detail="Model or tokenizer not loaded")
    
    try:
        # DeepSeek models often use a specific chat template format for prompts.
        # For simplicity, this example directly uses the prompt string.
        # For chat applications, apply the tokenizer's chat template.
        # messages = [{"role": "user", "content": request.prompt}]
        # text = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
        # inputs = tokenizer(text, return_tensors="pt")
        
        inputs = tokenizer(request.prompt, return_tensors="pt")
        
        # Move inputs to the same device as model
        inputs = {k: v.to(model.device) for k, v in inputs.items()}
        
        # Generate text
        with torch.no_grad():
            outputs = model.generate(
                **inputs,
                max_length=request.max_length,
                temperature=request.temperature,
                top_p=request.top_p,
                top_k=request.top_k,
                num_return_sequences=request.num_return_sequences,
                pad_token_id=tokenizer.eos_token_id
            )
        
        # Decode generated text
        # For DeepSeek, sometimes the prompt is included in the output, so we might need to slice it off.
        # This depends on the specific model and generation parameters.
        # For now, decode full output.
        generated_texts = [tokenizer.decode(output, skip_special_tokens=True) for output in outputs]
        
        return {
            "generated_texts": generated_texts,
            "parameters": {
                "prompt": request.prompt,
                "max_length": request.max_length,
                "temperature": request.temperature,
                "top_p": request.top_p,
                "top_k": request.top_k,
                "num_return_sequences": request.num_return_sequences
            }
        }
    
    except Exception as e:
        logger.error(f"Error during text generation: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Generation failed: {str(e)}")

if __name__ == "__main__":
    # Run the API server
    # The port 2025 is kept as per original requirement
    uvicorn.run("model_server:app", host="0.0.0.0", port=2025, reload=False)

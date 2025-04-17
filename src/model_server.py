#!/usr/bin/env python3
# model_server.py
# Script to deploy DeepSeek R1 1.5B model with FastAPI

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
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler("model_server.log")
    ]
)
logger = logging.getLogger(__name__)

# Initialize FastAPI app
app = FastAPI(
    title="DeepSeek R1 1.5B API",
    description="API for DeepSeek R1 1.5B language model",
    version="1.0.0"
)

# Define request model
class GenerationRequest(BaseModel):
    prompt: str
    max_length: int = 256
    temperature: float = 0.7
    top_p: float = 0.9
    top_k: int = 50
    num_return_sequences: int = 1

# Global variables for model and tokenizer
model = None
tokenizer = None

@app.on_event("startup")
async def startup_event():
    """Load model and tokenizer on startup"""
    global model, tokenizer
    
    logger.info("Loading DeepSeek R1 1.5B model and tokenizer...")
    
    try:
        model_path = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 
                                 "models/deepseek-r1-1.5b")
        
        # Load tokenizer
        tokenizer = AutoTokenizer.from_pretrained(model_path)
        
        # Load model
        model = AutoModelForCausalLM.from_pretrained(
            model_path,
            torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
            low_cpu_mem_usage=True,
            device_map="auto"
        )
        
        logger.info(f"Model and tokenizer loaded successfully from {model_path}")
    except Exception as e:
        logger.error(f"Error loading model: {str(e)}")
        raise RuntimeError(f"Failed to load model: {str(e)}")

@app.get("/")
async def root():
    """Root endpoint with API information"""
    return {
        "name": "DeepSeek R1 1.5B API",
        "version": "1.0.0",
        "status": "active",
        "model": "deepseek-ai/deepseek-coder-1.5b-base"
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
        # Tokenize input
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
    uvicorn.run("model_server:app", host="0.0.0.0", port=2025, reload=False)

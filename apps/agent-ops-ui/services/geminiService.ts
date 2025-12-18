
import { GoogleGenAI } from "@google/genai";

/**
 * analyzeCommand - Uses Gemini to process terminal commands
 * Follows @google/genai coding guidelines:
 * - Initializes new instance per call
 * - Uses gemini-3-flash-preview for basic text tasks
 * - Directly accesses .text property
 */
export const analyzeCommand = async (command: string, context: string): Promise<string> => {
  // Ensure the API key is available
  if (!process.env.API_KEY) {
    return "ManusProject AI: API Key not configured. Using offline mode.";
  }

  // Always initialize right before making an API call
  const ai = new GoogleGenAI({ apiKey: process.env.API_KEY });

  try {
    // Basic Text Tasks: 'gemini-3-flash-preview'
    const model = 'gemini-3-flash-preview';
    const response = await ai.models.generateContent({
      model,
      contents: `
        You are an intelligent MLOps terminal assistant for ManusProject.
        The user typed: "${command}".
        Current Context: ${context}.
        
        Provide a brief, technical, CLI-style output simulation or a helpful explanation if the command is ambiguous.
        Do not use markdown formatting (bold, italics) excessively, keep it raw text like a terminal.
        If it's a known kubernetes/docker command, mock the output realistically.
      `,
    });
    
    // The GenerateContentResponse features a text property (not a method)
    return response.text || "No output generated.";
  } catch (error) {
    console.error("Gemini API Error:", error);
    return "Error communicating with ManusProject Intelligence Core.";
  }
};

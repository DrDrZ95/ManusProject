import { GoogleGenAI } from "@google/genai";

const apiKey = process.env.API_KEY || ''; // In a real app, ensure this is set safely
let aiClient: GoogleGenAI | null = null;

if (apiKey) {
  aiClient = new GoogleGenAI({ apiKey });
}

export const analyzeCommand = async (command: string, context: string): Promise<string> => {
  if (!aiClient) {
    return "ManusProject AI: API Key not configured. Using offline mode.";
  }

  try {
    const model = 'gemini-2.5-flash';
    const response = await aiClient.models.generateContent({
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
    return response.text || "No output generated.";
  } catch (error) {
    console.error("Gemini API Error:", error);
    return "Error communicating with ManusProject Intelligence Core.";
  }
};
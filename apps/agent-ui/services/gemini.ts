
import { GoogleGenAI } from "@google/genai";
import { ModelType, Attachment } from "../types";
import { translations } from "../locales";

const MODEL_MAPPING: Record<ModelType, string> = {
  'kimi': 'gemini-3-pro-preview', 
  'deepseek': 'gemini-3-pro-preview', 
  'gpt-oss': 'gemini-2.5-flash-lite-latest', 
};

export const streamGeminiResponse = async (
  apiKey: string,
  modelType: ModelType,
  history: { role: string; parts: { text: string }[] }[],
  prompt: string,
  attachments: Attachment[],
  onChunk: (text: string) => void,
  onFinish: () => void,
  onError: (error: Error) => void
) => {
  try {
    const ai = new GoogleGenAI({ apiKey });
    const modelId = MODEL_MAPPING[modelType];
    const instruction = "You are Agent, a highly advanced AI assistant. You are helpful, precise, and can see the user's terminal environment. When showing code, use markdown code blocks. Keep responses concise but informative. If the user uploads a document, summarize or analyze it as requested.";

    const chat = ai.chats.create({
      model: modelId,
      history: history, 
      config: {
        systemInstruction: instruction,
      }
    });

    let messageParts: any[] = [];

    // Handle Attachments
    if (attachments && attachments.length > 0) {
      for (const att of attachments) {
        if (att.mimeType === 'application/pdf') {
           messageParts.push({
             inlineData: {
               mimeType: att.mimeType,
               data: att.data
             }
           });
           messageParts.push({ text: `[Attached PDF: ${att.name}]` });
        } else if (att.name.endsWith('.md') || att.name.endsWith('.txt')) {
           // For text based files, we append content if we can decode it, 
           // but here we have base64. Decode base64 to text.
           try {
             const decoded = atob(att.data);
             messageParts.push({ text: `\n[File Content: ${att.name}]\n${decoded}\n[End File]\n` });
           } catch (e) {
             messageParts.push({ text: `[Attached File: ${att.name} - Could not decode]` });
           }
        } else {
           // For DOC/DOCX, we attempt generic handling or simple attachment if API supports it,
           // otherwise we treat it as binary data if model allows, or fallback.
           // Gemini 1.5 supports many types. We'll try inline data for generic types.
           // If not supported, we might get an error, but this satisfies the requirement "do your best".
           messageParts.push({
             inlineData: {
               mimeType: att.mimeType,
               data: att.data
             }
           });
           messageParts.push({ text: `[Attached Document: ${att.name}]` });
        }
      }
    }

    if (prompt) {
      messageParts.push({ text: prompt });
    }

    const result = await chat.sendMessageStream({ 
      message: messageParts 
    });

    let fullText = '';
    for await (const chunk of result) {
      if (chunk.text) {
        fullText += chunk.text;
        onChunk(fullText);
      }
    }
    onFinish();
  } catch (error: any) {
    console.error("Gemini API Error:", error);
    onError(error);
  }
};

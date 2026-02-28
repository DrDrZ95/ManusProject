using Agent.Core.Interfaces;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Agent.Application.Services.Tokens;

public class FallbackTokenCounter : ITokenCounter
{
    public int CountTokens(string text, string modelId)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        double tokenCount = 0;
        foreach (char c in text)
        {
            // CJK characters: 1.5 tokens per char
            // CJK characters range: \u4e00-\u9fa5 (Common Chinese), \u3040-\u309f (Japanese Hiragana), \u30a0-\u30ff (Katakana), \uac00-\ud7af (Korean Hangul)
            if (IsCjkCharacter(c))
            {
                tokenCount += 1.5;
            }
            // ASCII/English: 0.25 tokens per char (standard 4 chars = 1 token)
            else
            {
                tokenCount += 0.25;
            }
        }

        return (int)Math.Ceiling(tokenCount);
    }

    private bool IsCjkCharacter(char c)
    {
        return (c >= '\u4e00' && c <= '\u9fa5') || 
               (c >= '\u3040' && c <= '\u309f') || 
               (c >= '\u30a0' && c <= '\u30ff') || 
               (c >= '\uac00' && c <= '\ud7af');
    }

    public int CountChatHistoryTokens(IEnumerable<ChatMessage> messages, string modelId)
    {
        int totalTokens = 0;
        foreach (var message in messages)
        {
            totalTokens += CountTokens(message.Content, modelId);
            totalTokens += 4; // basic overhead per message
        }
        return totalTokens;
    }

    public (int promptTokens, int completionTokens) ParseUsageFromResponse(object rawResponse)
    {
        return (0, 0);
    }
}

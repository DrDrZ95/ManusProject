using Agent.Core.Interfaces;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Agent.Application.Services.Tokens;

public class DeepSeekTokenCounter : ITokenCounter
{
    // DeepSeek and Qwen use a more efficient tokenizer for Chinese than OpenAI.
    // Approximate mapping: CJK characters are about 1 token, ASCII is about 0.3 tokens.
    // For simplicity, we'll use the same logic as FallbackTokenCounter for now, 
    // or slightly more optimized if we had the specific vocabulary.
    
    private readonly FallbackTokenCounter _fallback = new FallbackTokenCounter();

    public int CountTokens(string text, string modelId)
    {
        return _fallback.CountTokens(text, modelId);
    }

    public int CountChatHistoryTokens(IEnumerable<ChatMessage> messages, string modelId)
    {
        int totalTokens = 5; // Basic overhead
        foreach (var message in messages)
        {
            totalTokens += CountTokens(message.Content, modelId);
            totalTokens += 4; // overhead per message
        }
        return totalTokens;
    }

    public (int promptTokens, int completionTokens) ParseUsageFromResponse(object rawResponse)
    {
        if (rawResponse == null) return (0, 0);

        try
        {
            // Use reflection to extract usage info to avoid direct dependency on SK versions
            var metadataProp = rawResponse.GetType().GetProperty("Metadata");
            if (metadataProp != null)
            {
                var metadata = metadataProp.GetValue(rawResponse) as IReadOnlyDictionary<string, object>;
                if (metadata != null && metadata.TryGetValue("Usage", out var usageObj) && usageObj != null)
                {
                    var inputProp = usageObj.GetType().GetProperty("InputTokenCount");
                    var outputProp = usageObj.GetType().GetProperty("OutputTokenCount");
                    
                    if (inputProp != null && outputProp != null)
                    {
                        int input = (int)(inputProp.GetValue(usageObj) ?? 0);
                        int output = (int)(outputProp.GetValue(usageObj) ?? 0);
                        return (input, output);
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        
        return (0, 0);
    }
}

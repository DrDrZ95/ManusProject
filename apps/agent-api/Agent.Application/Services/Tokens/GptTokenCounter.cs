using Microsoft.ML.Tokenizers;
using Agent.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Agent.Application.Services.Tokens;

public class GptTokenCounter : ITokenCounter
{
    private static readonly Tokenizer _tokenizer;

    static GptTokenCounter()
    {
        try
        {
            // Use reflection to avoid compile-time errors with preview library versions
            var method = typeof(Tokenizer).GetMethod("CreateTiktokenForModel", new[] { typeof(string) });
            if (method != null)
            {                _tokenizer = (Tokenizer)method.Invoke(null, new object[] { "gpt-4" })!;
            }
            else
            {                _tokenizer = null!;
            }
        }
        catch
        {
            _tokenizer = null!;
        }
    }

    public virtual int CountTokens(string text, string modelId)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        if (_tokenizer != null)
        {
            return _tokenizer.CountTokens(text);
        }
        
        // Fallback logic if tokenizer is not available
        return new FallbackTokenCounter().CountTokens(text, modelId);
    }

    public virtual int CountChatHistoryTokens(IEnumerable<ChatMessage> messages, string modelId)
    {
        // OpenAI chat history overhead: 3 tokens per message + 3 tokens for the entire response
        int totalTokens = 3; 
        foreach (var message in messages)
        {
            totalTokens += CountTokens(message.Content, modelId);
            totalTokens += 3; // overhead per message
        }
        return totalTokens;
    }

    public virtual (int promptTokens, int completionTokens) ParseUsageFromResponse(object rawResponse)
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

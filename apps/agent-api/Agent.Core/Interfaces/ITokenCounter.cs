using System.Collections.Generic;

namespace Agent.Core.Interfaces;

/// <summary>
/// Token counter interface for estimating and calculating tokens for different LLM models.
/// Token 计数器接口，用于估算和计算不同 LLM 模型的 Token 数量。
/// </summary>
public interface ITokenCounter
{
    /// <summary>
    /// Counts tokens for a single text string.
    /// 计算单个文本字符串的 Token 数量。
    /// </summary>
    int CountTokens(string text, string modelId);

    /// <summary>
    /// Counts tokens for a chat history (messages).
    /// 计算聊天历史（消息列表）的 Token 数量。
    /// </summary>
    int CountChatHistoryTokens(IEnumerable<ChatMessage> messages, string modelId);

    /// <summary>
    /// Parses usage information from a raw LLM response.
    /// 从原始 LLM 响应中解析使用量信息。
    /// </summary>
    (int promptTokens, int completionTokens) ParseUsageFromResponse(object rawResponse);
}

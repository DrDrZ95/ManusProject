using System;

namespace Agent.Core.Services.RAG;

/// <summary>
/// RAG 服务配置选项 (RAG Service Configuration Options)
/// </summary>
public class RagOptions
{
    /// <summary>
    /// 配置节名称 (Configuration section name)
    /// </summary>
    public const string Rag = "Rag";

    /// <summary>
    /// 热门 RAG 查询结果缓存 TTL (Popular RAG Query Results Cache TTL) - 1 小时
    /// </summary>
    public TimeSpan PopularRagQueryTtl { get; set; } = TimeSpan.FromHours(1);
}

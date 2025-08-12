using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Core.Services.VectorDatabase;

/// <summary>
/// 兼容用的集合模型占位。仅用于让现有代码编译通过。
/// 注意：方法均会抛 NotImplementedException。
/// </summary>
public class Collection
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// 占位：向集合添加文档/向量/元数据。
    /// 你的现有代码调用签名：AddAsync(ids, null, metadatas, documents)
    /// </summary>
    public Task AddAsync(
        IEnumerable<string>? ids = null,
        IEnumerable<float[]?>? embeddings = null,
        IEnumerable<Dictionary<string, object>>? metadatas = null,
        IEnumerable<string>? documents = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Compat placeholder: Collection.AddAsync is not implemented.");

    /// <summary>
    /// 占位：更新集合文档/向量/元数据。
    /// 你的现有代码调用签名：UpdateAsync(ids, null, metadatas, documents)
    /// </summary>
    public Task UpdateAsync(
        IEnumerable<string> ids,
        IEnumerable<float[]?>? embeddings = null,
        IEnumerable<Dictionary<string, object>>? metadatas = null,
        IEnumerable<string>? documents = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Compat placeholder: Collection.UpdateAsync is not implemented.");

    /// <summary>
    /// 占位：按 ids 或 where 获取文档。
    /// 你的现有代码调用签名：GetAsync(ids, where)
    /// </summary>
    public Task<GetResponse> GetAsync(
        IEnumerable<string>? ids = null,
        Dictionary<string, object>? where = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Compat placeholder: Collection.GetAsync is not implemented.");

    /// <summary>
    /// 占位：按 ids 或 where 删除文档。
    /// 你的现有代码调用签名：DeleteAsync(ids, where)
    /// </summary>
    public Task DeleteAsync(
        IEnumerable<string>? ids = null,
        Dictionary<string, object>? where = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Compat placeholder: Collection.DeleteAsync is not implemented.");

    /// <summary>
    /// 占位：文本查询（你的代码：QueryAsync(queryTexts, nResults, where)）。
    /// 实际 SDK 通常按向量查询；此处仅作占位以过编译。
    /// </summary>
    public Task<QueryResponse> QueryAsync(
        IEnumerable<string> queryTexts,
        int nResults = 10,
        Dictionary<string, object>? where = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Compat placeholder: Collection.QueryAsync is not implemented.");
}

/// <summary>
/// 占位：查询返回。为了兼容常见用法，提供分组命中结果。
/// </summary>
public sealed class QueryResponse
{
    public IReadOnlyList<IReadOnlyList<QueryHit>> Groups { get; init; } = Array.Empty<IReadOnlyList<QueryHit>>();
}

/// <summary>
/// 占位：单个命中项（常见字段）。
/// </summary>
public sealed class QueryHit
{
    public string Id { get; init; } = string.Empty;
    public double? Distance { get; init; }
    public string? Document { get; init; }
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// 占位：Get 返回。提供最常用的字段集合。
/// </summary>
public sealed class GetResponse
{
    public IReadOnlyList<string> Ids { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string>? Documents { get; init; }
    public IReadOnlyList<IReadOnlyDictionary<string, object>>? Metadatas { get; init; }
    public IReadOnlyList<float[]?>? Embeddings { get; init; }
}

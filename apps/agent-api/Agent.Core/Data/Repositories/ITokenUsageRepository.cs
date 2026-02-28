using Agent.Core.Data.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Core.Data.Repositories;

/// <summary>
/// Repository interface for Token usage records
/// Token 使用记录的仓储接口
/// </summary>
public interface ITokenUsageRepository : IRepository<TokenUsageRecord, Guid>
{
    /// <summary>
    /// Adds a new token usage record and saves changes.
    /// 添加新的 Token 使用记录并保存更改。
    /// </summary>
    Task<TokenUsageRecord> AddRecordAsync(TokenUsageRecord record, CancellationToken cancellationToken = default);
}

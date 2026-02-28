using Agent.Core.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Core.Data.Repositories;

/// <summary>
/// Repository implementation for Token usage records using EF Core
/// 使用 EF Core 的 Token 使用记录仓储实现
/// </summary>
public class TokenUsageRepository : Repository<TokenUsageRecord, Guid>, ITokenUsageRepository
{
    public TokenUsageRepository(AgentDbContext context, ILogger<TokenUsageRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<TokenUsageRecord> AddRecordAsync(TokenUsageRecord record, CancellationToken cancellationToken = default)
    {
        return await AddAsync(record, cancellationToken);
    }
}

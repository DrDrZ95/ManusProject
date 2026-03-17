namespace Agent.Core.Cache;

/// <summary>
/// 后台服务：定期清理 AgentCacheService 中已过期的内存缓存注册键
/// </summary>
public class CacheRegistryCleanupService : BackgroundService
{
    private readonly IAgentCacheService _cacheService;
    private readonly ILogger<CacheRegistryCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

    public CacheRegistryCleanupService(
        IAgentCacheService cacheService,
        ILogger<CacheRegistryCleanupService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cache Registry Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_cacheService is AgentCacheService concreteCacheService)
                {
                    _logger.LogDebug("Cleaning up expired cache keys from registry...");
                    concreteCacheService.CleanupExpiredKeys();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired cache keys.");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Cache Registry Cleanup Service is stopping.");
    }
}

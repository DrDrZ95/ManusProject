using Agent.Core.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Agent.Core.Tests.Cache;

public class AgentCacheServiceAvalancheTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<IConnectionMultiplexer> _redisConnectionMock;
    private readonly Mock<ILogger<AgentCacheService>> _loggerMock;
    private readonly IOptions<CacheOptions> _options;
    private readonly AgentCacheService _cacheService;

    public AgentCacheServiceAvalancheTests()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _distributedCacheMock = new Mock<IDistributedCache>();
        _redisConnectionMock = new Mock<IConnectionMultiplexer>();
        _loggerMock = new Mock<ILogger<AgentCacheService>>();
        _options = Options.Create(new CacheOptions());

        _cacheService = new AgentCacheService(
            _memoryCacheMock.Object,
            _distributedCacheMock.Object,
            _redisConnectionMock.Object,
            _loggerMock.Object,
            _options);
    }

    [Fact]
    public async Task GetOrCreateAsync_UsesDistributedLock_ToPreventMultipleFactoryCalls()
    {
        // Arrange
        var key = "cold-key";
        var expectedValue = "factory-value";
        object? l1Value = null;

        _memoryCacheMock.Setup(m => m.TryGetValue(key, out l1Value)).Returns(false);
        _distributedCacheMock.Setup(d => d.GetAsync(key, default)).ReturnsAsync((byte[]?)null);
        
        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Mock Redis Database and Lock
        var dbMock = new Mock<IDatabase>();
        _redisConnectionMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);
        
        // First call acquires lock
        dbMock.Setup(d => d.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync(true);
        dbMock.Setup(d => d.LockReleaseAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync(true);

        var factoryMock = new Mock<Func<Task<string>>>();
        factoryMock.Setup(f => f()).ReturnsAsync(expectedValue);

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factoryMock.Object);

        // Assert
        Assert.Equal(expectedValue, result);
        dbMock.Verify(d => d.LockTakeAsync(It.Is<RedisKey>(k => k.ToString().Contains(key)), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()), Times.Once);
        factoryMock.Verify(f => f(), Times.Once);
        dbMock.Verify(d => d.LockReleaseAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_FallbackToFactory_WhenRedisIsDown()
    {
        // Arrange
        var key = "redis-down-key";
        var expectedValue = "fallback-value";
        object? l1Value = null;

        _memoryCacheMock.Setup(m => m.TryGetValue(key, out l1Value)).Returns(false);
        _distributedCacheMock.Setup(d => d.GetAsync(key, default)).ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis is down"));
        
        var dbMock = new Mock<IDatabase>();
        _redisConnectionMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);
        dbMock.Setup(d => d.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
              .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis is down"));

        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        var factoryMock = new Mock<Func<Task<string>>>();
        factoryMock.Setup(f => f()).ReturnsAsync(expectedValue);

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factoryMock.Object);

        // Assert
        Assert.Equal(expectedValue, result);
        factoryMock.Verify(f => f(), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Distributed cache failure")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}

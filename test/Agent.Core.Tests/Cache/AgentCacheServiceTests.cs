namespace Agent.Core.Tests.Cache;

public class AgentCacheServiceTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<IConnectionMultiplexer> _redisConnectionMock;
    private readonly Mock<ILogger<AgentCacheService>> _loggerMock;
    private readonly IOptions<CacheOptions> _options;
    private readonly AgentCacheService _cacheService;

    public AgentCacheServiceTests()
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
    public async Task GetOrCreateAsync_ReturnsCachedL1Value_WhenExists()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestData { Name = "L1" };
        object? cachedValue = expectedValue;

        _memoryCacheMock.Setup(m => m.TryGetValue(key, out cachedValue)).Returns(true);

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () => Task.FromResult(new TestData()));

        // Assert
        Assert.Equal(expectedValue, result);
        _memoryCacheMock.Verify(m => m.TryGetValue(key, out cachedValue), Times.Once);
        _distributedCacheMock.Verify(d => d.GetAsync(key, default), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsL2Value_AndRefillsL1_WhenL1Misses()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestData { Name = "L2" };
        var json = JsonSerializer.Serialize(expectedValue);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        object? l1Value = null;

        _memoryCacheMock.Setup(m => m.TryGetValue(key, out l1Value)).Returns(false);
        _distributedCacheMock.Setup(d => d.GetAsync(key, default)).ReturnsAsync(bytes);
        _memoryCacheMock.Setup(m => m.CreateEntry(key)).Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () => Task.FromResult(new TestData()));

        // Assert
        Assert.Equal(expectedValue.Name, result.Name);
        _distributedCacheMock.Verify(d => d.GetAsync(key, default), Times.Once);
        _memoryCacheMock.Verify(m => m.CreateEntry(key), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_ExecutesFactory_AndSetsCache_WhenL1AndL2Miss()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestData { Name = "Factory" };
        object? l1Value = null;

        _memoryCacheMock.Setup(m => m.TryGetValue(key, out l1Value)).Returns(false);
        _distributedCacheMock.Setup(d => d.GetAsync(key, default)).ReturnsAsync((byte[]?)null);
        _memoryCacheMock.Setup(m => m.CreateEntry(key)).Returns(Mock.Of<ICacheEntry>());

        // Mock Redis Lock
        var dbMock = new Mock<IDatabase>();
        _redisConnectionMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);
        dbMock.Setup(d => d.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync(true);
        dbMock.Setup(d => d.LockReleaseAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync(true);

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, () => Task.FromResult(expectedValue));

        // Assert
        Assert.Equal(expectedValue.Name, result.Name);
        _memoryCacheMock.Verify(m => m.CreateEntry(key), Times.Once); // Called once in SetAsync
        _distributedCacheMock.Verify(d => d.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_CallsRedisScanAndDelete()
    {
        // Arrange
        var prefix = "rag:";
        var endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379);
        var serverMock = new Mock<IServer>();
        var dbMock = new Mock<IDatabase>();

        _redisConnectionMock.Setup(r => r.GetEndPoints(It.IsAny<bool>())).Returns(new[] { endpoint });
        _redisConnectionMock.Setup(r => r.GetServer(endpoint, It.IsAny<object>())).Returns(serverMock.Object);
        _redisConnectionMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        var keys = new RedisKey[] { "AgentApi:rag:1", "AgentApi:rag:2" };
        serverMock.Setup(s => s.Keys(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
                  .Returns(keys);

        // Act
        await _cacheService.RemoveByPrefixAsync(prefix);

        // Assert
        dbMock.Verify(d => d.KeyDeleteAsync(keys, It.IsAny<CommandFlags>()), Times.Once);
    }

    public class TestData
    {
        public string Name { get; set; } = "";
    }
}

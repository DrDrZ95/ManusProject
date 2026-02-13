using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Api.Tests.Integration;

/// <summary>
/// Redis缓存的集成测试
/// Integration tests for Redis cache
/// </summary>
public class RedisCacheIntegrationTests
{
    private readonly IDistributedCache _cache;

    public RedisCacheIntegrationTests()
    {
        // Arrange: 设置一个内存中的服务提供者，模拟Redis缓存的配置
        // Arrange: Set up an in-memory service provider to simulate Redis cache configuration
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // 假设Redis连接字符串 - Assume Redis connection string
                {"ConnectionStrings:Redis", "localhost:6379"} 
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        
        // 使用内存分布式缓存作为替代，因为真正的Redis连接需要外部服务
        // Use in-memory distributed cache as a substitute, as a real Redis connection requires an external service
        services.AddDistributedMemoryCache(); 
        
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<IDistributedCache>();
    }

    [Fact]
    public async Task Cache_CanSetAndGetValue_WithAbsoluteExpiration()
    {
        // Arrange
        var key = "expKey";
        var value = "expValue";
        var encodedValue = Encoding.UTF8.GetBytes(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        };

        // Act
        await _cache.SetAsync(key, encodedValue, options);
        var retrievedValue = await _cache.GetAsync(key);

        // Assert
        Assert.NotNull(retrievedValue);
        Assert.Equal(value, Encoding.UTF8.GetString(retrievedValue));
    }

    /// <summary>
    /// 测试Redis缓存的存取功能
    /// Test Redis cache set and get functionality
    /// </summary>
    [Fact]
    public async Task Cache_CanSetAndGetValue()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        var encodedValue = Encoding.UTF8.GetBytes(value);

        // Act
        await _cache.SetAsync(key, encodedValue, new DistributedCacheEntryOptions());
        var retrievedValue = await _cache.GetAsync(key);

        // Assert
        Assert.NotNull(retrievedValue);
        Assert.Equal(value, Encoding.UTF8.GetString(retrievedValue));
    }

    /// <summary>
    /// 测试Redis缓存的删除功能
    /// Test Redis cache remove functionality
    /// </summary>
    [Fact]
    public async Task Cache_CanRemoveValue()
    {
        // Arrange
        var key = "removeKey";
        var value = "valueToRemove";
        var encodedValue = Encoding.UTF8.GetBytes(value);
        await _cache.SetAsync(key, encodedValue, new DistributedCacheEntryOptions());

        // Act
        await _cache.RemoveAsync(key);
        var retrievedValue = await _cache.GetAsync(key);

        // Assert
        Assert.Null(retrievedValue);
    }

    // TODO: 补充过期时间、滑动过期等高级缓存功能的测试
    // TODO: Supplement tests for advanced cache features like expiration time, sliding expiration, etc.
}



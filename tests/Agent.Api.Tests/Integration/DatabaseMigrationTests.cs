using Agent.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Api.Tests.Integration
{
    /// <summary>
    /// 数据库迁移的集成测试
    /// Integration tests for database migration
    /// </summary>
    public class DatabaseMigrationTests
    {
        /// <summary>
        /// 测试数据库是否能成功应用所有待处理的迁移
        /// Test if the database can successfully apply all pending migrations
        /// </summary>
        [Fact(Skip = "Requires a configured database connection string")]
        public async Task Database_CanApplyPendingMigrations()
        {
            // Arrange
            // 假设有一个配置了测试数据库连接字符串的IConfiguration - Assume an IConfiguration configured with a test database connection string
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test_db;Username=user;Password=password"}
                })
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AgentDbContext>()
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            using var context = new AgentDbContext(optionsBuilder.Options);

            // Act
            // 尝试应用所有待处理的迁移 - Attempt to apply all pending migrations
            // Note: This will create the database if it doesn't exist.
            await context.Database.MigrateAsync();

            // Assert
            // 验证数据库是否已创建且是最新的 - Verify that the database is created and up-to-date
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            Assert.Empty(pendingMigrations);
        }

        // TODO: 补充回滚测试和特定迁移的测试
        // TODO: Supplement tests for rollback and specific migrations
    }
}


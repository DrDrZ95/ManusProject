namespace Agent.Api.Tests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly RedisContainer _redisContainer;

    protected IntegrationTestBase()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("agent_db")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:latest")
            .Build();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Replace services with test implementations
                    services.AddSingleton<IHostedService, ChromaDbInitializer>(); // Placeholder for ChromaDB
                });

                builder.UseSetting("ConnectionStrings:DefaultConnection", _postgreSqlContainer.GetConnectionString());
                builder.UseSetting("Redis:ConnectionString", _redisContainer.GetConnectionString());
            });

        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await _redisContainer.StopAsync();
        Factory.Dispose();
    }
}

// Placeholder for ChromaDB Testcontainer
public class ChromaDbInitializer : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // In a real scenario, this would start a ChromaDB container.
        // For now, we just log a message.
        Console.WriteLine("ChromaDB container placeholder started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("ChromaDB container placeholder stopped.");
        return Task.CompletedTask;
    }
}


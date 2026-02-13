namespace Agent.Api.Tests.Integration.Fixtures;

public class ApiTestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Configure test services here if needed
        });
    }
}

[CollectionDefinition("IntegrationTestCollection")]
public class IntegrationTestCollection : ICollectionFixture<ApiTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}


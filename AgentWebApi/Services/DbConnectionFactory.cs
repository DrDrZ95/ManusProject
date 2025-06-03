using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AgentWebApi.Services;

/// <summary>
/// Implementation of the database connection factory.
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbConnectionFactory> _logger;

    public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Creates a database connection using the specified connection name.
    /// </summary>
    /// <param name="connectionName">The name of the connection in configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An open database connection.</returns>
    public async Task<DbConnection> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString(connectionName);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"Connection string '{connectionName}' not found in configuration.", nameof(connectionName));
        }

        _logger.LogInformation("Creating database connection for {ConnectionName}", connectionName);
        
        // Create the appropriate connection type based on the connection string
        // This example uses SQL Server, but could be extended for other database types
        DbConnection connection = new SqlConnection(connectionString);
        
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open database connection for {ConnectionName}", connectionName);
            connection.Dispose();
            throw;
        }
    }
}

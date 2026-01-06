using System.Data.Common;

namespace Agent.Application.Services;

/// <summary>
/// Factory interface for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a database connection using the specified connection name.
    /// </summary>
    /// <param name="connectionName">The name of the connection in configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An open database connection.</returns>
    Task<DbConnection> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
}

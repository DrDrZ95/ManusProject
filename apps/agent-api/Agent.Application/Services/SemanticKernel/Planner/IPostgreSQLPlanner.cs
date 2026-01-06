using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agent.Application.Services.SemanticKernel.Planner;

/// <summary>
/// PostgreSQL Planner Interface
/// </summary>
public interface IPostgreSQLPlanner
{
    /// <summary>
    /// Executes a SQL query against a PostgreSQL database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="query">The SQL query to execute.</param>
    /// <returns>A JSON string representing the query result.</returns>
    [KernelFunction, Description("Executes a SQL query against a PostgreSQL database.")]
    Task<string> ExecuteSqlQuery(
        [Description("The PostgreSQL connection string.")] string connectionString,
        [Description("The SQL query to execute.")] string query);

    /// <summary>
    /// Creates a new table in a PostgreSQL database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="tableName">The name of the table to create.</param>
    /// <param name="columns">A JSON string representing column definitions (e.g., {"id": "SERIAL PRIMARY KEY", "name": "VARCHAR(255)"}).</param>
    /// <returns>A message indicating the table creation status.</returns>
    [KernelFunction, Description("Creates a new table in a PostgreSQL database.")]
    Task<string> CreateTable(
        [Description("The PostgreSQL connection string.")] string connectionString,
        [Description("The name of the table to create.")] string tableName,
        [Description("A JSON string representing column definitions (e.g., {\"id\": \"SERIAL PRIMARY KEY\", \"name\": \"VARCHAR(255)\"}).")] string columns);

    /// <summary>
    /// Inserts data into a PostgreSQL table.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="data">A JSON string representing the data to insert (e.g., {"name": "John Doe", "age": 30}).</param>
    /// <returns>A message indicating the insertion status.</returns>
    [KernelFunction, Description("Inserts data into a PostgreSQL table.")]
    Task<string> InsertData(
        [Description("The PostgreSQL connection string.")] string connectionString,
        [Description("The name of the table.")] string tableName,
        [Description("A JSON string representing the data to insert (e.g., {\"name\": \"John Doe\", \"age\": 30}).")] string data);

    /// <summary>
    /// Backs up a PostgreSQL database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="backupPath">The path to save the backup file.</param>
    /// <returns>A message indicating the backup status.</returns>
    [KernelFunction, Description("Backs up a PostgreSQL database.")]
    Task<string> BackupDatabase(
        [Description("The PostgreSQL connection string.")] string connectionString,
        [Description("The path to save the backup file.")] string backupPath);
}



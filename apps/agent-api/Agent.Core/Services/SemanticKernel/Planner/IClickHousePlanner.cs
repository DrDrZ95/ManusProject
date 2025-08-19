using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agent.Core.Services.SemanticKernel.Planner;

/// <summary>
/// ClickHouse Planner Interface
/// </summary>
public interface IClickHousePlanner
{
    /// <summary>
    /// Executes a SQL query against a ClickHouse database.
    /// </summary>
    /// <param name="connectionString">The ClickHouse connection string.</param>
    /// <param name="query">The SQL query to execute.</param>
    /// <returns>A JSON string representing the query result.</returns>
    [KernelFunction, Description("Executes a SQL query against a ClickHouse database.")]
    Task<string> ExecuteSqlQuery(
        [Description("The ClickHouse connection string.")] string connectionString,
        [Description("The SQL query to execute.")] string query);

    /// <summary>
    /// Creates a new table in a ClickHouse database.
    /// </summary>
    /// <param name="connectionString">The ClickHouse connection string.</param>
    /// <param name="tableName">The name of the table to create.</param>
    /// <param name="columns">A JSON string representing column definitions (e.g., {"id": "UInt64", "name": "String"}).</param>
    /// <returns>A message indicating the table creation status.</returns>
    [KernelFunction, Description("Creates a new table in a ClickHouse database.")]
    Task<string> CreateTable(
        [Description("The ClickHouse connection string.")] string connectionString,
        [Description("The name of the table to create.")] string tableName,
        [Description("A JSON string representing column definitions (e.g., {\"id\": \"UInt64\", \"name\": \"String\"}).")] string columns);

    /// <summary>
    /// Inserts data into a ClickHouse table.
    /// </summary>
    /// <param name="connectionString">The ClickHouse connection string.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="data">A JSON string representing the data to insert (e.g., [{"name": "John Doe", "age": 30}]).</param>
    /// <returns>A message indicating the insertion status.</returns>
    [KernelFunction, Description("Inserts data into a ClickHouse table.")]
    Task<string> InsertData(
        [Description("The ClickHouse connection string.")] string connectionString,
        [Description("The name of the table.")] string tableName,
        [Description("A JSON string representing the data to insert (e.g., [{\"name\": \"John Doe\", \"age\": 30}]).")] string data);

    /// <summary>
    /// Gets the schema of a ClickHouse table.
    /// </summary>
    /// <param name="connectionString">The ClickHouse connection string.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A JSON string representing the table schema.</returns>
    [KernelFunction, Description("Gets the schema of a ClickHouse table.")]
    Task<string> GetTableSchema(
        [Description("The ClickHouse connection string.")] string connectionString,
        [Description("The name of the table.")] string tableName);
}



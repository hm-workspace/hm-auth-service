using System.Data;
using AuthService.Data;
using Dapper;

namespace AuthService.Repository;

public abstract class BaseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    protected BaseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Executes a database operation with automatic connection management.
    /// Falls back to the provided fallback function if database operation fails.
    /// </summary>
    protected async Task<TResult> ExecuteWithConnectionAsync<TResult>(
        Func<IDbConnection, Task<TResult>> databaseOperation,
        Func<Task<TResult>> fallbackOperation)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await databaseOperation(connection);
        }
        catch
        {
            return await fallbackOperation();
        }
    }

    /// <summary>
    /// Executes a database operation with automatic connection management.
    /// Falls back to the provided fallback function if database operation fails.
    /// Synchronous fallback version.
    /// </summary>
    protected async Task<TResult> ExecuteWithConnectionAsync<TResult>(
        Func<IDbConnection, Task<TResult>> databaseOperation,
        Func<TResult> fallbackOperation)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await databaseOperation(connection);
        }
        catch
        {
            return fallbackOperation();
        }
    }

    /// <summary>
    /// Executes a database operation with automatic connection management.
    /// No fallback - throws exception on failure.
    /// </summary>
    protected async Task<TResult> ExecuteWithConnectionAsync<TResult>(
        Func<IDbConnection, Task<TResult>> databaseOperation)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await databaseOperation(connection);
    }

    /// <summary>
    /// Executes a void database operation with automatic connection management.
    /// Falls back to the provided fallback action if database operation fails.
    /// </summary>
    protected async Task ExecuteWithConnectionAsync(
        Func<IDbConnection, Task> databaseOperation,
        Func<Task> fallbackOperation)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await databaseOperation(connection);
        }
        catch
        {
            await fallbackOperation();
        }
    }

    /// <summary>
    /// Executes a void database operation with automatic connection management.
    /// Falls back to the provided fallback action if database operation fails.
    /// Synchronous fallback version.
    /// </summary>
    protected async Task ExecuteWithConnectionAsync(
        Func<IDbConnection, Task> databaseOperation,
        Action fallbackOperation)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await databaseOperation(connection);
        }
        catch
        {
            fallbackOperation();
        }
    }

    /// <summary>
    /// Helper method for Dapper QuerySingleOrDefaultAsync
    /// </summary>
    protected Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, Func<T?>? fallback = null)
        where T : class
    {
        return ExecuteWithConnectionAsync(
            async connection => await connection.QuerySingleOrDefaultAsync<T>(sql, param),
            fallback ?? (() => default(T)));
    }

    /// <summary>
    /// Helper method for Dapper QueryAsync
    /// </summary>
    protected Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, Func<IEnumerable<T>>? fallback = null)
    {
        return ExecuteWithConnectionAsync(
            async connection => await connection.QueryAsync<T>(sql, param),
            fallback ?? (() => Enumerable.Empty<T>()));
    }

    /// <summary>
    /// Helper method for Dapper ExecuteAsync
    /// </summary>
    protected Task<int> ExecuteAsync(string sql, object? param = null, Func<int>? fallback = null)
    {
        return ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, param),
            fallback ?? (() => 0));
    }

    /// <summary>
    /// Helper method for Dapper ExecuteScalarAsync
    /// </summary>
    protected Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, Func<T>? fallback = null)
    {
        return ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteScalarAsync<T>(sql, param),
            fallback ?? (() => default(T)!));
    }
}

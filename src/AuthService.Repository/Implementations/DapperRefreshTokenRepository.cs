using Dapper;
using AuthService.Data;
using AuthService.InternalModels.Entities;
using AuthService.Repository.Interfaces;

namespace AuthService.Repository.Implementations;

public class DapperRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperRefreshTokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        EnsureTableExists();
    }

    private void EnsureTableExists()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
IF OBJECT_ID('RefreshTokens', 'U') IS NULL
BEGIN
    CREATE TABLE RefreshTokens
    (
        TokenHash NVARCHAR(128) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        ExpiresAtUtc DATETIME2 NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        RevokedAtUtc DATETIME2 NULL
    );

    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
    CREATE INDEX IX_RefreshTokens_ExpiresAtUtc ON RefreshTokens(ExpiresAtUtc);
END";
            connection.Execute(sql);
        }
        catch
        {
            // Ignore table bootstrap errors and rely on in-memory fallback paths.
        }
    }

    public async Task CreateAsync(RefreshTokenEntity refreshToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"INSERT INTO RefreshTokens (TokenHash, UserId, ExpiresAtUtc, CreatedAtUtc, RevokedAtUtc)
VALUES (@TokenHash, @UserId, @ExpiresAtUtc, @CreatedAtUtc, @RevokedAtUtc)";
            await connection.ExecuteAsync(sql, refreshToken);
        }
        catch
        {
            InMemoryAuthStore.RefreshTokens[refreshToken.TokenHash] = refreshToken;
        }
    }

    public async Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"SELECT TokenHash, UserId, ExpiresAtUtc, CreatedAtUtc, RevokedAtUtc
FROM RefreshTokens
WHERE TokenHash = @TokenHash";
            return await connection.QuerySingleOrDefaultAsync<RefreshTokenEntity>(sql, new { TokenHash = tokenHash });
        }
        catch
        {
            InMemoryAuthStore.RefreshTokens.TryGetValue(tokenHash, out var entity);
            return entity;
        }
    }

    public async Task<bool> RevokeAsync(string tokenHash)
    {
        var revokedAt = DateTime.UtcNow;
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"UPDATE RefreshTokens
SET RevokedAtUtc = @RevokedAtUtc
WHERE TokenHash = @TokenHash AND RevokedAtUtc IS NULL";
            return await connection.ExecuteAsync(sql, new { TokenHash = tokenHash, RevokedAtUtc = revokedAt }) > 0;
        }
        catch
        {
            if (!InMemoryAuthStore.RefreshTokens.TryGetValue(tokenHash, out var token))
            {
                return false;
            }

            if (token.RevokedAtUtc.HasValue)
            {
                return false;
            }

            token.RevokedAtUtc = revokedAt;
            return true;
        }
    }

    public async Task<int> DeleteExpiredOrRevokedAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"DELETE FROM RefreshTokens
WHERE ExpiresAtUtc <= @Now OR RevokedAtUtc IS NOT NULL";
            return await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow });
        }
        catch
        {
            var keysToRemove = InMemoryAuthStore.RefreshTokens
                .Where(x => x.Value.ExpiresAtUtc <= DateTime.UtcNow || x.Value.RevokedAtUtc.HasValue)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                InMemoryAuthStore.RefreshTokens.Remove(key);
            }

            return keysToRemove.Count;
        }
    }
}

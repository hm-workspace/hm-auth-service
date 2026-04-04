using Dapper;
using Dapper;
using AuthService.Data;
using AuthService.InternalModels.Entities;
using AuthService.Repository.Interfaces;
using AuthService.Utils.Common;

namespace AuthService.Repository.Implementations;

public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory)
    {
    }

    public Task CreateAsync(RefreshTokenEntity refreshToken)
    {
        const string sql = @"INSERT INTO RefreshTokens (TokenHash, UserId, ExpiresAtUtc, CreatedAtUtc, RevokedAtUtc)
VALUES (@TokenHash, @UserId, @ExpiresAtUtc, @CreatedAtUtc, @RevokedAtUtc)";

        return ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, refreshToken),
            () => InMemoryAuthStore.RefreshTokens[refreshToken.TokenHash] = refreshToken);
    }

    public Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash)
    {
        const string sql = @"SELECT TokenHash, UserId, ExpiresAtUtc, CreatedAtUtc, RevokedAtUtc
FROM RefreshTokens
WHERE TokenHash = @TokenHash";

        return QuerySingleOrDefaultAsync<RefreshTokenEntity>(
            sql,
            new { TokenHash = tokenHash },
            () =>
            {
                InMemoryAuthStore.RefreshTokens.TryGetValue(tokenHash, out var entity);
                return entity;
            });
    }

    public async Task<bool> RevokeAsync(string tokenHash)
    {
        var revokedAt = DateTime.UtcNow;
        const string sql = @"UPDATE RefreshTokens
SET RevokedAtUtc = @RevokedAtUtc
WHERE TokenHash = @TokenHash AND RevokedAtUtc IS NULL";

        return await ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, new { TokenHash = tokenHash, RevokedAtUtc = revokedAt }) > 0,
            () =>
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
            });
    }

    public async Task<int> DeleteExpiredOrRevokedAsync()
    {
        const string sql = @"DELETE FROM RefreshTokens
WHERE ExpiresAtUtc <= @Now OR RevokedAtUtc IS NOT NULL";

        return await ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow }),
            () =>
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
            });
    }
}

using Dapper;
using AuthService.Data;
using AuthService.InternalModels.Entities;
using AuthService.Repository;
using AuthService.Repository.Interfaces;
using System.Data;

namespace AuthService.Repository.Implementations;

public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public Task CreateAsync(RefreshTokenEntity refreshToken)
    {
        return ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.CreateRefreshToken,
                    refreshToken,
                    commandType: CommandType.StoredProcedure));
    }

    public Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash)
    {
        return ExecuteWithConnectionAsync(async connection =>
            await connection.QuerySingleOrDefaultAsync<RefreshTokenEntity>(
                StoredProcedureNames.GetRefreshTokenByHash,
                new { TokenHash = tokenHash },
                commandType: CommandType.StoredProcedure));
    }

    public async Task<bool> RevokeAsync(string tokenHash)
    {
        var revokedAt = DateTime.UtcNow;
        return await ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.RevokeRefreshToken,
                    new { TokenHash = tokenHash, RevokedAtUtc = revokedAt },
                    commandType: CommandType.StoredProcedure) > 0);
    }

    public async Task<int> DeleteExpiredOrRevokedAsync()
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.CleanupRefreshTokens,
                    new { Now = DateTime.UtcNow },
                    commandType: CommandType.StoredProcedure));
    }
}

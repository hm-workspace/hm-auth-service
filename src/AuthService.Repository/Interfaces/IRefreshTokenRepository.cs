using AuthService.InternalModels.Entities;

namespace AuthService.Repository.Interfaces;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshTokenEntity refreshToken);
    Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash);
    Task<bool> RevokeAsync(string tokenHash);
    Task<int> DeleteExpiredOrRevokedAsync();
}

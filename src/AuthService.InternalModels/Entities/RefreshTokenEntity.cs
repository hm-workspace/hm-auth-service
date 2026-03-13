namespace AuthService.InternalModels.Entities;

public class RefreshTokenEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired => ExpiresAtUtc <= DateTime.UtcNow;
}

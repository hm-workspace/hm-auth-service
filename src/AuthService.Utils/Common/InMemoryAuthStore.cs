using AuthService.InternalModels.Entities;

namespace AuthService.Utils.Common;

public static class InMemoryAuthStore
{
    public static int UserSeed = 1;
    public static int RefreshTokenSeed = 1;

    public static List<UserEntity> Users { get; } = new();
    public static Dictionary<string, RefreshTokenEntity> RefreshTokens { get; } = new();
}

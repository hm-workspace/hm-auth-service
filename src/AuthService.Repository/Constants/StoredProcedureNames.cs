namespace AuthService.Repository;

public static class StoredProcedureNames
{
    public const string GetUserById = "dbo.GetUserById";
    public const string GetUserByEmail = "dbo.GetUserByEmail";
    public const string GetUsersPaged = "dbo.GetUsersPaged";
    public const string CreateUser = "dbo.CreateUser";
    public const string UpdateUser = "dbo.UpdateUser";
    public const string UpdateUserLastLogin = "dbo.UpdateUserLastLogin";
    public const string DeleteUser = "dbo.DeleteUser";
    public const string SetUserActiveStatus = "dbo.SetUserActiveStatus";

    public const string CreateRefreshToken = "dbo.CreateRefreshToken";
    public const string GetRefreshTokenByHash = "dbo.GetRefreshTokenByHash";
    public const string RevokeRefreshToken = "dbo.RevokeRefreshToken";
    public const string CleanupRefreshTokens = "dbo.CleanupRefreshTokens";
}

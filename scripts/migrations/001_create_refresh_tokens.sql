-- Migration: 001_create_refresh_tokens
-- Purpose: Create refresh token persistence schema for OAuth token rotation/revocation.

IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        TokenHash NVARCHAR(128) NOT NULL,
        UserId INT NOT NULL,
        ExpiresAtUtc DATETIME2(0) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL,
        RevokedAtUtc DATETIME2(0) NULL,
        CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (TokenHash)
    );

    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens (UserId);
    CREATE INDEX IX_RefreshTokens_ExpiresAtUtc ON dbo.RefreshTokens (ExpiresAtUtc);
END;
GO

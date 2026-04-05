-- =============================================
-- Database Setup Script for AuthService
-- =============================================
-- Run this script on your localhost SQL Server

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'healthplus')
BEGIN
    CREATE DATABASE healthplus;
    PRINT 'Database healthplus created successfully.';
END
ELSE
BEGIN
    PRINT 'Database healthplus already exists.';
END
GO

USE healthplus;
GO

-- =============================================
-- Create Users Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(255) NOT NULL UNIQUE,
        [Password] NVARCHAR(255) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Phone] NVARCHAR(20) NULL,
        [RoleName] NVARCHAR(50) NOT NULL DEFAULT 'Patient',
        [IsActive] BIT NOT NULL DEFAULT 1,
        [LastLogin] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Create index on Email for faster lookups
    CREATE NONCLUSTERED INDEX IX_Users_Email ON [dbo].[Users] ([Email]);

    PRINT 'Users table created successfully.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END
GO

-- =============================================
-- Create RefreshTokens Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [TokenHash] NVARCHAR(255) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [ExpiresAtUtc] DATETIME2 NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [RevokedAtUtc] DATETIME2 NULL,

        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );

    -- Create index on UserId for faster lookups
    CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId ON [dbo].[RefreshTokens] ([UserId]);

    -- Create index on ExpiresAtUtc for cleanup queries
    CREATE NONCLUSTERED INDEX IX_RefreshTokens_ExpiresAtUtc ON [dbo].[RefreshTokens] ([ExpiresAtUtc]);

    PRINT 'RefreshTokens table created successfully.';
END
ELSE
BEGIN
    PRINT 'RefreshTokens table already exists.';
END
GO

-- =============================================
-- Seed Test Data
-- =============================================
-- Insert test user if not exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE Email = 'manoj.peercoder@gmail.com')
BEGIN
    INSERT INTO [dbo].[Users] 
        ([Username], [Email], [Password], [FirstName], [LastName], [Phone], [RoleName], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES 
        ('manoj.peercoder', 'manoj.peercoder@gmail.com', 'Manoj@1234', 'Manoj', 'Peer', '1234567890', 'Admin', 1, GETUTCDATE(), GETUTCDATE());

    PRINT 'Test user inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Test user already exists.';
END
GO

-- Insert additional test users
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE Email = 'john.doe@example.com')
BEGIN
    INSERT INTO [dbo].[Users] 
        ([Username], [Email], [Password], [FirstName], [LastName], [Phone], [RoleName], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES 
        ('john.doe', 'john.doe@example.com', 'Password@123', 'John', 'Doe', '9876543210', 'Doctor', 1, GETUTCDATE(), GETUTCDATE()),
        ('jane.smith', 'jane.smith@example.com', 'Password@123', 'Jane', 'Smith', '5551234567', 'Nurse', 1, GETUTCDATE(), GETUTCDATE()),
        ('patient.test', 'patient@example.com', 'Password@123', 'Test', 'Patient', '5559876543', 'Patient', 1, GETUTCDATE(), GETUTCDATE());

    PRINT 'Additional test users inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Additional test users already exist.';
END
GO

-- =============================================
-- Verify Setup
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'Database Setup Complete!';
PRINT '==============================================';
PRINT '';
PRINT 'Users Count: ' + CAST((SELECT COUNT(*) FROM [dbo].[Users]) AS NVARCHAR(10));
PRINT 'RefreshTokens Count: ' + CAST((SELECT COUNT(*) FROM [dbo].[RefreshTokens]) AS NVARCHAR(10));
PRINT '';
PRINT 'Test User Credentials:';
PRINT '  Email: manoj.peercoder@gmail.com';
PRINT '  Password: Manoj@1234';
PRINT '  Role: Admin';
PRINT '';
PRINT 'Connection String for appsettings.json:';
PRINT 'Server=localhost;Database=healthplus;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;';
PRINT '==============================================';
GO

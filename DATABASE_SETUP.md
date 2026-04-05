# Database Setup Guide

This guide will help you set up the local SQL Server database for the AuthService application.

## Prerequisites

- SQL Server installed on localhost (SQL Server Express, Developer, or Standard edition)
- SQL Server Management Studio (SSMS) or Azure Data Studio (recommended)

### Download SQL Server

If you don't have SQL Server installed:
1. **SQL Server 2022 Express (Free)**: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. **Azure Data Studio (Free)**: https://aka.ms/azuredatastudio

## Step 1: Run the Database Setup Script

### Option A: Using Azure Data Studio (Recommended)
1. Open **Azure Data Studio**
2. Connect to your local SQL Server:
   - Server: `localhost` or `(localdb)\MSSQLLocalDB`
   - Authentication: Windows Authentication
3. Click **File** ? **Open File**
4. Select `database-setup.sql` from the project root
5. Click **Run** (or press F5)

### Option B: Using SQL Server Management Studio (SSMS)
1. Open **SSMS**
2. Connect to `localhost` using Windows Authentication
3. Click **File** ? **Open** ? **File**
4. Select `database-setup.sql`
5. Click **Execute** (or press F5)

### Option C: Using Command Line (sqlcmd)
```powershell
sqlcmd -S localhost -E -i database-setup.sql
```

## Step 2: Verify Database Creation

Run this query to verify everything is set up correctly:

```sql
USE healthplus;

-- Check Users table
SELECT * FROM Users;

-- Check RefreshTokens table
SELECT * FROM RefreshTokens;
```

You should see 4 test users in the Users table.

## Step 3: Update Connection String (Already Done)

The connection string has been updated in both:
- `src/AuthService.Api/appsettings.json`
- `src/AuthService.Api/appsettings.Development.json`

Current connection string:
```
Server=localhost;Database=healthplus;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

### Connection String Options

**For Windows Authentication (Default):**
```
Server=localhost;Database=healthplus;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

**For SQL Server Authentication:**
```
Server=localhost;Database=healthplus;User Id=your_username;Password=your_password;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

**For LocalDB:**
```
Server=(localdb)\\MSSQLLocalDB;Database=healthplus;Integrated Security=True;TrustServerCertificate=True;
```

## Step 4: Run the Application

1. Start the application in Visual Studio or via command line:
   ```powershell
   cd src/AuthService.Api
   dotnet run
   ```

2. The application will now connect to the SQL Server database instead of in-memory storage.

## Step 5: Test the API

### Test Login with Database User

**Using HTTP (Recommended):**
```http
POST http://localhost:5053/api/auth/login
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Using HTTPS:**
```http
POST https://localhost:7180/api/auth/login
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

### Expected Response
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true
    }
  }
}
```

## Available Test Users

| Email | Password | Role |
|-------|----------|------|
| manoj.peercoder@gmail.com | Manoj@1234 | Admin |
| john.doe@example.com | Password@123 | Doctor |
| jane.smith@example.com | Password@123 | Nurse |
| patient@example.com | Password@123 | Patient |

## Troubleshooting

### Issue: Cannot connect to SQL Server

**Solution 1: Check SQL Server is running**
```powershell
# Open Services
services.msc
# Look for "SQL Server (MSSQLSERVER)" or "SQL Server (SQLEXPRESS)"
# Ensure it's Running
```

**Solution 2: Enable TCP/IP**
1. Open **SQL Server Configuration Manager**
2. Expand **SQL Server Network Configuration**
3. Click **Protocols for MSSQLSERVER** (or your instance name)
4. Right-click **TCP/IP** ? **Enable**
5. Restart SQL Server service

**Solution 3: Use LocalDB instead**
Update the connection string to:
```
Server=(localdb)\\MSSQLLocalDB;Database=healthplus;Integrated Security=True;TrustServerCertificate=True;
```

### Issue: Login denied for user

This means Windows Authentication is not working. Switch to SQL Server Authentication:

1. Open SSMS and connect as Administrator
2. Right-click your server ? **Properties** ? **Security**
3. Select **SQL Server and Windows Authentication mode**
4. Restart SQL Server
5. Create a SQL login and update the connection string

### Issue: Database does not exist

Run the `database-setup.sql` script again to create the database and tables.

### Issue: Application still uses in-memory storage

Check the debug logs. If you see database errors, the app falls back to in-memory storage. Common causes:
- SQL Server not running
- Connection string incorrect
- Firewall blocking connection

## Changes Made

### ? Configuration Updates

1. **appsettings.json**
   - Updated connection string to use `localhost`
   - Changed from Azure SQL to local SQL Server format
   - Added `TrustServerCertificate=True` for development

2. **appsettings.Development.json**
   - Added connection string override for development
   - Enabled SQL logging for debugging

3. **InMemoryAuthStore.cs**
   - Fixed `RefreshTokens` to use `Dictionary` instead of `List`
   - Matches the repository implementation

4. **Program.cs**
   - Removed in-memory seed data
   - Application now relies on database only

### ? Database Script

- **database-setup.sql**
  - Creates `healthplus` database
  - Creates `Users` table with indexes
  - Creates `RefreshTokens` table with foreign key
  - Seeds test user data
  - Includes verification queries

## Next Steps

1. ? Run `database-setup.sql` to create the database
2. ? Verify connection in SSMS/Azure Data Studio
3. ? Run the application
4. ? Test the login endpoint with Postman
5. ? Check that data is persisted in SQL Server

## Monitoring Database Operations

To see SQL queries being executed, check the application logs. SQL logging is enabled in Development mode:

```
Microsoft.EntityFrameworkCore.Database.Command: Information
```

You'll see output like:
```
Executed DbCommand (Xms) [Parameters=[@Email='manoj.peercoder@gmail.com'], CommandType='Text', CommandTimeout='30']
SELECT Id, Username, Email, Password, FirstName, LastName, Phone, RoleName, IsActive, LastLogin, CreatedAt, UpdatedAt
FROM Users WHERE LOWER(Email) = LOWER(@Email)
```

This confirms the application is using the database instead of in-memory storage.

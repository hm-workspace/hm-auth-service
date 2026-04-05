# API Testing Guide

## Prerequisites

? **Database Setup Required**

Before testing the API, you must set up the SQL Server database:
1. Run the `database-setup.sql` script (see `DATABASE_SETUP.md` for detailed instructions)
2. Verify the database is created and contains test users

## Quick Fix for "Socket Hang Up" Error

**The issue:** You're trying to connect to `https://localhost:7180` but experiencing SSL certificate errors.

**Fastest solution:** Use the HTTP endpoint instead:

### Change This:
```
POST https://localhost:7180/api/auth/login
```

### To This:
```
POST http://localhost:5053/api/auth/login
```

## Postman Configuration

### Request Details
- **Method:** POST
- **URL:** `http://localhost:5053/api/auth/login`
- **Headers:**
  - Content-Type: `application/json`

### Body (raw JSON)
```json
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

### Expected Response (200 OK)
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

## Available Test Users (From Database)

After running `database-setup.sql`, you'll have these test users:

| Email | Password | Role | Description |
|-------|----------|------|-------------|
| manoj.peercoder@gmail.com | Manoj@1234 | Admin | System administrator |
| john.doe@example.com | Password@123 | Doctor | Medical professional |
| jane.smith@example.com | Password@123 | Nurse | Nursing staff |
| patient@example.com | Password@123 | Patient | Regular patient |

## Alternative: Fix HTTPS Connection

If you prefer to use HTTPS (`https://localhost:7180`), you need to:

### Option 1: Disable SSL Verification in Postman
1. Click the **Settings** icon (gear) in Postman
2. Go to **Settings** ? **General**
3. Turn **OFF** "SSL certificate verification"
4. Retry your request

### Option 2: Trust the .NET Development Certificate
Run in PowerShell/Terminal as Administrator:
```powershell
dotnet dev-certs https --trust
```

Then restart the application.

## Other Endpoints

Once logged in, you can test other endpoints using the access token:

### Get All Users (Requires Authentication)
```
GET http://localhost:5053/api/users
Authorization: Bearer {your-access-token}
```

### Register New User
```
POST http://localhost:5053/api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "Password@123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "9876543210",
  "role": "Patient"
}
```

### Refresh Token
```
POST http://localhost:5053/api/auth/token
Content-Type: application/json

{
  "grantType": "refresh_token",
  "refreshToken": "{your-refresh-token}"
}
```

## Swagger UI

Access Swagger documentation at:
- HTTP: `http://localhost:5053/api/auth/swagger`
- HTTPS: `https://localhost:7180/api/auth/swagger` (after fixing SSL)

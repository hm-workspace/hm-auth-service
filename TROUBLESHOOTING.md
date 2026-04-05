# Fixing the "Socket Hang Up" Error

The "socket hang up" error you're experiencing is due to **SSL/TLS certificate validation issues** when connecting to the HTTPS endpoint in development.

## Solutions

### Option 1: Use HTTP Instead of HTTPS (Recommended for Development)
Change your Postman request URL from:
```
https://localhost:7180/api/auth/login
```
to:
```
http://localhost:5053/api/auth/login
```

### Option 2: Disable SSL Verification in Postman
1. In Postman, go to **Settings** (gear icon)
2. Under the **General** tab, find **SSL certificate verification**
3. Turn **OFF** "SSL certificate verification"
4. Try your request again

### Option 3: Trust the Development Certificate
Run this command in PowerShell/Terminal (Administrator):
```powershell
dotnet dev-certs https --trust
```

Then restart your application and try again.

## Test Credentials

The application now includes seed data for development. Use these credentials:

```json
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

## Changes Made

1. **Created `InMemoryAuthStore.cs`** - Provides fallback storage when database is unavailable
2. **Added seed data** - Pre-populated test user for development
3. **Added exception handling** - Better error reporting in development mode
4. **Fixed Swagger routing** - Swagger now accessible at `/api/auth/swagger`

## Testing the API

### Using HTTP (Recommended)
```bash
POST http://localhost:5053/api/auth/login
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

### Using HTTPS (After trusting certificate)
```bash
POST https://localhost:7180/api/auth/login
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

# Quick Setup Guide

## 1?? Setup Database (One-time)

Run this in SQL Server Management Studio, Azure Data Studio, or sqlcmd:

```bash
sqlcmd -S localhost -E -i database-setup.sql
```

Or open `database-setup.sql` in SSMS/Azure Data Studio and execute it.

## 2?? Verify Connection String

Already configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=healthplus;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
}
```

## 3?? Run the Application

```bash
cd src/AuthService.Api
dotnet run
```

Or press **F5** in Visual Studio.

## 4?? Test with Postman

**Endpoint:** `POST http://localhost:5053/api/auth/login`

**Body:**
```json
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Expected:** 200 OK with JWT token

## 5?? Access Swagger

Open browser: `http://localhost:5053/api/auth/swagger`

---

## ?? Troubleshooting

**Issue:** Login fails with "Invalid email or password"
- ? Run `database-setup.sql` to create test users
- ? Check SQL Server is running: `services.msc` ? SQL Server

**Issue:** "Cannot connect to database"
- ? Verify SQL Server is running on localhost
- ? Try LocalDB connection string: `Server=(localdb)\\MSSQLLocalDB;...`

**Issue:** "Socket hang up" in Postman
- ? Use HTTP port 5053 instead of HTTPS 7180
- ? Or disable SSL verification in Postman settings

---

## ?? Detailed Documentation

- **DATABASE_SETUP.md** - Complete database setup instructions
- **API_TESTING_GUIDE.md** - All API endpoints and examples
- **TROUBLESHOOTING.md** - Common issues and solutions

---

## ?? Test Credentials

| Email | Password | Role |
|-------|----------|------|
| manoj.peercoder@gmail.com | Manoj@1234 | Admin |
| john.doe@example.com | Password@123 | Doctor |
| jane.smith@example.com | Password@123 | Nurse |
| patient@example.com | Password@123 | Patient |

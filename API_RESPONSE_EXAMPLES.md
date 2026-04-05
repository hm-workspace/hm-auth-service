# API Response Examples - Different Cultures

This document shows real API response examples for different cultures.

---

## Test Data
```
User: manoj.peercoder@gmail.com
Last Login: December 25, 2024, 2:30:45 PM EST (UTC: 2024-12-25T19:30:45Z)
Created: December 1, 2024, 10:00:00 AM EST (UTC: 2024-12-01T15:00:00Z)
```

---

## 1. United States (en-US)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: en-US
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "rB8YHqK+3mN...",
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "12/25/2024 02:30:45 PM"  ? MM/dd/yyyy hh:mm:ss tt
    }
  }
}
```

**Format:** 12-hour clock with AM/PM, Month first

---

## 2. India (en-IN)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: en-IN
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "rB8YHqK+3mN...",
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "26-12-2024 01:00:45"  ? dd-MM-yyyy HH:mm:ss (IST)
    }
  }
}
```

**Format:** 24-hour clock, Day-Month-Year with dashes  
**Timezone:** IST (+5:30 hours ahead of UTC)

---

## 3. United Kingdom (en-GB)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: en-GB
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "25/12/2024 19:30:45"  ? dd/MM/yyyy HH:mm:ss (GMT)
    }
  }
}
```

**Format:** 24-hour clock, Day/Month/Year with slashes  
**Timezone:** GMT (same as UTC)

---

## 4. Germany (de-DE)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: de-DE
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "25.12.2024 20:30:45"  ? dd.MM.yyyy HH:mm:ss (CET)
    }
  }
}
```

**Format:** 24-hour clock, Day.Month.Year with dots  
**Timezone:** CET (+1 hour ahead of UTC)

---

## 5. Japan (ja-JP)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: ja-JP
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "2024/12/26 04:30:45"  ? yyyy/MM/dd HH:mm:ss (JST)
    }
  }
}
```

**Format:** 24-hour clock, Year/Month/Day with slashes  
**Timezone:** JST (+9 hours ahead of UTC)

---

## 6. China (zh-CN)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: zh-CN
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "2024-12-26 03:30:45"  ? yyyy-MM-dd HH:mm:ss (CST)
    }
  }
}
```

**Format:** 24-hour clock, Year-Month-Day with dashes  
**Timezone:** CST (+8 hours ahead of UTC)

---

## 7. Brazil (pt-BR)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: pt-BR
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "25/12/2024 16:30:45"  ? dd/MM/yyyy HH:mm:ss (BRT)
    }
  }
}
```

**Format:** 24-hour clock, Day/Month/Year with slashes  
**Timezone:** BRT (-3 hours behind UTC)

---

## 8. France (fr-FR)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: fr-FR
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "25/12/2024 20:30:45"  ? dd/MM/yyyy HH:mm:ss (CET)
    }
  }
}
```

**Format:** 24-hour clock, Day/Month/Year with slashes  
**Timezone:** CET (+1 hour ahead of UTC)

---

## 9. Saudi Arabia (ar-SA)

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Culture: ar-SA
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "25/12/2024 10:30:45 PM"  ? dd/MM/yyyy hh:mm:ss tt (AST)
    }
  }
}
```

**Format:** 12-hour clock with AM/PM, Day/Month/Year with slashes  
**Timezone:** AST (+3 hours ahead of UTC)

---

## 10. Using Country Code

**Request:**
```http
POST http://localhost:5053/api/auth/login
X-Country-Code: JP
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "lastLogin": "2024/12/26 04:30:45"  ? Automatically uses ja-JP format
    }
  }
}
```

Country codes automatically map to cultures:
- `US` ? `en-US`
- `IN` ? `en-IN`
- `JP` ? `ja-JP`
- `DE` ? `de-DE`
- etc.

---

## 11. No Culture Header (Default)

**Request:**
```http
POST http://localhost:5053/api/auth/login
Content-Type: application/json

(No X-Culture header)
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "lastLogin": "12/25/2024 02:30:45 PM"  ? Falls back to en-US (default)
    }
  }
}
```

---

## Format Comparison Table

| Culture | Date Separator | Order | Time | Example |
|---------|---------------|-------|------|---------|
| en-US | `/` | M/D/Y | 12h | 12/25/2024 02:30:45 PM |
| en-GB | `/` | D/M/Y | 24h | 25/12/2024 14:30:45 |
| en-IN | `-` | D-M-Y | 24h | 25-12-2024 14:30:45 |
| de-DE | `.` | D.M.Y | 24h | 25.12.2024 14:30:45 |
| fr-FR | `/` | D/M/Y | 24h | 25/12/2024 14:30:45 |
| ja-JP | `/` | Y/M/D | 24h | 2024/12/25 14:30:45 |
| zh-CN | `-` | Y-M-D | 24h | 2024-12-25 14:30:45 |
| pt-BR | `/` | D/M/Y | 24h | 25/12/2024 14:30:45 |
| es-ES | `/` | D/M/Y | 24h | 25/12/2024 14:30:45 |
| ar-SA | `/` | D/M/Y | 12h | 25/12/2024 10:30:45 PM |

---

## Timezone Conversion Examples

All examples show the same UTC time: `2024-12-25T19:30:45Z`

| Culture | Timezone | UTC Offset | Local Time |
|---------|----------|------------|------------|
| en-US | EST | -5h | 12/25/2024 02:30:45 PM |
| en-GB | GMT | 0h | 25/12/2024 19:30:45 |
| en-IN | IST | +5:30h | 26-12-2024 01:00:45 |
| de-DE | CET | +1h | 25.12.2024 20:30:45 |
| ja-JP | JST | +9h | 2024/12/26 04:30:45 |
| zh-CN | CST | +8h | 2024-12-26 03:30:45 |
| pt-BR | BRT | -3h | 25/12/2024 16:30:45 |
| ar-SA | AST | +3h | 25/12/2024 10:30:45 PM |

---

## Summary

? **Same UTC time** ? **Different local formats**  
? **Automatic timezone conversion**  
? **Culture-specific formatting**  
? **No code changes required**  

**Just add the header and get formatted responses!** ??

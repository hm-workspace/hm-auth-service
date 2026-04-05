# Quick Start: DateTime Culture Formatting

## ? What's Been Implemented

Your AuthService now automatically formats DateTime values based on the client's culture/country!

---

## ?? How to Use

### 1. Send Culture Information in HTTP Headers

**Option 1: Using X-Culture Header (Recommended)**
```http
POST http://localhost:5053/api/auth/login
X-Culture: en-IN
Content-Type: application/json

{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Option 2: Using X-Country-Code Header**
```http
POST http://localhost:5053/api/auth/login
X-Country-Code: IN
Content-Type: application/json
```

**Option 3: Using Accept-Language Header**
```http
POST http://localhost:5053/api/auth/login
Accept-Language: en-IN
Content-Type: application/json
```

### 2. Get Formatted Response

The API will automatically format all DateTime fields according to your culture:

**Response with en-IN (India):**
```json
{
  "success": true,
  "data": {
    "user": {
      "lastLogin": "25-12-2024 14:30:45"  ? dd-MM-yyyy HH:mm:ss
    }
  }
}
```

**Response with en-US (United States):**
```json
{
  "success": true,
  "data": {
    "user": {
      "lastLogin": "12/25/2024 02:30:45 PM"  ? MM/dd/yyyy hh:mm:ss tt
    }
  }
}
```

---

## ?? Supported Formats

| Culture | Example DateTime Format | Country |
|---------|------------------------|---------|
| **en-US** | 12/25/2024 02:30:45 PM | United States |
| **en-GB** | 25/12/2024 14:30:45 | United Kingdom |
| **en-IN** | 25-12-2024 14:30:45 | India |
| **de-DE** | 25.12.2024 14:30:45 | Germany |
| **fr-FR** | 25/12/2024 14:30:45 | France |
| **ja-JP** | 2024/12/25 14:30:45 | Japan |
| **zh-CN** | 2024-12-25 14:30:45 | China |
| **pt-BR** | 25/12/2024 14:30:45 | Brazil |
| **es-ES** | 25/12/2024 14:30:45 | Spain |
| **ar-SA** | 25/12/2024 02:30:45 PM | Saudi Arabia |

---

## ?? Testing with Postman

### Test 1: Login with US Format
```
POST http://localhost:5053/api/auth/login
Headers:
  X-Culture: en-US
  Content-Type: application/json
Body:
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Expected:** `lastLogin: "12/25/2024 02:30:45 PM"`

### Test 2: Login with India Format
```
POST http://localhost:5053/api/auth/login
Headers:
  X-Culture: en-IN
  Content-Type: application/json
Body:
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Expected:** `lastLogin: "25-12-2024 14:30:45"`

### Test 3: Login with Japan Format
```
POST http://localhost:5053/api/auth/login
Headers:
  X-Culture: ja-JP
  Content-Type: application/json
Body:
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Expected:** `lastLogin: "2024/12/25 14:30:45"`

### Test 4: Using Country Code
```
POST http://localhost:5053/api/auth/login
Headers:
  X-Country-Code: DE
  Content-Type: application/json
Body:
{
  "email": "manoj.peercoder@gmail.com",
  "password": "Manoj@1234"
}
```

**Expected:** `lastLogin: "25.12.2024 14:30:45"` (German format)

---

## ?? Configuration

All formats are configured in `appsettings.json`:

```json
{
  "Localization": {
    "DefaultCulture": "en-US",
    "Cultures": {
      "en-IN": {
        "DateFormat": "dd-MM-yyyy",
        "TimeFormat": "HH:mm:ss",
        "DateTimeFormat": "dd-MM-yyyy HH:mm:ss",
        "TimeZoneId": "India Standard Time"
      }
    }
  }
}
```

### Add a New Culture

Just add to `appsettings.json`:

```json
"Localization": {
  "Cultures": {
    "it-IT": {
      "DateFormat": "dd/MM/yyyy",
      "TimeFormat": "HH:mm:ss",
      "DateTimeFormat": "dd/MM/yyyy HH:mm:ss",
      "TimeZoneId": "W. Europe Standard Time"
    }
  }
}
```

---

## ?? Country Code ? Culture Mapping

The system automatically maps country codes to cultures:

```
US ? en-US
GB ? en-GB
IN ? en-IN
DE ? de-DE
FR ? fr-FR
JP ? ja-JP
CN ? zh-CN
BR ? pt-BR
ES ? es-ES
IT ? it-IT
CA ? en-CA
AU ? en-AU
... and 25+ more
```

---

## ?? Usage in Code

### Automatic (No Code Required)

DateTime fields in API responses are automatically formatted:

```csharp
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
{
    var result = await _authService.LoginAsync(loginDto);
    return Ok(result);  // lastLogin automatically formatted!
}
```

### Manual Formatting (If Needed)

```csharp
public class MyService
{
    private readonly ICultureService _cultureService;

    public MyService(ICultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public string GetFormattedNow()
    {
        var now = DateTime.UtcNow;

        // Format complete datetime
        return _cultureService.FormatDateTime(now);

        // Or just date
        // return _cultureService.FormatDate(now);

        // Or just time
        // return _cultureService.FormatTime(now);
    }
}
```

---

## ? Key Features

- ? **Automatic Detection** - No client code changes needed
- ? **Timezone Aware** - Converts UTC to local time automatically
- ? **10+ Cultures** - Pre-configured for major countries
- ? **Easy to Extend** - Add new cultures in config
- ? **Multiple Headers** - Supports X-Culture, X-Country-Code, Accept-Language
- ? **Fallback** - Uses default culture if not specified

---

## ?? Real-World Examples

### Example 1: US Client
```
Request:  X-Culture: en-US
Response: "createdAt": "12/25/2024 02:30:45 PM"
          "lastLogin": "12/24/2024 09:15:30 AM"
```

### Example 2: Indian Client
```
Request:  X-Country-Code: IN
Response: "createdAt": "25-12-2024 14:30:45"
          "lastLogin": "24-12-2024 09:15:30"
```

### Example 3: Japanese Client
```
Request:  Accept-Language: ja-JP
Response: "createdAt": "2024/12/25 14:30:45"
          "lastLogin": "2024/12/24 09:15:30"
```

### Example 4: No Culture (Default)
```
Request:  (no culture header)
Response: "createdAt": "12/25/2024 02:30:45 PM"  ? Falls back to en-US
          "lastLogin": "12/24/2024 09:15:30 AM"
```

---

## ?? Documentation

For complete documentation, see:
- **DATETIME_CULTURE_FORMATTING.md** - Full guide with all features
- **appsettings.json** - Culture configurations

---

## ?? How It Works

```
1. Client sends request with X-Culture: en-IN
2. CultureMiddleware detects culture
3. CultureService stores culture for this request
4. Service processes request (returns DateTime in UTC)
5. JSON serializer formats DateTime using culture
6. Client receives formatted response
```

---

## ? Quick Commands

### Test All Cultures
```bash
# US
curl -H "X-Culture: en-US" http://localhost:5053/api/auth/login -d '{"email":"user@example.com","password":"pass"}'

# India
curl -H "X-Culture: en-IN" http://localhost:5053/api/auth/login -d '{"email":"user@example.com","password":"pass"}'

# Japan
curl -H "X-Culture: ja-JP" http://localhost:5053/api/auth/login -d '{"email":"user@example.com","password":"pass"}'

# Germany
curl -H "X-Culture: de-DE" http://localhost:5053/api/auth/login -d '{"email":"user@example.com","password":"pass"}'
```

---

## ? Summary

| What | How | Example |
|------|-----|---------|
| **Send culture** | X-Culture header | `X-Culture: en-IN` |
| **Or use country** | X-Country-Code | `X-Country-Code: IN` |
| **Or standard** | Accept-Language | `Accept-Language: en-IN` |
| **Get formatted** | Automatic | `"lastLogin": "25-12-2024 14:30:45"` |
| **Add culture** | appsettings.json | Add to Localization:Cultures |
| **Default** | If no header | Uses en-US |

**Your API is now internationally ready! ????**

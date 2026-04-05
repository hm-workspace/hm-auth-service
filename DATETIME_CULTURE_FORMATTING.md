# Culture-Aware DateTime Formatting Guide

## Overview

The AuthService now supports **configurable DateTime formatting** based on client culture/country. This allows the application to automatically format dates and times according to the user's locale preferences.

## Features

? **Multiple Culture Support** - Pre-configured for 10+ cultures (US, GB, IN, DE, FR, JP, CN, BR, ES, AR)  
? **Automatic Detection** - Detects culture from HTTP headers  
? **Timezone Conversion** - Automatically converts between UTC and local time  
? **Flexible Configuration** - Easy to add new cultures via appsettings.json  
? **Custom Formats** - Define date/time formats per culture  
? **JSON Serialization** - Automatic formatting in API responses  

---

## How It Works

### 1. Client Sends Culture Information

The client can specify culture in multiple ways (priority order):

#### Option 1: Custom `X-Culture` Header (Recommended)
```http
POST /api/auth/login
X-Culture: en-IN
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password"
}
```

#### Option 2: Country Code Header
```http
POST /api/auth/login
X-Country-Code: IN
Content-Type: application/json
```

#### Option 3: Accept-Language Header (Standard)
```http
POST /api/auth/login
Accept-Language: en-IN,en;q=0.9
Content-Type: application/json
```

#### Option 4: Query String Parameter
```http
POST /api/auth/login?culture=en-IN
Content-Type: application/json
```

### 2. Server Formats Response

The server automatically formats all DateTime fields according to the client's culture:

**Example Response (en-US):**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": 1,
      "email": "user@example.com",
      "lastLogin": "12/25/2024 02:30:45 PM"  ? MM/dd/yyyy hh:mm:ss tt
    }
  }
}
```

**Example Response (en-IN):**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": 1,
      "email": "user@example.com",
      "lastLogin": "25-12-2024 14:30:45"  ? dd-MM-yyyy HH:mm:ss
    }
  }
}
```

**Example Response (ja-JP):**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": 1,
      "email": "user@example.com",
      "lastLogin": "2024/12/25 14:30:45"  ? yyyy/MM/dd HH:mm:ss
    }
  }
}
```

---

## Configuration

### appsettings.json

```json
{
  "Localization": {
    "DefaultCulture": "en-US",
    "Cultures": {
      "en-US": {
        "DateFormat": "MM/dd/yyyy",
        "TimeFormat": "hh:mm:ss tt",
        "DateTimeFormat": "MM/dd/yyyy hh:mm:ss tt",
        "TimeZoneId": "Eastern Standard Time"
      },
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

### Supported Cultures (Pre-configured)

| Culture | Country | Date Format | Time Format | Timezone |
|---------|---------|-------------|-------------|----------|
| en-US | United States | MM/dd/yyyy | hh:mm:ss tt | Eastern Standard Time |
| en-GB | United Kingdom | dd/MM/yyyy | HH:mm:ss | GMT Standard Time |
| en-IN | India | dd-MM-yyyy | HH:mm:ss | India Standard Time |
| de-DE | Germany | dd.MM.yyyy | HH:mm:ss | W. Europe Standard Time |
| fr-FR | France | dd/MM/yyyy | HH:mm:ss | Romance Standard Time |
| ja-JP | Japan | yyyy/MM/dd | HH:mm:ss | Tokyo Standard Time |
| zh-CN | China | yyyy-MM-dd | HH:mm:ss | China Standard Time |
| pt-BR | Brazil | dd/MM/yyyy | HH:mm:ss | E. South America Standard Time |
| es-ES | Spain | dd/MM/yyyy | HH:mm:ss | Romance Standard Time |
| ar-SA | Saudi Arabia | dd/MM/yyyy | hh:mm:ss tt | Arab Standard Time |

### Adding New Cultures

Add to `appsettings.json`:

```json
"Localization": {
  "Cultures": {
    "fr-CA": {
      "DateFormat": "yyyy-MM-dd",
      "TimeFormat": "HH:mm:ss",
      "DateTimeFormat": "yyyy-MM-dd HH:mm:ss",
      "TimeZoneId": "Eastern Standard Time"
    }
  }
}
```

---

## Usage Examples

### In Controllers

The culture is automatically applied via middleware. No code changes needed:

```csharp
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
{
    var result = await _authService.LoginAsync(loginDto);
    return Ok(result);  // DateTime fields automatically formatted!
}
```

### In Services (Manual Formatting)

If you need to manually format dates:

```csharp
public class MyService
{
    private readonly ICultureService _cultureService;

    public MyService(ICultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public string GetFormattedDate()
    {
        var now = DateTime.UtcNow;

        // Format as date-time
        var formatted = _cultureService.FormatDateTime(now);

        // Format as date only
        var dateOnly = _cultureService.FormatDate(now);

        // Format as time only
        var timeOnly = _cultureService.FormatTime(now);

        // Convert UTC to local time
        var localTime = _cultureService.ConvertFromUtc(now);

        return formatted;
    }
}
```

### Using Extension Methods

```csharp
using AuthService.Utils.Extensions;

public class MyService
{
    private readonly ICultureService _cultureService;

    public MyService(ICultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public void Example()
    {
        var now = DateTime.UtcNow;

        // Convert to localized string
        var str = now.ToLocalizedString(_cultureService);

        // Convert to local time
        var local = now.ToLocalTime(_cultureService);

        // Date only
        var date = now.ToLocalizedDateString(_cultureService);

        // Time only
        var time = now.ToLocalizedTimeString(_cultureService);
    }
}
```

---

## Testing

### Postman Example

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
    "token": "eyJhbGc...",
    "refreshToken": "base64...",
    "user": {
      "id": 1,
      "username": "manoj.peercoder",
      "email": "manoj.peercoder@gmail.com",
      "firstName": "Manoj",
      "lastName": "Peer",
      "phone": "1234567890",
      "roleName": "Admin",
      "isActive": true,
      "lastLogin": "25-12-2024 14:30:45"  ? Formatted as dd-MM-yyyy HH:mm:ss
    }
  }
}
```

### Testing Different Cultures

```bash
# US Format
curl -H "X-Culture: en-US" http://localhost:5053/api/auth/login
# Response: "lastLogin": "12/25/2024 02:30:45 PM"

# UK Format
curl -H "X-Culture: en-GB" http://localhost:5053/api/auth/login
# Response: "lastLogin": "25/12/2024 14:30:45"

# India Format
curl -H "X-Culture: en-IN" http://localhost:5053/api/auth/login
# Response: "lastLogin": "25-12-2024 14:30:45"

# Japan Format
curl -H "X-Culture: ja-JP" http://localhost:5053/api/auth/login
# Response: "lastLogin": "2024/12/25 14:30:45"

# Germany Format
curl -H "X-Culture: de-DE" http://localhost:5053/api/auth/login
# Response: "lastLogin": "25.12.2024 14:30:45"
```

---

## Country Code Mapping

The middleware automatically maps country codes to cultures:

```csharp
X-Country-Code: US  ? en-US
X-Country-Code: GB  ? en-GB
X-Country-Code: IN  ? en-IN
X-Country-Code: DE  ? de-DE
X-Country-Code: FR  ? fr-FR
X-Country-Code: JP  ? ja-JP
X-Country-Code: CN  ? zh-CN
X-Country-Code: BR  ? pt-BR
X-Country-Code: ES  ? es-ES
X-Country-Code: SA  ? ar-SA
```

Full list of 35+ country codes supported in `CultureMiddleware.cs`.

---

## Architecture

```
???????????????????????????????????????????????
?  Client Request                             ?
?  Headers: X-Culture: en-IN                  ?
???????????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????????
?  CultureMiddleware                          ?
?  - Detects culture from headers             ?
?  - Sets culture in CultureService           ?
???????????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????????
?  Controller/Service                         ?
?  - Processes business logic                 ?
?  - Returns DTOs with DateTime fields        ?
???????????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????????
?  JSON Serialization                         ?
?  - CultureAwareDateTimeConverter            ?
?  - Formats DateTime using CultureService    ?
???????????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????????
?  Client Response                            ?
?  { "lastLogin": "25-12-2024 14:30:45" }    ?
???????????????????????????????????????????????
```

---

## Components

### 1. ICultureService / CultureService
- Manages current culture per request
- Provides formatting methods
- Handles timezone conversions

### 2. CultureMiddleware
- Detects culture from HTTP headers
- Sets culture in CultureService
- Runs before authentication

### 3. CultureAwareDateTimeConverter
- Custom JSON converter for DateTime
- Automatically formats in JSON responses
- Supports both DateTime and DateTime?

### 4. DateTimeExtensions
- Extension methods for easy formatting
- ToLocalizedString()
- ToLocalTime() / ToUtcTime()

---

## Benefits

### ? **User Experience**
- Users see dates in familiar format
- Automatic timezone conversion
- No manual formatting needed

### ? **Developer Experience**
- Automatic formatting in responses
- Simple configuration
- Easy to add new cultures

### ? **Maintainability**
- Centralized culture management
- Consistent formatting across app
- Easy to modify formats

### ? **Scalability**
- Supports unlimited cultures
- Per-request culture isolation
- Thread-safe implementation

---

## Best Practices

### 1. Always Store in UTC
```csharp
user.LastLogin = DateTime.UtcNow;  // ? Good
user.LastLogin = DateTime.Now;      // ? Bad
```

### 2. Let the Framework Handle Formatting
```csharp
return Ok(user);  // ? Automatic formatting

// ? Don't manually format in service layer
user.LastLogin = user.LastLogin.ToString("dd-MM-yyyy");
```

### 3. Use Consistent Headers
```csharp
// ? Good - Use X-Culture for explicit control
X-Culture: en-IN

// ? Also good - Use Accept-Language for standard compliance
Accept-Language: en-IN,en;q=0.9
```

### 4. Validate Culture Input
```csharp
// The middleware handles invalid cultures by falling back to default
X-Culture: invalid  ? Falls back to en-US (default)
```

---

## Troubleshooting

### Issue: Dates not formatting correctly

**Solution:** Check that X-Culture header is being sent:
```bash
curl -H "X-Culture: en-IN" http://localhost:5053/api/auth/login
```

### Issue: Timezone not converting

**Solution:** Verify TimeZoneId in appsettings.json is correct for your platform:
```json
// Windows
"TimeZoneId": "India Standard Time"

// Linux/Mac
"TimeZoneId": "Asia/Kolkata"
```

### Issue: Culture not detected

**Solution:** Check middleware order in Program.cs:
```csharp
app.UseMiddleware<CultureMiddleware>();  // Must be before controllers
app.UseAuthentication();
app.MapControllers();
```

---

## Summary

| Feature | Status |
|---------|--------|
| Multiple culture support | ? 10+ cultures pre-configured |
| Automatic detection | ? From headers/query string |
| Timezone conversion | ? UTC ? Local |
| JSON formatting | ? Automatic in responses |
| Easy configuration | ? appsettings.json |
| Extension methods | ? Available |
| Thread-safe | ? Per-request isolation |
| Production-ready | ? Yes |

**Your application now supports international date/time formatting! ??**

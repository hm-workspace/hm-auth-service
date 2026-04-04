using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace AuthService.Utils.Localization;

/// <summary>
/// Service for managing culture-specific formatting and localization
/// </summary>
public class CultureService : ICultureService
{
    private readonly IConfiguration _configuration;
    private readonly AsyncLocal<string> _currentCulture = new();
    private readonly Dictionary<string, CultureConfig> _cultureConfigs;

    public CultureService(IConfiguration configuration)
    {
        _configuration = configuration;
        _cultureConfigs = LoadCultureConfigs();
        _currentCulture.Value = _configuration["Localization:DefaultCulture"] ?? "en-US";
    }

    private Dictionary<string, CultureConfig> LoadCultureConfigs()
    {
        var configs = new Dictionary<string, CultureConfig>(StringComparer.OrdinalIgnoreCase);
        var section = _configuration.GetSection("Localization:Cultures");

        foreach (var culture in section.GetChildren())
        {
            var key = culture.Key;
            configs[key] = new CultureConfig
            {
                CultureCode = key,
                DateFormat = culture["DateFormat"] ?? "yyyy-MM-dd",
                TimeFormat = culture["TimeFormat"] ?? "HH:mm:ss",
                DateTimeFormat = culture["DateTimeFormat"] ?? "yyyy-MM-dd HH:mm:ss",
                TimeZoneId = culture["TimeZoneId"] ?? "UTC"
            };
        }

        // Add default culture if not configured
        if (!configs.ContainsKey("en-US"))
        {
            configs["en-US"] = new CultureConfig
            {
                CultureCode = "en-US",
                DateFormat = "MM/dd/yyyy",
                TimeFormat = "hh:mm:ss tt",
                DateTimeFormat = "MM/dd/yyyy hh:mm:ss tt",
                TimeZoneId = "UTC"
            };
        }

        return configs;
    }

    public string GetCurrentCulture()
    {
        return _currentCulture.Value ?? _configuration["Localization:DefaultCulture"] ?? "en-US";
    }

    public void SetCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = _configuration["Localization:DefaultCulture"] ?? "en-US";
        }

        _currentCulture.Value = culture;
    }

    private CultureConfig GetCurrentConfig()
    {
        var culture = GetCurrentCulture();
        if (_cultureConfigs.TryGetValue(culture, out var config))
        {
            return config;
        }

        // Try to get base culture (e.g., "en" from "en-GB")
        var baseCulture = culture.Split('-')[0];
        if (_cultureConfigs.TryGetValue(baseCulture, out var baseConfig))
        {
            return baseConfig;
        }

        // Fallback to default
        return _cultureConfigs["en-US"];
    }

    public string GetDateFormat()
    {
        return GetCurrentConfig().DateFormat;
    }

    public string GetTimeFormat()
    {
        return GetCurrentConfig().TimeFormat;
    }

    public string GetDateTimeFormat()
    {
        return GetCurrentConfig().DateTimeFormat;
    }

    public string FormatDateTime(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;

        var localTime = ConvertFromUtc(dateTime.Value);
        return localTime.ToString(GetDateTimeFormat(), CultureInfo.InvariantCulture);
    }

    public string FormatDate(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;

        var localTime = ConvertFromUtc(dateTime.Value);
        return localTime.ToString(GetDateFormat(), CultureInfo.InvariantCulture);
    }

    public string FormatTime(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return string.Empty;

        var localTime = ConvertFromUtc(dateTime.Value);
        return localTime.ToString(GetTimeFormat(), CultureInfo.InvariantCulture);
    }

    public TimeSpan GetTimezoneOffset()
    {
        var config = GetCurrentConfig();
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(config.TimeZoneId);
            return timeZone.GetUtcOffset(DateTime.UtcNow);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    public DateTime ConvertFromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        var config = GetCurrentConfig();
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(config.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch
        {
            return utcDateTime;
        }
    }

    public DateTime ConvertToUtc(DateTime localDateTime)
    {
        var config = GetCurrentConfig();
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(config.TimeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }
        catch
        {
            return localDateTime.ToUniversalTime();
        }
    }

    private class CultureConfig
    {
        public string CultureCode { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
        public string TimeFormat { get; set; } = string.Empty;
        public string DateTimeFormat { get; set; } = string.Empty;
        public string TimeZoneId { get; set; } = "UTC";
    }
}

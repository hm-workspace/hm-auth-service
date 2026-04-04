using AuthService.Utils.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Middleware;

/// <summary>
/// Middleware to detect and set culture from request headers
/// </summary>
public class CultureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CultureMiddleware> _logger;

    public CultureMiddleware(RequestDelegate next, ILogger<CultureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICultureService cultureService)
    {
        var culture = DetectCulture(context);

        if (!string.IsNullOrEmpty(culture))
        {
            cultureService.SetCulture(culture);
            _logger.LogDebug("Culture set to: {Culture}", culture);
        }

        await _next(context);
    }

    private string? DetectCulture(HttpContext context)
    {
        // Priority 1: X-Culture header (custom header for explicit culture)
        if (context.Request.Headers.TryGetValue("X-Culture", out var cultureHeader))
        {
            var culture = cultureHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(culture))
            {
                _logger.LogDebug("Culture detected from X-Culture header: {Culture}", culture);
                return culture;
            }
        }

        // Priority 2: X-Country-Code header (map country to culture)
        if (context.Request.Headers.TryGetValue("X-Country-Code", out var countryHeader))
        {
            var countryCode = countryHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(countryCode))
            {
                var culture = MapCountryToCulture(countryCode);
                _logger.LogDebug("Culture detected from X-Country-Code header: {CountryCode} -> {Culture}", countryCode, culture);
                return culture;
            }
        }

        // Priority 3: Accept-Language header
        if (context.Request.Headers.TryGetValue("Accept-Language", out var langHeader))
        {
            var languages = langHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(languages))
            {
                // Parse Accept-Language header (e.g., "en-US,en;q=0.9,es;q=0.8")
                var culture = ParseAcceptLanguage(languages);
                if (!string.IsNullOrEmpty(culture))
                {
                    _logger.LogDebug("Culture detected from Accept-Language header: {Culture}", culture);
                    return culture;
                }
            }
        }

        // Priority 4: Query string parameter
        if (context.Request.Query.TryGetValue("culture", out var cultureQuery))
        {
            var culture = cultureQuery.FirstOrDefault();
            if (!string.IsNullOrEmpty(culture))
            {
                _logger.LogDebug("Culture detected from query string: {Culture}", culture);
                return culture;
            }
        }

        return null;
    }

    private string? ParseAcceptLanguage(string acceptLanguage)
    {
        // Parse the Accept-Language header and return the highest priority culture
        var languages = acceptLanguage.Split(',')
            .Select(l => l.Split(';')[0].Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToArray();

        return languages.FirstOrDefault();
    }

    private string MapCountryToCulture(string countryCode)
    {
        // Map common country codes to cultures
        return countryCode.ToUpperInvariant() switch
        {
            "US" => "en-US",
            "GB" => "en-GB",
            "IN" => "en-IN",
            "CA" => "en-CA",
            "AU" => "en-AU",
            "DE" => "de-DE",
            "FR" => "fr-FR",
            "ES" => "es-ES",
            "IT" => "it-IT",
            "JP" => "ja-JP",
            "CN" => "zh-CN",
            "BR" => "pt-BR",
            "MX" => "es-MX",
            "AR" => "es-AR",
            "NL" => "nl-NL",
            "SE" => "sv-SE",
            "NO" => "nb-NO",
            "DK" => "da-DK",
            "FI" => "fi-FI",
            "PL" => "pl-PL",
            "RU" => "ru-RU",
            "TR" => "tr-TR",
            "KR" => "ko-KR",
            "SA" => "ar-SA",
            "AE" => "ar-AE",
            "ZA" => "en-ZA",
            "NZ" => "en-NZ",
            "IE" => "en-IE",
            "SG" => "en-SG",
            "MY" => "ms-MY",
            "TH" => "th-TH",
            "VN" => "vi-VN",
            "ID" => "id-ID",
            "PH" => "en-PH",
            _ => "en-US" // Default fallback
        };
    }
}

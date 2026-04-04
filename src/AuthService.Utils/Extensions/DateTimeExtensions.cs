using AuthService.Utils.Localization;

namespace AuthService.Utils.Extensions;

/// <summary>
/// Extension methods for DateTime formatting
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Formats DateTime to string using current culture
    /// </summary>
    public static string ToLocalizedString(this DateTime dateTime, ICultureService cultureService)
    {
        return cultureService.FormatDateTime(dateTime);
    }

    /// <summary>
    /// Formats nullable DateTime to string using current culture
    /// </summary>
    public static string ToLocalizedString(this DateTime? dateTime, ICultureService cultureService)
    {
        return cultureService.FormatDateTime(dateTime);
    }

    /// <summary>
    /// Formats DateTime to date string using current culture
    /// </summary>
    public static string ToLocalizedDateString(this DateTime dateTime, ICultureService cultureService)
    {
        return cultureService.FormatDate(dateTime);
    }

    /// <summary>
    /// Formats DateTime to time string using current culture
    /// </summary>
    public static string ToLocalizedTimeString(this DateTime dateTime, ICultureService cultureService)
    {
        return cultureService.FormatTime(dateTime);
    }

    /// <summary>
    /// Converts UTC DateTime to local time based on current culture
    /// </summary>
    public static DateTime ToLocalTime(this DateTime utcDateTime, ICultureService cultureService)
    {
        return cultureService.ConvertFromUtc(utcDateTime);
    }

    /// <summary>
    /// Converts local DateTime to UTC based on current culture
    /// </summary>
    public static DateTime ToUtcTime(this DateTime localDateTime, ICultureService cultureService)
    {
        return cultureService.ConvertToUtc(localDateTime);
    }
}

namespace AuthService.Utils.Localization;

/// <summary>
/// Service for managing culture-specific formatting and localization
/// </summary>
public interface ICultureService
{
    /// <summary>
    /// Gets the current culture for the request
    /// </summary>
    string GetCurrentCulture();

    /// <summary>
    /// Sets the culture for the current request
    /// </summary>
    void SetCulture(string culture);

    /// <summary>
    /// Gets the date format pattern for the current culture
    /// </summary>
    string GetDateFormat();

    /// <summary>
    /// Gets the time format pattern for the current culture
    /// </summary>
    string GetTimeFormat();

    /// <summary>
    /// Gets the date-time format pattern for the current culture
    /// </summary>
    string GetDateTimeFormat();

    /// <summary>
    /// Formats a DateTime according to current culture
    /// </summary>
    string FormatDateTime(DateTime? dateTime);

    /// <summary>
    /// Formats a date according to current culture
    /// </summary>
    string FormatDate(DateTime? dateTime);

    /// <summary>
    /// Formats a time according to current culture
    /// </summary>
    string FormatTime(DateTime? dateTime);

    /// <summary>
    /// Gets timezone offset for the current culture
    /// </summary>
    TimeSpan GetTimezoneOffset();

    /// <summary>
    /// Converts UTC to local time based on culture
    /// </summary>
    DateTime ConvertFromUtc(DateTime utcDateTime);

    /// <summary>
    /// Converts local time to UTC based on culture
    /// </summary>
    DateTime ConvertToUtc(DateTime localDateTime);
}

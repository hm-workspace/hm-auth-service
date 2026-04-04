using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthService.Utils.Localization;

/// <summary>
/// Custom JSON converter for DateTime that uses culture-specific formatting
/// </summary>
public class CultureAwareDateTimeConverter : JsonConverter<DateTime>
{
    private readonly ICultureService _cultureService;

    public CultureAwareDateTimeConverter(ICultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
            return default;

        // Try parsing with current culture format first
        if (DateTime.TryParseExact(dateString, _cultureService.GetDateTimeFormat(), null, System.Globalization.DateTimeStyles.None, out var result))
        {
            return _cultureService.ConvertToUtc(result);
        }

        // Fallback to standard parsing
        if (DateTime.TryParse(dateString, out result))
        {
            return result.Kind == DateTimeKind.Utc ? result : _cultureService.ConvertToUtc(result);
        }

        throw new JsonException($"Unable to parse '{dateString}' as DateTime");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var formatted = _cultureService.FormatDateTime(value);
        writer.WriteStringValue(formatted);
    }
}

/// <summary>
/// Custom JSON converter for nullable DateTime that uses culture-specific formatting
/// </summary>
public class CultureAwareNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly ICultureService _cultureService;

    public CultureAwareNullableDateTimeConverter(ICultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
            return null;

        // Try parsing with current culture format first
        if (DateTime.TryParseExact(dateString, _cultureService.GetDateTimeFormat(), null, System.Globalization.DateTimeStyles.None, out var result))
        {
            return _cultureService.ConvertToUtc(result);
        }

        // Fallback to standard parsing
        if (DateTime.TryParse(dateString, out result))
        {
            return result.Kind == DateTimeKind.Utc ? result : _cultureService.ConvertToUtc(result);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        var formatted = _cultureService.FormatDateTime(value.Value);
        writer.WriteStringValue(formatted);
    }
}

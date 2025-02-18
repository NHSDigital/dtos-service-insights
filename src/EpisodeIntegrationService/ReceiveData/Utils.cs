using System.Globalization;

namespace NHS.ServiceInsights.EpisodeIntegrationService;
public static class Utils
{
    private static readonly string[] AllowedDateFormats = ["dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd"];

    public static short? ParseBooleanStringToShort(string booleanString)
    {
        if (string.IsNullOrEmpty(booleanString))
        {
            return null;
        }

        if (booleanString.ToLower() != "true" && booleanString.ToLower() != "false")
        {
            throw new ArgumentException($"Invalid boolean value: {booleanString}");
        }

        return booleanString.ToLower() == "true" ? (short)1 : (short)0;
    }

    public static DateOnly? ParseNullableDate(string? date)
    {
        if (string.IsNullOrEmpty(date)) return null;

        var dateTime = DateTime.ParseExact(date, AllowedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
        return DateOnly.FromDateTime(dateTime);
    }

    public static DateTime? ParseNullableDateTime(string? dateTime, string format)
    {
        if (string.IsNullOrEmpty(dateTime)) return null;

        return DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
    }

    public static void CheckForNullOrEmptyStrings(params string[] values)
    {
        if (values.Any(v => string.IsNullOrEmpty(v)))
        {
            throw new ArgumentException("Value cannot be null or empty");
        }
    }

    public static void ValidateDataValue(string dataValue)
    {
        if (!string.IsNullOrWhiteSpace(dataValue) && dataValue.ToLower() != "true" && dataValue.ToLower() != "false")
        {
            throw new ArgumentException($"Invalid value: {dataValue}. Expected 'true', 'false', null, or an empty string");
        }
    }
}

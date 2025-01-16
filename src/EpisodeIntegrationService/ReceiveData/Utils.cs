using System.Globalization;

namespace NHS.ServiceInsights.EpisodeIntegrationService;
public class Utils
{
    private static readonly string[] AllowedDateFormats = ["dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd"];
    public static short? ParseBooleanStringToShort(string booleanString)
    {
        if (booleanString.ToUpper() == "TRUE")
        {
            return (short)1;
        }
        else if (booleanString.ToUpper() == "FALSE")
        {
            return (short)0;
        }
        else
        {
            return null;
        }
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

        return DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

}

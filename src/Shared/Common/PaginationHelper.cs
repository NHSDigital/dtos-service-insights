using System.Collections.Specialized;
using System.Globalization;

namespace NHS.ServiceInsights.Common;

public static class PaginationHelper
{
    public static bool TryValidatePaginationQuery(NameValueCollection query, out int page, out int pageSize, out DateTime startDate, out DateTime endDate, out string errorMessage)
    {
        page = 0;
        pageSize = 0;
        startDate = default;
        endDate = default;
        errorMessage = "";

        if (!int.TryParse(query["page"], out page))
        {
            errorMessage = "The page number is invalid.";
            return false;
        }

        if (!DateTime.TryParse(query["startDate"], CultureInfo.InvariantCulture, out startDate) || !DateTime.TryParse(query["endDate"], CultureInfo.InvariantCulture, out endDate))
        {
            errorMessage = "The startDate or endDate is invalid.";
            return false;
        }

        if (startDate > endDate)
        {
            errorMessage = "The startDate is greater than the endDate.";
            return false;
        }

        if (startDate > DateTime.Now)
        {
            errorMessage = "The startDate is greater than today date.";
            return false;
        }

        if (endDate > DateTime.Now.AddDays(1))
        {
            errorMessage = "The endDate is greater than today date.";
            return false;
        }

        if (!int.TryParse(query["pageSize"], out pageSize))
        {
            errorMessage = "The pageSize is invalid.";
            return false;
        }

        if (page < 1)
        {
            errorMessage = "Page cannot be less than one.";
            return false;
        }

        if (pageSize < 1)
        {
            errorMessage = "The pageSize cannot be less than one.";
            return false;
        }

        if (pageSize > 5000)
        {
            errorMessage = "The pageSize cannot be more than 5000.";
            return false;
        }

        return true;
    }

    public static string BuildUrl(string baseUrl, int page, int pageSize, DateTime startDate, DateTime endDate)
    {
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate.ToString(CultureInfo.InvariantCulture)}&endDate={endDate.ToString(CultureInfo.InvariantCulture)}";
        return url;
    }
}

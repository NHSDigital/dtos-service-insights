using System.Collections.Specialized;
using System.Globalization;

namespace NHS.ServiceInsights.Common;

public class PaginationHelper
{
    public bool TryValidatePaginationQuery (NameValueCollection query, out int page, out int pageSize, out DateTime startDate, out DateTime endDate, out string errorMessage)
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

        if (!int.TryParse(query["pageSize"], out pageSize))
        {
            errorMessage = "The pageSize is invalid.";
            return false;
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 5000) pageSize = 5000;

        return true;
    }

    public string BuildUrl(string baseUrl, int page, int pageSize, DateTime startDate, DateTime endDate)
    {
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}&endDate={endDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}";
        return url;
    }
}

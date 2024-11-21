using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;

namespace NHS.ServiceInsights.Common;

public class PaginationHelper
{
    private readonly ILogger _logger;
    public PaginationHelper (ILogger logger)
    {
        _logger = logger;
    }

    public bool TryValidatePaginationQuery (HttpRequestData req, out int page, out int pageSize, out DateTime startDate, out DateTime endDate)
    {
        page = 0;
        pageSize = 0;
        startDate = default;
        endDate = default;

        if (!int.TryParse(req.Query["page"], out page))
        {
            _logger.LogError("The page number is invalid.");
            return false;
        }

        if (!DateTime.TryParse(req.Query["startDate"], CultureInfo.InvariantCulture, out startDate) || !DateTime.TryParse(req.Query["endDate"], CultureInfo.InvariantCulture, out endDate))
        {
            _logger.LogError("The startDate or endDate is invalid.");
            return false;
        }

        if (!int.TryParse(req.Query["pageSize"], out pageSize))
        {
            _logger.LogError("The pageSize is invalid.");
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
        _logger.LogInformation("Requesting URL: {Url}", url);
        return url;
    }
}

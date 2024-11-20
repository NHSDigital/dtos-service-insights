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

    public async Task<(bool IsValid, int page, int pageSize, DateTime startDate, DateTime endDate)> ValidateQuery(HttpRequestData req)
    {
        int page = 0;
        int pageSize = 0;
        DateTime startDate = default;
        DateTime endDate = default;

        if (!int.TryParse(req.Query["page"], out page))
        {
            _logger.LogError("The page number is invalid.");
            return (false, 0, 0, default, default);
        }

        if (!DateTime.TryParse(req.Query["startDate"], CultureInfo.InvariantCulture, out startDate) || !DateTime.TryParse(req.Query["endDate"], CultureInfo.InvariantCulture, out endDate))
        {
            _logger.LogError("The startDate or endDate is invalid.");
            return (false, 0, 0, default, default);
        }

        if (!int.TryParse(req.Query["pageSize"], out pageSize))
        {
            pageSize = 1000;
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 5000) pageSize = 5000;

        await Task.CompletedTask;

        return (true, page, pageSize, startDate, endDate);
    }
}

public class RequestHandlerHelper
{
    private readonly ILogger _logger;

    public RequestHandlerHelper(ILogger logger)
    {
        _logger = logger;
    }

    public string BuildUrl(string baseUrl, int page, int pageSize, DateTime startDate, DateTime endDate)
    {
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}&endDate={endDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}";
        _logger.LogInformation("Requesting URL: {Url}", url);
        return url;
    }
}

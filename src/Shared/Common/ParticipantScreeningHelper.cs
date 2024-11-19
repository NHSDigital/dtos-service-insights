using System.Globalization;
using System.Net;
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

    public HttpResponseData QueryValidator (out int page, out int pageSize, out DateTime startDate, out DateTime endDate, HttpRequestData req)
    {
        page = 0;
        pageSize = 0;
        startDate = default;
        endDate = default;

        if(!int.TryParse(req.Query["page"], out page))
        {
            _logger.LogError("The page number is invalid.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        if(!DateTime.TryParse(req.Query["startDate"], CultureInfo.InvariantCulture, out startDate) || !DateTime.TryParse(req.Query["endDate"],  CultureInfo.InvariantCulture, out endDate))
        {
            _logger.LogError("The startDate or endDate is invalid.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        if(!int.TryParse(req.Query["pageSize"], out pageSize))
        {
            pageSize = 1000;
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 5000) pageSize = 5000;

        return null;
    }
}

public class RequestHandlerHelper
{
    private readonly ILogger _logger;

    public RequestHandlerHelper(ILogger logger)
    {
        _logger = logger;
    }

    public HttpResponseData ValidateAndPrepareUrlRequest(HttpRequestData req, out int page, out int pageSize, out DateTime startDate, out DateTime endDate, string baseUrl, out string url)
    {
        var paginationHelper = new PaginationHelper(_logger);

        var validationResponse = paginationHelper.QueryValidator(out page, out pageSize, out startDate, out endDate, req);

        if (validationResponse != null)
        {
            url = null;
            return validationResponse;
        }

        url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}&endDate={endDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}";
        _logger.LogInformation("Requesting URL: {Url}", url);

        return null;
    }
}

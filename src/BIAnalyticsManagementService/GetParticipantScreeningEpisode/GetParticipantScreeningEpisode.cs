using System.Globalization;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.BIAnalyticsManagementService;

public class GetParticipantScreeningEpisode
{
    private readonly ILogger<GetParticipantScreeningEpisode> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public GetParticipantScreeningEpisode(ILogger<GetParticipantScreeningEpisode> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("GetParticipantScreeningEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetParticipantScreeningEpisode start");
        var paginationHelper = new PaginationHelper(_logger);
        var (isValid, page, pageSize, startDate, endDate) = paginationHelper.ValidateQuery(req);

        if (!isValid)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return errorResponse;
        }

        var requestHandler = new RequestHandlerHelper(_logger);
        string baseUrl = Environment.GetEnvironmentVariable("GetParticipantScreeningEpisodeDataUrl");
        string url = requestHandler.BuildUrl(baseUrl, page, pageSize, startDate, endDate);

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve episodes. Status Code: {response.StatusCode}");
                var errorResponse = req.CreateResponse(response.StatusCode);
                return errorResponse;
            }

            var episodesPageJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Episode data retrieved");

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            successResponse.Headers.Add("Content-Type", "application/json");
            await successResponse.WriteStringAsync(episodesPageJson);

            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when calling the GetParticipantScreeningData function. \nUrl:{url}\nException: " + ex.Message, url);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}


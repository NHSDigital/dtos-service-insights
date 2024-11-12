using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.BIAnalyticsService;

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

        int page;
        int pageSize;
        DateTime startDate;
        DateTime endDate;

        if(!int.TryParse(req.Query["page"], out page))
        {
            _logger.LogError("Invalid page number");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        if(!DateTime.TryParse(req.Query["startDate"], out startDate) || !DateTime.TryParse(req.Query["endDate"], out endDate))
        {
            _logger.LogError("Invalid startDate or endDate");
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

        var baseUrl = Environment.GetEnvironmentVariable("GetParticipantScreeningEpisodeUrl");
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate}&endDate={endDate}";
        _logger.LogInformation("Requesting URL: {Url}", url);

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
            _logger.LogError(ex, "Exception when calling the GetParticipantScreeningEpisode data service. \nUrl:{url}\nException: " + ex.Message, url);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}


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

        if (!PaginationHelper.TryValidatePaginationQuery(req.Query, out int page, out int pageSize, out DateTime startDate, out DateTime endDate, out string errorMessage))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteStringAsync(errorMessage);
            return errorResponse;
        }

        string baseUrl = Environment.GetEnvironmentVariable("GetParticipantScreeningEpisodeDataUrl");
        string url = PaginationHelper.BuildUrl(baseUrl, page, pageSize, startDate, endDate);
        _logger.LogInformation("Requesting URL: {Url}", url);

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve episodes. Status Code: {StatusCode}", response.StatusCode);
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
            _logger.LogError(ex, "Exception when calling the GetParticipantScreeningEpisodeData function. \nUrl:{url}\nException: {Message}", url, ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}


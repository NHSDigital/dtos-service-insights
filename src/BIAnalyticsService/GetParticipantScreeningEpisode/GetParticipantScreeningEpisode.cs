using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

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

        int page = int.TryParse(req.Query["page"], out int p) ? p : 1;
        int pageSize = int.TryParse(req.Query["pageSize"], out int ps) ? ps : 1000;

        DateTime? startDate = DateTime.TryParse(req.Query["startDate"], out DateTime start) ? start : (DateTime?)null;
        DateTime? endDate = DateTime.TryParse(req.Query["endDate"], out DateTime end) ? end : (DateTime?)null;

        if (startDate == null || endDate == null)
        {
            _logger.LogError("Please enter a valid start and end date");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var baseUrl = Environment.GetEnvironmentVariable("GetParticipantScreeningEpisodeUrl");
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate}&endDate={endDate}";
        _logger.LogInformation("Requesting URL: {Url}", url);

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve participant screening episodes with parameters: page {page}, page size {pageSize}, start date {startDate}, end date {endDate}. Status Code: {response.StatusCode}");
                var errorResponse = req.CreateResponse(response.StatusCode);
                return errorResponse;
            }

            var episodesPageJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Participant Screening Episode pages retrieved");

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            successResponse.Headers.Add("Content-Type", "application/json");
            await successResponse.WriteStringAsync(episodesPageJson);

            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to call the GetParticipantScreeningEpisode Data Service. \nUrl:{url}\nException: {ex}", url, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}


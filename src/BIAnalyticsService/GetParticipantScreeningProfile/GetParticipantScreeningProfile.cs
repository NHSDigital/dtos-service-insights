using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeManagementService;

public class GetParticipantScreeningProfile
{
    private readonly ILogger<GetParticipantScreeningProfile> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public GetParticipantScreeningProfile(ILogger<GetParticipantScreeningProfile> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("GetParticipantScreeningProfile")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetParticipantScreeningProfile start");

        int page = int.TryParse(req.Query["page"], out int p) ? p : 1;
        int pageSize = int.TryParse(req.Query["pageSize"], out int ps) ? ps : 1000;

        DateTime? startDate = DateTime.TryParse(req.Query["startDate"], out DateTime start) ? start : (DateTime?)null;
        DateTime? endDate = DateTime.TryParse(req.Query["endDate"], out DateTime end) ? end : (DateTime?)null;

        if (startDate == null || endDate == null){
            _logger.LogError("Please enter a valid start and end date");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        if (page < 1) page = 1;
        if (pageSize < 20) pageSize = 20;
        if (pageSize > 5000) pageSize = 5000;

        var baseUrl = Environment.GetEnvironmentVariable("GetProfilesUrl");
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate}&endDate={endDate}";
        _logger.LogInformation("Requesting URL: {Url}", url);

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"");
                var errorResponse = req.CreateResponse(response.StatusCode);
                return errorResponse;
            }

            var profilesPageJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Episode data retrieved");

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            successResponse.Headers.Add("Content-Type", "application/json");
            await successResponse.WriteStringAsync(profilesPageJson);

            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to call the GetEpisode Data Service. \nUrl:{url}\nException: {ex}", url, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

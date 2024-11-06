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

        int page;
        int pageSize;
        DateTime startDate;
        DateTime endDate;

        if(!int.TryParse(req.Query["page"], out page) || !int.TryParse(req.Query["pageSize"], out pageSize))
        {
            _logger.LogError("Invalid page or pageSize");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        if(!DateTime.TryParse(req.Query["startDate"], out startDate) || !DateTime.TryParse(req.Query["endDate"], out endDate))
        {
            _logger.LogError("Invalid startDate or endDate");
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
                _logger.LogError($"Failed to retrieve profiles. Status Code: {response.StatusCode}");
                var errorResponse = req.CreateResponse(response.StatusCode);
                return errorResponse;
            }

            var profilesPageJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Profile data retrieved");

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            successResponse.Headers.Add("Content-Type", "application/json");
            await successResponse.WriteStringAsync(profilesPageJson);

            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception when calling the GetParticipantScreeningProfileData function. \nUrl:{url}\nException: {ex}", url, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

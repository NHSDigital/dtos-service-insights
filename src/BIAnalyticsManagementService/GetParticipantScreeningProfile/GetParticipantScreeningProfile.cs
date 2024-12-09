using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.BIAnalyticsManagementService;

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

        if (!PaginationHelper.TryValidatePaginationQuery(req.Query, out int page, out int pageSize, out DateTime startDate, out DateTime endDate, out string errorMessage))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteStringAsync(errorMessage);
            return errorResponse;
        }

        string baseUrl = Environment.GetEnvironmentVariable("GetProfilesUrl");
        string url = PaginationHelper.BuildUrl(baseUrl, page, pageSize, startDate, endDate);
        _logger.LogInformation("Requesting URL: {Url}", url);

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve profiles. Status Code: {StatusCode}", response.StatusCode);
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
            _logger.LogError(ex, "Exception when calling the GetParticipantScreeningProfileData function. \nUrl:{url}\nException: {Message}", url, ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

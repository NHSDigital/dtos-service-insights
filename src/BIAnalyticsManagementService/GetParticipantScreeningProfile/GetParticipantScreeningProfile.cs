using System.Net;
using System.Globalization;
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
        var paginationHelper = new PaginationHelper(_logger);

        if (!paginationHelper.TryValidatePaginationQuery(req, out int page, out int pageSize, out DateTime startDate, out DateTime endDate))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return errorResponse;
        }

        var requestHandler = new PaginationHelper(_logger);
        string baseUrl = Environment.GetEnvironmentVariable("GetProfilesUrl");
        string url = requestHandler.BuildUrl(baseUrl, page, pageSize, startDate, endDate);

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

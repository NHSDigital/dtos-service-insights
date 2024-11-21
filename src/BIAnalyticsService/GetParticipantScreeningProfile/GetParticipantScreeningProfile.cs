using System.Net;
using System.Globalization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.BIAnalyticsService;

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

        if(!int.TryParse(req.Query["page"], out page))
        {
            _logger.LogError("Invalid page number");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequestResponse;
        }

        if(!DateTime.TryParse(req.Query["startDate"], CultureInfo.InvariantCulture, out startDate) || !DateTime.TryParse(req.Query["endDate"],  CultureInfo.InvariantCulture, out endDate))
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

        var baseUrl = Environment.GetEnvironmentVariable("GetProfilesUrl");
        var url = $"{baseUrl}?page={page}&pageSize={pageSize}&startDate={startDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}&endDate={endDate.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}";
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

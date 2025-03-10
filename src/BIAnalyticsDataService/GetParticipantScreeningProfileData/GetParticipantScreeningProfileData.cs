using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Globalization;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsDataService;

public class GetParticipantScreeningProfileData
{
    private readonly ILogger<GetParticipantScreeningProfileData> _logger;

    private readonly IParticipantScreeningProfileRepository _participantScreeningProfileRepository;

    public GetParticipantScreeningProfileData(ILogger<GetParticipantScreeningProfileData> logger, IParticipantScreeningProfileRepository participantScreeningProfileRepository)
    {
        _logger = logger;
        _participantScreeningProfileRepository = participantScreeningProfileRepository;
    }

    [Function("GetParticipantScreeningProfileData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        int page = int.Parse(req.Query["page"]);
        int pageSize = int.Parse(req.Query["pageSize"]);
        DateTime startDate = DateTime.Parse(req.Query["startDate"], CultureInfo.InvariantCulture);
        DateTime endDate = DateTime.Parse(req.Query["endDate"], CultureInfo.InvariantCulture);

        var numberOfRowsToSkip = (page - 1) * pageSize;

        try
        {
            ProfilesDataPage result = await _participantScreeningProfileRepository.GetParticipantProfile(page, pageSize, startDate, endDate, numberOfRowsToSkip);

            _logger.LogInformation("GetParticipantScreeningProfileData: Participant profiles found successfully.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await JsonSerializer.SerializeAsync(response.Body, result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetParticipantScreeningProfileData: Failed to get participant profiles from the database.\nException: {Message}", ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

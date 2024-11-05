using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.EpisodeManagementService;

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
        int page = req.Query["page"];
        int pageSize = req.Query["pageSize"];
        DateTime? startDate = req.Query["startDate"];
        DateTime? endDate = req.Query["endDate"];

        var numberOfRowsToSkip = (page - 1) * pageSize;

        try
        {
            ProfilesDataPage result = await _participantScreeningProfileRepository.GetParticipantProfile(page, pageSize, startDate, endDate, numberOfRowsToSkip);
            if (result.profiles.Count == 0)
            {
                _logger.LogError("CreateParticipantScreeningProfile: Could not find any participant profiles between the dates specified.");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogInformation("CreateParticipantScreeningProfile: participant profiles found successfully");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            var json = JsonSerializer.Serialize(result);
            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateParticipantScreeningProfile: Failed to get participant profiles from the database.\nException: " + ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

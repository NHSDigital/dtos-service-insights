using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsDataService;

public class CreateParticipantScreeningProfile
{
    private readonly ILogger<CreateParticipantScreeningProfile> _logger;
    private readonly IParticipantScreeningProfileRepository _participantScreeningProfileRepository;

    public CreateParticipantScreeningProfile(ILogger<CreateParticipantScreeningProfile> logger, IParticipantScreeningProfileRepository participantScreeningProfileRepository)
    {
        _logger = logger;
        _participantScreeningProfileRepository = participantScreeningProfileRepository;
    }

    [Function("CreateParticipantScreeningProfile")]
    public  HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        ParticipantScreeningProfile Profile = new ParticipantScreeningProfile();

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                Profile = JsonSerializer.Deserialize<ParticipantScreeningProfile>(postData);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError("CreateParticipantScreeningProfile: Could not read Json data.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            bool successful = _participantScreeningProfileRepository.CreateParticipantProfile(Profile);
            if (!successful)
            {
                _logger.LogError("CreateParticipantScreeningProfile: Could not save participant profile. Data: " + Profile);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            _logger.LogInformation("CreateParticipantScreeningProfile: participant profile saved successfully.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateParticipantScreeningProfile: Failed to save participant profile to the database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

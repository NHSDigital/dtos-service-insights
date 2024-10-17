using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsDataService;

public class CreateParticipantScreeningEpisode
{
    private readonly ILogger<CreateParticipantScreeningEpisode> _logger;
    private readonly IParticipantScreeningEpisodeRepository _participantScreeningEpisodeRepository;

    public CreateParticipantScreeningEpisode(ILogger<CreateParticipantScreeningEpisode> logger, IParticipantScreeningEpisodeRepository participantScreeningEpisodeRepository)
    {
        _logger = logger;
        _participantScreeningEpisodeRepository = participantScreeningEpisodeRepository;
    }

    [Function("CreateParticipantScreeningEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        ParticipantScreeningEpisode episode = new ParticipantScreeningEpisode();

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                episode = JsonSerializer.Deserialize<ParticipantScreeningEpisode>(postData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateParticipantScreeningEpisode: Could not read Json data.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            bool successful = await _participantScreeningEpisodeRepository.CreateParticipantEpisode(episode);
            if (!successful)
            {
                _logger.LogError("CreateParticipantScreeningEpisode: Could not save participant episode. Data: " + episode);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            _logger.LogInformation("CreateParticipantScreeningEpisode: participant episode saved successfully.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateParticipantScreeningEpisode: Failed to save participant episode to the database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

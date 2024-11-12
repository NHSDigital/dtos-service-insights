using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsDataService;

public class GetParticipantScreeningEpisode
{
    private readonly ILogger<GetParticipantScreeningEpisode> _logger;

    private readonly IParticipantScreeningEpisodeRepository _participantScreeningEpisodeRepository;
    public GetParticipantScreeningEpisode(ILogger<GetParticipantScreeningEpisode> logger, IParticipantScreeningEpisodeRepository participantScreeningEpisodeRepository)
    {
        _logger = logger;
        _participantScreeningEpisodeRepository = participantScreeningEpisodeRepository;
    }

    [Function("GetParticipantScreeningEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        int page = int.Parse(req.Query["page"]);
        int pageSize = int.Parse(req.Query["pageSize"]);
        DateTime startDate = DateTime.Parse(req.Query["startDate"]);
        DateTime endDate = DateTime.Parse(req.Query["endDate"]);

        var numberOfRowsToSkip = (page - 1) * pageSize;

        try
        {
            EpisodesDataPage result = await _participantScreeningEpisodeRepository.GetParticipantScreeningEpisode(page, pageSize, startDate, endDate, numberOfRowsToSkip);
            if(result.episodes.Count == 0)
            {
                _logger.LogError("CreateParticipantScreeningEpisode: Could not find any participant episodes");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogInformation("CreateParticipantScreeningEpisode: Participant episodes found successfully");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await JsonSerializer.SerializeAsync(response.Body, result);
            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetParticipantScreeningEpisode: Failed to get participant episodes from the database.\nException: " + ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

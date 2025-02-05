using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;
using System.Globalization;

namespace NHS.ServiceInsights.BIAnalyticsDataService;

public class GetParticipantScreeningEpisodeData
{
    private readonly ILogger<GetParticipantScreeningEpisodeData> _logger;

    private readonly IParticipantScreeningEpisodeRepository _participantScreeningEpisodeRepository;
    public GetParticipantScreeningEpisodeData(ILogger<GetParticipantScreeningEpisodeData> logger, IParticipantScreeningEpisodeRepository participantScreeningEpisodeRepository)
    {
        _logger = logger;
        _participantScreeningEpisodeRepository = participantScreeningEpisodeRepository;
    }

    [Function("GetParticipantScreeningEpisodeData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        int page = int.Parse(req.Query["page"]);
        int pageSize = int.Parse(req.Query["pageSize"]);
        DateTime startDate = DateTime.Parse(req.Query["startDate"], CultureInfo.InvariantCulture);
        DateTime endDate = DateTime.Parse(req.Query["endDate"], CultureInfo.InvariantCulture);

        var numberOfRowsToSkip = (page - 1) * pageSize;

        try
        {
            EpisodesDataPage result = await _participantScreeningEpisodeRepository.GetParticipantScreeningEpisode(page, pageSize, startDate, endDate, numberOfRowsToSkip);

            _logger.LogInformation("GetParticipantScreeningEpisode: Participant episodes found successfully");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await JsonSerializer.SerializeAsync(response.Body, result);
            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetParticipantScreeningEpisode: Failed to get participant episodes from the database.\nException: {Message}", ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

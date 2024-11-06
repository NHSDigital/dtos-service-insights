using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsDataService;

public class GetParticipantScreeningEpisode
{
    private readonly ILogger<GetParticipantScreeningEpisode> _logger;
    private readonly IHttpRequestService _httpRequestService;
    private readonly IParticipantScreeningEpisodeRepository _participantScreeningEpisodeRepository;
    public GetParticipantScreeningEpisode(ILogger<GetParticipantScreeningEpisode> logger, IHttpRequestService httpRequestService, IParticipantScreeningEpisodeRepository participantScreeningEpisodeRepository)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
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

            _logger.LogInformation("CreateParticipantScreeningEpisode: participant episodes found successfully");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            string jsonEpisodesDataPage;

            using (var memoryStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync<EpisodesDataPage?>(memoryStream, result);
                jsonEpisodesDataPage = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            await response.WriteStringAsync(jsonEpisodesDataPage);
            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get participant screening episodes from database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

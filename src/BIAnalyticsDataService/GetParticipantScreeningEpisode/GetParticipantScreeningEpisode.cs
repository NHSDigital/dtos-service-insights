using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Data;
using Microsoft.EntityFrameworkCore;
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
        _logger.LogInformation("GetParticipantScreeningEpisode start");

        int page = int.TryParse(req.Query["page"], out int p) ? p : 1;
        int pageSize = int.TryParse(req.Query["pageSize"], out int ps) ? ps : 1000;

        DateTime? startDate = DateTime.TryParse(req.Query["startDate"], out DateTime start) ? start : (DateTime?)null;
        DateTime? endDate = DateTime.TryParse(req.Query["endDate"], out DateTime end) ? end : (DateTime?)null;

        if (startDate == null || endDate == null)
        {
            _logger.LogError("Please enter a valid start and end date");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 10000) pageSize = 1000;

        var numberOfRowsToSkip = (page - 1) * pageSize;

        try
        {
            ProfilesDataPage result = await _participantScreeningEpisodeRepository.GetParticipantScreeningEpisode(page, pageSize, startDate, endDate, numberOfRowsToSkip);
            if(result.episodes.Count == 0)
            {
                _logger.LogError("CreateParticipantScreeningEpisode: Could not find any participant profiles between the dates specified");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogInformation("CreateParticipantScreeningEpisode: participant episodes found successfully");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            var json = JsonSerializer.Serialize(result);
            response.WriteString(json);
            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get participant screening episodes from database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

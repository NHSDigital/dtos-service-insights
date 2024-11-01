using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsService;

public class CreateParticipantScreeningEpisode
{
    private readonly ILogger<CreateParticipantScreeningEpisode> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public CreateParticipantScreeningEpisode(ILogger<CreateParticipantScreeningEpisode> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("CreateParticipantScreeningEpisode")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Create Participant Screening Episode function start,");

        string episodeId = req.Query["EpisodeId"];

        if (string.IsNullOrEmpty(episodeId))
        {
            _logger.LogError("episodeId is null or empty.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var baseUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var getEpisodeUrl = $"{baseUrl}?EpisodeId={episodeId}";
        _logger.LogInformation("Requesting episode URL: {Url}", getEpisodeUrl);

        Episode episode;

        try
        {
            var response = await _httpRequestService.SendGet(getEpisodeUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve episode with Episode ID {episodeId}. Status Code: {response.StatusCode}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            string episodeJson = await response.Content.ReadAsStringAsync();
            episode = JsonSerializer.Deserialize<Episode>(episodeJson);
            _logger.LogInformation("Episode data retrieved and deserialised");
        }

        catch (Exception ex)
        {
            _logger.LogError("Failed to deserialise or retrieve episode from {getEpisodeUrl}. \nException: {ex}", getEpisodeUrl, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        try
        {
            await SendToCreateParticipantScreeningEpisodeAsync(episode);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participant screening episode.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(Episode episode)
    {
        var screeningEpisode = new ParticipantScreeningEpisode
        {
            EpisodeId = episode.EpisodeId,
            ScreeningName = episode.ScreeningId.ToString(),
            NhsNumber = episode.NhsNumber.ToString(),
            EpisodeType = episode.EpisodeTypeId.ToString(),
            EpisodeTypeDescription = String.Empty,
            EpisodeOpenDate = episode.EpisodeOpenDate.ToString(),
            AppointmentMadeFlag = episode.AppointmentMadeFlag,
            FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate.ToString(),
            ActualScreeningDate = episode.ActualScreeningDate.ToString(),
            EarlyRecallDate = episode.EarlyRecallDate.ToString(),
            CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy,
            EndCode = episode.EndCodeId.ToString(),
            EndCodeDescription = String.Empty,
            EndCodeLastUpdated = episode.EndCodeLastUpdated.ToString(),
            OrganisationCode = episode.OrganisationId.ToString(),
            OrganisationName = String.Empty,
            BatchId = episode.BatchId,
            RecordInsertDatetime = DateTime.Now.ToString()
        };

        var screeningEpisodeUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl");

        string serializedParticipantScreeningEpisode = JsonSerializer.Serialize(screeningEpisode);

        _logger.LogInformation($"Sending ParticipantScreeningEpisode to {screeningEpisodeUrl}: {serializedParticipantScreeningEpisode}");

        await _httpRequestService.SendPost(screeningEpisodeUrl, serializedParticipantScreeningEpisode);
    }
}

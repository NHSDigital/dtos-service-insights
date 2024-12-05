using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.BIAnalyticsManagementService;

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
                _logger.LogError("Failed to retrieve episode with Episode ID {EpisodeId}. Status Code: {StatusCode}", episodeId, response.StatusCode);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            string episodeJson = await response.Content.ReadAsStringAsync();
            episode = JsonSerializer.Deserialize<Episode>(episodeJson);
            _logger.LogInformation("Episode data retrieved and deserialised");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialise or retrieve episode from {Url}.", getEpisodeUrl);
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

    private async Task<ScreeningLkp> GetScreeningDataAsync(long screeningId)
    {
        var baseScreeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");
        var getScreeningDataUrl = $"{baseScreeningDataServiceUrl}?screening_id={screeningId}";
        _logger.LogInformation("Requesting screening data from {Url}", getScreeningDataUrl);

        ScreeningLkp screeningLkp;

        var response = await _httpRequestService.SendGet(getScreeningDataUrl);
        response.EnsureSuccessStatusCode();

        var screeningDataJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Screening data retrieved successfully.");

        screeningLkp = JsonSerializer.Deserialize<ScreeningLkp>(screeningDataJson);
        if (screeningLkp == null)
        {
            _logger.LogError("Failed to deserialize screening data or screening data is null.");
            throw new Exception("Screening data retrieval failed.");
        }

        return screeningLkp;
    }

    private async Task<OrganisationLkp> GetOrganisationDataAsync(long? organisationId)
    {
        var baseReferenceServiceUrl = Environment.GetEnvironmentVariable("GetReferenceDataUrl");
        var getReferenceDataUrl = $"{baseReferenceServiceUrl}?organisation_id={organisationId}";
        _logger.LogInformation("Requesting organisation data from {Url}", getReferenceDataUrl);

        OrganisationLkp organisationLkp;

        var response = await _httpRequestService.SendGet(getReferenceDataUrl);
        response.EnsureSuccessStatusCode();

        var organisationDataJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Organisation data retrieved successfully.");

        organisationLkp = JsonSerializer.Deserialize<OrganisationLkp>(organisationDataJson);
        if (organisationLkp == null)
        {
            _logger.LogError("Failed to deserialize organisation data or organisation data is null.");
            throw new Exception("Organisation data retrieval failed.");
        }

        return organisationLkp;
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(Episode episode)
    {
        ScreeningLkp screeningLkp = await GetScreeningDataAsync(episode.ScreeningId);

        OrganisationLkp organisationLkp = await GetOrganisationDataAsync(episode.OrganisationId);

        var screeningEpisode = new ParticipantScreeningEpisode
        {
            EpisodeId = episode.EpisodeId,
            ScreeningName = screeningLkp.ScreeningName,
            NhsNumber = episode.NhsNumber,
            EpisodeType = episode.EpisodeTypeId.ToString(),
            EpisodeTypeDescription = String.Empty,
            EpisodeOpenDate = episode.EpisodeOpenDate,
            AppointmentMadeFlag = episode.AppointmentMadeFlag,
            FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate,
            ActualScreeningDate = episode.ActualScreeningDate,
            EarlyRecallDate = episode.EarlyRecallDate,
            CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy,
            EndCode = episode.EndCodeId.ToString(),
            EndCodeDescription = String.Empty,
            EndCodeLastUpdated = episode.EndCodeLastUpdated,
            OrganisationCode = organisationLkp.OrganisationCode,
            OrganisationName = organisationLkp.OrganisationName,
            BatchId = episode.BatchId,
            RecordInsertDatetime = DateTime.Now
        };

        var screeningEpisodeUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl");

        string serializedParticipantScreeningEpisode = JsonSerializer.Serialize(screeningEpisode);

        _logger.LogInformation("Sending ParticipantScreeningEpisode to {Url}: {Request}", screeningEpisodeUrl, serializedParticipantScreeningEpisode);


        await _httpRequestService.SendPost(screeningEpisodeUrl, serializedParticipantScreeningEpisode);
    }
}

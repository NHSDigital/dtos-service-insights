using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using System.Text;

namespace NHS.ServiceInsights.BIAnalyticsService;

public class CreateDataAssets
{
    private readonly ILogger<CreateDataAssets> _logger;
    private readonly IHttpRequestService _httpRequestService;
    public CreateDataAssets(ILogger<CreateDataAssets> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("CreateDataAssets")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("CreateDataAssets function start");

        string episodeId = req.Query["EpisodeId"];

        if (string.IsNullOrEmpty(episodeId))
        {
            _logger.LogError("episodeId is null or empty");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var baseUrl = Environment.GetEnvironmentVariable("GetEpisodeUrl");
        var GetEpisodeUrl = $"{baseUrl}?EpisodeId={episodeId}";
        _logger.LogInformation("Requesting episode URL: {Url}", GetEpisodeUrl);

        string episodeJson;
        Episode episode;

        try
        {
            var response = await _httpRequestService.SendGet(GetEpisodeUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve episode with Episode ID {episodeId}. Status Code: {response.StatusCode}");
                return req.CreateResponse(response.StatusCode);
            }

            episodeJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Episode data retrieved");
            episode = JsonSerializer.Deserialize<Episode>(episodeJson);
        }

        catch (Exception ex)
        {
            _logger.LogError("Issue when getting episode from db. \nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        string nhsNumber = "1111111112";

        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting participant URL: {Url}",participantUrl);

        Participant participant;

        try
        {
            var participantResponse = await _httpRequestService.SendGet(participantUrl);

            if (!participantResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve participant data with NHS number {nhsNumber}. Status Code: {participantResponse.StatusCode}");
                return req.CreateResponse(participantResponse.StatusCode);
            }

            var participantJson = await participantResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Participant data retrieved");

            participant = JsonSerializer.Deserialize<Participant>(participantJson);
        }
        catch (Exception ex)
        {
            _logger.LogError("Issue when getting participant from db. \nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        var (screeningEpisodeUrl, screeningProfileUrl) = GetConfigurationUrls();
        if (string.IsNullOrEmpty(screeningEpisodeUrl) || string.IsNullOrEmpty(screeningProfileUrl))
        {
            _logger.LogError("One or both URLs are not configured. \nUrl:{screeningProfileUrl}\nUrl:{screeningEpisodeUrl}", participantUrl, screeningEpisodeUrl);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        try
        {
            await SendToCreateParticipantScreeningEpisodeAsync(episode, screeningEpisodeUrl);
            await SendToCreateParticipantScreeningProfileAsync(participant, screeningProfileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create episode or profile.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    public (string screeningEpisodeUrl, string screeningProfileUrl) GetConfigurationUrls()
    {
        return (Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl"), Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl"));
    }

    public async Task SendToCreateParticipantScreeningProfileAsync(Participant participant, string screeningProfileUrl)
    {
        if (participant != null)
        {
            _logger.LogInformation("Mapping participant profile data.");

                var screeningProfile = new ParticipantScreeningProfile
                {
                    NhsNumber = participant.nhs_number,
                    ScreeningName = String.Empty,
                    PrimaryCareProvider = String.Empty,
                    PreferredLanguage = participant.preferred_language,
                    ReasonForRemoval = participant.removal_reason,
                    ReasonForRemovalDt = String.Empty,
                    NextTestDueDate = participant.next_test_due_date,
                    NextTestDueDateCalculationMethod = participant.ntdd_calculation_method,
                    ParticipantScreeningStatus = participant.subject_status_code,
                    ScreeningCeasedReason = String.Empty,
                    IsHigherRisk = participant.is_higher_risk,
                    IsHigherRiskActive = participant.is_higher_risk_active,
                    HigherRiskNextTestDueDate = participant.higher_risk_next_test_due_date,
                    HigherRiskReferralReasonCode = participant.higher_risk_referral_reason_code,
                    HrReasonCodeDescription = String.Empty,
                    DateIrradiated = participant.date_irradiated,
                    GeneCode = participant.gene_code,
                    GeneCodeDescription = String.Empty,
                    RecordInsertDatetime = DateTime.Now.ToString()
                };

                string serializedParticipantScreeningProfile = JsonSerializer.Serialize(screeningProfile, new JsonSerializerOptions { WriteIndented = true });

            _logger.LogInformation($"Sending ParticipantScreeningProfile Profile to {screeningProfileUrl}: {serializedParticipantScreeningProfile}");

            await _httpRequestService.SendPost(screeningProfileUrl, serializedParticipantScreeningProfile);
        }
        else
        {
            _logger.LogInformation("No profile data found.");
        }
    }

    public async Task SendToCreateParticipantScreeningEpisodeAsync(Episode episode, string screeningEpisodeUrl)
    {
        if (episode != null)
        {
            _logger.LogInformation("Processing episode data.");

                var screeningEpisode = new ParticipantScreeningEpisode
                {
                    EpisodeId = episode.EpisodeId,
                    ScreeningName = episode.ScreeningId,
                    NhsNumber= episode.NhsNumber,
                    EpisodeType = episode.EpisodeTypeId,
                    EpisodeTypeDescription = String.Empty,
                    EpisodeOpenDate = episode.EpisodeOpenDate,
                    AppointmentMadeFlag = episode.AppointmentMadeFlag,
                    FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate,
                    ActualScreeningDate = episode.ActualScreeningDate,
                    EarlyRecallDate = episode.EarlyRecallDate,
                    CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy,
                    EndCode = episode.EndCodeId,
                    EndCodeDescription = String.Empty,
                    EndCodeLastUpdated = episode.EndCodeLastUpdated,
                    OrganisationCode = episode.OrganisationId,
                    OrganisationName = String.Empty,
                    BatchId = episode.BatchId,
                    RecordInsertDatetime = DateTime.Now.ToString()
                };

                string serializedParticipantScreeningEpisode = JsonSerializer.Serialize(screeningEpisode, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation($"Sending ParticipantScreeningEpisode to {screeningEpisodeUrl}: {serializedParticipantScreeningEpisode}");

                await _httpRequestService.SendPost(screeningEpisodeUrl, serializedParticipantScreeningEpisode);
        }
        else
        {
            _logger.LogInformation("No episode data found.");
        }
    }
}

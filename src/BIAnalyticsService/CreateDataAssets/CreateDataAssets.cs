using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

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
                return req.CreateResponse(response.StatusCode);
            }

            string episodeJson;
            episodeJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Episode data retrieved");
            episode = JsonSerializer.Deserialize<Episode>(episodeJson);
        }

        catch (Exception ex)
        {
            _logger.LogError("Issue when getting episode from {getEpisodeUrl}. \nException: {ex}", getEpisodeUrl, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        string nhsNumber = "1111111112";

        var baseParticipantUrl = Environment.GetEnvironmentVariable("GetParticipantUrl");
        var participantUrl = $"{baseParticipantUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting participant URL: {Url}", participantUrl);

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
            _logger.LogError("Issue when getting participant from {participantUrl}. \nException: {ex}", participantUrl, ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        try
        {
            await SendToCreateParticipantScreeningEpisodeAsync(episode);
            await SendToCreateParticipantScreeningProfileAsync(participant);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create episode or profile.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private async Task<DemographicsData> GetDemographicsDataAsync(string nhsNumber)
    {
        var baseDemographicsServiceUrl = Environment.GetEnvironmentVariable("DemographicsServiceUrl");
        var demographicsServiceUrl = $"{baseDemographicsServiceUrl}?nhs_number={nhsNumber}";
        _logger.LogInformation("Requesting demographic service URL: {Url}", demographicsServiceUrl);

        DemographicsData demographicsData;

        var demographicsResponse = await _httpRequestService.SendGet(demographicsServiceUrl);
        demographicsResponse.EnsureSuccessStatusCode();

        var demographicsJson = await demographicsResponse.Content.ReadAsStringAsync();
        _logger.LogInformation("Demographics data retrieved");
        demographicsData = JsonSerializer.Deserialize<DemographicsData>(demographicsJson);

        return demographicsData;
    }

    private async Task SendToCreateParticipantScreeningProfileAsync(Participant participant)
    {
        DemographicsData demographicsData = await GetDemographicsDataAsync(participant.nhs_number);

        var screeningProfile = new ParticipantScreeningProfile
        {
            NhsNumber = participant.nhs_number,
            ScreeningName = String.Empty,
            PrimaryCareProvider = demographicsData.PrimaryCareProvider,
            PreferredLanguage = demographicsData.PreferredLanguage,
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

        var screeningProfileUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningProfileUrl");

        string serializedParticipantScreeningProfile = JsonSerializer.Serialize(screeningProfile);

        _logger.LogInformation($"Sending ParticipantScreeningProfile Profile to {screeningProfileUrl}: {serializedParticipantScreeningProfile}");

        await _httpRequestService.SendPost(screeningProfileUrl, serializedParticipantScreeningProfile);
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(Episode episode)
    {
        var screeningEpisode = new ParticipantScreeningEpisode
        {
            EpisodeId = episode.EpisodeId,
            ScreeningName = episode.ScreeningId,
            NhsNumber = episode.NhsNumber,
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

        var screeningEpisodeUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl");

        string serializedParticipantScreeningEpisode = JsonSerializer.Serialize(screeningEpisode);

        _logger.LogInformation($"Sending ParticipantScreeningEpisode to {screeningEpisodeUrl}: {serializedParticipantScreeningEpisode}");

        await _httpRequestService.SendPost(screeningEpisodeUrl, serializedParticipantScreeningEpisode);
    }
}

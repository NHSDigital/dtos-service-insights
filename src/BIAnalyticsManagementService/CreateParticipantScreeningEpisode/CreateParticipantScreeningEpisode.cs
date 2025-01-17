using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using System.Text.Json.Serialization;

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

    [Function(nameof(CreateParticipantScreeningEpisode))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Create Participant Screening Episode function start");

        string serializedEvent = JsonSerializer.Serialize(eventGridEvent);
        _logger.LogInformation(serializedEvent);

        FinalizedEpisodeDto episode;

        try
        {
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            episode = JsonSerializer.Deserialize<FinalizedEpisodeDto>(eventGridEvent.Data.ToString(), options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize event data to Episode object.");
            return;
        }

        try
        {
            await SendToCreateParticipantScreeningEpisodeAsync(episode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participant screening episode.");
        }
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

        return organisationLkp;
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(FinalizedEpisodeDto episode)
    {
        ScreeningLkp screeningLkp = await GetScreeningDataAsync(episode.ScreeningId);
        OrganisationLkp organisationLkp = await GetOrganisationDataAsync(episode.OrganisationId);

        var screeningEpisode = new ParticipantScreeningEpisode
        {
            EpisodeId = episode.EpisodeId,
            ScreeningName = screeningLkp.ScreeningName,
            NhsNumber = episode.NhsNumber,
            EpisodeType = episode.EpisodeType,
            EpisodeTypeDescription = episode.EpisodeTypeDescription,
            EpisodeOpenDate = episode.EpisodeOpenDate,
            AppointmentMadeFlag = episode.AppointmentMadeFlag,
            FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate,
            ActualScreeningDate = episode.ActualScreeningDate,
            EarlyRecallDate = episode.EarlyRecallDate,
            CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy,
            EndCode = episode.EndCode,
            EndCodeDescription = episode.EndCodeDescription,
            EndCodeLastUpdated = episode.EndCodeLastUpdated,
            ReasonClosedCode = episode.ReasonClosedCode,
            ReasonClosedCodeDescription = episode.ReasonClosedCodeDescription,
            FinalActionCode =  episode.FinalActionCode,
            FinalActionCodeDescription = episode.FinalActionCodeDescription,
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

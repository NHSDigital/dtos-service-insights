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

        var createScreeningEpisodeUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl");
        var screeningDataServiceUrl = Environment.GetEnvironmentVariable("GetScreeningDataUrl");
        var referenceDataServiceUrl = Environment.GetEnvironmentVariable("GetReferenceDataUrl");

        if (string.IsNullOrEmpty(screeningDataServiceUrl))
        {
            throw new InvalidOperationException("Environment variable 'GetScreeningDataUrl' is missing.");
        }

        if (string.IsNullOrEmpty(referenceDataServiceUrl))
        {
            throw new InvalidOperationException("Environment variable 'GetReferenceDataUrl' is missing.");
        }

        if (string.IsNullOrEmpty(createScreeningEpisodeUrl))
        {
            throw new InvalidOperationException("Environment variable 'CreateParticipantScreeningEpisodeUrl' is missing.");
        }

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
            DateTime historicDataCutOffDate = new DateTime(2025, 03, 01, 0, 0, 0, DateTimeKind.Utc);

            bool isHistoric = episode.SrcSysProcessedDatetime < historicDataCutOffDate;

            if (isHistoric)
            {
                _logger.LogInformation("Data is historic.");
            }

            else
            {
                _logger.LogInformation("Data is not historic.");
            }

            await SendToCreateParticipantScreeningEpisodeAsync(episode, isHistoric);
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
        _logger.LogInformation("Requesting screening data from {Url} for Screening ID: {ScreeningId}", getScreeningDataUrl, screeningId);

        try
        {
            var response = await _httpRequestService.SendGet(getScreeningDataUrl);
            response.EnsureSuccessStatusCode();

            var screeningDataJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Screening data retrieved successfully: {ScreeningDataJson}", screeningDataJson);

            return JsonSerializer.Deserialize<ScreeningLkp>(screeningDataJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve screening data. Screening ID: {ScreeningId}, Service: {getScreeningDataUrl}, Timestamp: {Timestamp}",
                screeningId, getScreeningDataUrl, DateTime.UtcNow);

            throw new HttpRequestException($"Failed to retrieve screening data from {baseScreeningDataServiceUrl}", ex);
        }
    }

    private async Task<OrganisationLkp> GetOrganisationDataAsync(long? organisationId)
    {
        if (organisationId == null)
        {
            _logger.LogInformation("Organisation id is null... Setting Organisation Name and Code to null");
            return null;
        }

        var baseReferenceServiceUrl = Environment.GetEnvironmentVariable("GetReferenceDataUrl");
        var getReferenceDataUrl = $"{baseReferenceServiceUrl}?organisation_id={organisationId}";
        _logger.LogInformation("Requesting organisation data from {Url}", getReferenceDataUrl);

        try
        {
            var response = await _httpRequestService.SendGet(getReferenceDataUrl);
            response.EnsureSuccessStatusCode();

            var organisationDataJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Organisation data retrieved successfully: {OrganisationDataJson}", organisationDataJson);

            return JsonSerializer.Deserialize<OrganisationLkp>(organisationDataJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve organisation data. Organisation ID: {OrganisationId}, Service: {getReferenceDataUrl}, Timestamp: {Timestamp}",
                organisationId, getReferenceDataUrl, DateTime.UtcNow);

            throw new HttpRequestException($"Failed to retrieve organisation data from {baseReferenceServiceUrl}", ex);
        }
    }

    private async Task SendToCreateParticipantScreeningEpisodeAsync(FinalizedEpisodeDto episode, bool isHistoric)
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
            FinalActionCode = episode.FinalActionCode,
            FinalActionCodeDescription = episode.FinalActionCodeDescription,
            OrganisationCode = organisationLkp?.OrganisationCode,
            OrganisationName = organisationLkp?.OrganisationName,
            BatchId = episode.BatchId,
            SrcSysProcessedDatetime = episode.SrcSysProcessedDatetime,
            RecordInsertDatetime = isHistoric ? episode.SrcSysProcessedDatetime.AddDays(1) : DateTime.UtcNow,
            RecordUpdateDatetime = isHistoric ? episode.SrcSysProcessedDatetime.AddDays(1) : DateTime.UtcNow,
            ExceptionFlag = episode.ExceptionFlag
        };

        var screeningEpisodeUrl = Environment.GetEnvironmentVariable("CreateParticipantScreeningEpisodeUrl");

        string serializedParticipantScreeningEpisode = JsonSerializer.Serialize(screeningEpisode);

        _logger.LogInformation("Sending ParticipantScreeningEpisode to {Url}: {Request}", screeningEpisodeUrl, serializedParticipantScreeningEpisode);


        await _httpRequestService.SendPost(screeningEpisodeUrl, serializedParticipantScreeningEpisode);
    }
}
